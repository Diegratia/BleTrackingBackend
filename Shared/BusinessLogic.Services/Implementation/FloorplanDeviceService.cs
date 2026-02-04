using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Helpers.Consumer.Mqtt;


namespace BusinessLogic.Services.Implementation
{
    public class FloorplanDeviceService : BaseService, IFloorplanDeviceService
    {
        private readonly FloorplanDeviceRepository _repository;
        private readonly MstBleReaderRepository _mstBleReaderRepository;
        private readonly MstAccessCctvRepository _mstAccessCctvRepository;
        private readonly MstAccessControlRepository _mstAccessControlRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FloorplanDevice> _logger;
        private readonly IDistributedCache _cache;
        private readonly IDatabase _redis;
        private readonly IMqttClientService _mqttClient;
        private bool cacheDisabled = false;
        private readonly IAuditEmitter _audit;

        public FloorplanDeviceService(FloorplanDeviceRepository repository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        MstBleReaderRepository mstBleReaderRepository,
        MstAccessCctvRepository mstAccessCctvRepository,
        MstAccessControlRepository mstAccessControlRepository,
        ILogger<FloorplanDevice> logger,
        IDistributedCache cache,
        IConnectionMultiplexer redis,
        IMqttClientService mqttClient,
        IAuditEmitter audit
        ) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _mstBleReaderRepository = mstBleReaderRepository;
            _mstAccessCctvRepository = mstAccessCctvRepository;
            _mstAccessControlRepository = mstAccessControlRepository;
            _logger = logger;
            _redis = redis?.GetDatabase();
            _cache = cache;
            _mqttClient = mqttClient;
            _audit = audit;
        }
        
        private bool IsRedisAlive()
        {
            if (cacheDisabled) return false;

            try
            {
                return _redis?.Multiplexer.IsConnected ?? false;
            }
            catch
            {
                return false;
            }
        }

        private string Key(string key)
            => $"cache:floorplandevice:{AppId}:{key}";
        private string GroupKey 
            => $"cache:floorplandevice:group:{AppId}";
        
        public async Task RemoveGroupAsync()
        {
            if (!IsRedisAlive()) return;

            try
            {
                var keys = await _redis.SetMembersAsync(GroupKey);

                foreach (var k in keys)
                {
                    try { await _cache.RemoveAsync(k!); }
                    catch (Exception ex) { _logger.LogWarning(ex, "Redis remove failed"); }
                }

                await _redis.KeyDeleteAsync(GroupKey);
            }
            catch
            {
                cacheDisabled = true;  
            }
        }

        public async Task<FloorplanDeviceRead> GetByIdAsync(Guid id)
        {
            var device = await _repository.GetByIdAsync(id);
            if (device == null)
                throw new NotFoundException($"Floorplan device with id {id} not found");
            return device;
        }

        public async Task<IEnumerable<FloorplanDeviceRead>> GetAllAsync()
        {
            var cacheKey = Key("getall");

            if (IsRedisAlive())
            {
                try
                {
                    var cached = await _cache.GetStringAsync(cacheKey);
                    if (cached != null)
                    {
                        _logger.LogInformation($"Cache hit: {cacheKey}");
                        return JsonSerializer.Deserialize<IEnumerable<FloorplanDeviceRead>>(cached)!;
                    }
                }
                catch
                {
                    _logger.LogWarning("Redis get failed → fallback to DB");
                    cacheDisabled = true;
                }
            }
            _logger.LogInformation($"Cache miss: {cacheKey}");

            var devices = await _repository.GetAllAsync();

            if (IsRedisAlive())
            {
                try
                {
                    await _cache.SetStringAsync(
                        cacheKey,
                        JsonSerializer.Serialize(devices),
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                        }
                    );

                    await _redis.SetAddAsync(GroupKey, cacheKey);
                }
                catch
                {
                    cacheDisabled = true;
                }
            }
            return devices;
        }
        public async Task<IEnumerable<OpenFloorplanDeviceDto>> OpenGetAllAsync()
        {
            var devices = await _repository.GetAllExportAsync();
            return _mapper.Map<IEnumerable<OpenFloorplanDeviceDto>>(devices);
        }
        

    // ===========================================================
    // 🔹 Helper reusable
    // ===========================================================
    public async Task SetDeviceAssignmentAsync(Guid? readerId, Guid? cctvId, Guid? controlId, bool isAssigned, string username)
    {
        if (readerId.HasValue)
        {
            var reader = await _repository.GetReaderByIdAsync(readerId.Value);
            reader.IsAssigned = isAssigned;
            reader.UpdatedBy = username;
            reader.UpdatedAt = DateTime.UtcNow;
            await _mstBleReaderRepository.UpdateAsync(reader);
        }

        if (cctvId.HasValue)
        {
            var cctv = await _repository.GetAccessCctvByIdAsync(cctvId.Value);
            cctv.IsAssigned = isAssigned;
            cctv.UpdatedBy = username;
            cctv.UpdatedAt = DateTime.UtcNow;
            await _mstAccessCctvRepository.UpdateAsync(cctv);
        }

        if (controlId.HasValue)
        {
            var control = await _repository.GetAccessControlByIdAsync(controlId.Value);
            control.IsAssigned = isAssigned;
            control.UpdatedBy = username;
            control.UpdatedAt = DateTime.UtcNow;
            await _mstAccessControlRepository.UpdateAsync(control);
        }
    }

        // ===========================================================
        // 🔹 CREATE
        // ===========================================================
        public async Task<FloorplanDeviceRead> CreateAsync(FloorplanDeviceCreateDto dto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            await RemoveGroupAsync();
            await _mqttClient.PublishAsync("engine/refresh/area-related", "");
            using var transaction = await _repository.BeginTransactionAsync();
            try
            {
                if (await _repository.GetFloorplanByIdAsync(dto.FloorplanId) == null)
                    throw new NotFoundException($"Floorplan with ID {dto.FloorplanId} not found.");

                if (dto.ReaderId.HasValue && await _repository.GetReaderByIdAsync(dto.ReaderId.Value) == null)
                    throw new NotFoundException($"Reader with ID {dto.ReaderId} not found.");

                if (dto.AccessCctvId.HasValue && await _repository.GetAccessCctvByIdAsync(dto.AccessCctvId.Value) == null)
                    throw new NotFoundException($"Access CCTV with ID {dto.AccessCctvId} not found.");

                if (dto.AccessControlId.HasValue && await _repository.GetAccessControlByIdAsync(dto.AccessControlId.Value) == null)
                    throw new NotFoundException($"Access Control with ID {dto.AccessControlId} not found.");

                if (dto.FloorplanMaskedAreaId != null && await _repository.GetFloorplanMaskedAreaByIdAsync(dto.FloorplanMaskedAreaId) == null)
                    throw new NotFoundException($"FloorplanMaskedArea with ID {dto.FloorplanMaskedAreaId} not found.");

                var device = _mapper.Map<FloorplanDevice>(dto);
                device.CreatedBy = username;
                device.CreatedAt = DateTime.UtcNow;
                device.UpdatedBy = username;
                device.UpdatedAt = DateTime.UtcNow;

                await SetDeviceAssignmentAsync(dto.ReaderId, dto.AccessCctvId, dto.AccessControlId, true, username);
                await _repository.AddAsync(device);
                await _audit.Created(
                    "Floorplan Device",
                    device.Id,
                    "Created floorplan device",
                    new { device.Name }
                );
                await transaction.CommitAsync();
                var result = await _repository.GetByIdAsync(device.Id);
                return result!;
            } 
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
    }
    

        // ===========================================================
        // 🔹 UPDATE
        // ===========================================================
        public async Task UpdateAsync(Guid id, FloorplanDeviceUpdateDto dto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            using var transaction = await _repository.BeginTransactionAsync();
            try
            {
                var device = await _repository.GetByIdEntityAsync(id);
                if (device == null)
                    throw new NotFoundException($"Floorplan device with id {id} not found");

                // Jika ada perubahan device, unassign lama dan assign baru
                if (device.ReaderId != dto.ReaderId ||
                    device.AccessCctvId != dto.AccessCctvId ||
                    device.AccessControlId != dto.AccessControlId)
                {
                    await SetDeviceAssignmentAsync(device.ReaderId, device.AccessCctvId, device.AccessControlId, false, username);
                    await SetDeviceAssignmentAsync(dto.ReaderId, dto.AccessCctvId, dto.AccessControlId, true, username);
                }

                _mapper.Map(dto, device);
                device.UpdatedBy = username;
                device.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(device);
                await transaction.CommitAsync();
                await _audit.Updated(
                    "Floorplan Device",
                    device.Id,
                    "Updated floorplan device",
                    new { device.Name }
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            await RemoveGroupAsync();
            await _mqttClient.PublishAsync("engine/refresh/area-related", "");
        }

        //     // ===========================================================
        //     // 🔹 DELETE (Soft Delete)
        //     // ===========================================================
            public async Task DeleteAsync(Guid id)
            {
                var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

                var device = await _repository.GetByIdEntityAsync(id);
                using var transaction = await _repository.BeginTransactionAsync();
                try
                {
                    if (device == null)
                        throw new NotFoundException($"Floorplan device with id {id} not found");

                    await SetDeviceAssignmentAsync(device.ReaderId, device.AccessCctvId, device.AccessControlId, false, username);

                    device.UpdatedBy = username;
                    device.UpdatedAt = DateTime.UtcNow;
                    device.Status = 0;

                    await _repository.SoftDeleteAsync(id);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
                await _audit.Deleted(
                        "Floorplan Device",
                        device.Id,
                        "Deleted floorplan device",
                        new { device.Name }
                    );
                await RemoveGroupAsync();
                await _mqttClient.PublishAsync("engine/refresh/area-related", "");
        }
    

    // ============================================================
    // INTERNAL CASCADE DELETE
    // ============================================================
    public async Task CascadeDeleteAsync(Guid id, string username)
    {
        var device = await _repository.GetByIdEntityAsync(id);
        if (device == null) return;

        await SetDeviceAssignmentAsync(
            device.ReaderId,
            device.AccessCctvId,
            device.AccessControlId,
            false,
            username
        );

        device.Status = 0;
        device.UpdatedBy = username;
        device.UpdatedAt = DateTime.UtcNow;

        await _repository.SoftDeleteAsync(device.Id);

        // ❌ NO TRANSACTION
        // ❌ NO AUDIT
        // ❌ NO MQTT
    }

        public async Task<IEnumerable<FloorplanDeviceRead>> ImportAsync(IFormFile file)
        {
            var devices = new List<FloorplanDevice>();
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var userApplicationId = _httpContextAccessor.HttpContext?.User.FindFirst("ApplicationId")?.Value;

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1);

            int rowNumber = 2;
            foreach (var row in rows)
            {

                var floorplanStr = row.Cell(1).GetValue<string>();
                if (!Guid.TryParse(floorplanStr, out var floorplanId))
                    throw new ArgumentException($"Invalid floorplanId format at row {rowNumber}");

                var floorplan = await _repository.GetFloorplanByIdAsync(floorplanId);
                if (floorplan == null)
                    throw new ArgumentException($"floorplanId {floorplanId} not found at row {rowNumber}");

                var accessCctvStr = row.Cell(2).GetValue<string>();
                if (!Guid.TryParse(accessCctvStr, out var accessCctvId))
                    throw new ArgumentException($"Invalid AccessCctvId format at row {rowNumber}");

                var accessCctv = await _repository.GetAccessCctvByIdAsync(accessCctvId);
                if (accessCctv == null)
                    throw new ArgumentException($"AccessCctvId {accessCctvId} not found at row {rowNumber}");

                var readerStr = row.Cell(3).GetValue<string>();
                if (!Guid.TryParse(readerStr, out var readerId))
                    throw new ArgumentException($"Invalid ReaderId format at row {rowNumber}");

                var reader = await _repository.GetReaderByIdAsync(readerId);
                if (reader == null)
                    throw new ArgumentException($"ReaderId {readerId} not found at row {rowNumber}");

                var accessControlStr = row.Cell(4).GetValue<string>();
                if (!Guid.TryParse(accessControlStr, out var accessControlId))
                    throw new ArgumentException($"Invalid AccessControlId format at row {rowNumber}");

                var accessControl = await _repository.GetAccessControlByIdAsync(accessControlId);
                if (accessControl == null)
                    throw new ArgumentException($"AccessControlId {accessControlId} not found at row {rowNumber}");

                var floorplanMaskedAreaStr = row.Cell(5).GetValue<string>();
                if (!Guid.TryParse(floorplanMaskedAreaStr, out var floorplanMaskedAreaId))
                    throw new ArgumentException($"Invalid FloorplanMaskedAreaId format at row {rowNumber}");

                var floorplanMaskedArea = await _repository.GetFloorplanMaskedAreaByIdAsync(floorplanMaskedAreaId);
                if (floorplanMaskedArea == null)
                    throw new ArgumentException($"FloorplanMaskedAreaId {floorplanMaskedAreaId} not found at row {rowNumber}");

                var device = new FloorplanDevice
                {
                    Id = Guid.NewGuid(),
                    FloorplanId = floorplanId,
                    AccessCctvId = accessCctvId,
                    ReaderId = readerId,
                    AccessControlId = accessControlId,
                    FloorplanMaskedAreaId = floorplanMaskedAreaId,
                    ApplicationId = Guid.Parse(userApplicationId),
                    Name = row.Cell(6).GetValue<string>(),
                    Type = (DeviceType)Enum.Parse(typeof(DeviceType), row.Cell(7).GetValue<string>()),
                    PosX = row.Cell(8).GetValue<float>(),
                    PosY = row.Cell(9).GetValue<float>(),
                    PosPxX = row.Cell(10).GetValue<float>(),
                    PosPxY = row.Cell(11).GetValue<float>(),
                    DeviceStatus = (DeviceStatus)Enum.Parse(typeof(DeviceStatus), row.Cell(12).GetValue<string>()),
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                };

                devices.Add(device);
                rowNumber++;
            }

            // Simpan ke database
            foreach (var device in devices)
            {
                await _repository.AddAsync(device);
            }
            var ids = devices.Select(x => x.Id).ToList();
            var query = _repository.GetAllQueryable()
                .Where(x => ids.Contains(x.Id));
            return await _repository.ProjectToRead(query).ToListAsync();
        }


        public async Task<object> FilterAsync(
            DataTablesProjectedRequest request,
            FloorplanDeviceFilter filter
        )
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            if (request.DateFilters != null)
            {
                if (request.DateFilters.TryGetValue("UpdatedAt", out var dateFilter))
                {
                    filter.DateFrom = dateFilter.DateFrom;
                    filter.DateTo = dateFilter.DateTo;
                }
            }

            var (data, total, filtered) = await _repository.FilterAsync(filter);

            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data
            };
        }

        
        public async Task<byte[]> ExportPdfAsync()
        {
            var devices = await _repository.GetAllExportAsync();

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Floorplan Device Report")
                        .SemiBold().FontSize(16).FontColor(Colors.Black).AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Type").SemiBold();
                            header.Cell().Element(CellStyle).Text("Floorplan").SemiBold();
                            header.Cell().Element(CellStyle).Text("CCTV").SemiBold();
                            header.Cell().Element(CellStyle).Text("Reader").SemiBold();
                            header.Cell().Element(CellStyle).Text("Access Control").SemiBold();
                            header.Cell().Element(CellStyle).Text("Device Status").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        int index = 1;
                        foreach (var device in devices)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(device.Name);
                            table.Cell().Element(CellStyle).Text(device.Type.ToString());
                            table.Cell().Element(CellStyle).Text(device.Floorplan?.Name);
                            table.Cell().Element(CellStyle).Text(device.AccessCctv?.Name);
                            table.Cell().Element(CellStyle).Text(device.Reader?.Name);
                            table.Cell().Element(CellStyle).Text(device.AccessControl?.Name);
                            table.Cell().Element(CellStyle).Text(device.DeviceStatus.ToString());
                            table.Cell().Element(CellStyle).Text(device.Status.ToString());
                        }

                        static IContainer CellStyle(IContainer container) =>
                            container
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(4)
                                .PaddingHorizontal(6);
                    });

                    page.Footer()
                        .AlignRight()
                        .Text(txt =>
                        {
                            txt.Span("Generated at: ").SemiBold();
                            txt.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
                        });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> ExportExcelAsync()
        {
            var devices = await _repository.GetAllExportAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Devices");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Type";
            worksheet.Cell(1, 4).Value = "Floorplan";
            worksheet.Cell(1, 5).Value = "CCTV";
            worksheet.Cell(1, 6).Value = "Reader";
            worksheet.Cell(1, 7).Value = "Access Control";
            worksheet.Cell(1, 8).Value = "Device Status";
            worksheet.Cell(1, 9).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var device in devices)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = device.Name;
                worksheet.Cell(row, 3).Value = device.Type.ToString();
                worksheet.Cell(row, 4).Value = device.Floorplan?.Name;
                worksheet.Cell(row, 5).Value = device.AccessCctv?.Name;
                worksheet.Cell(row, 6).Value = device.Reader?.Name;
                worksheet.Cell(row, 7).Value = device.AccessControl?.Name;
                worksheet.Cell(row, 8).Value = device.DeviceStatus.ToString();
                worksheet.Cell(row, 9).Value = device.Status;
                row++;
            }

            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

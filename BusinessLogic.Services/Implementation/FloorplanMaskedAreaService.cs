using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Helpers.Consumer.Mqtt;
using System.IO;

namespace BusinessLogic.Services.Implementation
{
    public class FloorplanMaskedAreaService : BaseService, IFloorplanMaskedAreaService
    {
        private readonly FloorplanMaskedAreaRepository _repository;
        private readonly FloorplanDeviceRepository _floorplanDeviceRepository;
        private readonly IFloorplanDeviceService _floorplanDeviceService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FloorplanMaskedArea> _logger;
        private readonly IDistributedCache _cache;
        private readonly IDatabase _redis;
        private readonly IMqttClientService _mqttClient;
        private bool cacheDisabled = false;

        public FloorplanMaskedAreaService(
            FloorplanMaskedAreaRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            FloorplanDeviceRepository floorplanDeviceRepository,
            IFloorplanDeviceService floorplanDeviceService,
            ILogger<FloorplanMaskedArea> logger,
            IDistributedCache cache,
            IConnectionMultiplexer redis,
            IMqttClientService mqttClient
        ) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _floorplanDeviceRepository = floorplanDeviceRepository;
            _floorplanDeviceService = floorplanDeviceService;
            _logger = logger;
            _cache = cache;
            _redis = redis?.GetDatabase();
            _mqttClient = mqttClient;
        }

        // Redis safety wrapper
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

        private string Key(string key) =>
            $"cache:area:{AppId}:{key}";

        private string GroupKey =>
            $"cache:area:group:{AppId}";


        // ============================================================
        // GROUP CACHE REMOVAL
        // ============================================================
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
                cacheDisabled = true;   // matikan redis cache total
            }
        }


        // ============================================================
        // GET BY ID
        // ============================================================
        public async Task<FloorplanMaskedAreaDto> GetByIdAsync(Guid id)
        {
            var area = await _repository.GetByIdAsync(id);
            return area == null ? null : _mapper.Map<FloorplanMaskedAreaDto>(area);
        }

        public async Task<IEnumerable<FloorplanMaskedAreaDto>> GetAllAsync()
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
                        return JsonSerializer.Deserialize<IEnumerable<FloorplanMaskedAreaDto>>(cached)!;
                    }
                }
                catch
                {
                    _logger.LogWarning("Redis get failed → fallback to DB");
                    cacheDisabled = true;
                }
            }

            _logger.LogInformation($"Cache miss: {cacheKey}");

            var data = await _repository.GetAllAsync();
            var mapped = _mapper.Map<IEnumerable<FloorplanMaskedAreaDto>>(data);

            if (IsRedisAlive())
            {
                try
                {
                    await _cache.SetStringAsync(
                        cacheKey,
                        JsonSerializer.Serialize(mapped),
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

            return mapped;
        }

        public async Task<IEnumerable<OpenFloorplanMaskedAreaDto>> OpenGetAllAsync()
        {
            var areas = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<OpenFloorplanMaskedAreaDto>>(areas);
        }


        // ============================================================
        // CREATE
        // ============================================================
        public async Task<FloorplanMaskedAreaDto> CreateAsync(FloorplanMaskedAreaCreateDto createDto)
        {
            var floor = await _repository.GetFloorByIdAsync(createDto.FloorId);
            if (floor == null)
                throw new ArgumentException($"Floor with ID {createDto.FloorId} not found.");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            var area = _mapper.Map<FloorplanMaskedArea>(createDto);
            area.Id = Guid.NewGuid();
            area.Status = 1;
            area.CreatedBy = username;
            area.CreatedAt = DateTime.UtcNow;
            area.UpdatedBy = username;
            area.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(area);

            await RemoveGroupAsync();
            await _mqttClient.PublishAsync("engine/refresh/area-related", "");

            return _mapper.Map<FloorplanMaskedAreaDto>(area);
        }


        // ============================================================
        // UPDATE
        // ============================================================
        public async Task UpdateAsync(Guid id, FloorplanMaskedAreaUpdateDto updateDto)
        {
            var floor = await _repository.GetFloorByIdAsync(updateDto.FloorId);
            if (floor == null)
                throw new ArgumentException($"Floor with ID {updateDto.FloorId} not found.");

            var area = await _repository.GetByIdAsync(id);
            if (area == null)
                throw new KeyNotFoundException("Area not found");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            _mapper.Map(updateDto, area);

            area.UpdatedBy = username;
            area.UpdatedAt = DateTime.UtcNow;

            await RemoveGroupAsync();
            await _repository.UpdateAsync(area);
            await _mqttClient.PublishAsync("engine/refresh/area-related", "");
        }


        // ============================================================
        // DELETE
        // ============================================================
        // public async Task DeleteAsync(Guid id)
        // {
        //     var area = await _repository.GetByIdAsync(id);
        //     if (area == null) throw new KeyNotFoundException("Area not found");

        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

        //     area.Status = 0;
        //     area.UpdatedBy = username;
        //     area.UpdatedAt = DateTime.UtcNow;

        //     await RemoveGroupAsync();
        //     await _repository.SoftDeleteAsync(id);
        //     await _mqttClient.PublishAsync("engine/refresh/area-related", "");
        // }


        // ============================================================
        // SOFT DELETE WITH CHILDREN
        // ============================================================
        public async Task SoftDeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            var area = await _repository.GetByIdAsync(id);
            if (area == null)
                throw new KeyNotFoundException("Area not found");

            await _repository.ExecuteInTransactionAsync(async () =>
            {
                area.Status = 0;
                area.UpdatedBy = username;
                area.UpdatedAt = DateTime.UtcNow;

                await RemoveGroupAsync();
                await _repository.SoftDeleteAsync(id);
                var devices = await _floorplanDeviceRepository.GetByAreaIdAsync(id);
                if (devices == null)
                    throw new KeyNotFoundException("FloorplanDevice not found");

                foreach (var d in devices)
                {
                    // d.Status = 0;
                    // d.UpdatedBy = username;
                    // d.UpdatedAt = DateTime.UtcNow;
                    await _floorplanDeviceService.DeleteAsync(d.Id);
                }

                await _mqttClient.PublishAsync("engine/refresh/area-related", "");
            });
        }


        // ============================================================
        // FILTER DATATABLE
        // ============================================================
        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable().AsNoTracking();

            var searchableColumns = new[] { "Name", "Floor.Name", "Floorplan.Name" };
            var validSortColumns = new[] { "Name", "Floor.Name", "Floorplan.Name", "CreatedAt", "UpdatedAt", "RestrictedStatus", "Status" };

            var filterService = new GenericDataTableService<FloorplanMaskedArea, FloorplanMaskedAreaDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns
            );

            return await filterService.FilterAsync(request);
        }


        // ============================================================
        // EXPORT PDF
        // ============================================================
        public async Task<byte[]> ExportPdfAsync()
        {
            var areas = await _repository.GetAllExportAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Text("Floorplan Masked Area Report")
                        .SemiBold().FontSize(16).AlignCenter();

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
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Floor").SemiBold();
                            header.Cell().Element(CellStyle).Text("Floorplan").SemiBold();
                            header.Cell().Element(CellStyle).Text("Restricted").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created").SemiBold();
                            header.Cell().Element(CellStyle).Text("By").SemiBold();
                        });

                        int i = 1;

                        foreach (var area in areas)
                        {
                            table.Cell().Element(CellStyle).Text(i++.ToString());
                            table.Cell().Element(CellStyle).Text(area.Name);
                            table.Cell().Element(CellStyle).Text(area.Floor?.Name);
                            table.Cell().Element(CellStyle).Text(area.Floorplan?.Name);
                            table.Cell().Element(CellStyle).Text(area.RestrictedStatus.ToString());
                            table.Cell().Element(CellStyle).Text(area.CreatedAt.ToString());
                            table.Cell().Element(CellStyle).Text(area.CreatedBy);
                        }

                        static IContainer CellStyle(IContainer container) =>
                            container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4);
                    });
                });
            });

            return document.GeneratePdf();
        }


        // ============================================================
        // EXPORT EXCEL
        // ============================================================
        public async Task<byte[]> ExportExcelAsync()
        {
            var areas = await _repository.GetAllExportAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Masked Areas");

            ws.Cell(1, 1).Value = "#";
            ws.Cell(1, 2).Value = "Name";
            ws.Cell(1, 3).Value = "Floor";
            ws.Cell(1, 4).Value = "Floorplan";
            ws.Cell(1, 5).Value = "Restricted";
            ws.Cell(1, 6).Value = "CreatedAt";
            ws.Cell(1, 7).Value = "CreatedBy";

            int r = 2;
            int n = 1;

            foreach (var a in areas)
            {
                ws.Cell(r, 1).Value = n++;
                ws.Cell(r, 2).Value = a.Name;
                ws.Cell(r, 3).Value = a.Floor?.Name;
                ws.Cell(r, 4).Value = a.Floorplan?.Name;
                ws.Cell(r, 5).Value = a.RestrictedStatus.ToString();
                ws.Cell(r, 6).Value = a.CreatedAt;
                ws.Cell(r, 7).Value = a.CreatedBy;
                r++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

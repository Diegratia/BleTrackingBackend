using AutoMapper;
using BusinessLogic.Services.Background;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using System.IO;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using DataView;
using Shared.Contracts;
using Shared.Contracts.Read;


namespace BusinessLogic.Services.Implementation
{
    public class MstFloorService : BaseService, IMstFloorService
    {
        private readonly MstFloorRepository _repository;
        private readonly IMapper _mapper;
        private readonly string[] _allowedImageTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
        private const long MaxFileSize = 50 * 1024 * 1024; // Maksimal 50 MB
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FloorplanMaskedAreaRepository _maskedAreaRepository;
        private readonly MstFloorplanRepository _floorplanRepository;
        private readonly IMstFloorplanService _floorplanService;
        private readonly ILogger<MstFloor> _logger;
        private readonly IDistributedCache _cache;
        private readonly IDatabase _redis;
        private readonly IMqttPubQueue _mqttQueue;
        private readonly IAuditEmitter _audit;
        private bool cacheDisabled = false;



        public MstFloorService(
                                        MstFloorRepository repository,
                                        IMapper mapper,
                                        IHttpContextAccessor httpContextAccessor,
                                        FloorplanMaskedAreaRepository maskedAreaRepository,
                                        MstFloorplanRepository floorplanRepository,
                                        IMstFloorplanService floorplanService,
                                        ILogger<MstFloor> logger,
                                        IDistributedCache cache,
                                        IConnectionMultiplexer redis,
                                        IMqttPubQueue mqttQueue,
                                        IAuditEmitter audit
                                        ) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _maskedAreaRepository = maskedAreaRepository;
            _floorplanRepository = floorplanRepository;
            _floorplanService = floorplanService;
            _logger = logger;
            _mqttQueue = mqttQueue;
            _cache = cache;
            _audit = audit;
            _redis = redis?.GetDatabase();
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
            => $"cache:mstfloor:{AppId}:{key}";
        private string GroupKey 
            => $"cache:mstfloor:group:{AppId}";


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


        public async Task<MstFloorRead> GetByIdAsync(Guid id)
        {
            var floor = await _repository.GetByIdAsync(id);
            if (floor == null)
                throw new NotFoundException($"Floor with id {id} not found");
            return floor;
        }

            public async Task<IEnumerable<MstFloorRead>> GetAllAsync()
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
                        return JsonSerializer.Deserialize<IEnumerable<MstFloorRead>>(cached)!;
                    }
                }
                catch
                {
                    _logger.LogWarning("Redis get failed → fallback to DB");
                    cacheDisabled = true;
                }
            }
            _logger.LogInformation($"Cache miss: {cacheKey}");

            var floors = await _repository.GetAllAsync();
            if (IsRedisAlive())
            {
                try
                {
                    await _cache.SetStringAsync(
                        cacheKey,
                        JsonSerializer.Serialize(floors),
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
            return floors;
        }


        public async Task<IEnumerable<MstFloorRead>> OpenGetAllAsync()
        {
            var floors = await _repository.GetAllAsync();
            return floors;
        }


        public async Task<MstFloorRead> CreateAsync(MstFloorCreateDto createDto)
        {
            var floor = _mapper.Map<MstFloor>(createDto);
            if (!await _repository.BuildingExistsAsync(createDto.BuildingId))
                throw new NotFoundException($"Building with id {createDto.BuildingId} not found");

            var invalidBuildingId =
                await _repository.CheckInvalidBuildingOwnershipAsync(createDto.BuildingId, AppId);
            if (invalidBuildingId.Any())
            {
                throw new UnauthorizedException(
                    $"BuildingId does not belong to this Application: {string.Join(", ", invalidBuildingId)}"
                );
            }

            var building = await _repository.GetBuildingByIdAsync(createDto.BuildingId);
            if (building == null)
                throw new NotFoundException($"Building with id {createDto.BuildingId} not found");
            floor.BuildingId = building.Id;
            floor.ApplicationId = building.ApplicationId;

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            floor.Id = Guid.NewGuid();
            floor.Status = 1;
            floor.CreatedBy = username ?? "Annonymous";
            floor.CreatedAt = DateTime.UtcNow;
            floor.UpdatedBy = username ?? "Annonymous";
            floor.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(floor);
            _audit.Created(
                "Floor Area",
                floor.Id,
                "Created floor",
                new { floor.Name }
            );
            await RemoveGroupAsync();
            _mqttQueue.Enqueue("engine/refresh/area-related", "");
            var result = await _repository.GetByIdAsync(floor.Id);
            return result!;
        }
        
        

        public async Task UpdateAsync(Guid id, MstFloorUpdateDto updateDto)
        {
            var floor = await _repository.GetByIdEntityAsync(id);
            if (floor == null)
                throw new NotFoundException($"Floor with id {id} not found");

            if (updateDto.BuildingId.HasValue && updateDto.BuildingId.Value != floor.BuildingId)
            {
                if (!await _repository.BuildingExistsAsync(updateDto.BuildingId.Value))
                    throw new NotFoundException($"Building with id {updateDto.BuildingId.Value} not found");

                var invalidBuildingId =
                    await _repository.CheckInvalidBuildingOwnershipAsync(updateDto.BuildingId.Value, AppId);
                if (invalidBuildingId.Any())
                {
                    throw new UnauthorizedException(
                        $"BuildingId does not belong to this Application: {string.Join(", ", invalidBuildingId)}"
                    );
                }

                var building = await _repository.GetBuildingByIdAsync(updateDto.BuildingId.Value);
                if (building == null)
                    throw new NotFoundException($"Building with id {updateDto.BuildingId.Value} not found");
                floor.BuildingId = building.Id;
            }

            // if (updateDto.FloorImage != null && updateDto.FloorImage.Length > 0)
            // {
            //     if (string.IsNullOrEmpty(updateDto.FloorImage.ContentType) || !_allowedImageTypes.Contains(updateDto.FloorImage.ContentType))
            //         throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

            //     if (updateDto.FloorImage.Length > MaxFileSize)
            //         throw new ArgumentException("File size exceeds 50 MB limit.");

            //     var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "FloorImages");
            //     Directory.CreateDirectory(uploadDir);

            //     if (!string.IsNullOrEmpty(floor.FloorImage))
            //     {
            //         var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), floor.FloorImage.TrimStart('/'));
            //         if (File.Exists(oldFilePath))
            //         {
            //             try
            //             {
            //                 File.Delete(oldFilePath);
            //             }
            //             catch (IOException ex)
            //             {
            //                 throw new IOException("Failed to delete old image file.", ex);
            //             }
            //         }
            //     }

            //     var fileExtension = Path.GetExtension(updateDto.FloorImage.FileName)?.ToLower() ?? ".jpg";
            //     var fileName = $"{Guid.NewGuid()}{fileExtension}";
            //     var filePath = Path.Combine(uploadDir, fileName);

            //     try
            //     {
            //         using (var stream = new FileStream(filePath, FileMode.Create))
            //         {
            //             await updateDto.FloorImage.CopyToAsync(stream);
            //         }
            //     }
            //     catch (IOException ex)
            //     {
            //         throw new IOException("Failed to save image file.", ex);
            //     }

            //     floor.FloorImage = $"/Uploads/FloorImages/{fileName}";
            // }

            _mapper.Map(updateDto, floor);
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            floor.UpdatedBy = username;
            floor.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(floor);
            await RemoveGroupAsync();
            _mqttQueue.Enqueue("engine/refresh/area-related", "");
            _audit.Updated(
                "Floor Area",
                floor.Id,
                "Updated floor",
                new { floor.Name }
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var floor = await _repository.GetByIdEntityAsync(id);
            if (floor == null)
                throw new NotFoundException($"Floor with id {id} not found");

            await _repository.ExecuteInTransactionAsync(async () =>
            {
                var floorplans = await _floorplanRepository.GetByFloorIdAsync(id);
                foreach (var floorplan in floorplans)
                {
                    await _floorplanService.DeleteAsync(floorplan.Id);
                }
                floor.UpdatedBy = username;
                floor.UpdatedAt = DateTime.UtcNow;
                floor.Status = 0;
                await _repository.SoftDeleteAsync(id);
            });
             _audit.Deleted(
                    "Floor Area",
                    floor.Id,
                    "Deleted floor",
                    new { floor.Name }
                );
            await RemoveGroupAsync();
            await _floorplanService.RemoveGroupAsync();
            _mqttQueue.Enqueue("engine/refresh/area-related", "");
        }

        public async Task CascadeDeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var floor = await _repository.GetByIdAsync(id);
            if (floor == null)
                throw new NotFoundException($"Floor with id {id} not found");
            var floorplans = await _floorplanRepository.GetByFloorIdAsync(id);
            foreach (var floorplan in floorplans)
            {
                await _floorplanService.CascadeDeleteAsync(floorplan.Id);
            }
            floor.UpdatedBy = username;
            floor.UpdatedAt = DateTime.UtcNow;
            floor.Status = 0;
            await _repository.SoftDeleteAsync(id);
        }

        public async Task<IEnumerable<MstFloorRead>> ImportAsync(IFormFile file)
        {
            var floors = new List<MstFloor>();
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // skip header

            int rowNumber = 2; // start dari baris ke 2
            foreach (var row in rows)
            {
                // validasi
                var buildingIdStr = row.Cell(1).GetValue<string>();
                if (!Guid.TryParse(buildingIdStr, out var buildingId))
                    throw new ArgumentException($"Invalid BuildingId format at row {rowNumber}");

                var building = await _repository.GetBuildingByIdAsync(buildingId);
                if (building == null)
                    throw new ArgumentException($"BuildingId {buildingId} not found at row {rowNumber}");

                var floor = new MstFloor
                {
                    Id = Guid.NewGuid(),
                    BuildingId = buildingId,
                    Name = row.Cell(2).GetValue<string>(),
                    ApplicationId = building.ApplicationId,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                };

                floors.Add(floor);
                rowNumber++;
            }

            foreach (var floor in floors)
            {
                await _repository.AddAsync(floor);
            }

            return floors.Select(f => new MstFloorRead
            {
                Id = f.Id,
                BuildingId = f.BuildingId,
                Name = f.Name,
                Status = f.Status ?? 0,
                ApplicationId = f.ApplicationId,
                CreatedBy = f.CreatedBy,
                CreatedAt = f.CreatedAt,
                UpdatedBy = f.UpdatedBy,
                UpdatedAt = f.UpdatedAt
            }).ToList();
        }

        public async Task<object> FilterAsync(
            DataTablesProjectedRequest request,
            MstFloorFilter filter
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
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            var floors = await _repository.GetAllExportAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Floor Report")
                        .SemiBold().FontSize(16).FontColor(Colors.Black).AlignCenter();

                    page.Content().Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);   // No.
                            columns.RelativeColumn(2); // Building Name
                            columns.RelativeColumn(2); // Building Id
                            columns.RelativeColumn(2); // Floor Name
                            columns.RelativeColumn(1); // MeterPerPx
                            columns.RelativeColumn(2); // CreatedAt
                            columns.RelativeColumn(2); // CreatedBy
                        });

                        // Table header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Building").SemiBold();
                            header.Cell().Element(CellStyle).Text("BuildingId").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Meter/Px").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created At").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created By").SemiBold();
                        });

                        // Table body
                        int index = 1;
                        foreach (var floor in floors)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(floor.Building?.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(floor.BuildingId.ToString() ?? "-");
                            table.Cell().Element(CellStyle).Text(floor.Name);
                            table.Cell().Element(CellStyle).Text(floor.CreatedAt.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(floor.CreatedBy ?? "-");
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
            var floors = await _repository.GetAllExportAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Floors");

            // Header
            worksheet.Cell(1, 1).Value = "No";
            worksheet.Cell(1, 2).Value = "Building";
            worksheet.Cell(1, 3).Value = "BuildingId";
            worksheet.Cell(1, 4).Value = "Floor Name";
            worksheet.Cell(1, 5).Value = "Created By";
            worksheet.Cell(1, 6).Value = "Created At";
            worksheet.Cell(1, 7).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var floor in floors)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = floor.Building?.Name ?? "-";
                worksheet.Cell(row, 3).Value = floor.BuildingId.ToString();
                worksheet.Cell(row, 4).Value = floor.Name;
                worksheet.Cell(row, 5).Value = floor.CreatedBy;
                worksheet.Cell(row, 6).Value = floor.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cell(row, 7).Value = floor.Status == 1 ? "Active" : "Inactive";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }


    }

}



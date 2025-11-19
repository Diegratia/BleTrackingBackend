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
using Helpers.Consumer.Mqtt;


namespace BusinessLogic.Services.Implementation
{
    public class MstBuildingService : BaseService, IMstBuildingService
    {
        private readonly MstBuildingRepository _repository;
        private readonly IMstFloorService _floorService;
        private readonly MstFloorRepository _floorRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string[] _allowedImageTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
        private const long MaxFileSize = 5 * 1024 * 1024; // Maksimal 1 MB
         private readonly IMqttClientService _mqttClient;
        private readonly IDistributedCache _cache;
        private readonly IDatabase _redis;
        private readonly ILogger<MstBuilding>_logger;
        private bool cacheDisabled = false;

        public MstBuildingService(
            MstBuildingRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IMstFloorService floorService,
            MstFloorRepository floorRepository,
            IDistributedCache cache,
            IConnectionMultiplexer redis,
            ILogger<MstBuilding> logger
            ): base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _floorService = floorService;
            _floorRepository = floorRepository;
            _cache = cache;
            _redis = redis?.GetDatabase();
            _logger = logger;
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
            => $"cache:mstbuilding:{AppId}:{key}";
        private string GroupKey 
            => $"cache:mstbuilding:group:{AppId}";

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

        public async Task<MstBuildingDto> GetByIdAsync(Guid id)
        {
            var building = await _repository.GetByIdAsync(id);
            return building == null ? null : _mapper.Map<MstBuildingDto>(building);
        }

            public async Task<IEnumerable<MstBuildingDto>> GetAllAsync()
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
                        return JsonSerializer.Deserialize<IEnumerable<MstBuildingDto>>(cached)!;
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
            var mapped = _mapper.Map<IEnumerable<MstBuildingDto>>(data);

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


            public async Task<IEnumerable<OpenMstBuildingDto>> OpenGetAllAsync()
        {
            var buildings = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<OpenMstBuildingDto>>(buildings);
        }

        public async Task<MstBuildingDto> CreateAsync(MstBuildingCreateDto createDto)
        {
            var building = _mapper.Map<MstBuilding>(createDto);
            if (createDto.Image != null && createDto.Image.Length > 0)
            {
                if (string.IsNullOrEmpty(createDto.Image.ContentType) || !_allowedImageTypes.Contains(createDto.Image.ContentType))
                    throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

                if (createDto.Image.Length > MaxFileSize)
                    throw new ArgumentException("File size exceeds 5 MB limit.");

                // var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "BuildingImages");
                // Directory.CreateDirectory(uploadDir);

                var basePath = AppContext.BaseDirectory;
                var uploadDir = Path.Combine(basePath, "Uploads", "BuildingImages");
                Directory.CreateDirectory(uploadDir);

                var fileExtension = Path.GetExtension(createDto.Image.FileName)?.ToLower() ?? ".jpg";
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadDir, fileName);

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await createDto.Image.CopyToAsync(stream);
                    }
                }
                catch (IOException ex)
                {
                    throw new IOException("Failed to save image file.", ex);
                }

                building.Image = $"/Uploads/BuildingImages/{fileName}";
            }

            var username = UsernameFormToken;
            building.Id = Guid.NewGuid();
            building.CreatedBy = username;
            building.CreatedAt = DateTime.UtcNow;
            building.UpdatedBy = username;
            building.UpdatedAt = DateTime.UtcNow;
            building.Status = 1;

            var createdBuilding = await _repository.AddAsync(building);
            await RemoveGroupAsync();
            await _mqttClient.PublishAsync("engine/refresh/area-related", "");
            return _mapper.Map<MstBuildingDto>(createdBuilding);
        }

        public async Task<MstBuildingDto> UpdateAsync(Guid id, MstBuildingUpdateDto updateDto)
        {
            var username = UsernameFormToken;
            var building = await _repository.GetByIdAsync(id);
            if (building == null)
                throw new KeyNotFoundException("Building not found");

            if (updateDto.Image != null && updateDto.Image.Length > 0)
            {
                if (string.IsNullOrEmpty(updateDto.Image.ContentType) || !_allowedImageTypes.Contains(updateDto.Image.ContentType))
                    throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

                if (updateDto.Image.Length > MaxFileSize)
                    throw new ArgumentException("File size exceeds 5 MB limit.");

                // var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "BuildingImages");
                // Directory.CreateDirectory(uploadDir);

                var basePath = AppContext.BaseDirectory;
                var uploadDir = Path.Combine(basePath, "Uploads", "BuildingImages");
                Directory.CreateDirectory(uploadDir);

                if (!string.IsNullOrEmpty(building.Image))
                {
                    // var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), building.Image.TrimStart('/'));
                    var oldFilePath = Path.Combine(AppContext.BaseDirectory, building.Image.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                    {
                        try
                        {
                            File.Delete(oldFilePath);
                        }
                        catch (IOException ex)
                        {
                            throw new IOException("Failed to delete old image file.", ex);
                        }
                    }
                }

                var fileExtension = Path.GetExtension(updateDto.Image.FileName)?.ToLower() ?? ".jpg";
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadDir, fileName);

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await updateDto.Image.CopyToAsync(stream);
                    }
                }
                catch (IOException ex)
                {
                    throw new IOException("Failed to save image file.", ex);
                }

                building.Image = $"/Uploads/BuildingImages/{fileName}";
            }

            _mapper.Map(updateDto, building);
            building.UpdatedBy = username;
            building.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(building);
            await RemoveGroupAsync();
            await _mqttClient.PublishAsync("engine/refresh/area-related", "");
            return _mapper.Map<MstBuildingDto>(building);
        }

        // public async Task DeleteAsync(Guid id)
        // {
        //     var building = await _repository.GetByIdAsync(id);
        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        //     building.UpdatedBy = username;
        //     building.UpdatedAt = DateTime.UtcNow;
        //     building.Status = 0;
        //     await _repository.DeleteAsync(id);
        // }

        public async Task DeleteAsync(Guid id)
    {
        var username = UsernameFormToken;
        var building = await _repository.GetByIdAsync(id);
        if (building == null)
            throw new KeyNotFoundException("building not found");

        await _repository.ExecuteInTransactionAsync(async () =>
        {
            await _floorService.RemoveGroupAsync();
            var floors = await _floorRepository.GetByBuildingIdAsync(id);
            foreach (var floor in floors)
            {
                await _floorService.DeleteAsync(floor.Id);
            }
            building.UpdatedBy = username;
            building.UpdatedAt = DateTime.UtcNow;
            building.Status = 0;
            await RemoveGroupAsync();
            // await _mqttClient.PublishAsync("engine/refresh/area-related", "");
            await _repository.DeleteAsync(id);
        });
    }
        

         public async Task<IEnumerable<MstBuildingDto>> ImportAsync(IFormFile file)
        {
            var buildings = new List<MstBuilding>();
            var username = UsernameFormToken;
            var userApplicationId = _httpContextAccessor.HttpContext?.User.FindFirst("ApplicationId")?.Value;

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1);

            int rowNumber = 2;
            foreach (var row in rows)
            {

                var building = new MstBuilding
                {
                    Id = Guid.NewGuid(),
                    Name = row.Cell(1).GetValue<string>(),
                    Image = row.Cell(2).GetValue<string>(),
                    ApplicationId = Guid.Parse(userApplicationId),
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                };

                buildings.Add(building);
                rowNumber++;
            }

            // Simpan ke database
            foreach (var building in buildings)
            {
                await _repository.AddAsync(building);
            }

            return _mapper.Map<IEnumerable<MstBuildingDto>>(buildings);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "UpdatedAt", "Name", "CreatedAt", "Status" };

            var filterService = new GenericDataTableService<MstBuilding, MstBuildingDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
        public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var buildings = await _repository.GetAllExportAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Building Report")
                        .SemiBold().FontSize(16).FontColor(Colors.Black).AlignCenter();

                    page.Content().Table(table =>
                    {
                        // Define columns
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
                        });

                        // Table header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Image").SemiBold();
                            header.Cell().Element(CellStyle).Text("CreatedBy").SemiBold();
                            header.Cell().Element(CellStyle).Text("CreatedAt").SemiBold();
                            header.Cell().Element(CellStyle).Text("UpdatedBy").SemiBold();
                            header.Cell().Element(CellStyle).Text("UpdatedAt").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        // Table body
                        int index = 1;
                        foreach (var building in buildings)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(building.Name);
                            table.Cell().Element(CellStyle).Text(building.Image);
                            table.Cell().Element(CellStyle).Text(building.CreatedBy);
                            table.Cell().Element(CellStyle).Text(building.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(building.UpdatedBy);
                            table.Cell().Element(CellStyle).Text(building.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(building.Status.ToString());
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
            var buildings = await _repository.GetAllExportAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Buildings");

            // Header
            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Image";
            worksheet.Cell(1, 4).Value = "Created By";
            worksheet.Cell(1, 5).Value = "Created At";
            worksheet.Cell(1, 6).Value = "Updated By";
            worksheet.Cell(1, 7).Value = "Updated At";
            worksheet.Cell(1, 8).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var building in buildings)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = building.Name;
                worksheet.Cell(row, 3).Value = building.Image;
                worksheet.Cell(row, 4).Value = building.CreatedBy;
                worksheet.Cell(row, 5).Value = building.CreatedAt;
                worksheet.Cell(row, 6).Value = building.UpdatedBy;
                worksheet.Cell(row, 7).Value = building.UpdatedAt;
                worksheet.Cell(row, 8).Value = building.Status;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

}


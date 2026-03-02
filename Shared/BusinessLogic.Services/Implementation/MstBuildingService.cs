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
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using DataView;
using BusinessLogic.Services.Extension.FileStorageService;
using Shared.Contracts;
using Shared.Contracts.Read;
using Shared.Contracts.Reporting;

namespace BusinessLogic.Services.Implementation
{
    public class MstBuildingService : BaseService, IMstBuildingService
    {
        private readonly MstBuildingRepository _repository;
        private readonly IMstFloorService _floorService;
        private readonly MstFloorRepository _floorRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFileStorageService _fileStorageService;
        private const long MaxFileSize = 5 * 1024 * 1024; // Maksimal 5 MB
        private readonly IMqttPubQueue _mqttQueue;
        private readonly IDistributedCache _cache;
        private readonly IDatabase _redis;
        private readonly IAuditEmitter _audit;
        private readonly ILogger<MstBuilding> _logger;
        private readonly IReportExportService _reportExportService;
        private readonly IReportImportService _reportImportService;
        private bool cacheDisabled = false;

        public MstBuildingService(
            MstBuildingRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IMstFloorService floorService,
            MstFloorRepository floorRepository,
            IDistributedCache cache,
            IConnectionMultiplexer redis,
            ILogger<MstBuilding> logger,
            IMqttPubQueue mqttQueue,
            IFileStorageService fileStorageService,
            IAuditEmitter audit,
            IReportExportService reportExportService,
            IReportImportService reportImportService
            ) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _floorService = floorService;
            _floorRepository = floorRepository;
            _cache = cache;
            _redis = redis?.GetDatabase();
            _mqttQueue = mqttQueue;
            _logger = logger;
            _fileStorageService = fileStorageService;
            _audit = audit;
            _reportExportService = reportExportService;
            _reportImportService = reportImportService;
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
            if (building == null)
                throw new NotFoundException($"Building {id} not found");
            if (building.Status == 0)
                throw new BusinessException("Building is inactive", "BUILDING_INACTIVE");
            return _mapper.Map<MstBuildingDto>(building);
        }

        public async Task<IEnumerable<MstBuildingDto>> GetAllAsync()
        {
            // No caching - data is per-user based on building access
            var data = await _repository.GetAllAsync();
            var mapped = _mapper.Map<IEnumerable<MstBuildingDto>>(data);
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
            if (createDto.Image != null)
            {
                building.Image = await _fileStorageService
                    .SaveImageAsync(createDto.Image, "BuildingImages", MaxFileSize, ImagePurpose.Photo);
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
            _mqttQueue.Enqueue("engine/refresh/area-related", "");
            _audit.Created(
                "Building Area",
                building.Id,
                "Created building",
                new { building.Name }
            );
            return _mapper.Map<MstBuildingDto>(createdBuilding);
        }

        public async Task<MstBuildingDto> UpdateAsync(Guid id, MstBuildingUpdateDto updateDto)
        {
            var username = UsernameFormToken;
            var building = await _repository.GetByIdEntityAsync(id);
            if (building == null)
                throw new NotFoundException("Building not found");

            if (updateDto.Image != null)
            {
                // hapus image lama
                await _fileStorageService.DeleteAsync(building.Image);

                // simpan image baru
                building.Image = await _fileStorageService
                    .SaveImageAsync(updateDto.Image, "BuildingImages", MaxFileSize, ImagePurpose.Photo);
            }

            _mapper.Map(updateDto, building);
            building.UpdatedBy = username;
            building.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(building);
            await RemoveGroupAsync();
            _mqttQueue.Enqueue("engine/refresh/area-related", "");
            _audit.Updated(
                "Building Area",
                building.Id,
                "Updated building",
                new { building.Name }
            );
            return _mapper.Map<MstBuildingDto>(building);
        }

        public async Task DeleteAsync(Guid id)
        {
            var username = UsernameFormToken;
            var building = await _repository.GetByIdEntityAsync(id);
            if (building == null)
                throw new NotFoundException("building not found");

            await _repository.ExecuteInTransactionAsync(async () =>
            {
                var floors = await _floorRepository.GetByBuildingIdAsync(id);
                foreach (var floor in floors)
                {
                    await _floorService.DeleteAsync(floor.Id);
                }
                building.UpdatedBy = username;
                building.UpdatedAt = DateTime.UtcNow;
                building.Status = 0;
                await _repository.UpdateAsync(building);
            });
             _audit.Deleted(
                "Building Area",
                building.Id,
                "Deleted building",
                new { building.Name }
            );
            await _floorService.RemoveGroupAsync();
            await RemoveGroupAsync();
            _mqttQueue.Enqueue("engine/refresh/area-related", "");
        }

        public async Task<ImportResult<MstBuildingDto>> ImportAsync(IFormFile file)
        {
            var username = UsernameFormToken;
            var mappings = new List<ImportColumnMapping>
            {
                new() { ColumnIndex = 0, PropertyName = "Name", Required = true, PropertyType = typeof(string), DisplayName = "Name" },
                new() { ColumnIndex = 1, PropertyName = "Image", Required = false, PropertyType = typeof(string), DisplayName = "Image" }
            };

            var importResult = await _reportImportService.ImportFromExcelAsync<MstBuildingCreateDto>(
                file, mappings, headerRow: 0, dataStartRow: 1);

            if (importResult.Errors.Any())
            {
                return new ImportResult<MstBuildingDto>
                {
                    Errors = importResult.Errors,
                    FailureCount = importResult.FailureCount,
                    TotalRows = importResult.TotalRows,
                    SuccessCount = 0
                };
            }

            var result = new List<MstBuildingDto>();
            foreach (var dto in importResult.ImportedData)
            {
                var created = await CreateAsync(dto);
                result.Add(created);
            }

            return new ImportResult<MstBuildingDto>
            {
                ImportedData = result,
                SuccessCount = result.Count,
                FailureCount = importResult.FailureCount,
                TotalRows = importResult.TotalRows,
                Errors = importResult.Errors
            };
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, MstBuildingFilter filter)
        {
            // Set base pagination properties
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            // Map Date Filters (Generic Dictionary -> Specific Prop)
            if (request.DateFilters != null)
            {
                if (request.DateFilters.TryGetValue("UpdatedAt", out var dateFilter))
                {
                    filter.DateFrom = dateFilter.DateFrom;
                    filter.DateTo = dateFilter.DateTo;
                }
            }

            // Call Repo
            var (data, total, filtered) = await _repository.FilterAsync(filter);

            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data
            };
        }

        public async Task<byte[]> ExportPdfAsync(ReportExportRequest request)
        {
            request = _reportExportService.ApplyDefaultPagination(request);

            var buildings = await _repository.GetPaginatedExportAsync(
                request.Page,
                request.PageSize);

            var readDtos = await _repository.GetAllQueryable()
                .OrderBy(x => x.Name)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new MstBuildingRead
                {
                    Id = x.Id,
                    Name = x.Name,
                    Image = x.Image,
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy,
                    UpdatedAt = x.UpdatedAt,
                    UpdatedBy = x.UpdatedBy,
                    Status = x.Status,
                    ApplicationId = x.ApplicationId
                })
                .ToListAsync();

            var metadata = new ReportMetadata
            {
                Title = _reportExportService.GenerateReportTitle(
                    "Master Building Report",
                    null,
                    request.TimeRange),
                FilterInfo = _reportExportService.GenerateFilterInfo(new ReportFilterInfo
                {
                    From = request.DateFrom,
                    To = request.DateTo,
                    Search = request.Search,
                    Status = request.Status
                }),
                TotalRecords = readDtos.Count,
                Columns = GetBuildingReportColumns(),
                Orientation = ReportOrientation.Landscape,
                IncludePageNumbers = true
            };

            return await _reportExportService.ExportToPdfAsync(readDtos, metadata);
        }

        public async Task<byte[]> ExportExcelAsync(ReportExportRequest request)
        {
            request = _reportExportService.ApplyDefaultPagination(request);

            var readDtos = await _repository.GetAllQueryable()
                .OrderBy(x => x.Name)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new MstBuildingRead
                {
                    Id = x.Id,
                    Name = x.Name,
                    Image = x.Image,
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy,
                    UpdatedAt = x.UpdatedAt,
                    UpdatedBy = x.UpdatedBy,
                    Status = x.Status,
                    ApplicationId = x.ApplicationId
                })
                .ToListAsync();

            var metadata = new ReportMetadata
            {
                Title = _reportExportService.GenerateReportTitle(
                    "Master Building Report",
                    null,
                    request.TimeRange),
                FilterInfo = _reportExportService.GenerateFilterInfo(new ReportFilterInfo
                {
                    From = request.DateFrom,
                    To = request.DateTo,
                    Search = request.Search,
                    Status = request.Status
                }),
                TotalRecords = readDtos.Count,
                Columns = GetBuildingReportColumns(),
                Orientation = ReportOrientation.Landscape,
                IncludePageNumbers = false
            };

            return await _reportExportService.ExportToExcelAsync(readDtos, metadata);
        }

        private static List<ReportColumn> GetBuildingReportColumns()
        {
            return new()
            {
                new() { Header = "Name", PropertyName = "Name", Width = 2 },
                new() { Header = "Image", PropertyName = "Image", Width = 2 },
                new() { Header = "CreatedBy", PropertyName = "CreatedBy", Width = 2 },
                new() { Header = "CreatedAt", PropertyName = "CreatedAt", Format = "yyyy-MM-dd HH:mm:ss", Width = 2 },
                new() { Header = "UpdatedBy", PropertyName = "UpdatedBy", Width = 2 },
                new() { Header = "UpdatedAt", PropertyName = "UpdatedAt", Format = "yyyy-MM-dd HH:mm:ss", Width = 2 },
                new() { Header = "Status", PropertyName = "Status", Width = 1 }
            };
        }
    }
}



using AutoMapper;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Extension.RootExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    public class MonitoringConfigService : BaseService, IMonitoringConfigService
    {
        private readonly MonitoringConfigRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAuditEmitter _audit;

        public MonitoringConfigService(
            MonitoringConfigRepository repository,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _audit = audit;
        }

        public async Task<MonitoringConfigRead> GetByIdAsync(Guid id)
        {
            var config = await _repository.GetByIdAsync(id);
            if (config == null)
                throw new NotFoundException($"MonitoringConfig with id {id} not found");
            return config;
        }

        public async Task<IEnumerable<MonitoringConfigRead>> GetAllAsync()
        {
            var configs = await _repository.GetAllAsync();
            return configs;
        }

        public async Task<MonitoringConfigDto> CreateAsync(MonitoringConfigCreateDto createDto)
        {
            // Handle backward compatibility: if old BuildingId provided, add to BuildingIds
            if (createDto.BuildingId.HasValue &&
                (createDto.BuildingIds == null || !createDto.BuildingIds.Contains(createDto.BuildingId.Value)))
            {
                createDto.BuildingIds ??= new List<Guid>();
                createDto.BuildingIds.Add(createDto.BuildingId.Value);
            }

            var config = _mapper.Map<MonitoringConfig>(createDto);

            SetCreateAudit(config);
            // ValidateApplicationIdForEntity(config);

            config = await _repository.AddAsync(config, createDto.BuildingIds);
            await _audit.Created(config.Id.ToString(), config.Name ?? "Config", "MonitoringConfig");

            return _mapper.Map<MonitoringConfigDto>(config);
        }

        public async Task UpdateAsync(Guid id, MonitoringConfigUpdateDto updateDto)
        {
            // Handle backward compatibility: if old BuildingId provided, add to BuildingIds
            if (updateDto.BuildingId.HasValue &&
                (updateDto.BuildingIds == null || !updateDto.BuildingIds.Contains(updateDto.BuildingId.Value)))
            {
                updateDto.BuildingIds ??= new List<Guid>();
                updateDto.BuildingIds.Add(updateDto.BuildingId.Value);
            }

            var config = await _repository.GetByIdEntityAsync(id);
            if (config == null)
                throw new NotFoundException($"MonitoringConfig with id {id} not found");

            _mapper.Map(updateDto, config);
            SetUpdateAudit(config);
            // ValidateApplicationIdForEntity(config);

            await _repository.UpdateAsync(config, updateDto.BuildingIds);
            await _audit.Updated(config.Id.ToString(), config.Name ?? "Config", "MonitoringConfig");
        }

        public async Task DeleteAsync(Guid id)
        {
            var config = await _repository.GetByIdEntityAsync(id);
            if (config == null)
                throw new NotFoundException($"MonitoringConfig with id {id} not found");

            SetDeleteAudit(config);
            await _repository.DeleteAsync(id);
            await _audit.Deleted(config.Id.ToString(), config.Name ?? "Config", "MonitoringConfig");
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, MonitoringConfigFilter filter)
        {
            // Map DataTablesProjectedRequest to filter
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            // Map Date Filters
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

        // private void ValidateApplicationIdForEntity(MonitoringConfig config)
        // {
        //     var (applicationId, isSystemAdmin) = _repository.GetApplicationIdAndRole();
        //     _repository.ValidateApplicationIdForEntity(config, applicationId, isSystemAdmin);
        // }
    }
}

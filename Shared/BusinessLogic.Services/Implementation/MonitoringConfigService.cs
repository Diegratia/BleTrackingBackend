using AutoMapper;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Extension.RootExtension;
using System;
using System.Collections.Generic;
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
        private readonly IUserService _userService;

        public MonitoringConfigService(
            MonitoringConfigRepository repository,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor,
            IUserService userService) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _audit = audit;
            _userService = userService;
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

        public async Task<MonitoringConfigRead> CreateAsync(MonitoringConfigCreateDto createDto)
        {
            var currentUser = await _userService.GetFromTokenAsync();
            if (currentUser == null)
                throw new UnauthorizedException("User not found");

            if (!currentUser.HasCreateMonitoringConfigPermission())
                throw new UnauthorizedException("Anda tidak memiliki akses create monitoring config");

            var config = _mapper.Map<MonitoringConfig>(createDto);
            SetCreateAudit(config);

            config = await _repository.AddAsync(config, createDto.BuildingIds);
             _audit.Created(config.Id.ToString(), config.Name ?? "Config", "MonitoringConfig");
            var result = await _repository.GetByIdAsync(config.Id);

            return result ?? throw new Exception("Failed to reload MonitoringConfig after create");
        }

        public async Task UpdateAsync(Guid id, MonitoringConfigUpdateDto updateDto)
        {
            // Cek permission menggunakan extension method
            var currentUser = await _userService.GetFromTokenAsync();
            if (currentUser == null)
                throw new UnauthorizedException("User not found");

            if (!currentUser.HasUpdateMonitoringConfigPermission())
                throw new UnauthorizedException("Anda tidak memiliki akses update monitoring config");

            var config = await _repository.GetByIdEntityAsync(id);
            if (config == null)
                throw new NotFoundException($"MonitoringConfig with id {id} not found");

            _mapper.Map(updateDto, config);
            SetUpdateAudit(config);

            await _repository.UpdateAsync(config, updateDto.BuildingIds);
             _audit.Updated(config.Id.ToString(), config.Name ?? "Config", "MonitoringConfig");
        }

        public async Task DeleteAsync(Guid id)
        {
            // Hanya System dan SuperAdmin yang bisa delete
            if (!IsSystemAdminOrHigher())
                throw new UnauthorizedAccessException("Hanya System dan SuperAdmin yang bisa menghapus monitoring config");

            var config = await _repository.GetByIdEntityAsync(id);
            if (config == null)
                throw new NotFoundException($"MonitoringConfig with id {id} not found");

            SetDeleteAudit(config);
            await _repository.DeleteAsync(id);
             _audit.Deleted(config.Id.ToString(), config.Name ?? "Config", "MonitoringConfig");
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, MonitoringConfigFilter filter)
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
    }
}

using AutoMapper;
using BusinessLogic.Services.Background;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementation
{
    public class AlarmCategorySettingsService : BaseService, IAlarmCategorySettingsService
    {
        private readonly AlarmCategorySettingsRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<AlarmCategorySettings> _logger;
        private readonly IMqttPubQueue _mqttQueue;
        private readonly IAuditEmitter _audit;

        public AlarmCategorySettingsService(
            AlarmCategorySettingsRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AlarmCategorySettings> logger,
            IMqttPubQueue mqttQueue,
            IAuditEmitter audit)
            : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _mqttQueue = mqttQueue;
            _audit = audit;
        }

        public async Task<AlarmCategorySettingsRead?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity;
        }

        public async Task<IEnumerable<AlarmCategorySettingsRead>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task UpdateAsync(Guid id, AlarmCategorySettingsUpdateDto updateDto)
        {
            var entity = await _repository.GetByIdEntityAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Category not found");

            SetUpdateAudit(entity);
            _mapper.Map(updateDto, entity);
            await _repository.UpdateAsync(entity);

            _mqttQueue.Enqueue("engine/refresh/alarm-related", "");
            _audit.Updated("AlarmCategorySettings", id, $"Updated alarm category settings");
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdEntityAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Category not found");

            SetDeleteAudit(entity);
            await _repository.DeleteAsync(entity);

            _mqttQueue.Enqueue("engine/refresh/alarm-related", "");
            _audit.Deleted("AlarmCategorySettings", id, $"Deleted alarm category settings");
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, AlarmCategorySettingsFilter filter)
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

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

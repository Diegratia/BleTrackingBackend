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
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
namespace BusinessLogic.Services.Implementation
{
    public class MonitoringConfigService : IMonitoringConfigService
    {
        private readonly MonitoringConfigRepository _repository;
        private readonly IMapper _mapper;

        public MonitoringConfigService(MonitoringConfigRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<MonitoringConfigDto> GetByIdAsync(Guid id)
        {
            var config = await _repository.GetByIdAsync(id);
            return config == null ? null : _mapper.Map<MonitoringConfigDto>(config);
        }

        public async Task<IEnumerable<MonitoringConfigDto>> GetAllAsync()

        {
            var configs = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MonitoringConfigDto>>(configs);
        }

        public async Task<MonitoringConfigDto> CreateAsync(MonitoringConfigCreateDto createDto)
        {
            var config = _mapper.Map<MonitoringConfig>(createDto);

            await _repository.AddAsync(config);
            return _mapper.Map<MonitoringConfigDto>(config);
        }

        public async Task UpdateAsync(Guid id, MonitoringConfigUpdateDto updateDto)
        {
            var config = await _repository.GetByIdAsync(id);
            if (config == null)
                throw new KeyNotFoundException("Config not found");
            _mapper.Map(updateDto, config);

            await _repository.UpdateAsync(config);
        }

        public async Task DeleteAsync(Guid id)
        {
            var config = await _repository.GetByIdAsync(id);
            if (config == null)
                throw new KeyNotFoundException("Config not found");
            await _repository.DeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "Name" };

            var filterService = new GenericDataTableService<MonitoringConfig, MonitoringConfigDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
    }
}
using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Security.Claims;
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
    public class StayOnAreaService : BaseService, IStayOnAreaService
    {
        private readonly StayOnAreaRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;

        public StayOnAreaService(
            StayOnAreaRepository repository,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _audit = audit;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<StayOnAreaRead> GetByIdAsync(Guid id)
        {
            var onArea = await _repository.GetByIdAsync(id);
            if (onArea == null)
                throw new NotFoundException($"StayOnArea with id {id} not found");
            return onArea;
        }

        public async Task<IEnumerable<StayOnAreaRead>> GetAllAsync()
        {
            var onAreas = await _repository.GetAllAsync();
            return onAreas;
        }

        public async Task<StayOnAreaDto> CreateAsync(StayOnAreaCreateDto createDto)
        {
            var onArea = _mapper.Map<StayOnArea>(createDto);
            onArea.Id = Guid.NewGuid();
            onArea.Status = 1;
            onArea.ApplicationId = AppId;
            SetCreateAudit(onArea);

            await _repository.AddAsync(onArea);

            await _audit.Created(
                "StayOnArea",
                onArea.Id,
                "Created StayOnArea",
                new { onArea.Name }
            );

            var result = await _repository.GetByIdAsync(onArea.Id);
            return _mapper.Map<StayOnAreaDto>(result);
        }

        public async Task UpdateAsync(Guid id, StayOnAreaUpdateDto updateDto)
        {
            var onArea = await _repository.GetByIdEntityAsync(id);
            if (onArea == null)
                throw new NotFoundException($"StayOnArea with id {id} not found");

            SetUpdateAudit(onArea);
            _mapper.Map(updateDto, onArea);
            await _repository.UpdateAsync(onArea);

            await _audit.Updated(
                "StayOnArea",
                onArea.Id,
                "Updated StayOnArea",
                new { onArea.Name }
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            var onArea = await _repository.GetByIdEntityAsync(id);
            if (onArea == null)
                throw new NotFoundException($"StayOnArea with id {id} not found");

            SetDeleteAudit(onArea);
            await _repository.SoftDeleteAsync(id);

            await _audit.Deleted(
                "StayOnArea",
                onArea.Id,
                "Deleted StayOnArea",
                new { onArea.Name }
            );
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, StayOnAreaFilter filter)
        {
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

using AutoMapper;
using BusinessLogic.Services.Interface;
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
    public class OverpopulatingService : BaseService, IOverpopulatingService
    {
        private readonly OverpopulatingRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;

        public OverpopulatingService(
            OverpopulatingRepository repository,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _audit = audit;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<OverpopulatingRead> GetByIdAsync(Guid id)
        {
            var overpopulating = await _repository.GetByIdAsync(id);
            if (overpopulating == null)
                throw new NotFoundException($"Overpopulating with id {id} not found");
            return overpopulating;
        }

        public async Task<IEnumerable<OverpopulatingRead>> GetAllAsync()
        {
            var overpopulatings = await _repository.GetAllAsync();
            return overpopulatings;
        }

        public async Task<OverpopulatingRead> CreateAsync(OverpopulatingCreateDto createDto)
        {
            var overpopulating = _mapper.Map<Overpopulating>(createDto);
            overpopulating.Id = Guid.NewGuid();
            overpopulating.Status = 1;
            overpopulating.ApplicationId = AppId;
            SetCreateAudit(overpopulating);

            await _repository.AddAsync(overpopulating);

            await _audit.Created(
                "Overpopulating",
                overpopulating.Id,
                "Created Overpopulating",
                new { overpopulating.Name }
            );

            var result = await _repository.GetByIdAsync(overpopulating.Id);
            return _mapper.Map<OverpopulatingRead>(result);
        }

        public async Task UpdateAsync(Guid id, OverpopulatingUpdateDto updateDto)
        {
            var overpopulating = await _repository.GetByIdEntityAsync(id);
            if (overpopulating == null)
                throw new NotFoundException($"Overpopulating with id {id} not found");

            SetUpdateAudit(overpopulating);
            _mapper.Map(updateDto, overpopulating);
            await _repository.UpdateAsync(overpopulating);

            await _audit.Updated(
                "Overpopulating",
                overpopulating.Id,
                "Updated Overpopulating",
                new { overpopulating.Name }
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            var overpopulating = await _repository.GetByIdEntityAsync(id);
            if (overpopulating == null)
                throw new NotFoundException($"Overpopulating with id {id} not found");

            SetDeleteAudit(overpopulating);
            overpopulating.IsActive = 0;
            await _repository.SoftDeleteAsync(id);

            await _audit.Deleted(
                "Overpopulating",
                overpopulating.Id,
                "Deleted Overpopulating",
                new { overpopulating.Name }
            );
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, OverpopulatingFilter filter)
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

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
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

namespace BusinessLogic.Services.Implementation
{
    public class BoundaryService : BaseService, IBoundaryService
    {
        private readonly BoundaryRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;

        public BoundaryService(
            BoundaryRepository repository,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _audit = audit;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BoundaryRead> GetByIdAsync(Guid id)
        {
            var boundary = await _repository.GetByIdAsync(id);
            if (boundary == null)
                throw new NotFoundException($"Boundary with id {id} not found");
            return boundary;
        }

        public async Task<IEnumerable<BoundaryRead>> GetAllAsync()
        {
            var boundaries = await _repository.GetAllAsync();
            return boundaries;
        }

        public async Task<BoundaryDto> CreateAsync(BoundaryCreateDto createDto)
        {
            var boundary = _mapper.Map<Boundary>(createDto);
            boundary.Id = Guid.NewGuid();
            boundary.Status = 1;
            boundary.ApplicationId = AppId;
            SetCreateAudit(boundary);

            await _repository.AddAsync(boundary);

            await _audit.Created(
                "Boundary",
                boundary.Id,
                "Created Boundary",
                new { boundary.Name }
            );

            var result = await _repository.GetByIdAsync(boundary.Id);
            return _mapper.Map<BoundaryDto>(result);
        }

        public async Task UpdateAsync(Guid id, BoundaryUpdateDto updateDto)
        {
            var boundary = await _repository.GetByIdEntityAsync(id);
            if (boundary == null)
                throw new NotFoundException($"Boundary with id {id} not found");

            SetUpdateAudit(boundary);
            _mapper.Map(updateDto, boundary);
            await _repository.UpdateAsync(boundary);

            await _audit.Updated(
                "Boundary",
                boundary.Id,
                "Updated Boundary",
                new { boundary.Name }
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            var boundary = await _repository.GetByIdEntityAsync(id);
            if (boundary == null)
                throw new NotFoundException($"Boundary with id {id} not found");

            SetDeleteAudit(boundary);
            await _repository.SoftDeleteAsync(id);

            await _audit.Deleted(
                "Boundary",
                boundary.Id,
                "Deleted Boundary",
                new { boundary.Name }
            );
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, BoundaryFilter filter)
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
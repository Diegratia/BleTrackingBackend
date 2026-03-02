using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using ClosedXML.Excel;
using Data.ViewModels;
using Data.ViewModels.Shared.ExceptionHelper;
using DataView;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    public class PatrolAreaService : BaseService, IPatrolAreaService
    {
        private readonly PatrolAreaRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;


        public PatrolAreaService(
            PatrolAreaRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IAuditEmitter audit
            ) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _audit = audit;
        }

        public async Task<PatrolAreaDto> GetByIdAsync(Guid id)
        {
            var patrolArea = await _repository.GetByIdAsync(id);
            if (patrolArea == null)
                throw new NotFoundException($"PatrolArea with id {id} not found");
            return patrolArea == null ? null : _mapper.Map<PatrolAreaDto>(patrolArea);
        }

        public async Task<IEnumerable<PatrolAreaRead>> GetAllAsync()
        {
            var patrolAreas = await _repository.GetAllAsync();
            return patrolAreas;
        }

        public async Task<PatrolAreaDto> CreateAsync(PatrolAreaCreateDto createDto)
        {
            if (!await _repository.FloorExistsAsync(createDto.FloorId!.Value))
                throw new NotFoundException($"Floor with id {createDto.FloorId} not found");

            var invalidFloorId = await _repository.CheckInvalidFloorOwnershipAsync(createDto.FloorId.Value, AppId);
            if (invalidFloorId.Any())
                throw new UnauthorizedException($"FloorId does not belong to this Application: {string.Join(", ", invalidFloorId)}");

            if (createDto.FloorplanId.HasValue)
            {
                if (!await _repository.FloorplanExistsAsync(createDto.FloorplanId.Value))
                    throw new NotFoundException($"Floorplan with id {createDto.FloorplanId} not found");

                var invalidFloorplanId = await _repository.CheckInvalidFloorplanOwnershipAsync(createDto.FloorplanId.Value, AppId);
                if (invalidFloorplanId.Any())
                {
                    throw new UnauthorizedException(
                        $"FloorplanId does not belong to this Application: {string.Join(", ", invalidFloorplanId)}"
                    );
                }
            }


            var patrolArea = _mapper.Map<PatrolArea>(createDto);
            SetCreateAudit(patrolArea);
            await _repository.AddAsync(patrolArea);
             _audit.Created(
                "Patrol Area",
                patrolArea.Id,
                "Created patrolArea",
                new { patrolArea.Name }
            );
            return _mapper.Map<PatrolAreaDto>(patrolArea);
        }

        public async Task<PatrolAreaDto> UpdateAsync(Guid id, PatrolAreaUpdateDto updateDto)
        {
            var patrolArea = await _repository.GetByIdEntityAsync(id);
            if (patrolArea == null)
                throw new NotFoundException($"PatrolArea with id {id} not found");
            if (!await _repository.FloorExistsAsync(updateDto.FloorId!.Value))
                throw new NotFoundException($"Floor with id {updateDto.FloorId} not found");

            var invalidFloorId = await _repository.CheckInvalidFloorOwnershipAsync(updateDto.FloorId.Value, AppId);
            if (invalidFloorId.Any())
                throw new UnauthorizedException($"FloorId does not belong to this Application: {string.Join(", ", invalidFloorId)}");

            if (updateDto.FloorplanId.HasValue && updateDto.FloorplanId != patrolArea.FloorplanId)
            {
                if (!await _repository.FloorplanExistsAsync(updateDto.FloorplanId.Value))
                    throw new NotFoundException($"Floorplan with id {updateDto.FloorplanId} not found");

                var invalidFloorplanId = await _repository.CheckInvalidFloorplanOwnershipAsync(updateDto.FloorplanId.Value, AppId);
                if (invalidFloorplanId.Any())
                {
                    throw new UnauthorizedException(
                        $"FloorplanId does not belong to this Application: {string.Join(", ", invalidFloorplanId)}"
                    );
                }
            }

            // var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            // patrolArea.UpdatedBy = username;
            // patrolArea.UpdatedAt = DateTime.UtcNow;
            SetUpdateAudit(patrolArea);
            _mapper.Map(updateDto, patrolArea);
            await _repository.UpdateAsync(patrolArea);
             _audit.Updated(
                "Patrol Area",
                patrolArea.Id,
                "Updated patrolArea",
                new { patrolArea.Name }
            );
            return _mapper.Map<PatrolAreaDto>(patrolArea);
        }

        public async Task DeleteAsync(Guid id)
        {
            var patrolArea = await _repository.GetByIdEntityAsync(id);
            if (patrolArea == null)
                throw new NotFoundException($"PatrolArea with id {id} not found");
            // var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            patrolArea.Status = 0;
            patrolArea.IsActive = 0;
            // patrolArea.UpdatedBy = username;
            // patrolArea.UpdatedAt = DateTime.UtcNow;
            SetDeleteAudit(patrolArea);
             _audit.Deleted(
                "Patrol Area",
                patrolArea.Id,
                "Deleted patrolArea",
                new { patrolArea.Name }
            );
            await _repository.DeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, PatrolAreaFilter filter)
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

        public async Task<IEnumerable<PatrolAreaLookUpDto>> GetAllLookUpAsync()
        {
            var patrolareas = await _repository.GetAllLookUpAsync();
            return _mapper.Map<IEnumerable<PatrolAreaLookUpDto>>(patrolareas);
        }


    }
}
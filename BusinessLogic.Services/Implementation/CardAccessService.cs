using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementation
{
    public class CardAccessService : ICardAccessService
    {
        private readonly CardAccessRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private FloorplanMaskedAreaRepository _areaRepository;

        public CardAccessService(CardAccessRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, FloorplanMaskedAreaRepository areaRepository)
        {
            _repository = repository;
            _areaRepository = areaRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CardAccessDto> CreateAsync(CardAccessCreateDto dto)
        {
            var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId");
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            var entity = _mapper.Map<CardAccess>(dto);
            entity.Id = Guid.NewGuid();
            int? currentMax = _repository.GetAllQueryable().Max(m => m.AccessNumber);
            int increment = currentMax.HasValue ? currentMax.Value + 1 : 1;
            entity.AccessNumber = increment;
            entity.CreatedBy = username;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.Status = 1;

            if (applicationIdClaim != null)
                entity.ApplicationId = Guid.Parse(applicationIdClaim.Value);

            // inject masked areas
         if (dto.MaskedAreaIds.Any())
        {
            // validasi id yang bener-bener ada
            var validMaskedAreas = await _areaRepository.GetAllQueryable()
                .Where(m => dto.MaskedAreaIds.Contains(m.Id))
                .Select(m => m.Id)
                .ToListAsync();

            if (validMaskedAreas.Count != dto.MaskedAreaIds.Count)
            {
                throw new ArgumentException("Some MaskedAreaIds are invalid.");
            }

            entity.CardAccessMaskedAreas = validMaskedAreas
                .Select(id => new CardAccessMaskedArea
                {
                    CardAccessId = entity.Id,
                    MaskedAreaId = id,
                    ApplicationId = entity.ApplicationId
                }).ToList();
        }


                var dtoResult = _mapper.Map<CardAccessDto>(entity);
                dtoResult.MaskedAreaIds = entity.CardAccessMaskedAreas?
                .Select(x => (Guid?)x.MaskedAreaId)
                .ToList();

                await _repository.AddAsync(entity);
                return dtoResult;
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name", "AccessNumber", "AccessScope" };
            var validSortColumns = new[] { "UpdatedAt", "Status" , "AccessNumber", "AccessScope" };

            var filterService = new GenericDataTableService<CardAccess, CardAccessDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }

        public async Task UpdateAsync(Guid id, CardAccessUpdateDto dto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
                throw new KeyNotFoundException("CardAccess not found");

            _mapper.Map(dto, entity);

            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;

            // update masked areas
            entity.CardAccessMaskedAreas.Clear();
            if (dto.MaskedAreaIds.Any())
            {
                entity.CardAccessMaskedAreas = dto.MaskedAreaIds
                    .Where(id => id.HasValue)
                    .Select(id => new CardAccessMaskedArea
                    {
                        CardAccessId = entity.Id,
                        MaskedAreaId = id.Value,
                        ApplicationId = entity.ApplicationId
                    })
                    .ToList();
            }

            await _repository.UpdateAsync(entity);
        }

        // public async Task<IEnumerable<CardAccessDto>> GetAllAsync()
        // {
        //     var list = await _repository.GetAllAsync();

        //     return _mapper.Map<IEnumerable<CardAccessDto>>(list);

        // }
        
        public async Task<IEnumerable<CardAccessDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<List<CardAccessDto>>(entities);

            foreach (var dto in dtos)
            {
                var entity = entities.First(e => e.Id == dto.Id);
                dto.MaskedAreaIds = entity.CardAccessMaskedAreas
                    .Select(x => (Guid?)x.MaskedAreaId)
                    .ToList();
            }

            return dtos;
        }


            public async Task<CardAccessDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) 
                return null;

            var dto = _mapper.Map<CardAccessDto>(entity);
            dto.MaskedAreaIds = entity.CardAccessMaskedAreas
                .Select(x => (Guid?)x.MaskedAreaId)
                .ToList();

            return dto;
        }


        public async Task DeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Card Access not found");

            entity.Status = 0;
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;
            await _repository.DeleteAsync(id);
        }
    }
}

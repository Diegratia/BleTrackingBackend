using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementation
{
    public class VisitorBlacklistAreaService : IVisitorBlacklistAreaService
    {
        private readonly VisitorBlacklistAreaRepository _repository;
        private readonly IMapper _mapper;

        public VisitorBlacklistAreaService(VisitorBlacklistAreaRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<VisitorBlacklistAreaDto> CreateVisitorBlacklistAreaAsync(VisitorBlacklistAreaCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (!await _repository.FloorplanMaskedAreaExists(dto.FloorplanMaskedAreaId))
                throw new ArgumentException($"FloorplanMaskedArea with ID {dto.FloorplanMaskedAreaId} not found.");
            if (!await _repository.VisitorExists(dto.VisitorId))
                throw new ArgumentException($"Visitor with ID {dto.VisitorId} not found.");

            var entity = _mapper.Map<VisitorBlacklistArea>(dto);
            entity.Id = Guid.NewGuid();

            await _repository.AddAsync(entity);
            return _mapper.Map<VisitorBlacklistAreaDto>(entity);
        }

        public async Task<VisitorBlacklistAreaDto> GetVisitorBlacklistAreaByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return _mapper.Map<VisitorBlacklistAreaDto>(entity);
        }

        public async Task<IEnumerable<VisitorBlacklistAreaDto>> GetAllVisitorBlacklistAreasAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<VisitorBlacklistAreaDto>>(entities);
        }

        public async Task UpdateVisitorBlacklistAreaAsync(Guid id, VisitorBlacklistAreaUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"VisitorBlacklistArea with ID {id} not found.");

            if (entity.FloorplanMaskedAreaId != dto.FloorplanMaskedAreaId)
            {
                if (!await _repository.FloorplanMaskedAreaExists(dto.FloorplanMaskedAreaId))
                    throw new ArgumentException($"FloorplanMaskedArea with ID {dto.FloorplanMaskedAreaId} not found.");
                entity.FloorplanMaskedAreaId = dto.FloorplanMaskedAreaId;
            }

            if (entity.VisitorId != dto.VisitorId)
            {
                if (!await _repository.VisitorExists(dto.VisitorId))
                    throw new ArgumentException($"Visitor with ID {dto.VisitorId} not found.");
                entity.VisitorId = dto.VisitorId;
            }

            _mapper.Map(dto, entity);
            await _repository.UpdateAsync();
        }

        public async Task DeleteVisitorBlacklistAreaAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"VisitorBlacklistArea with ID {id} not found.");

            await _repository.DeleteAsync(entity);
        }
    }
}

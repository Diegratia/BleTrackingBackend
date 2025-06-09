using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;

namespace BusinessLogic.Services.Implementation
{
    public class MstEngineService : IMstEngineService
    {
        private readonly MstEngineRepository _engineRepository;
        private readonly IMapper _mapper;

        public MstEngineService(MstEngineRepository engineRepository, IMapper mapper)
        {
            _engineRepository = engineRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MstEngineDto>> GetAllEnginesAsync()
        {
            var engines = await _engineRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstEngineDto>>(engines);
        }

        public async Task<MstEngineDto> GetEngineByIdAsync(Guid id)
        {
            var engine = await _engineRepository.GetByIdAsync(id);
            return engine == null ? null : _mapper.Map<MstEngineDto>(engine);
        }

        public async Task<MstEngineDto> CreateEngineAsync(MstEngineCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var engine = _mapper.Map<MstEngine>(dto);
            engine.Status = 1; 

            var createdEngine = await _engineRepository.AddAsync(engine);
            return _mapper.Map<MstEngineDto>(createdEngine);
        }

        public async Task UpdateEngineAsync(Guid id, MstEngineUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var engine = await _engineRepository.GetByIdAsync(id);
            if (engine == null)
                throw new KeyNotFoundException($"Engine with ID {id} not found");

            _mapper.Map(dto, engine);
            await _engineRepository.UpdateAsync(engine);
        }

        public async Task DeleteEngineAsync(Guid id)
        {
            var engine = await _engineRepository.GetByIdAsync(id);
            if (engine == null)
                throw new KeyNotFoundException($"Engine with ID {id} not found");
            engine.Status = 0; 
            engine.IsLive = 0; 

            await _engineRepository.DeleteAsync(id);
        }
    }
}
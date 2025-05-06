using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;
using Repositories.DbContexts;

namespace BusinessLogic.Services.Implementation
{
    public class MstBleReaderService : IMstBleReaderService
    {
        private readonly MstBleReaderRepository _repository;
        private readonly IMapper _mapper;

        public MstBleReaderService(MstBleReaderRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<MstBleReaderDto> GetByIdAsync(Guid id)
        {
            var bleReader = await _repository.GetByIdAsync(id);
            return bleReader == null ? null : _mapper.Map<MstBleReaderDto>(bleReader);
        }

        public async Task<IEnumerable<MstBleReaderDto>> GetAllAsync()
        {
            var bleReaders = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstBleReaderDto>>(bleReaders);
        }

        public async Task<MstBleReaderDto> CreateAsync(MstBleReaderCreateDto createDto)
        {
            var bleReader = _mapper.Map<MstBleReader>(createDto);
            bleReader.Id = Guid.NewGuid(); 
            bleReader.CreatedBy = "System"; 
            bleReader.UpdatedBy = "System";
            bleReader.Status = 1;

            var createdBleReader = await _repository.AddAsync(bleReader);
            return _mapper.Map<MstBleReaderDto>(createdBleReader);
        }

        

        public async Task UpdateAsync(Guid id, MstBleReaderUpdateDto updateDto)
        {
            var bleReader = await _repository.GetByIdAsync(id);
            if (bleReader == null)
                throw new KeyNotFoundException("BLE Reader not found");

            _mapper.Map(updateDto, bleReader);
            bleReader.UpdatedBy = "System"; 

            await _repository.UpdateAsync(bleReader);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id); 
        }
    }
}
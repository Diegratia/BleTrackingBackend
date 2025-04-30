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
        private readonly MstBleReaderRepository _mstBleReaderRepository;
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;

        public MstBleReaderService(BleTrackingDbContext context, IMapper mapper, MstBleReaderRepository mstBleReaderRepository)
        {
            _mstBleReaderRepository = mstBleReaderRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<MstBleReaderDto> GetByIdAsync(Guid id)
        {
            // var bleReader = await _context.MstBleReaders
            //     .FirstOrDefaultAsync(b => b.Id == id);
            // return bleReader == null ? null : _mapper.Map<MstBleReaderDto>(bleReader);

             var bleReader = await _mstBleReaderRepository.GetByIdAsync(id);

            return bleReader == null ? null : _mapper.Map<MstBleReaderDto>(bleReader);
        }

        public async Task<IEnumerable<MstBleReaderDto>> GetAllAsync()
        {
            // var bleReaders = await _context.MstBleReaders.ToListAsync();
            // return _mapper.Map<IEnumerable<MstBleReaderDto>>(bleReaders);

            var bleReaders = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstBleReaderDto>>(bleReaders);
        }

        public async Task<MstBleReaderDto> CreateAsync(MstBleReaderCreateDto createDto)
        {
             // Validasi BrandId
            var brand = await _context.MstBrands.FirstOrDefaultAsync(b => b.Id == createDto.BrandId);
            if (brand == null)
                throw new ArgumentException($"Brand with ID {createDto.BrandId} not found.");

            var bleReader = _mapper.Map<MstBleReader>(createDto);

            bleReader.CreatedBy = "";
            bleReader.UpdatedBy = "";
            bleReader.Status = 1;

            _context.MstBleReaders.Add(bleReader);
            await _context.SaveChangesAsync();
            return _mapper.Map<MstBleReaderDto>(bleReader);
        }

        public async Task UpdateAsync(Guid id, MstBleReaderUpdateDto updateDto)
        {
            var bleReader = await _context.MstBleReaders.FindAsync(id);
            if (bleReader == null)
                throw new KeyNotFoundException("BLE Reader not found");

             if (bleReader.BrandId != updateDto.BrandId)
            {
                var brand = await _context.MstBrands.FirstOrDefaultAsync(b => b.Id == updateDto.BrandId);
                if (brand == null)
                    throw new ArgumentException($"Brand with ID {updateDto.BrandId} not found.");
                // Tidak perlu set BrandData, cukup update BrandId
                bleReader.BrandId = updateDto.BrandId;
            }

            bleReader.UpdatedBy = "";

            _mapper.Map(updateDto, bleReader);
            // _context.MstBleReaders.Update(bleReader);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var bleReader = await _context.MstBleReaders.FindAsync(id);
            if (bleReader == null)
                throw new KeyNotFoundException("BLE Reader not found");
            
            bleReader.Status = 0;

            // _context.MstBleReaders.Remove(bleReader);
            await _context.SaveChangesAsync();
        }
    }
}
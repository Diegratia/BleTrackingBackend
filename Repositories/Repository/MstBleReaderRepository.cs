using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstBleReaderRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstBleReaderRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<MstBleReader> GetByIdAsync(Guid id)
        {
            return await _context.MstBleReaders
                .FirstOrDefaultAsync(b => b.Id == id && b.Status != 0);
        }

        public async Task<IEnumerable<MstBleReader>> GetAllAsync()
        {
            return await _context.MstBleReaders
                .Where(b => b.Status != 0)
                .ToListAsync();
        }

        public async Task<MstBleReader> AddAsync(MstBleReader bleReader)
        {
            var brand = await _context.MstBrands
                .FirstOrDefaultAsync(b => b.Id == bleReader.BrandId && b.Status != 0);
            if (brand == null)
                throw new ArgumentException($"Brand with ID {bleReader.BrandId} not found.");

            _context.MstBleReaders.Add(bleReader);
            await _context.SaveChangesAsync();
            return bleReader;
        }

        public async Task UpdateAsync(MstBleReader bleReader)
        {
            var brand = await _context.MstBrands
                .FirstOrDefaultAsync(b => b.Id == bleReader.BrandId && b.Status != 0);
            if (brand == null)
                throw new ArgumentException($"Brand with ID {bleReader.BrandId} not found.");

            _context.MstBleReaders.Update(bleReader);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var bleReader = await _context.MstBleReaders
                .FirstOrDefaultAsync(b => b.Id == id && b.Status != 0);
            if (bleReader == null)
                throw new KeyNotFoundException("BLE Reader not found");

            bleReader.Status = 0; 
            await _context.SaveChangesAsync();
        }
    }
}
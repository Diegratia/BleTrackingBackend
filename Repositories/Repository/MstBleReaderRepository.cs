using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstBleReaderRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstBrandRepository(BleTrackingDbContext context)
        {
            _context = context;
        }
        

        public async Task<MstBleReader> GetByIdAsync(Guid id)
        {
            return await _context.MstBleReaders
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<MstBleReader>> GetAllAsync()
        {
            return await _context.MstBleReaders.ToListAsync();
        }
    }
}
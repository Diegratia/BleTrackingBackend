using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstDepartmentRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstDepartmentRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<MstBleReader> GetByIdAsync(Guid id)
        {
    
        }

        public async Task<IEnumerable<MstBleReader>> GetAllAsync()
        {
       
        }

        public async Task<MstBleReader> AddAsync(MstBleReader bleReader)
        {
        
        }

        public async Task UpdateAsync(MstBleReader bleReader)
        {
      
        }

        public async Task DeleteAsync(Guid id)
        {

        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstEngineRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstEngineRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MstEngine>> GetAllAsync()
        {
            return await _context.MstEngines
            .Where(e => e.Status != 0)
            .ToListAsync();
        }

        public async Task<MstEngine> GetByIdAsync(Guid id)
        {
            return await _context.MstEngines
             .FirstOrDefaultAsync(d => d.Id == id && d.Status != 0);
        }

        public async Task<MstEngine> AddAsync(MstEngine engine)
        {
            _context.MstEngines.Add(engine);
            await _context.SaveChangesAsync();
            return engine;
        }

        public async Task UpdateAsync(MstEngine engine)
        {
            // _context.MstEngines.Update(engine);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var engine = await GetByIdAsync(id);
            if (engine != null)
            {
                // _context.MstEngines.Remove(engine);
                await _context.SaveChangesAsync();
            }
        }

        public IQueryable<MstEngine> GetAllQueryable()
        {
            return _context.MstEngines
                .Where(f => f.Status != 0)
                .AsQueryable();
        }

        public async Task<IEnumerable<MstEngine>> GetAllExportAsync()
        {
            return await _context.MstEngines
            .Where(f => f.Status != 0).ToListAsync();
        } 
        
    }
}
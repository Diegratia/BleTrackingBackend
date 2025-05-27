using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class BleReaderNodeRepository
    {
        private readonly BleTrackingDbContext _context;

        public BleReaderNodeRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<MstBleReader> GetReaderByIdAsync(Guid readerId)
        {
            return await _context.MstBleReaders
                .FirstOrDefaultAsync(r => r.Id == readerId && r.Status != 0);
        }

        public async Task<MstApplication> GetApplicationByIdAsync(Guid applicationId)
        {
            return await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId);
        }

        public async Task<BleReaderNode> GetByIdAsync(Guid id)
        {
            return await _context.BleReaderNodes
                .Include(n => n.Reader)
                .Include(n => n.Application)
                .FirstOrDefaultAsync(n => n.Id == id && (n.Reader == null || n.Reader.Status != 0));
        }

        public async Task<IEnumerable<BleReaderNode>> GetAllAsync()
        {
            return await _context.BleReaderNodes
                .Include(n => n.Reader)
                .Include(n => n.Application)
                .Where(n => n.Reader == null || n.Reader.Status != 0)
                .ToListAsync();
        }

        public async Task<BleReaderNode> AddAsync(BleReaderNode bleReaderNode)
        {
            _context.BleReaderNodes.Add(bleReaderNode);
            await _context.SaveChangesAsync();
            return bleReaderNode;
        }

        public async Task UpdateAsync(BleReaderNode bleReaderNode)
        {
            // _context.BleReaderNodes.Update(bleReaderNode);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var bleReaderNode = await GetByIdAsync(id);
            if (bleReaderNode == null)
                throw new KeyNotFoundException("BleReaderNode not found");

            _context.BleReaderNodes.Remove(bleReaderNode);
            await _context.SaveChangesAsync();
        }
    }
}
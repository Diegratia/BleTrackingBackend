using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class BleReaderNodeRepository : BaseRepository
    {
        public BleReaderNodeRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<BleReaderNode?> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.BleReaderNodes
                .Include(n => n.Reader)
                .Include(n => n.Application)
                .Where(n => n.Id == id && (n.Reader == null || n.Reader.Status != 0));

            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<BleReaderNode>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public IQueryable<BleReaderNode> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.BleReaderNodes
                .Include(n => n.Reader)
                .Include(n => n.Application)
                .Where(n => n.Reader == null || n.Reader.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<BleReaderNode> AddAsync(BleReaderNode bleReaderNode)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin user.");

                bleReaderNode.ApplicationId = applicationId.Value;
            }
            else if (bleReaderNode.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System Admin must specify ApplicationId explicitly.");
            }

            await ValidateApplicationIdAsync(bleReaderNode.ApplicationId);
            ValidateApplicationIdForEntity(bleReaderNode, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(bleReaderNode, applicationId, isSystemAdmin);

            _context.BleReaderNodes.Add(bleReaderNode);
            await _context.SaveChangesAsync();
            return bleReaderNode;
        }

        public async Task UpdateAsync(BleReaderNode bleReaderNode)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(bleReaderNode.ApplicationId);
            ValidateApplicationIdForEntity(bleReaderNode, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(bleReaderNode, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var bleReaderNode = await _context.BleReaderNodes
                .FirstOrDefaultAsync(n => n.Id == id);

            if (bleReaderNode == null)
                throw new KeyNotFoundException("BleReaderNode not found");

            if (!isSystemAdmin && bleReaderNode.ApplicationId != applicationId)
                throw new UnauthorizedAccessException("You don’t have permission to delete this entity.");

            _context.BleReaderNodes.Remove(bleReaderNode);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<BleReaderNode>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        private async Task ValidateRelatedEntitiesAsync(BleReaderNode bleReaderNode, Guid? applicationId, bool isSystemAdmin)
        {
            if (isSystemAdmin) return;

            if (!applicationId.HasValue)
                throw new UnauthorizedAccessException("Missing ApplicationId for non-admin.");

            if (bleReaderNode.ReaderId.HasValue)
            {
                var reader = await _context.MstBleReaders
                    .FirstOrDefaultAsync(r => r.Id == bleReaderNode.ReaderId && r.ApplicationId == applicationId);

                if (reader == null)
                    throw new UnauthorizedAccessException("Reader not found or not accessible in your application.");
            }
        }
    }
}

using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class TrackingTransactionRepository : BaseRepository
    {
        public TrackingTransactionRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<TrackingTransaction?> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TrackingTransactions
                .Include(t => t.Reader)
                .Include(t => t.FloorplanMaskedArea)
                .Where(t => t.Id == id);

            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }

        public async Task<TrackingTransaction?> GetByIdWithIncludesAsync(Guid id)
        {
            return await GetByIdAsync(id); // sudah include
        }

        public async Task<IEnumerable<TrackingTransaction>> GetAllWithIncludesAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<IEnumerable<TrackingTransaction>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task AddAsync(TrackingTransaction transaction)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin users.");

                transaction.ApplicationId = applicationId.Value;
            }
            else if (transaction.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("SystemAdmin Must provide ApplicationId.");
            }

            await ValidateApplicationIdAsync(transaction.ApplicationId);
            ValidateApplicationIdForEntity(transaction, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(transaction, applicationId, isSystemAdmin);

            _context.TrackingTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TrackingTransaction transaction)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(transaction.ApplicationId);
            ValidateApplicationIdForEntity(transaction, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(transaction, applicationId, isSystemAdmin);

            _context.TrackingTransactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TrackingTransaction transaction)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin && transaction.ApplicationId != applicationId)
                throw new UnauthorizedAccessException("You don’t have permission to delete this transaction.");

            _context.TrackingTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }

        public IQueryable<TrackingTransaction> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TrackingTransactions
                .IgnoreQueryFilters()  
                .Include(t => t.Member)
                .Include(t => t.Visitor)
                .Include(t => t.Reader)
                .Include(t => t.Card)
                .Include(t => t.FloorplanMaskedArea);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        private async Task ValidateRelatedEntitiesAsync(TrackingTransaction transaction, Guid? applicationId, bool isSystemAdmin)
        {
            if (isSystemAdmin) return;
            
            if (!applicationId.HasValue)
                throw new UnauthorizedAccessException("Missing ApplicationId for non-admin.");

            var reader = await _context.MstBleReaders
                .FirstOrDefaultAsync(r => r.Id == transaction.ReaderId && r.ApplicationId == applicationId);
            if (reader == null)
                throw new UnauthorizedAccessException("Invalid ReaderId for this application.");

            var area = await _context.FloorplanMaskedAreas
                .FirstOrDefaultAsync(f => f.Id == transaction.FloorplanMaskedAreaId && f.ApplicationId == applicationId);
            if (area == null)
                throw new UnauthorizedAccessException("Invalid FloorplanMaskedAreaId for this application.");
        }
    }
}

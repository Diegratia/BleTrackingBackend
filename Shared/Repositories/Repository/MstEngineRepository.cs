using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Shared.Contracts;


namespace Repositories.Repository
{
    public class MstEngineRepository : BaseRepository
    {
        public MstEngineRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<IEnumerable<MstEngine>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }
        public async Task<IEnumerable<MstEngine>> GetAllOnlineAsync()
        {
            return await GetAllQueryable().Where(e => e.ServiceStatus == ServiceStatus.Online).ToListAsync() ?? new List<MstEngine>();
        }

        public async Task<MstEngine?> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstEngines
                .Where(e => e.Id == id && e.Status != 0);

            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }
            public async Task<MstEngine?> GetByEngineIdAsync(string engineId)
        {

            var query = _context.MstEngines
                .Where(e => e.EngineTrackingId == engineId && e.Status != 0);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<MstEngine> AddAsync(MstEngine engine)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin user.");

                engine.ApplicationId = applicationId.Value;
            }
            else if (engine.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("Admin must specify ApplicationId explicitly.");
            }

            await ValidateApplicationIdAsync(engine.ApplicationId);
            ValidateApplicationIdForEntity(engine, applicationId, isSystemAdmin);

            _context.MstEngines.Add(engine);
            await _context.SaveChangesAsync();
            return engine;
        }

        public async Task UpdateAsync(MstEngine engine)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(engine.ApplicationId);
            ValidateApplicationIdForEntity(engine, applicationId, isSystemAdmin);

            // _context.MstEngines.Update(engine);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateByEngineStringAsync(MstEngine engine)
        {
            _context.MstEngines.Attach(engine);
            _context.Entry(engine).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }


        public async Task DeleteAsync(Guid id)
        {
            var engine = await GetByIdAsync(id);
            if (engine == null)
                throw new KeyNotFoundException("Engine not found.");

            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            if (!isSystemAdmin && engine.ApplicationId != applicationId)
                throw new UnauthorizedAccessException("You don’t have permission to delete this entity.");

            await _context.SaveChangesAsync();
        }

        public IQueryable<MstEngine> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstEngines
                .Where(e => e.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<MstEngine>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }
    }
}

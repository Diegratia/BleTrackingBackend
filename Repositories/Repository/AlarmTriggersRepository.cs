using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Helpers.Consumer;
using Data.ViewModels;

namespace Repositories.Repository
{
    public class AlarmTriggersRepository : BaseRepository
    {

        public AlarmTriggersRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
           : base(context, httpContextAccessor)
        {
        }
        public async Task<IEnumerable<AlarmTriggers>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }
        
            public IQueryable<AlarmTriggers> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.AlarmTriggers
                .Include(b => b.FloorplanId);

            // query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }
    }
}
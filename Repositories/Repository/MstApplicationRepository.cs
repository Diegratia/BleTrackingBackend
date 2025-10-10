using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class MstApplicationRepository : BaseRepository
    {
        // private readonly BleTrackingDbContext _context;

        // public MstApplicationRepository(BleTrackingDbContext context)
        // {
        //     _context = context;
        // }
         public MstApplicationRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<MstApplication> GetByIdAsync(Guid id)
        {
            return await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == id && a.ApplicationStatus != 0);
        }

        public async Task<IEnumerable<MstApplication>> GetAllAsync()
        {
            return await _context.MstApplications
                .Where(a => a.ApplicationStatus != 0)
                .ToListAsync();
        }

        public async Task<MstApplication> AddAsync(MstApplication application)
        {
            _context.MstApplications.Add(application);
            await _context.SaveChangesAsync();
            return application;
        }

        public async Task UpdateAsync(MstApplication application)
        {
            // _context.MstApplications.Update(application);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var application = await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == id && a.ApplicationStatus != 0);
            if (application == null)
                throw new KeyNotFoundException("Application not found");

            application.ApplicationStatus = 0;
            await _context.SaveChangesAsync();
        }

        public IQueryable<MstApplication> GetAllQueryable()
        {
            return _context.MstApplications
                .Where(f => f.ApplicationStatus != 0)
                .AsQueryable();
        }
        
             public async Task<IEnumerable<MstApplication>> GetAllExportAsync()
        {
            return await _context.MstApplications
                .Where(a => a.ApplicationStatus != 0)
                .ToListAsync();
        }
    }
}
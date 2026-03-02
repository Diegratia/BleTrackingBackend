using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Shared.Contracts;
using Shared.Contracts.Read;
using Repositories.Extensions;

namespace Repositories.Repository
{
    public class MstBuildingRepository : BaseRepository
    {
        public MstBuildingRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        private IQueryable<MstBuildingRead> ProjectToRead(IQueryable<MstBuilding> query)
        {
            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any())
            {
                query = query.Where(x => accessibleBuildingIds.Contains(x.Id));
            }
            return query.AsNoTracking()
                .Select(x => new MstBuildingRead
                {
                    Id = x.Id,
                    Name = x.Name,
                    Image = x.Image,
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy,
                    UpdatedAt = x.UpdatedAt,
                    UpdatedBy = x.UpdatedBy,
                    Status = x.Status,
                    ApplicationId = x.ApplicationId
                });
        }

        public async Task<(List<MstBuildingRead> Data, int Total, int Filtered)> FilterAsync(MstBuildingFilter filter)
        {
            var query = GetAllQueryable();

            var total = await query.CountAsync();

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            if (!string.IsNullOrEmpty(filter.Search))
            {
                string searchLower = filter.Search.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(searchLower));
            }

            if (filter.DateFrom.HasValue)
            {
                query = query.Where(x => x.UpdatedAt >= filter.DateFrom.Value);
            }

            if (filter.DateTo.HasValue)
            {
                query = query.Where(x => x.UpdatedAt <= filter.DateTo.Value);
            }

            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        public async Task<MstBuildingRead?> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstBuildings
                .Where(d => d.Id == id && d.Status != 0);
            query = query.WithActiveRelations();

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<MstBuilding?> GetByIdEntityAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstBuildings
                .Where(d => d.Id == id && d.Status != 0);
            query = query.WithActiveRelations();

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstBuildings
                .Where(d => d.Id == id && d.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var building = await query.FirstOrDefaultAsync();
            if (building == null)
                throw new KeyNotFoundException("Building not found");

            building.Status = 0;
            await _context.SaveChangesAsync();
        }

        public async Task<List<MstBuildingRead>> GetAllAsync()
        {
            return await ProjectToRead(GetAllQueryable()).ToListAsync();
        }

        public async Task<MstBuilding> AddAsync(MstBuilding building)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            // non system ambil dari claim
            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");
                building.ApplicationId = applicationId.Value;
            }
            // admin set applciation di body
            else if (building.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }
            await ValidateApplicationIdAsync(building.ApplicationId);
            ValidateApplicationIdForEntity(building, applicationId, isSystemAdmin);

            _context.MstBuildings.Add(building);
            await _context.SaveChangesAsync();
            return building;
        }

        public async Task UpdateAsync(MstBuilding building)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            await ValidateApplicationIdAsync(building.ApplicationId);
            ValidateApplicationIdForEntity(building, applicationId, isSystemAdmin);

            // _context.MstBuildings.Update(building);
            await _context.SaveChangesAsync();
        }

        public IQueryable<MstBuilding> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstBuildings
                .Where(d => d.Status != 0);
            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any())
            {
                query = query
                .Where(x => accessibleBuildingIds.Contains(x.Id));
            }
            query = query.WithActiveRelations();
            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<MstBuilding>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<IEnumerable<MstBuilding>> GetPaginatedExportAsync(int page, int pageSize)
        {
            var query = GetAllQueryable();
            query = query.OrderBy(x => x.Name);
            query = query.Skip((page - 1) * pageSize).Take(pageSize);
            return await query.ToListAsync();
        }
    }
}

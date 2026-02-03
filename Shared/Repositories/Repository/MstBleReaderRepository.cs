using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;
using System.Text.Json;

namespace Repositories.Repository
{
    public class MstBleReaderRepository : BaseRepository
    {
        public MstBleReaderRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<MstBleReaderRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(r => r.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<MstBleReader?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MstBleReaderRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<IEnumerable<MstBleReaderRead>> GetAllUnassignedAsync()
        {
            return await ProjectToRead(GetAllUnassignedQueryable()).ToListAsync();
        }

        public async Task<MstBleReader> AddAsync(MstBleReader reader)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required");

                reader.ApplicationId = applicationId.Value;
            }
            else if (reader.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System Admin must set ApplicationId");
            }

            await ValidateApplicationIdAsync(reader.ApplicationId);
            ValidateApplicationIdForEntity(reader, applicationId, isSystemAdmin);

            var brand = await _context.MstBrands
                .FirstOrDefaultAsync(b => b.Id == reader.BrandId && b.Status != 0);
            if (brand == null)
                throw new ArgumentException($"Brand with ID {reader.BrandId} not found.");

            _context.MstBleReaders.Add(reader);
            await _context.SaveChangesAsync();

            return reader;
        }

        public async Task UpdateAsync(MstBleReader reader)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(reader.ApplicationId);
            ValidateApplicationIdForEntity(reader, applicationId, isSystemAdmin);

            var brand = await _context.MstBrands
                .FirstOrDefaultAsync(b => b.Id == reader.BrandId && b.Status != 0);
            if (brand == null)
                throw new ArgumentException($"Brand with ID {reader.BrandId} not found.");

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstBleReaders
                .Where(r => r.Id == id && r.Status != 0);

            var reader = await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();

            if (reader == null)
                throw new KeyNotFoundException("BLE Reader not found or unauthorized");

            await _context.SaveChangesAsync();
        }

        public async Task<MstBrand?> GetBrandByIdAsync(Guid id)
        {
            return await _context.MstBrands
                .FirstOrDefaultAsync(b => b.Id == id && b.Status != 0);
        }

        private IQueryable<MstBleReader> BaseEntityQuery()
        {
            return GetAllQueryable();
        }

        public IQueryable<MstBleReader> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstBleReaders
                .Include(r => r.Brand)
                .Where(r => r.Status != 0);

            query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public IQueryable<MstBleReader> GetAllUnassignedQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstBleReaders
                .Include(r => r.Brand)
                .Where(r => r.IsAssigned == false && r.Status != 0);

            query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<MstBleReader>> GetAllExportAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstBleReaders
                .Where(d => d.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.ToListAsync();
        }

        public IQueryable<MstBleReaderRead> ProjectToRead(IQueryable<MstBleReader> query)
        {
            return query.AsNoTracking().Select(x => new MstBleReaderRead
            {
                Id = x.Id,
                BrandId = x.BrandId,
                Name = x.Name,
                Ip = x.Ip,
                Gmac = x.Gmac,
                IsAssigned = x.IsAssigned,
                ReaderType = x.ReaderType.ToString(),
                Status = x.Status ?? 0,
                ApplicationId = x.ApplicationId,
                Brand = x.Brand == null ? null : new MstBrandRead
                {
                    Id = x.Brand.Id,
                    Name = x.Brand.Name,
                    Status = x.Brand.Status ?? 0,
                    ApplicationId = x.Brand.ApplicationId
                }
            });
        }

        public async Task<(List<MstBleReaderRead> Data, int Total, int Filtered)> FilterAsync(
            MstBleReaderFilter filter
        )
        {
            var query = BaseEntityQuery();

            var total = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(search) ||
                    x.Brand.Name.ToLower().Contains(search) ||
                    x.Gmac.ToLower().Contains(search) ||
                    x.Ip.ToLower().Contains(search)
                );
            }

            var brandIds = ExtractIds(filter.BrandId);
            if (brandIds.Count > 0)
                query = query.Where(x => brandIds.Contains(x.BrandId));

            if (!string.IsNullOrWhiteSpace(filter.ReaderType))
            {
                if (Enum.TryParse<ReaderType>(filter.ReaderType, true, out var readerType))
                    query = query.Where(x => x.ReaderType == readerType);
                else if (int.TryParse(filter.ReaderType, out var readerTypeInt))
                    query = query.Where(x => (int)x.ReaderType == readerTypeInt);
            }

            if (filter.IsAssigned.HasValue)
                query = query.Where(x => x.IsAssigned == filter.IsAssigned.Value);

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.UpdatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.UpdatedAt <= filter.DateTo.Value);

            if (filter.Status.HasValue)
                query = query.Where(x => x.Status == filter.Status.Value);

            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        private static List<Guid> ExtractIds(JsonElement element)
        {
            var ids = new List<Guid>();

            if (element.ValueKind == JsonValueKind.String)
            {
                var raw = element.GetString();
                if (!string.IsNullOrWhiteSpace(raw) && Guid.TryParse(raw, out var singleId))
                    ids.Add(singleId);
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in element.EnumerateArray())
                {
                    if (el.ValueKind != JsonValueKind.String)
                        continue;
                    var raw = el.GetString();
                    if (string.IsNullOrWhiteSpace(raw))
                        continue;
                    if (Guid.TryParse(raw, out var parsed))
                        ids.Add(parsed);
                }
            }

            return ids;
        }
    }
}

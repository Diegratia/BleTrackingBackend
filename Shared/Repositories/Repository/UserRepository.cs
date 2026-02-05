using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class UserRepository : BaseRepository
    {
        public UserRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        private IQueryable<User> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<UserRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(x => x.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByIdEntityAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(x => x.Id == id);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserRead>> GetAllAsync()
        {
            var query = BaseEntityQuery();
            return await ProjectToRead(query).ToListAsync();
        }

        private IQueryable<UserRead> ProjectToRead(IQueryable<User> query)
        {
            return query.AsNoTracking().Select(x => new UserRead
            {
                Id = x.Id,
                Username = x.Username,
                Email = x.Email,
                GroupId = x.GroupId,
                GroupName = x.Group.Name,
                GroupLevel = x.Group.LevelPriority.Value,
                IsEmailConfirmation = x.IsEmailConfirmation,
                IsIntegration = x.IsIntegration,
                LastLoginAt = x.LastLoginAt,
                Status = (int)x.Status,
                ApplicationId = x.ApplicationId,
            });
        }

        public async Task<(List<UserRead> Data, int Total, int Filtered)> FilterAsync(
            UserFilter filter)
        {
            var query = BaseEntityQuery();

            var total = await query.CountAsync();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.ToLower();
                query = query.Where(x => x.Username.ToLower().Contains(s) || x.Email.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(filter.Username))
            {
                query = query.Where(x => x.Username.ToLower().Contains(filter.Username.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Email))
            {
                query = query.Where(x => x.Email.ToLower().Contains(filter.Email.ToLower()));
            }

            var groupIds = ExtractIds(filter.GroupId);
            if (groupIds.Count > 0)
                query = query.Where(x => groupIds.Contains(x.GroupId));

            if (filter.IsEmailConfirmed.HasValue)
                query = query.Where(x => x.IsEmailConfirmation == (filter.IsEmailConfirmed.Value ? 1 : 0));

            if (filter.IsIntegration.HasValue)
                query = query.Where(x => x.IsIntegration == filter.IsIntegration.Value);

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.UpdatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.UpdatedAt <= filter.DateTo.Value);

            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();
            return (data, total, filtered);
        }

        // ============ AUTH-RELATED METHODS (Keep These) ============

        // dengan filter aplikasi - for auth purposes
        public async Task<User> GetByIdAsyncRaw(Guid id)
        {
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Id == id && u.Status != 0);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("User not found");
        }

        public async Task<User> GetByIdAsyncRegister(Guid id)
        {
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Id == id && u.Status != 0);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("User not found");
        }

        // dipakai untuk update field pada konfirmasi visitor
        public async Task<User> GetByIdAsyncConfirm(Guid id)
        {
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Id == id && u.Status == 0);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Confirm Failed, User not found");
        }

        public async Task<List<User>> GetAllIntegrationAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Status != 0 && u.IsIntegration == true && u.Group.LevelPriority == LevelPriority.SuperAdmin);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.ToListAsync();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Email.ToLower() == email.ToLower() && u.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("2 .User not found");
        }

        public async Task<User> GetByEmailAsyncRaw(string email)
        {
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Email.ToLower() == email.ToLower() && u.Status != 0);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("2 .User not found");
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Username.ToLower() == username.ToLower() && u.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Username not found");
        }

        public async Task<User> GetByConfirmationCodeAsync(string EmailConfirmationCode)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.EmailConfirmationCode.ToLower() == EmailConfirmationCode.ToLower() && u.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Email Confirmation Code not found");
        }

        public async Task<User> GetByIntegrationUsername(string Username)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Status != 0 && u.IsIntegration == true && u.Group.LevelPriority == LevelPriority.SuperAdmin);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Email Confirmation Code not found");
        }

        public async Task<User> GetByEmailConfirmPasswordAsync(string email)
        {
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Email == email); // ⛔ tanpa filter dulu
            return await query.FirstOrDefaultAsync(); // lihat apakah user ada
        }

        public async Task<User> GetByEmailConfirmPasswordAsyncRaw(string email)
        {
            return await _context.Users
                .Include(u => u.Group)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsEmailConfirmation == 0 && u.Status == 0);
        }

        public async Task<User> GetByEmailSetPasswordAsync(string email)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Email == email && u.IsEmailConfirmation == 1 && u.Status == 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException(" 3. User not found");
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.Users
                .Where(u => u.Email == email && u.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.AnyAsync();
        }

        public async Task<bool> EmailExistsAsyncRaw(string email)
        {
            var query = _context.Users
                .Where(u => u.Email == email && u.Status != 0);
            return await query.AnyAsync();
        }

        public async Task<bool> EmailExistsAsyncNotActiveRaw(string email)
        {
            var query = _context.Users
                .Where(u => u.Email == email && u.Status == 0);
            return await query.AnyAsync();
        }

        public async Task<User> AddAsync(User user)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            await ValidateApplicationIdAsync(user.ApplicationId);
            ValidateApplicationIdForEntity(user, applicationId, isSystemAdmin);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> AddRawAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var existingUser = await GetByIdAsyncRaw(user.Id);
            if (existingUser == null)
                throw new KeyNotFoundException("User not found");

            await ValidateApplicationIdAsync(user.ApplicationId);
            ValidateApplicationIdForEntity(user, applicationId, isSystemAdmin);

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRawAsync(User user)
        {
            var existingUser = await GetByIdAsyncRaw(user.Id);
            if (existingUser == null)
                throw new KeyNotFoundException("User not found");

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateConfirmAsync(User user)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var existingUser = await GetByIdAsyncConfirm(user.Id);
            if (existingUser == null)
                throw new KeyNotFoundException("User not found");

            await ValidateApplicationIdAsync(user.ApplicationId);
            ValidateApplicationIdForEntity(user, applicationId, isSystemAdmin);

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var user = await GetByIdEntityAsync(id);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.Status = 0;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var user = await GetByIdEntityAsync(id);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.Status = 0;
            await _context.SaveChangesAsync();
        }
    }
}

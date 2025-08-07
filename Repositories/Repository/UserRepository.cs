using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Microsoft.AspNetCore.Http;

namespace Repositories.Repository
{
    public class UserRepository : BaseRepository
    {
        public UserRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        // dengan filter aplikasi
        public async Task<User> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Id == id && u.StatusActive != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("User not found");
        }

        //tanpa filter applikasi
           public async Task<User> GetByIdAsyncRaw(Guid id)
        {
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Id == id && u.StatusActive != 0);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("User not found");
        }
        
              public async Task<User> GetByIdAsyncRegister(Guid id)
        {
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Id == id && u.StatusActive != 0);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("User not found");
        }

        // dipakai untuk update field pada konfirmasi visitor
        public async Task<User> GetByIdAsyncConfirm(Guid id)
        {
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Id == id && u.StatusActive == 0);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Confirm Failed, User not found");
        }

        public async Task<List<User>> GetAllAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.StatusActive != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.ToListAsync();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Email.ToLower() == email.ToLower() && u.StatusActive != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("2 .User not found");
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Username.ToLower() == username.ToLower() && u.StatusActive != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("UserName not found");
        }

        // public async Task<User> GetByEmailConfirmPasswordAsync(string email)
        // {

        //      var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
        //     var query = _context.Users
        //         .Include(u => u.Group)
        //         .Where(u => u.Email == email && u.IsEmailConfirmation == 0 && u.StatusActive == 0);
        //          query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        //     return await query.FirstOrDefaultAsync();
        // }

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
            .FirstOrDefaultAsync(u => u.Email == email && u.IsEmailConfirmation == 0 && u.StatusActive == 0);
        }

        public async Task<User> GetByEmailSetPasswordAsync(string email)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Email == email && u.IsEmailConfirmation == 1 && u.StatusActive == 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException(" 3. User not found");
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.Users
                .Where(u => u.Email == email && u.StatusActive != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.AnyAsync();
        }

        public async Task<bool> EmailExistsAsyncRaw(string email)
        {
            var query = _context.Users
                .Where(u => u.Email == email && u.StatusActive != 0);
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


        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var user = await GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.StatusActive = 0;
            await _context.SaveChangesAsync();
        }
    }
}













// using System;
// using System.Threading.Tasks;
// using Entities.Models;
// using Microsoft.EntityFrameworkCore;
// using Repositories.DbContexts;

// namespace Repositories.Repository
// {
//     public class UserRepository
//     {
//         private readonly BleTrackingDbContext _context;

//         public UserRepository(BleTrackingDbContext context)
//         {
//             _context = context;
//         }

//         public async Task<User> GetByIdAsync(Guid id)
//         {
//             return await _context.Users
//                 .Include(u => u.Group)
//                 .FirstOrDefaultAsync(u => u.Id == id && u.StatusActive != 0);
//         }

//         public async Task<List<User>> GetAllAsync()
//         {
//             return await _context.Users
//                 .Include(u => u.Group)
//                 .Where(u => u.StatusActive != 0)
//                 .ToListAsync();
//         }

//         public async Task<User> GetByUsernameAsync(string username)
//         {
//             return await _context.Users
//                 .Include(u => u.Group)
//                 .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
//         }
//         public async Task<User> GetByEmailConfirmPasswordAsync(string email)
//         {
//             return await _context.Users
//                 .Include(u => u.Group)
//                 .FirstOrDefaultAsync(u => u.Email == email && u.IsEmailConfirmation == 0 && u.StatusActive == 0);
//         }
//         public async Task<User> GetByEmailSetPasswordAsync(string email)
//         {
//             return await _context.Users
//                 .Include(u => u.Group)
//                 .FirstOrDefaultAsync(u => u.Email == email && u.IsEmailConfirmation == 1 && u.StatusActive == 0);
//         }

//         public async Task<bool> EmailExistsAsync(string email)
//         {
//             return await _context.Users
//                 .AnyAsync(u => u.Email == email && u.StatusActive != 0);
//         }

//         public async Task<User> AddAsync(User user)
//         {
//             _context.Users.Add(user);
//             await _context.SaveChangesAsync();
//             return user;
//         }

//         public async Task UpdateAsync(User user)
//         {
//             // _context.Users.Update(user);
//             await _context.SaveChangesAsync();
//         }
//     }
// }


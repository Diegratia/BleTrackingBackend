using System;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class UserRepository
    {
        private readonly BleTrackingDbContext _context;

        public UserRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.Id == id && u.StatusActive != 0);
        }

            public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Group)
                .ToListAsync();
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.Email == email && u.StatusActive != 0);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email && u.StatusActive != 0);
        }

        public async Task<User> AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            // _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using System;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class RefreshTokenRepository
    {
        private readonly BleTrackingDbContext _context;

        public RefreshTokenRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token && rt.ExpiryDate > DateTime.UtcNow);
        }

        public async Task<RefreshToken> GetActiveRefreshTokenForUserAsync(Guid userId)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.ExpiryDate > DateTime.UtcNow);
        }

        public async Task DeleteRefreshTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
            if (refreshToken != null)
            {
                _context.RefreshTokens.Remove(refreshToken);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Logout by refresh token ID (optional, tapi bersih)
        public async Task DeleteByIdAsync(Guid id)
        {
            var refreshToken = await _context.RefreshTokens.FindAsync(id);
            if (refreshToken == null)
                return;

            _context.RefreshTokens.Remove(refreshToken);
            await _context.SaveChangesAsync();
        }

        // ✅ Logout ALL devices (recommended feature)
        public async Task DeleteAllByUserIdAsync(Guid userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId)
                .ToListAsync();

            if (!tokens.Any())
                return;

            _context.RefreshTokens.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        }
    }
}
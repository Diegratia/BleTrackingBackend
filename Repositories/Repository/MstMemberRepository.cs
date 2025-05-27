using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstMemberRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstMemberRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<MstDepartment> GetDepartmentByIdAsync(Guid departmentId)
        {
            return await _context.MstDepartments
                .FirstOrDefaultAsync(d => d.Id == departmentId && d.Status != 0);
        }

        public async Task<MstOrganization> GetOrganizationByIdAsync(Guid organizationId)
        {
            return await _context.MstOrganizations
                .FirstOrDefaultAsync(o => o.Id == organizationId && o.Status != 0);
        }

        public async Task<MstDistrict> GetDistrictByIdAsync(Guid districtId)
        {
            return await _context.MstDistricts
                .FirstOrDefaultAsync(d => d.Id == districtId && d.Status != 0);
        }

        public async Task<MstMember> GetByIdAsync(Guid id)
        {
            return await _context.MstMembers
                .Include(m => m.Department)
                .Include(m => m.District)
                .Include(m => m.Organization)
                .FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
        }

        public async Task<IEnumerable<MstMember>> GetAllAsync()
        {
            return await _context.MstMembers
                .Include(m => m.Department)
                .Include(m => m.District)
                .Include(m => m.Organization)
                .Where(m => m.Status != 0)
                .ToListAsync();
        }

        public async Task<MstMember> AddAsync(MstMember member)
        {
            _context.MstMembers.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task UpdateAsync(MstMember member)
        {
            // _context.MstMembers.Update(member);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var member = await GetByIdAsync(id);
            if (member == null)
                throw new KeyNotFoundException("Member not found");

            member.Status = 0;
            member.ExitDate = DateOnly.FromDateTime(DateTime.UtcNow);
            member.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
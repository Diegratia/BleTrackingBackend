using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;

namespace BusinessLogic.Services.Implementation
{
    public class MstOrganizationService : IMstOrganizationService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;

        public MstOrganizationService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MstOrganizationDto>> GetAllOrganizationsAsync()
        {
            var organizations = await _context.MstOrganizations.ToListAsync();
            return _mapper.Map<IEnumerable<MstOrganizationDto>>(organizations);
        }

        public async Task<MstOrganizationDto> GetOrganizationByIdAsync(Guid id)
        {
            var organization = await _context.MstOrganizations.FirstOrDefaultAsync(o => o.Id == id);
            return _mapper.Map<MstOrganizationDto>(organization);
        }

        public async Task<MstOrganizationDto> CreateOrganizationAsync(MstOrganizationCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var organization = _mapper.Map<MstOrganization>(dto);
            organization.Id = Guid.NewGuid();
            organization.Status = 1; 
            organization.CreatedBy = ""; 
            organization.CreatedAt = DateTime.UtcNow;
            organization.UpdatedBy = ""; 
            organization.UpdatedAt = DateTime.UtcNow;

            _context.MstOrganizations.Add(organization);
            await _context.SaveChangesAsync();

            return _mapper.Map<MstOrganizationDto>(organization);
        }

        public async Task UpdateOrganizationAsync(Guid id, MstOrganizationUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var organization = await _context.MstOrganizations.FindAsync(id);
            if (organization == null || organization.Status == 0) 
            {
                throw new KeyNotFoundException($"Organization with ID {id} not found or has been deleted.");
            }

          
            organization.UpdatedBy = ""; 
            organization.UpdatedAt = DateTime.UtcNow;
            
              _mapper.Map(dto, organization);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrganizationAsync(Guid id)
        {
            var organization = await _context.MstOrganizations.FindAsync(id);
            if (organization == null || organization.Status == 0) 
            {
                throw new KeyNotFoundException($"Organization with ID {id} not found or already deleted.");
            }

            organization.Status = 0;
            organization.UpdatedBy = ""; 
            organization.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
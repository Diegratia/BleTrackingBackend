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
    public class MstIntegrationService : IMstIntegrationService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;

        public MstIntegrationService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MstIntegrationDto> GetByIdAsync(Guid id)
        {
           var integration = await _context.MstIntegrations
                .Include(i => i.Brand) // Memuat Brand
                .FirstOrDefaultAsync(i => i.Id == id);
            return integration == null ? null : _mapper.Map<MstIntegrationDto>(integration);
        }

        public async Task<IEnumerable<MstIntegrationDto>> GetAllAsync()
        {
          var integrations = await _context.MstIntegrations
                .Include(i => i.Brand) // Memuat Brand
                .ToListAsync();
            return _mapper.Map<IEnumerable<MstIntegrationDto>>(integrations);
        }

        public async Task<MstIntegrationDto> CreateAsync(MstIntegrationCreateDto createDto)
        {
           // Validasi BrandId
            var brand = await _context.MstBrands.FirstOrDefaultAsync(b => b.Id == createDto.BrandId);
            if (brand == null)
                throw new ArgumentException($"Brand with ID {createDto.BrandId} not found.");
            //validasi untuuk id application
            var application = await _context.MstApplications.FirstOrDefaultAsync(a => a.Id == createDto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {createDto.ApplicationId} not found.");

            var integration = _mapper.Map<MstIntegration>(createDto);
            integration.Status = 1;
            integration.CreatedBy ??= "";
            integration.UpdatedBy ??= "";
       
           _context.MstIntegrations.Add(integration);
            await _context.SaveChangesAsync();

            // Muat kembali dengan brand dan applicaiton untuk DTO
            var savedIntegration = await _context.MstIntegrations
                .Include(i => i.Brand)
                .Include(i => i.Application)
                .FirstOrDefaultAsync(i => i.Id == integration.Id);
            return _mapper.Map<MstIntegrationDto>(savedIntegration);    
        }

        public async Task UpdateAsync(Guid id, MstIntegrationUpdateDto updateDto)
        {
            var integration = await _context.MstIntegrations.FindAsync(id);
            if (integration == null)
                throw new KeyNotFoundException("Integration not found");

          // Validasi BrandId jika berubah
            if (integration.BrandId != updateDto.BrandId)
            {
                var brand = await _context.MstBrands.FirstOrDefaultAsync(b => b.Id == updateDto.BrandId);
                if (brand == null)
                    throw new ArgumentException($"Brand with ID {updateDto.BrandId} not found.");
                // Tidak perlu set BrandData, cukup update BrandId
                integration.BrandId = updateDto.BrandId;
            }

            if (integration.ApplicationId != updateDto.ApplicationId)
            {
                var application = await _context.MstApplications.FirstOrDefaultAsync(b => b.Id == updateDto.ApplicationId);
                if (application == null)
                    throw new ArgumentException($"Application with ID {updateDto.ApplicationId} not found.");
                integration.ApplicationId = updateDto.ApplicationId;
            }

            integration.UpdatedBy ??= "";
            _mapper.Map(updateDto, integration);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var integration = await _context.MstIntegrations.FindAsync(id);
            if (integration == null)
                throw new KeyNotFoundException("Integration not found");

            integration.Status = 0;

            // _context.MstIntegrations.Remove(integration);
            await _context.SaveChangesAsync();
        }
    }
}
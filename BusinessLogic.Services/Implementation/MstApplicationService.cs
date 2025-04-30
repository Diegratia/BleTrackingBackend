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
    public class MstApplicationService : IMstApplicationService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper; // Tambahkan IMapper

        public MstApplicationService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MstApplicationDto>> GetAllApplicationsAsync()
        {
            var applications = await _context.MstApplications.ToListAsync();
            return _mapper.Map<IEnumerable<MstApplicationDto>>(applications);
        }

        public async Task<MstApplicationDto> GetApplicationByIdAsync(Guid id)
        {
            var application = await _context.MstApplications.FirstOrDefaultAsync(a => a.Id == id);
            return _mapper.Map<MstApplicationDto>(application);
        }

        // public async Task<MstApplicationDto> CreateApplicationAsync(MstApplicationCreateDto dto)
        // {
        //     if (dto == null) throw new ArgumentNullException(nameof(dto));

        //     // Map dari DTO ke model
        //     var application = _mapper.Map<MstApplication>(dto);
        //     application.Id = Guid.NewGuid(); // Tetap hasilkan Id di server
        //     application.ApplicationStatus = 1;

        //     _context.MstApplications.Add(application);
        //     await _context.SaveChangesAsync();

        //     // Map kembali ke DTO untuk respons
        //     return _mapper.Map<MstApplicationDto>(application);
        // }

                public async Task<MstApplicationDto> CreateApplicationAsync(MstApplicationCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Map dari DTO ke model
            var application = _mapper.Map<MstApplication>(dto);
            application.Id = Guid.NewGuid();
            application.ApplicationStatus = 1;

            _context.MstApplications.Add(application);
            await _context.SaveChangesAsync(); // Simpan aplikasi terlebih dahulu

            // Cari UserGroup dengan LevelPriority.Primary yang terkait dengan application ini
            var userGroup = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.ApplicationId == application.Id && ug.LevelPriority == LevelPriority.Primary);

          if (userGroup == null)
    {
        userGroup = new UserGroup
        {
            Id = Guid.NewGuid(),
            Name = $"Primary Group for {application.Id}",
            LevelPriority = LevelPriority.Primary, // Enum bernilai 1
            ApplicationId = application.Id,
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "system",
            UpdatedAt = DateTime.UtcNow,
            Status = 1
        };
        _context.UserGroups.Add(userGroup);
        await _context.SaveChangesAsync();
    }

            // Buat User dengan role Primary dan assign ke UserGroup yang sudah ada
            var primaryUser = new User
            {
                Id = Guid.NewGuid(),
                Username = $"primary_{application.Id}",
                Password = "hashed_default_password", 
                IsCreatedPassword = 0,
                Email = $"primary_{application.Id}@example.com",
                IsEmailConfirmation = 0,
                EmailConfirmationCode = Guid.NewGuid().ToString(),
                EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(7),
                EmailConfirmationAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                StatusActive = StatusActive.Active,
                GroupId = userGroup.Id
            };

            _context.Users.Add(primaryUser);
            await _context.SaveChangesAsync(); // Simpan User

            // Map kembali ke DTO untuk respons
            return _mapper.Map<MstApplicationDto>(application);
        }

        public async Task UpdateApplicationAsync(Guid id, MstApplicationUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var application = await _context.MstApplications.FindAsync(id);
            if (application == null)
            {
                throw new KeyNotFoundException($"Application with ID {id} not found.");
            }

            // Map dari DTO ke model yang sudah ada
            _mapper.Map(dto, application);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteApplicationAsync(Guid id)
        {
            var application = await _context.MstApplications.FindAsync(id);
            if (application == null)
            {
                throw new KeyNotFoundException($"Application with ID {id} not found.");
            }

            application.ApplicationStatus = 0;
            // _context.MstApplications.Remove(application);
            await _context.SaveChangesAsync();
        }
    }
}
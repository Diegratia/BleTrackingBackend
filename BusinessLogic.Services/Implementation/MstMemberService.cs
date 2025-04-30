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
    public class MstMemberService : IMstMemberService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;
        private readonly string[] _allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png" }; //tipe gambar

        private const long MaxFileSize = 5 * 1024 * 1024; // max 5mb

        public MstMemberService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MstMemberDto>> GetAllMembersAsync()
        {
            var members = await _context.MstMembers
                .Include(m => m.Department)
                .Include(m => m.District)
                .Include(m => m.Organization)
                .ToListAsync(); 
            return _mapper.Map<IEnumerable<MstMemberDto>>(members);
        }

        public async Task<MstMemberDto> GetMemberByIdAsync(Guid id)
        {
            var member = await _context.MstMembers
                .Include(m => m.Department)
                .Include(m => m.District)
                .Include(m => m.Organization)
                .FirstOrDefaultAsync(m => m.Id == id);
            return _mapper.Map<MstMemberDto>(member);
        }

        public async Task<MstMemberDto> CreateMemberAsync(MstMemberCreateDto createDto)
        {
            // validasi 
            var department = await _context.MstDepartments.FirstOrDefaultAsync(a => a.Id == createDto.DepartmentId);
            if (department == null)
                throw new ArgumentException($"Department with ID {createDto.DepartmentId} not found.");
             // validasi 
            var organization = await _context.MstOrganizations.FirstOrDefaultAsync(a => a.Id == createDto.OrganizationId);
            if (organization == null)
                throw new ArgumentException($"Organization with ID {createDto.OrganizationId} not found.");
             // validasi 
            var district = await _context.MstDistricts.FirstOrDefaultAsync(a => a.Id == createDto.DistrictId);
            if (district == null)
                throw new ArgumentException($"District with ID {createDto.DistrictId} not found.");

            var member = _mapper.Map<MstMember>(createDto);
            if (createDto == null) throw new ArgumentNullException(nameof(createDto));

            if (createDto.FaceImage != null && createDto.FaceImage.Length > 0)
            {  
               try{
                if(!_allowedImageTypes.Contains(createDto.FaceImage.ContentType))
                    throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");
                
                 // Validasi ukuran file
                if (createDto.FaceImage.Length > MaxFileSize)
                    throw new ArgumentException("File size exceeds 5 MB limit.");

                // folder penyimpanan di lokal server
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "MemberFaceImages");
                Directory.CreateDirectory(uploadDir); // akan membuat directory jika belum ada

                // buat nama file unik
                var fileName = $"{Guid.NewGuid()}_{createDto.FaceImage.FileName}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await createDto.FaceImage.CopyToAsync(stream);
                    }
                
                member.FaceImage = $"/Uploads/MemberFaceImages/{fileName}";
                member.UploadFr = 1; // Sukses
                member.UploadFrError = "Upload successful"; 
               }
               catch (Exception ex)
               {
                member.UploadFr = 2;
                member.UploadFrError = ex.Message;
                member.FaceImage = null;
               }
            }
            else
            {
                member.UploadFr = 0;
                member.UploadFrError = "No file uploaded";
                member.FaceImage = null;
            }
            
            member.Id = Guid.NewGuid();
            member.Status = 1; 
            member.CreatedBy = ""; 
            member.CreatedAt = DateTime.UtcNow;
            member.UpdatedBy = ""; // bisa ganti dgn logic auth
            member.UpdatedAt = DateTime.UtcNow;
          
        
            member.JoinDate = DateOnly.FromDateTime(DateTime.UtcNow); 
            member.ExitDate = DateOnly.MaxValue; 
            member.BirthDate = createDto.BirthDate; 

            _context.MstMembers.Add(member);
            await _context.SaveChangesAsync();
            var savedMember = await _context.MstMembers
                .Include(m => m.Department)
                .Include(m => m.District)
                .Include(m => m.Organization)
                .FirstOrDefaultAsync(d => d.Id == member.Id);
            return _mapper.Map<MstMemberDto>(member);
        }

        public async Task<MstMemberDto> UpdateMemberAsync(Guid id, MstMemberUpdateDto updateDto)
        {
            if (updateDto == null) throw new ArgumentNullException(nameof(updateDto));

            var member = await _context.MstMembers.FindAsync(id);
            if (member == null || member.Status == 0) // 0 = Deleted
            {
                throw new KeyNotFoundException($"Member with ID {id} not found or has been deleted.");
            }
            
                // validasi Department
                if (member.DepartmentId != updateDto.DepartmentId)
            {
                var department = await _context.MstDepartments.FirstOrDefaultAsync(a => a.Id == updateDto.DepartmentId);
                if (department == null)
                    throw new ArgumentException($"Department with ID {updateDto.DepartmentId} not found.");
                member.DepartmentId = updateDto.DepartmentId;
            }
                  // validasi Organization
                if (member.OrganizationId != updateDto.OrganizationId)
            {
                var organization = await _context.MstOrganizations.FirstOrDefaultAsync(a => a.Id == updateDto.OrganizationId);
                if (organization == null)
                    throw new ArgumentException($"Organization with ID {updateDto.OrganizationId} not found.");
                member.OrganizationId = updateDto.OrganizationId;
            }
               // validasi District
                if (member.DistrictId != updateDto.DistrictId)
            {
                var district = await _context.MstDistricts.FirstOrDefaultAsync(a => a.Id == updateDto.DistrictId);
                if (district == null)
                    throw new ArgumentException($"District with ID {updateDto.DistrictId} not found.");
                member.DistrictId = updateDto.DistrictId;
            }     

              if (updateDto.FaceImage != null && updateDto.FaceImage.Length > 0)
            {  
               try{
                
                if(!_allowedImageTypes.Contains(updateDto.FaceImage.ContentType))
                    throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");
                
                 // Validasi ukuran file
                if (updateDto.FaceImage.Length > MaxFileSize)
                    throw new ArgumentException("File size exceeds 5 MB limit.");

                // folder penyimpanan di lokal server
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "MemberFaceImages");
                Directory.CreateDirectory(uploadDir); // akan membuat directory jika belum ada

                // buat nama file unik
                var fileName = $"{Guid.NewGuid()}_{updateDto.FaceImage.FileName}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await updateDto.FaceImage.CopyToAsync(stream);
                    }
                
                member.FaceImage = $"/Uploads/MemberFaceImages/{fileName}";
                member.UploadFr = 1; // Sukses
                member.UploadFrError = "Upload successful"; 
               }
               catch (Exception ex)
               {
                member.UploadFr = 2;
                member.UploadFrError = ex.Message;
                member.FaceImage = null;
               }
            }
            else
            {
                member.UploadFr = 0;
                member.UploadFrError = "No file uploaded";
                member.FaceImage = null;
            }

            
            member.UpdatedBy = ""; 
            member.UpdatedAt = DateTime.UtcNow;

            member.BirthDate = updateDto.BirthDate; 

            _mapper.Map(updateDto, member);
            await _context.SaveChangesAsync();
            return _mapper.Map<MstMemberDto>(member);
            
        }

        public async Task DeleteMemberAsync(Guid id)
        {
            var member = await _context.MstMembers.FindAsync(id);
            if (member == null || member.Status == 0) 
            {
                throw new KeyNotFoundException($"Member with ID {id} not found or already deleted.");
            }

            member.Status = 0; 
            member.UpdatedBy = ""; 
            member.UpdatedAt = DateTime.UtcNow;
            member.ExitDate = DateOnly.FromDateTime(DateTime.UtcNow); 

            await _context.SaveChangesAsync();
        }
    }
}
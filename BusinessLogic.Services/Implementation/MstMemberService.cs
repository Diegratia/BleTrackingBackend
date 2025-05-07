using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;

namespace BusinessLogic.Services.Implementation
{
    public class MstMemberService : IMstMemberService
    {
        private readonly MstMemberRepository _repository;
        private readonly IMapper _mapper;
        private readonly string[] _allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
        private const long MaxFileSize = 5 * 1024 * 1024; // Max 5 MB

        public MstMemberService(MstMemberRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MstMemberDto>> GetAllMembersAsync()
        {
            var members = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstMemberDto>>(members);
        }

        public async Task<MstMemberDto> GetMemberByIdAsync(Guid id)
        {
            var member = await _repository.GetByIdAsync(id);
            return member == null ? null : _mapper.Map<MstMemberDto>(member);
        }

        public async Task<MstMemberDto> CreateMemberAsync(MstMemberCreateDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            // Validasi relasi
            var department = await _repository.GetDepartmentByIdAsync(createDto.DepartmentId);
            if (department == null)
                throw new ArgumentException($"Department with ID {createDto.DepartmentId} not found.");

            var organization = await _repository.GetOrganizationByIdAsync(createDto.OrganizationId);
            if (organization == null)
                throw new ArgumentException($"Organization with ID {createDto.OrganizationId} not found.");

            var district = await _repository.GetDistrictByIdAsync(createDto.DistrictId);
            if (district == null)
                throw new ArgumentException($"District with ID {createDto.DistrictId} not found.");

            var member = _mapper.Map<MstMember>(createDto);

            // Tangani upload gambar
            if (createDto.FaceImage != null && createDto.FaceImage.Length > 0)
            {
                try
                {
                    // Validasi tipe file
                    if (string.IsNullOrEmpty(createDto.FaceImage.ContentType) || !_allowedImageTypes.Contains(createDto.FaceImage.ContentType))
                        throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

                    // Validasi ukuran file
                    if (createDto.FaceImage.Length > MaxFileSize)
                        throw new ArgumentException("File size exceeds 5 MB limit.");

                    // Folder penyimpanan
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "MemberFaceImages");
                    Directory.CreateDirectory(uploadDir);

                    // Buat nama file unik
                    var fileExtension = Path.GetExtension(createDto.FaceImage.FileName)?.ToLower() ?? ".jpg";
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    // Simpan file
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
                    member.UploadFr = 2; // Gagal
                    member.UploadFrError = ex.Message;
                    member.FaceImage = null;
                }
            }
            else
            {
                member.UploadFr = 0; // Tidak ada file
                member.UploadFrError = "No file uploaded";
                member.FaceImage = null;
            }

            member.Id = Guid.NewGuid();
            member.Status = 1;
            member.CreatedBy = "System";
            member.CreatedAt = DateTime.UtcNow;
            member.UpdatedBy = "System";
            member.UpdatedAt = DateTime.UtcNow;
            member.JoinDate = DateOnly.FromDateTime(DateTime.UtcNow);
            member.ExitDate = DateOnly.MaxValue;
            member.BirthDate = createDto.BirthDate;

            await _repository.AddAsync(member);
            return _mapper.Map<MstMemberDto>(member);
        }

        public async Task<MstMemberDto> UpdateMemberAsync(Guid id, MstMemberUpdateDto updateDto)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            var member = await _repository.GetByIdAsync(id);
            if (member == null)
                throw new KeyNotFoundException($"Member with ID {id} not found or has been deleted.");

            // Validasi relasi jika berubah
            if (member.DepartmentId != updateDto.DepartmentId)
            {
                var department = await _repository.GetDepartmentByIdAsync(updateDto.DepartmentId);
                if (department == null)
                    throw new ArgumentException($"Department with ID {updateDto.DepartmentId} not found.");
                member.DepartmentId = updateDto.DepartmentId;
            }

            if (member.OrganizationId != updateDto.OrganizationId)
            {
                var organization = await _repository.GetOrganizationByIdAsync(updateDto.OrganizationId);
                if (organization == null)
                    throw new ArgumentException($"Organization with ID {updateDto.OrganizationId} not found.");
                member.OrganizationId = updateDto.OrganizationId;
            }

            if (member.DistrictId != updateDto.DistrictId)
            {
                var district = await _repository.GetDistrictByIdAsync(updateDto.DistrictId);
                if (district == null)
                    throw new ArgumentException($"District with ID {updateDto.DistrictId} not found.");
                member.DistrictId = updateDto.DistrictId;
            }

            // Tangani upload gambar
            if (updateDto.FaceImage != null && updateDto.FaceImage.Length > 0)
            {
                try
                {
                    // Validasi tipe file
                    if (string.IsNullOrEmpty(updateDto.FaceImage.ContentType) || !_allowedImageTypes.Contains(updateDto.FaceImage.ContentType))
                        throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

                    // Validasi ukuran file
                    if (updateDto.FaceImage.Length > MaxFileSize)
                        throw new ArgumentException("File size exceeds 5 MB limit.");

                    // Hapus file lama jika ada
                    if (!string.IsNullOrEmpty(member.FaceImage))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), member.FaceImage.TrimStart('/'));
                        if (File.Exists(oldFilePath))
                        {
                            try
                            {
                                File.Delete(oldFilePath);
                            }
                            catch (IOException ex)
                            {
                                throw new IOException("Failed to delete old image file.", ex);
                            }
                        }
                    }

                    // Folder penyimpanan
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "MemberFaceImages");
                    Directory.CreateDirectory(uploadDir);

                    // Buat nama file unik
                    var fileExtension = Path.GetExtension(updateDto.FaceImage.FileName)?.ToLower() ?? ".jpg";
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    // Simpan file
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
                    member.UploadFr = 2; // Gagal
                    member.UploadFrError = ex.Message;
                    member.FaceImage = null;
                }
            }

            member.UpdatedBy = "System";
            member.UpdatedAt = DateTime.UtcNow;
            member.BirthDate = updateDto.BirthDate;

            _mapper.Map(updateDto, member);
            await _repository.UpdateAsync(member);
            return _mapper.Map<MstMemberDto>(member);
        }

        public async Task DeleteMemberAsync(Guid id)
        {
            await _repository.SoftDeleteAsync(id);
        }
    }
}
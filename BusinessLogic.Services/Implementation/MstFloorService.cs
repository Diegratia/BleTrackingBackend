using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Services.Implementation
{
    public class MstFloorService : IMstFloorService
    {
        private readonly MstFloorRepository _repository;
        private readonly IMapper _mapper;
        private readonly string[] _allowedImageTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
        private const long MaxFileSize = 1 * 1024 * 1024; // Maksimal 1 MB
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MstFloorService(MstFloorRepository repository, IMapper mapper, HttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MstFloorDto> GetByIdAsync(Guid id)
        {
            var floor = await _repository.GetByIdAsync(id);
            return floor == null ? null : _mapper.Map<MstFloorDto>(floor);
        }

        public async Task<IEnumerable<MstFloorDto>> GetAllAsync()
        {
            var floors = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstFloorDto>>(floors);
        }

        public async Task<MstFloorDto> CreateAsync(MstFloorCreateDto createDto)
        {
            var floor = _mapper.Map<MstFloor>(createDto);

            // Upload gambar
            if (createDto.FloorImage != null && createDto.FloorImage.Length > 0)
            {
                // Validasi tipe file
                if (string.IsNullOrEmpty(createDto.FloorImage.ContentType) || !_allowedImageTypes.Contains(createDto.FloorImage.ContentType))
                    throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

                // Validasi ukuran file
                if (createDto.FloorImage.Length > MaxFileSize)
                    throw new ArgumentException("File size exceeds 1 MB limit.");

                // Folder penyimpanan
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "FloorImages");
                Directory.CreateDirectory(uploadDir);

                // Buat nama file unik
                var fileExtension = Path.GetExtension(createDto.FloorImage.FileName)?.ToLower() ?? ".jpg";
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadDir, fileName);

                // Simpan file
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await createDto.FloorImage.CopyToAsync(stream);
                    }
                }
                catch (IOException ex)
                {
                    throw new IOException("Failed to save image file.", ex);
                }

                // Simpan path relatif
                floor.FloorImage = $"/Uploads/FloorImages/{fileName}";
            }

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            floor.Id = Guid.NewGuid();
            floor.Status = 1;
            floor.CreatedBy = username;
            floor.CreatedAt = DateTime.UtcNow;
            floor.UpdatedBy = username;
            floor.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(floor);
            return _mapper.Map<MstFloorDto>(floor);
        }

        public async Task<MstFloorDto> UpdateAsync(Guid id, MstFloorUpdateDto updateDto)
        {
            var floor = await _repository.GetByIdAsync(id);
            if (floor == null)
                throw new KeyNotFoundException("Floor not found");

         
            if (updateDto.FloorImage != null && updateDto.FloorImage.Length > 0)
            {
     
                if (string.IsNullOrEmpty(updateDto.FloorImage.ContentType) || !_allowedImageTypes.Contains(updateDto.FloorImage.ContentType))
                    throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

         
                if (updateDto.FloorImage.Length > MaxFileSize)
                    throw new ArgumentException("File size exceeds 1 MB limit.");

       
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "FloorImages");
                Directory.CreateDirectory(uploadDir);

           
                if (!string.IsNullOrEmpty(floor.FloorImage))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), floor.FloorImage.TrimStart('/'));
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

                // Simpan file baru
                var fileExtension = Path.GetExtension(updateDto.FloorImage.FileName)?.ToLower() ?? ".jpg";
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadDir, fileName);

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await updateDto.FloorImage.CopyToAsync(stream);
                    }
                }
                catch (IOException ex)
                {
                    throw new IOException("Failed to save image file.", ex);
                }

                floor.FloorImage = $"/Uploads/FloorImages/{fileName}";
            }

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            floor.UpdatedBy = username;
            floor.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(updateDto, floor);

            await _repository.UpdateAsync(floor);
            return _mapper.Map<MstFloorDto>(floor);
        }

        public async Task DeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var floor = await _repository.GetByIdAsync(id);
            floor.UpdatedBy = username;
            await _repository.SoftDeleteAsync(id);
        }
    }
}
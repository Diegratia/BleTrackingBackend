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
    public class MstFloorService : IMstFloorService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;
        private readonly string[] _allowedImageTypes = new[] { "image/jpeg", "image/png", "image/jpg" }; // tipe gambar
        private const long MaxFileSize = 1 * 1024 * 1024; // maksimal 1mb

        public MstFloorService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MstFloorDto> GetByIdAsync(Guid id)
        {
            var floor = await _context.MstFloors
                .FirstOrDefaultAsync(f => f.Id == id);
            return floor == null ? null : _mapper.Map<MstFloorDto>(floor);
        }

        public async Task<IEnumerable<MstFloorDto>> GetAllAsync()
        {
            var floors = await _context.MstFloors.ToListAsync();
            return _mapper.Map<IEnumerable<MstFloorDto>>(floors);
        }

        public async Task<MstFloorDto> CreateAsync(MstFloorCreateDto createDto)
        {
            var floor = _mapper.Map<MstFloor>(createDto);

            // upload gambar
            if (createDto.FloorImage != null && createDto.FloorImage.Length > 0)
            {
                // Validasi tipe file
                if (!_allowedImageTypes.Contains(createDto.FloorImage.ContentType))
                    throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

                    // Validasi ukuran file
                if (createDto.FloorImage.Length > MaxFileSize)
                    throw new ArgumentException("File size exceeds 1 MB limit.");

                // folder penyimpanan di lokal server
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "FloorImages");
                Directory.CreateDirectory(uploadDir); // akan membuat directory jika belum ada

                // buat nama file unik
                var fileName = $"{Guid.NewGuid()}_{createDto.FloorImage.FileName}";
                var filePath = Path.Combine(uploadDir, fileName);

                // simpan file ke lokal
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await createDto.FloorImage.CopyToAsync(stream);
                }

                // simpan path relatif ke database
                floor.FloorImage = $"/Uploads/FloorImages/{fileName}";
            }

            floor.CreatedBy = "";
            floor.UpdatedBy = "";
            floor.Status = 1;

            _context.MstFloors.Add(floor);
            await _context.SaveChangesAsync();
            return _mapper.Map<MstFloorDto>(floor);
        }

        public async Task<MstFloorDto> UpdateAsync(Guid id, MstFloorUpdateDto updateDto)
        {
            var floor = await _context.MstFloors.FindAsync(id);
            if (floor == null)
                throw new KeyNotFoundException("Floor not found");

                 // Validasi tipe file
                if (!_allowedImageTypes.Contains(updateDto.FloorImage.ContentType))
                    throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

                // Validasi ukuran file
                if (updateDto.FloorImage.Length > MaxFileSize)
                    throw new ArgumentException("File size exceeds 2 MB limit.");

                // Tangani update gambar jika ada
            if (updateDto.FloorImage != null && updateDto.FloorImage.Length > 0)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "FloorImages");
                Directory.CreateDirectory(uploadDir);

                // Hapus file lama jika ada (opsional)
                if (!string.IsNullOrEmpty(floor.FloorImage))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), floor.FloorImage.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                        File.Delete(oldFilePath);
                }

                // Simpan file baru
                var fileName = $"{Guid.NewGuid()}_{updateDto.FloorImage.FileName}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await updateDto.FloorImage.CopyToAsync(stream);
                }

                floor.FloorImage = $"/Uploads/FloorImages/{fileName}";
            }

             floor.UpdatedBy = "";  
            _mapper.Map(updateDto, floor);
           

            // _context.MstFloors.Update(floor);
            await _context.SaveChangesAsync();
            return _mapper.Map<MstFloorDto>(floor);
        }

        public async Task DeleteAsync(Guid id)
        {
            var floor = await _context.MstFloors.FindAsync(id);
            if (floor == null)
                throw new KeyNotFoundException("Floor not found");

            floor.Status = 0;
            // _context.MstFloors.Remove(floor);
            await _context.SaveChangesAsync();
        }
    }
}
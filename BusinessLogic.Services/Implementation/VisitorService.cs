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
    public class VisitorService : IVisitorService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;

         private readonly string[] _allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png" }; //tipe gambar

        private const long MaxFileSize = 5 * 1024 * 1024; // max 5mb

        public VisitorService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<VisitorDto> CreateVisitorAsync(VisitorCreateDto createDto)
        {

            var visitor = _mapper.Map<Visitor>(createDto);
            if (createDto == null) throw new ArgumentNullException(nameof(createDto));

            if (!await _context.MstApplications.AnyAsync(f => f.Id == createDto.ApplicationId))
                throw new ArgumentException($"Application with ID {createDto.ApplicationId} not found.");

          
            if (createDto.FaceImage != null && createDto.FaceImage.Length > 0)
                {  
                try{
                    // extensi file yang diterima
                    if(!_allowedImageTypes.Contains(createDto.FaceImage.ContentType))
                        throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");
                    
                    // Validasi ukuran file
                    if (createDto.FaceImage.Length > MaxFileSize)
                        throw new ArgumentException("File size exceeds 5 MB limit.");

                    // folder penyimpanan di lokal server
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
                    Directory.CreateDirectory(uploadDir); // akan membuat directory jika belum ada

                    // buat nama file unik
                    var fileName = $"{Guid.NewGuid()}_{createDto.FaceImage.FileName}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await createDto.FaceImage.CopyToAsync(stream);
                        }
                    
                    visitor.FaceImage = $"/Uploads/visitorFaceImages/{fileName}";
                    visitor.UploadFr = 1; // Sukses
                    visitor.UploadFrError = "Upload successful"; 
                }
                catch (Exception ex)
                {
                    visitor.UploadFr = 2;
                    visitor.UploadFrError = ex.Message;
                    visitor.FaceImage = "";
                }
                }
                else
            {
                visitor.UploadFr = 0;
                visitor.UploadFrError = "No file uploaded";
                visitor.FaceImage = "";
            }
            
            visitor.Id = Guid.NewGuid();
            visitor.RegisteredDate = DateTime.UtcNow;
            visitor.CheckinBy ??= "";
            visitor.CheckoutBy ??= "";
            visitor.DenyBy ??= "";
            visitor.BlockBy ??= "";
            visitor.UnblockBy ??= "";
            visitor.ReasonDeny ??= "";
            visitor.ReasonBlock ??= "";
            visitor.ReasonUnblock ??= "";
            

            _context.Visitors.Add(visitor);
            await _context.SaveChangesAsync();
            return _mapper.Map<VisitorDto>(visitor);
        }

        public async Task<VisitorDto> GetVisitorByIdAsync(Guid id)
        {
            var visitor = await _context.Visitors
                .Include(v => v.Application)
                .FirstOrDefaultAsync(v => v.Id == id);
            return _mapper.Map<VisitorDto>(visitor);
        }

        public async Task<IEnumerable<VisitorDto>> GetAllVisitorsAsync()
        {
            var visitor = await _context.Visitors
                .Include(v => v.Application)
                .ToListAsync();
            return _mapper.Map<IEnumerable<VisitorDto>>(visitor);
        }

        public async Task<VisitorDto> UpdateVisitorAsync(Guid id, VisitorUpdateDto updateDto)
        {
            if (updateDto == null) throw new ArgumentNullException(nameof(updateDto));

            var visitor = await _context.Visitors.FindAsync(id);
            if (visitor == null)
            {
                throw new KeyNotFoundException($"Visitor with ID {id} not found.");
            }
            if (!await _context.MstApplications.AnyAsync(f => f.Id == updateDto.ApplicationId))
                throw new ArgumentException($"Application with ID {updateDto.ApplicationId} not found.");

             if (updateDto.FaceImage != null && updateDto.FaceImage.Length > 0)
                {  
                try{
                    if(!_allowedImageTypes.Contains(updateDto.FaceImage.ContentType))
                        throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");
                    
                    // Validasi ukuran file
                    if (updateDto.FaceImage.Length > MaxFileSize)
                        throw new ArgumentException("File size exceeds 5 MB limit.");

                    // folder penyimpanan di lokal server
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
                    Directory.CreateDirectory(uploadDir); // akan membuat directory jika belum ada

                    // buat nama file unik
                    var fileName = $"{Guid.NewGuid()}_{updateDto.FaceImage.FileName}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await updateDto.FaceImage.CopyToAsync(stream);
                        }
                    
                    visitor.FaceImage = $"/Uploads/visitorFaceImages/{fileName}";
                    visitor.UploadFr = 1; // Sukses
                    visitor.UploadFrError = "Upload successful"; 
                }
                catch (Exception ex)
                {
                    visitor.UploadFr = 2;
                    visitor.UploadFrError = ex.Message;
                    visitor.FaceImage = "";
                }
                }
                else
            {
                visitor.UploadFr = 0;
                visitor.UploadFrError = "No file uploaded";
                visitor.FaceImage = "";
            }     

            _mapper.Map(updateDto, visitor);
            await _context.SaveChangesAsync();
            return _mapper.Map<VisitorDto>(visitor);
        }

        public async Task DeleteVisitorAsync(Guid id)
        {
            var visitor = await _context.Visitors.FindAsync(id);
            if (visitor == null)
            {
                throw new KeyNotFoundException($"Visitor with ID {id} not found.");
            }

            _context.Visitors.Remove(visitor);
            await _context.SaveChangesAsync();
        }
    }
}
// using AutoMapper;
// using BusinessLogic.Services.Interface;
// using System;
// using System.Collections.Generic;
// using System.Security.Claims;
// using System.Threading.Tasks;
// using Data.ViewModels;
// using Entities.Models;
// using Microsoft.AspNetCore.Http;
// using Repositories.Repository;
// using CsvHelper;
// using CsvHelper.Configuration;
// using System.IO;
// using System.Globalization;
// using System.Linq;

// namespace BusinessLogic.Services.Implementation
// {
//     public class MstFloorService : IMstFloorService
//     {
//         private readonly MstFloorRepository _repository;
//         private readonly IMapper _mapper;
//         private readonly string[] _allowedImageTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
//         private const long MaxFileSize = 1 * 1024 * 1024; // Maksimal 1 MB
//         private readonly IHttpContextAccessor _httpContextAccessor;

//         public MstFloorService(MstFloorRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
//         {
//             _repository = repository;
//             _mapper = mapper;
//             _httpContextAccessor = httpContextAccessor;
//         }

//         public async Task<MstFloorDto> GetByIdAsync(Guid id)
//         {
//             var floor = await _repository.GetByIdAsync(id);
//             return floor == null ? null : _mapper.Map<MstFloorDto>(floor);
//         }

//         public async Task<IEnumerable<MstFloorDto>> GetAllAsync()
//         {
//             var floors = await _repository.GetAllAsync();
//             return _mapper.Map<IEnumerable<MstFloorDto>>(floors);
//         }

//         public async Task<MstFloorDto> CreateAsync(MstFloorCreateDto createDto)
//         {
//             var floor = _mapper.Map<MstFloor>(createDto);

//             if (createDto.FloorImage != null && createDto.FloorImage.Length > 0)
//             {
//                 if (string.IsNullOrEmpty(createDto.FloorImage.ContentType) || !_allowedImageTypes.Contains(createDto.FloorImage.ContentType))
//                     throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

//                 if (createDto.FloorImage.Length > MaxFileSize)
//                     throw new ArgumentException("File size exceeds 1 MB limit.");

//                 var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "FloorImages");
//                 Directory.CreateDirectory(uploadDir);

//                 var fileExtension = Path.GetExtension(createDto.FloorImage.FileName)?.ToLower() ?? ".jpg";
//                 var fileName = $"{Guid.NewGuid()}{fileExtension}";
//                 var filePath = Path.Combine(uploadDir, fileName);

//                 try
//                 {
//                     using (var stream = new FileStream(filePath, FileMode.Create))
//                     {
//                         await createDto.FloorImage.CopyToAsync(stream);
//                     }
//                 }
//                 catch (IOException ex)
//                 {
//                     throw new IOException("Failed to save image file.", ex);
//                 }

//                 floor.FloorImage = $"/Uploads/FloorImages/{fileName}";
//             }

//             var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
//             floor.Id = Guid.NewGuid();
//             floor.Status = 1;
//             floor.CreatedBy = username;
//             floor.CreatedAt = DateTime.UtcNow;
//             floor.UpdatedBy = username;
//             floor.UpdatedAt = DateTime.UtcNow;

//             await _repository.AddAsync(floor);
//             return _mapper.Map<MstFloorDto>(floor);
//         }

//         public async Task<MstFloorDto> UpdateAsync(Guid id, MstFloorUpdateDto updateDto)
//         {
//             var floor = await _repository.GetByIdAsync(id);
//             if (floor == null)
//                 throw new KeyNotFoundException("Floor not found");

//             if (updateDto.FloorImage != null && updateDto.FloorImage.Length > 0)
//             {
//                 if (string.IsNullOrEmpty(updateDto.FloorImage.ContentType) || !_allowedImageTypes.Contains(updateDto.FloorImage.ContentType))
//                     throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

//                 if (updateDto.FloorImage.Length > MaxFileSize)
//                     throw new ArgumentException("File size exceeds 1 MB limit.");

//                 var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "FloorImages");
//                 Directory.CreateDirectory(uploadDir);

//                 if (!string.IsNullOrEmpty(floor.FloorImage))
//                 {
//                     var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), floor.FloorImage.TrimStart('/'));
//                     if (File.Exists(oldFilePath))
//                     {
//                         try
//                         {
//                             File.Delete(oldFilePath);
//                         }
//                         catch (IOException ex)
//                         {
//                             throw new IOException("Failed to delete old image file.", ex);
//                         }
//                     }
//                 }

//                 var fileExtension = Path.GetExtension(updateDto.FloorImage.FileName)?.ToLower() ?? ".jpg";
//                 var fileName = $"{Guid.NewGuid()}{fileExtension}";
//                 var filePath = Path.Combine(uploadDir, fileName);

//                 try
//                 {
//                     using (var stream = new FileStream(filePath, FileMode.Create))
//                     {
//                         await updateDto.FloorImage.CopyToAsync(stream);
//                     }
//                 }
//                 catch (IOException ex)
//                 {
//                     throw new IOException("Failed to save image file.", ex);
//                 }

//                 floor.FloorImage = $"/Uploads/FloorImages/{fileName}";
//             }

//             var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
//             floor.UpdatedBy = username;
//             floor.UpdatedAt = DateTime.UtcNow;

//             _mapper.Map(updateDto, floor);

//             await _repository.UpdateAsync(floor);
//             return _mapper.Map<MstFloorDto>(floor);
//         }

//         public async Task DeleteAsync(Guid id)
//         {
//             var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
//             var floor = await _repository.GetByIdAsync(id);
//             if (floor == null)
//                 throw new KeyNotFoundException("Floor not found");

//             floor.UpdatedBy = username;
//             await _repository.SoftDeleteAsync(id);
//         }

//         public async Task<ImportResultDto>ImportAsync(IFormFile file)
//         {
//             var result = new ImportResultDto
//             {
//                 Success = false,
//                 ProcessedRows = 0,
//                 SuccessfulRows = 0,
//                 Msg = string.Empty,
//                 Code = 400
//             };

//             try
//             {
//                 // Validasi file
//                 if (file == null || file.Length == 0)
//                 {
//                     result.Msg = "File is empty or not provided.";
//                     return result;
//                 }

//                 if (!file.FileName.EndsWith(".csv"))
//                 {
//                     result.Msg = "Only CSV files are supported.";
//                     return result;
//                 }

//                 // Baca CSV
//                 var records = new List<MstFloorImportDto>();
//                 using (var stream = file.OpenReadStream())
//                 using (var reader = new StreamReader(stream))
//                 using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
//                 {
//                     HasHeaderRecord = true,
//                     Delimiter = ",",
//                     MissingFieldFound = null
//                 }))
//                 {
//                     records = csv.GetRecords<MstFloorImportDto>().ToList();
//                     result.ProcessedRows = records.Count;
//                 }

//                 var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
//                 var errors = new List<string>();

//                 // Validasi dan simpan data
//                 foreach (var record in records)
//                 {
//                     var rowNumber = records.IndexOf(record) + 2; // +2 untuk header dan 1-based indexing

//                     // Validasi BuildingId
//                     if (!Guid.TryParse(record.BuildingId, out var buildingId))
//                     {
//                         errors.Add($"Invalid BuildingId format at row {rowNumber}.");
//                         continue;
//                     }

//                     var building = await _repository.GetBuildingByIdAsync(buildingId);
//                     if (building == null)
//                     {
//                         errors.Add($"BuildingId {record.BuildingId} not found at row {rowNumber}.");
//                         continue;
//                     }

//                     // Validasi Name
//                     if (string.IsNullOrWhiteSpace(record.Name))
//                     {
//                         errors.Add($"Name is required at row {rowNumber}.");
//                         continue;
//                     }

//                     // Validasi duplikasi Name dalam BuildingId
//                     if (await _repository.NameExistsAsync(buildingId, record.Name))
//                     {
//                         errors.Add($"Floor name {record.Name} already exists for BuildingId {record.BuildingId} at row {rowNumber}.");
//                         continue;
//                     }

//                     // Validasi MeterPerPx
//                     if (record.MeterPerPx <= 0)
//                     {
//                         errors.Add($"MeterPerPx must be positive at row {rowNumber}.");
//                         continue;
//                     }

//                     // Validasi Status
//                     if (record.Status.HasValue && record.Status != 1 && record.Status != 0)
//                     {
//                         errors.Add($"Status must be 0 or 1 at row {rowNumber}.");
//                         continue;
//                     }

//                     // Validasi PixelX, PixelY, FloorX, FloorY
//                     if (record.PixelX <= 0)
//                         errors.Add($"PixelX must be positive at row {rowNumber}.");
//                     if (record.PixelY <= 0)
//                         errors.Add($"PixelY must be positive at row {rowNumber}.");
//                     if (record.FloorX < 0)
//                         errors.Add($"FloorX cannot be negative at row {rowNumber}.");
//                     if (record.FloorY < 0)
//                         errors.Add($"FloorY cannot be negative at row {rowNumber}.");

//                     if (errors.Any(e => e.Contains($"row {rowNumber}")))
//                         continue;

//                     // Buat entity MstFloor
//                     var floor = new MstFloor
//                     {
//                         Id = Guid.NewGuid(),
//                         BuildingId = buildingId,
//                         Name = record.Name,
//                         FloorImage = record.FloorImage ?? string.Empty,
//                         PixelX = record.PixelX,
//                         PixelY = record.PixelY,
//                         FloorX = record.FloorX,
//                         FloorY = record.FloorY,
//                         MeterPerPx = record.MeterPerPx,
//                         EngineFloorId = record.EngineFloorId,
//                         CreatedBy = record.CreatedBy ?? username,
//                         CreatedAt = DateTime.UtcNow,
//                         UpdatedBy = record.CreatedBy ?? username,
//                         UpdatedAt = DateTime.UtcNow,
//                         Status = record.Status ?? 1
//                     };

//                     // Simpan ke database
//                     await _repository.AddAsync(floor);
//                     result.SuccessfulRows++;
//                 }

//                 if (errors.Any())
//                 {
//                     result.Msg = $"Import completed with {errors.Count} errors: {string.Join("; ", errors)}";
//                 }
//                 else
//                 {
//                     result.Success = true;
//                     result.Msg = $"Imported {result.SuccessfulRows} floors successfully.";
//                     result.Code = 200;
//                 }
//             }
//             catch (Exception ex)
//             {
//                 result.Msg = $"Internal server error: {ex.Message}";
//                 result.Code = 500;
//             }

//             return result;
//         }
//     }
// }
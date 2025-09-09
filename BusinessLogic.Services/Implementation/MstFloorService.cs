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
using System.IO;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;


namespace BusinessLogic.Services.Implementation
{
    public class MstFloorService : IMstFloorService
    {
        private readonly MstFloorRepository _repository;
        private readonly IMapper _mapper;
        private readonly string[] _allowedImageTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
        private const long MaxFileSize = 1 * 1024 * 1024; // Maksimal 1 MB
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FloorplanMaskedAreaRepository _maskedAreaRepository;
        private readonly MstFloorplanRepository _floorplanRepository;
        private readonly IMstFloorplanService _floorplanService;

        public MstFloorService(
                                        MstFloorRepository repository,
                                        IMapper mapper,
                                        IHttpContextAccessor httpContextAccessor,
                                        FloorplanMaskedAreaRepository maskedAreaRepository,
                                        MstFloorplanRepository floorplanRepository,
                                        IMstFloorplanService floorplanService)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _maskedAreaRepository = maskedAreaRepository;
            _floorplanRepository = floorplanRepository;
            _floorplanService = floorplanService;
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

                public async Task<IEnumerable<OpenMstFloorDto>> OpenGetAllAsync()
        {
            var floors = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<OpenMstFloorDto>>(floors);
        }


        public async Task<MstFloorDto> CreateAsync(MstFloorCreateDto createDto)
        {
            var floor = _mapper.Map<MstFloor>(createDto);

            if (createDto.FloorImage != null && createDto.FloorImage.Length > 0)
            {
                if (string.IsNullOrEmpty(createDto.FloorImage.ContentType) || !_allowedImageTypes.Contains(createDto.FloorImage.ContentType))
                    throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

                if (createDto.FloorImage.Length > MaxFileSize)
                    throw new ArgumentException("File size exceeds 1 MB limit.");

                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "FloorImages");
                Directory.CreateDirectory(uploadDir);

                var fileExtension = Path.GetExtension(createDto.FloorImage.FileName)?.ToLower() ?? ".jpg";
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadDir, fileName);

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

                floor.FloorImage = $"/Uploads/FloorImages/{fileName}";
            }

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            floor.Id = Guid.NewGuid();
            floor.Status = 1;
            floor.CreatedBy = username ?? "Annonymous";
            floor.CreatedAt = DateTime.UtcNow;
            floor.UpdatedBy = username ?? "Annonymous";
            floor.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(floor);
            return _mapper.Map<MstFloorDto>(floor);
        }

        //   public async Task UpdateAsync(Guid id, MstDistrictUpdateDto updateDto)
        // {
        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        //     var district = await _repository.GetByIdAsync(id);
        //     if (district == null)
        //         throw new KeyNotFoundException("District not found");
        //     district.UpdatedAt = DateTime.UtcNow;
        //     district.UpdatedBy = username;
        //     _mapper.Map(updateDto, district);

        //     await _repository.UpdateAsync(district);
        // }
        
        

        public async Task UpdateAsync(Guid id, MstFloorUpdateDto updateDto)
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

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            floor.UpdatedBy = username;
            floor.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(updateDto, floor);

            await _repository.UpdateAsync(floor);

        }

        // public async Task DeleteAsync(Guid id)
        // {
        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        //     var floor = await _repository.GetByIdAsync(id);
        //     if (floor == null)
        //         throw new KeyNotFoundException("Floor not found");


        //     floor.UpdatedBy = username;
        //     floor.UpdatedAt = DateTime.UtcNow;
        //     floor.Status = 0;
        //     await _repository.SoftDeleteAsync(id);
        // }

        // public async Task DeleteAsync(Guid id)
        // {
        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        //     var floor = await _repository.GetByIdAsync(id);
        //     if (floor == null)
        //         throw new KeyNotFoundException("Floor not found");

        //     using var transaction = await _repository.BeginTransactionAsync();
        //     try
        //     {
        //         // Soft delete parent
        //         floor.UpdatedBy = username;
        //         floor.UpdatedAt = DateTime.UtcNow;
        //         floor.Status = 0;
        //         await _repository.SoftDeleteAsync(id);

        //         // Soft delete child entities via their repositories
        //         var maskedAreas = await _maskedAreaRepository.GetByFloorIdAsync(id);
        //         foreach (var maskedArea in maskedAreas)
        //         {
        //             maskedArea.UpdatedBy = username;
        //             maskedArea.UpdatedAt = DateTime.UtcNow;
        //             maskedArea.Status = 0;
        //             await _maskedAreaRepository.SoftDeleteAsync(maskedArea.Id);
        //         }

        //         var floorplans = await _floorplanRepository.GetByFloorIdAsync(id);
        //         foreach (var floorplan in floorplans)
        //         {
        //             floorplan.UpdatedBy = username;
        //             floorplan.UpdatedAt = DateTime.UtcNow;
        //             floorplan.Status = 0;
        //             await _floorplanRepository.DeleteAsync(floorplan.Id);
        //         }

        //         // await _repository.BeginTransactionAsync();
        //         await transaction.CommitAsync();
        //     }
        //     catch
        //     {
        //         await transaction.RollbackAsync();
        //         throw;
        //     }
        // }
        
    public async Task DeleteAsync(Guid id)
    {
        var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var floor = await _repository.GetByIdAsync(id);
        if (floor == null)
            throw new KeyNotFoundException("Floor not found");

        await _repository.ExecuteInTransactionAsync(async () =>
        {
            var floorplans = await _floorplanRepository.GetByFloorIdAsync(id);
            foreach (var floorplan in floorplans)
            {
                await _floorplanService.DeleteAsync(floorplan.Id);
            }
            floor.UpdatedBy = username;
            floor.UpdatedAt = DateTime.UtcNow;
            floor.Status = 0;
            await _repository.SoftDeleteAsync(id);
        });
    }

        public async Task<IEnumerable<MstFloorDto>> ImportAsync(IFormFile file)
        {
            var floors = new List<MstFloor>();
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // skip header

            int rowNumber = 2; // start dari baris ke 2
            foreach (var row in rows)
            {
                // validasi
                var buildingIdStr = row.Cell(1).GetValue<string>();
                if (!Guid.TryParse(buildingIdStr, out var buildingId))
                    throw new ArgumentException($"Invalid BuildingId format at row {rowNumber}");

                var building = await _repository.GetBuildingByIdAsync(buildingId);
                if (building == null)
                    throw new ArgumentException($"BuildingId {buildingId} not found at row {rowNumber}");

                var floor = new MstFloor
                {
                    Id = Guid.NewGuid(),
                    BuildingId = buildingId,
                    Name = row.Cell(2).GetValue<string>(),
                    FloorImage = row.Cell(3).GetValue<string>() ?? "",
                    PixelX = row.Cell(4).GetValue<long>(),
                    PixelY = row.Cell(5).GetValue<long>(),
                    FloorX = row.Cell(6).GetValue<long>(),
                    FloorY = row.Cell(7).GetValue<long>(),
                    MeterPerPx = row.Cell(8).GetValue<float>(),
                    EngineFloorId = row.Cell(9).GetValue<long>(),
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                };

                floors.Add(floor);
                rowNumber++;
            }

            foreach (var floor in floors)
            {
                await _repository.AddAsync(floor);
            }

            return _mapper.Map<IEnumerable<MstFloorDto>>(floors);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name", "Building.Name" };
            var validSortColumns = new[] { "Name", "CreatedAt", "UpdatedAt", "Status", "Building.Name", "EngineFloorId" };

            var filterService = new GenericDataTableService<MstFloor, MstFloorDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }



        public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var floors = await _repository.GetAllAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Floor Report")
                        .SemiBold().FontSize(16).FontColor(Colors.Black).AlignCenter();

                    page.Content().Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);   // No.
                            columns.RelativeColumn(2); // Building Name
                            columns.RelativeColumn(2); // Building Id
                            columns.RelativeColumn(2); // Floor Name
                            columns.RelativeColumn(1); // EngineFloorId
                            columns.RelativeColumn(1); // MeterPerPx
                            columns.RelativeColumn(2); // CreatedAt
                            columns.RelativeColumn(2); // CreatedBy
                        });

                        // Table header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Building").SemiBold();
                            header.Cell().Element(CellStyle).Text("BuildingId").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("EngineFloorId").SemiBold();
                            header.Cell().Element(CellStyle).Text("Meter/Px").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created At").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created By").SemiBold();
                        });

                        // Table body
                        int index = 1;
                        foreach (var floor in floors)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(floor.Building?.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(floor.BuildingId.ToString() ?? "-");
                            table.Cell().Element(CellStyle).Text(floor.Name);
                            table.Cell().Element(CellStyle).Text(floor.EngineFloorId.ToString());
                            table.Cell().Element(CellStyle).Text(floor.MeterPerPx.ToString("0.00"));
                            table.Cell().Element(CellStyle).Text(floor.CreatedAt.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(floor.CreatedBy ?? "-");
                        }

                        static IContainer CellStyle(IContainer container) =>
                            container
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(4)
                                .PaddingHorizontal(6);
                    });

                    page.Footer()
                        .AlignRight()
                        .Text(txt =>
                        {
                            txt.Span("Generated at: ").SemiBold();
                            txt.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
                        });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> ExportExcelAsync()
        {
            var floors = await _repository.GetAllExportAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Floors");

            // Header
            worksheet.Cell(1, 1).Value = "No";
            worksheet.Cell(1, 2).Value = "Building";
            worksheet.Cell(1, 3).Value = "BuildingId";
            worksheet.Cell(1, 4).Value = "Floor Name";
            worksheet.Cell(1, 5).Value = "Engine Floor ID";
            worksheet.Cell(1, 6).Value = "Meter/Px";
            worksheet.Cell(1, 7).Value = "Created By";
            worksheet.Cell(1, 8).Value = "Created At";
            worksheet.Cell(1, 9).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var floor in floors)
            {
                await _repository.GetAllExportAsync();

                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = floor.Building?.Name ?? "-";
                worksheet.Cell(row, 3).Value = floor.BuildingId.ToString();
                worksheet.Cell(row, 4).Value = floor.Name;
                worksheet.Cell(row, 5).Value = floor.EngineFloorId;
                worksheet.Cell(row, 6).Value = floor.MeterPerPx;
                worksheet.Cell(row, 7).Value = floor.CreatedBy;
                worksheet.Cell(row, 8).Value = floor.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cell(row, 9).Value = floor.Status == 1 ? "Active" : "Inactive";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }


    }

}




using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using System.ComponentModel.DataAnnotations;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services.Implementation
{
    public class MstFloorplanService : IMstFloorplanService
    {
        private readonly MstFloorplanRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MstFloorplanService(
            MstFloorplanRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MstFloorplanDto> GetByIdAsync(Guid id)
        {
            var floorplan = await _repository.GetByIdAsync(id);
            return floorplan == null ? null : _mapper.Map<MstFloorplanDto>(floorplan);
        }

        public async Task<IEnumerable<MstFloorplanDto>> GetAllAsync()
        {
            var floorplans = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstFloorplanDto>>(floorplans);
        }

        public async Task<MstFloorplanDto> CreateAsync(MstFloorplanCreateDto createDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var floorplan = _mapper.Map<MstFloorplan>(createDto);
            floorplan.Id = Guid.NewGuid();
            floorplan.Status = 1;
            floorplan.CreatedBy = username;
            floorplan.UpdatedBy = username;
            floorplan.CreatedAt = DateTime.UtcNow;
            floorplan.UpdatedAt = DateTime.UtcNow;

            var createdFloorplan = await _repository.AddAsync(floorplan);
            return _mapper.Map<MstFloorplanDto>(createdFloorplan);
        }

        public async Task UpdateAsync(Guid id, MstFloorplanUpdateDto updateDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var floorplan = await _repository.GetByIdAsync(id);
            if (floorplan == null)
                throw new KeyNotFoundException("Floorplan not found");

            floorplan.UpdatedBy = username;
            floorplan.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(updateDto, floorplan);
            await _repository.UpdateAsync(floorplan);
        }

        public async Task DeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var floorplan = await _repository.GetByIdAsync(id);
            if (floorplan == null)
                throw new KeyNotFoundException("Floorplan not found");

            floorplan.UpdatedBy = username;
            floorplan.UpdatedAt = DateTime.UtcNow;
            floorplan.Status = 0;
            await _repository.DeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name", "Floor.Name" };
            var validSortColumns = new[] { "Name", "CreatedAt", "UpdatedAt", "Floor.Name", "Status", "MaskedAreaCount", "DeviceCount" };
            var enumColumns = new Dictionary<string, Type> { { "Status", typeof(int) } };

            var filterService = new GenericDataTableService<MstFloorplan, MstFloorplanDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns,
                enumColumns);

            return await filterService.FilterAsync(request);
        }

        public async Task<IEnumerable<MstFloorplanDto>> ImportAsync(IFormFile file)
        {
            var floorplans = new List<MstFloorplan>();
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var userApplicationId = _httpContextAccessor.HttpContext?.User.FindFirst("ApplicationId")?.Value;

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // Lewati header

            int rowNumber = 2;
            foreach (var row in rows)
            {
                var floorIdStr = row.Cell(2).GetValue<string>();
                if (!Guid.TryParse(floorIdStr, out var floorId))
                    throw new ArgumentException($"Invalid FloorId format at row {rowNumber}");

                var floor = await _repository.GetFloorByIdAsync(floorId);
                if (floor == null)
                    throw new ArgumentException($"FloorId {floorId} not found at row {rowNumber}");

                var name = row.Cell(2).GetValue<string>();
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException($"Name is required at row {rowNumber}");

                var floorplan = new MstFloorplan
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    FloorId = floorId,
                    ApplicationId = Guid.Parse(userApplicationId),
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                };

                floorplans.Add(floorplan);
                rowNumber++;
            }

            foreach (var floorplan in floorplans)
            {
                await _repository.AddAsync(floorplan);
            }

            return _mapper.Map<IEnumerable<MstFloorplanDto>>(floorplans);
        }

        public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var floorplans = await _repository.GetAllExportAsync();
            var dtos = _mapper.Map<IEnumerable<MstFloorplanDto>>(floorplans);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Floorplan Report")
                        .SemiBold().FontSize(16).FontColor(Colors.Black).AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Floor").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Masked Areas").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created At").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created By").SemiBold();
                        });

                        int index = 1;
                        foreach (var dto in dtos)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(dto.Floor.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(dto.Name);
                            table.Cell().Element(CellStyle).Text(dto.MaskedAreaCount.ToString());
                            table.Cell().Element(CellStyle).Text(dto.CreatedAt.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(dto.CreatedBy ?? "-");
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
            var floorplans = await _repository.GetAllExportAsync();
            var dtos = _mapper.Map<IEnumerable<MstFloorplanDto>>(floorplans);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Floorplans");

            worksheet.Cell(1, 1).Value = "No";
            worksheet.Cell(1, 2).Value = "Floor";
            worksheet.Cell(1, 3).Value = "Name";
            worksheet.Cell(1, 4).Value = "Masked Areas";
            worksheet.Cell(1, 5).Value = "Created By";
            worksheet.Cell(1, 6).Value = "Created At";
            worksheet.Cell(1, 7).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var dto in dtos)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = dto.Floor.Name ?? "-";
                worksheet.Cell(row, 3).Value = dto.Name;
                worksheet.Cell(row, 4).Value = dto.MaskedAreaCount;
                worksheet.Cell(row, 5).Value = dto.CreatedBy;
                worksheet.Cell(row, 6).Value = dto.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cell(row, 7).Value = dto.Status == 1 ? "Active" : "Inactive";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

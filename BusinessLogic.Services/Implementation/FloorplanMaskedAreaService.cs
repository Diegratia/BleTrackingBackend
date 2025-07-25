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
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using Helpers.Consumer;

namespace BusinessLogic.Services.Implementation
{
    public class FloorplanMaskedAreaService : IFloorplanMaskedAreaService
    {
        private readonly FloorplanMaskedAreaRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FloorplanMaskedAreaService(FloorplanMaskedAreaRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<FloorplanMaskedAreaDto> GetByIdAsync(Guid id)
        {
            var area = await _repository.GetByIdAsync(id);
            return area == null ? null : _mapper.Map<FloorplanMaskedAreaDto>(area);
        }

        public async Task<IEnumerable<FloorplanMaskedAreaDto>> GetAllAsync()
        {
            var areas = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<FloorplanMaskedAreaDto>>(areas);
        }

        public async Task<FloorplanMaskedAreaDto> CreateAsync(FloorplanMaskedAreaCreateDto createDto)
        {
            var floor = await _repository.GetFloorByIdAsync(createDto.FloorId);
            if (floor == null)
                throw new ArgumentException($"Floor with ID {createDto.FloorId} not found.");

            var area = _mapper.Map<FloorplanMaskedArea>(createDto);

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            area.Id = Guid.NewGuid();
            area.Status = 1;
            area.CreatedBy = username;
            area.CreatedAt = DateTime.UtcNow;
            area.UpdatedBy = username;
            area.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(area);
            return _mapper.Map<FloorplanMaskedAreaDto>(area);
        }

        public async Task UpdateAsync(Guid id, FloorplanMaskedAreaUpdateDto updateDto)
        {
            var floor = await _repository.GetFloorByIdAsync(updateDto.FloorId);
            if (floor == null)
                throw new ArgumentException($"Floor with ID {updateDto.FloorId} not found.");

            var area = await _repository.GetByIdAsync(id);
            if (area == null)
                throw new KeyNotFoundException("Area not found");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            area.UpdatedBy = username;
            area.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(updateDto, area);
            await _repository.UpdateAsync(area);
        }

        public async Task DeleteAsync(Guid id)
        {
            var area = await _repository.GetByIdAsync(id);
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            area.UpdatedBy = username;
            await _repository.SoftDeleteAsync(id);
        }

        public async Task<IEnumerable<FloorplanMaskedAreaDto>> ImportAsync(IFormFile file)
        {
            var areas = new List<FloorplanMaskedArea>();
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var userApplicationId = _httpContextAccessor.HttpContext?.User.FindFirst("ApplicationId")?.Value;

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1);

            int rowNumber = 2; 
            foreach (var row in rows)
            {

                var floorplanStr = row.Cell(1).GetValue<string>();
                if (!Guid.TryParse(floorplanStr, out var floorplanId))
                    throw new ArgumentException($"Invalid floorplanId format at row {rowNumber}");

                var floorplan = await _repository.GetFloorplanByIdAsync(floorplanId);
                if (floorplan == null)
                    throw new ArgumentException($"floorplanId {floorplanId} not found at row {rowNumber}");

                var floorStr = row.Cell(2).GetValue<string>();
                if (!Guid.TryParse(floorStr, out var floorId))
                    throw new ArgumentException($"Invalid FloorId format at row {rowNumber}");

                var floor = await _repository.GetFloorByIdAsync(floorId);
                if (floor == null)
                    throw new ArgumentException($"FloorId {floorId} not found at row {rowNumber}");


                var area = new FloorplanMaskedArea
                {
                    Id = Guid.NewGuid(),
                    FloorplanId = floorplanId,
                    FloorId = floorId,
                    Name = row.Cell(3).GetValue<string>(),
                    AreaShape = row.Cell(4).GetValue<string>(),
                    ColorArea = row.Cell(5).GetValue<string>(),
                    RestrictedStatus = (RestrictedStatus)Enum.Parse(typeof(RestrictedStatus), row.Cell(6).GetValue<string>()),
                    EngineAreaId = row.Cell(7).GetValue<string>(),
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                };

                areas.Add(area);
                rowNumber++;
            }

            // Simpan ke database
            foreach (var area in areas)
            {
                await _repository.AddAsync(area);
            }

            return _mapper.Map<IEnumerable<FloorplanMaskedAreaDto>>(areas);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name", "Floor.Name", "Floorplan.Name" };
            var validSortColumns = new[] { "Name", "Floor.Name", "Floorplan.Name", "CreatedAt", "UpdatedAt", "RestrictedStatus", "Status" };

            var filterService = new GenericDataTableService<FloorplanMaskedArea, FloorplanMaskedAreaDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
        public async Task<byte[]> ExportPdfAsync()
        {
            var areas = await _repository.GetAllExportAsync();

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Floorplan Masked Area Report")
                        .SemiBold().FontSize(16).FontColor(Colors.Black).AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Floor").SemiBold();
                            header.Cell().Element(CellStyle).Text("Floorplan").SemiBold();
                            header.Cell().Element(CellStyle).Text("Restricted Status").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created At").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created By").SemiBold();
                        });

                        int index = 1;
                        foreach (var area in areas)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(area.Name);
                            table.Cell().Element(CellStyle).Text(area.Floor?.Name);
                            table.Cell().Element(CellStyle).Text(area.Floorplan?.Name);
                            table.Cell().Element(CellStyle).Text(area.RestrictedStatus.ToString());
                            table.Cell().Element(CellStyle).Text(area.CreatedAt.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(area.CreatedBy);
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
            var areas = await _repository.GetAllExportAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Masked Areas");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Floor";
            worksheet.Cell(1, 4).Value = "Floorplan";
            worksheet.Cell(1, 5).Value = "Restricted Status";
            worksheet.Cell(1, 6).Value = "Created At";
            worksheet.Cell(1, 7).Value = "Created By";

            int row = 2;
            int no = 1;

            foreach (var area in areas)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = area.Name;
                worksheet.Cell(row, 3).Value = area.Floor?.Name;
                worksheet.Cell(row, 4).Value = area.Floorplan?.Name;
                worksheet.Cell(row, 5).Value = area.RestrictedStatus.ToString();
                worksheet.Cell(row, 6).Value = area.CreatedAt;
                worksheet.Cell(row, 7).Value = area.CreatedBy;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
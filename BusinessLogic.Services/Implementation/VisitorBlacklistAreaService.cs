using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BusinessLogic.Services.Implementation
{
    public class VisitorBlacklistAreaService : IVisitorBlacklistAreaService
    {
        private readonly VisitorBlacklistAreaRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public VisitorBlacklistAreaService(VisitorBlacklistAreaRepository repository, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<VisitorBlacklistAreaDto> CreateVisitorBlacklistAreaAsync(VisitorBlacklistAreaCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

           var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            var entity = _mapper.Map<VisitorBlacklistArea>(dto);
            entity.Id = Guid.NewGuid();
            entity.Status = 1;
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.CreatedBy = username;
            entity.CreatedAt = DateTime.UtcNow;

            await _repository.AddAsync(entity);
            return _mapper.Map<VisitorBlacklistAreaDto>(entity);
        }

        public async Task<VisitorBlacklistAreaDto> GetVisitorBlacklistAreaByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return _mapper.Map<VisitorBlacklistAreaDto>(entity);
        }

        public async Task<IEnumerable<VisitorBlacklistAreaDto>> GetAllVisitorBlacklistAreasAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<VisitorBlacklistAreaDto>>(entities);
        }

        public async Task UpdateVisitorBlacklistAreaAsync(Guid id, VisitorBlacklistAreaUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
             var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"VisitorBlacklistArea with ID {id} not found.");
            
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(dto, entity);
            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteVisitorBlacklistAreaAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"VisitorBlacklistArea with ID {id} not found.");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repository.DeleteAsync(entity);
        }

         public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Visitor.Name", "FloorplanMaskedArea.Name" }; 
            var validSortColumns = new[] { "Visitor.Name", "FloorplanMaskedArea.Name" };

            var filterService = new GenericDataTableService<VisitorBlacklistArea, VisitorBlacklistAreaDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }

          public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var visitorBlacklistAreas = await _repository.GetAllAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Visitor Blacklist Area Report")
                        .SemiBold().FontSize(16).FontColor(Colors.Black).AlignCenter();

                    page.Content().Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        // Table header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Visitor").SemiBold();
                            header.Cell().Element(CellStyle).Text("Floorplan Masked Area").SemiBold();
                        });

                        // Table body
                        int index = 1;
                        foreach (var blacklistArea in visitorBlacklistAreas)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(blacklistArea.Visitor?.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(blacklistArea.FloorplanMaskedArea?.Name ?? "-");
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
            var visitorBlacklistAreas = await _repository.GetAllAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Visitor Blacklist Areas");

            // Header
            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Visitor";
            worksheet.Cell(1, 3).Value = "Floorplan Masked Area";

            int row = 2;
            int no = 1;

            foreach (var blacklistArea in visitorBlacklistAreas)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = blacklistArea.Visitor?.Name ?? "-";
                worksheet.Cell(row, 3).Value = blacklistArea.FloorplanMaskedArea?.Name ?? "-";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

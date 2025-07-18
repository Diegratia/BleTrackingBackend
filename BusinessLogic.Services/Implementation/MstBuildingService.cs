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
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;


namespace BusinessLogic.Services.Implementation
{
    public class MstBuildingService : IMstBuildingService
    {
        private readonly MstBuildingRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MstBuildingService(MstBuildingRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MstBuildingDto> GetByIdAsync(Guid id)
        {
            var building = await _repository.GetByIdAsync(id);
            return building == null ? null : _mapper.Map<MstBuildingDto>(building);
        }

        public async Task<IEnumerable<MstBuildingDto>> GetAllAsync()
        {
            var buildings = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstBuildingDto>>(buildings);
        }

        public async Task<MstBuildingDto> CreateAsync(MstBuildingCreateDto createDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var building = _mapper.Map<MstBuilding>(createDto);
            building.Id = Guid.NewGuid();
            building.CreatedBy = username;
            building.UpdatedBy = username;
            building.Status = 1;

            var createdBuilding = await _repository.AddAsync(building);
            return _mapper.Map<MstBuildingDto>(createdBuilding);
        }

        public async Task UpdateAsync(Guid id, MstBuildingUpdateDto updateDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var building = await _repository.GetByIdAsync(id);
            if (building == null)
                throw new KeyNotFoundException("Building not found");

            _mapper.Map(updateDto, building);
            building.UpdatedBy = username;

            await _repository.UpdateAsync(building);
        }

        public async Task DeleteAsync(Guid id)
        {
            var building = await _repository.GetByIdAsync(id);
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            building.UpdatedBy = username;
            await _repository.DeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "Name", "CreatedAt", "UpdatedAt", "Status" };

            var filterService = new GenericDataTableService<MstBuilding, MstBuildingDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
        public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var buildings = await _repository.GetAllExportAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Building Report")
                        .SemiBold().FontSize(16).FontColor(Colors.Black).AlignCenter();

                    page.Content().Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        // Table header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Image").SemiBold();
                            header.Cell().Element(CellStyle).Text("CreatedBy").SemiBold();
                            header.Cell().Element(CellStyle).Text("CreatedAt").SemiBold();
                            header.Cell().Element(CellStyle).Text("UpdatedBy").SemiBold();
                            header.Cell().Element(CellStyle).Text("UpdatedAt").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        // Table body
                        int index = 1;
                        foreach (var building in buildings)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(building.Name);
                            table.Cell().Element(CellStyle).Text(building.Image);
                            table.Cell().Element(CellStyle).Text(building.CreatedBy);
                            table.Cell().Element(CellStyle).Text(building.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(building.UpdatedBy);
                            table.Cell().Element(CellStyle).Text(building.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(building.Status.ToString());
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
            var buildings = await _repository.GetAllExportAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Buildings");

            // Header
            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Image";
            worksheet.Cell(1, 4).Value = "Created By";
            worksheet.Cell(1, 5).Value = "Created At";
            worksheet.Cell(1, 6).Value = "Updated By";
            worksheet.Cell(1, 7).Value = "Updated At";
            worksheet.Cell(1, 8).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var building in buildings)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = building.Name;
                worksheet.Cell(row, 3).Value = building.Image;
                worksheet.Cell(row, 4).Value = building.CreatedBy;
                worksheet.Cell(row, 5).Value = building.CreatedAt;
                worksheet.Cell(row, 6).Value = building.UpdatedBy;
                worksheet.Cell(row, 7).Value = building.UpdatedAt;
                worksheet.Cell(row, 8).Value = building.Status;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

}


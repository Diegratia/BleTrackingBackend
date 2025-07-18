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
    public class MstDepartmentService : IMstDepartmentService
    {
        private readonly MstDepartmentRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MstDepartmentService(MstDepartmentRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MstDepartmentDto> GetByIdAsync(Guid id)
        {
            var department = await _repository.GetByIdAsync(id);
            return department == null ? null : _mapper.Map<MstDepartmentDto>(department);
        }

        public async Task<IEnumerable<MstDepartmentDto>> GetAllAsync()
        {
            var departments = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstDepartmentDto>>(departments);
        }

        public async Task<MstDepartmentDto> CreateAsync(MstDepartmentCreateDto createDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var department = _mapper.Map<MstDepartment>(createDto);
            department.Id = Guid.NewGuid();
            department.CreatedBy = username;
            department.Status = 1;

            var createdDepartment = await _repository.AddAsync(department);
            return _mapper.Map<MstDepartmentDto>(createdDepartment);
        }

        public async Task UpdateAsync(Guid id, MstDepartmentUpdateDto updateDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var department = await _repository.GetByIdAsync(id);
            if (department == null)
                throw new KeyNotFoundException("Department not found");

            _mapper.Map(updateDto, department);
            department.UpdatedBy = username;

            await _repository.UpdateAsync(department);
        }

        public async Task DeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var department = await _repository.GetByIdAsync(id);
            department.UpdatedBy = username;
            await _repository.DeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "Name", "CreatedAt", "UpdatedAt", "Status" };

            var filterService = new GenericDataTableService<MstDepartment, MstDepartmentDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
         public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var districts = await _repository.GetAllExportAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Department Report")
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
                            columns.RelativeColumn(2);
                        });

                        // Table header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Code").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Department Host").SemiBold();
                            header.Cell().Element(CellStyle).Text("CreatedBy").SemiBold();
                            header.Cell().Element(CellStyle).Text("CreatedAt").SemiBold();
                            header.Cell().Element(CellStyle).Text("UpdatedBy").SemiBold();
                            header.Cell().Element(CellStyle).Text("UpdatedAt").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        // Table body
                        int index = 1;
                        foreach (var district in districts)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(district.Code);
                            table.Cell().Element(CellStyle).Text(district.Name);
                            table.Cell().Element(CellStyle).Text(district.DepartmentHost);
                            table.Cell().Element(CellStyle).Text(district.CreatedBy);
                            table.Cell().Element(CellStyle).Text(district.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(district.UpdatedBy);
                            table.Cell().Element(CellStyle).Text(district.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(district.Status.ToString());
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
            var districts = await _repository.GetAllExportAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Departments");

            // Header
            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Code";
            worksheet.Cell(1, 3).Value = "Name";
            worksheet.Cell(1, 4).Value = "Department Host";
            worksheet.Cell(1, 5).Value = "Created By";
            worksheet.Cell(1, 6).Value = "Created At";
            worksheet.Cell(1, 7).Value = "Updated By";
            worksheet.Cell(1, 8).Value = "Updated At";
            worksheet.Cell(1, 9).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var district in districts)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = district.Code;
                worksheet.Cell(row, 3).Value = district.Name;
                worksheet.Cell(row, 4).Value = district.DepartmentHost;
                worksheet.Cell(row, 5).Value = district.CreatedBy;
                worksheet.Cell(row, 6).Value = district.CreatedAt;
                worksheet.Cell(row, 7).Value = district.UpdatedBy;
                worksheet.Cell(row, 8).Value = district.UpdatedAt;
                worksheet.Cell(row, 9).Value = district.Status;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
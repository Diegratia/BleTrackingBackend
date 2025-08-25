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
    public class MstDistrictService : IMstDistrictService
    {
        private readonly MstDistrictRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MstDistrictService(MstDistrictRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MstDistrictDto> GetByIdAsync(Guid id)
        {
            var district = await _repository.GetByIdAsync(id);
            return district == null ? null : _mapper.Map<MstDistrictDto>(district);
        }

        public async Task<IEnumerable<MstDistrictDto>> GetAllAsync()
        {
            var districts = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstDistrictDto>>(districts);
        }

        public async Task<MstDistrictDto> CreateAsync(MstDistrictCreateDto createDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var district = _mapper.Map<MstDistrict>(createDto);

            district.Id = Guid.NewGuid();

            district.CreatedBy = username;
            district.UpdatedBy = username;
            district.CreatedAt = DateTime.UtcNow;
            district.UpdatedAt = DateTime.UtcNow;
            district.Status = 1;

            var createdDistrict = await _repository.AddAsync(district);
            return _mapper.Map<MstDistrictDto>(createdDistrict);
        }

        public async Task<List<MstDistrictDto>> CreateBatchAsync(List<MstDistrictCreateDto> dtos)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var result = new List<MstDistrictDto>();
            foreach (var dto in dtos)
            {
                var district = _mapper.Map<MstDistrict>(dto);
                district.Id = Guid.NewGuid();

                district.CreatedBy = username;
                district.UpdatedBy = username;
                district.CreatedAt = DateTime.UtcNow;
                district.UpdatedAt = DateTime.UtcNow;
                district.Status = 1;
                await _repository.AddAsync(district);
                result.Add( _mapper.Map<MstDistrictDto>(district));
            }
            return result;
        }

        public async Task UpdateAsync(Guid id, MstDistrictUpdateDto updateDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var district = await _repository.GetByIdAsync(id);
            if (district == null)
                throw new KeyNotFoundException("District not found");
            district.UpdatedAt = DateTime.UtcNow;
            district.UpdatedBy = username;
            _mapper.Map(updateDto, district);

            await _repository.UpdateAsync(district);
        }

        public async Task DeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var district = await _repository.GetByIdAsync(id);
            district.UpdatedAt = DateTime.UtcNow;
            district.UpdatedBy = username;
            await _repository.DeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "Name", "DistrictHost", "CreatedAt", "UpdatedAt", "Status" };

            var filterService = new GenericDataTableService<MstDistrict, MstDistrictDto>(
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
                        .Text("Master District Report")
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
                            header.Cell().Element(CellStyle).Text("DistrictHost").SemiBold();
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
                            table.Cell().Element(CellStyle).Text(district.DistrictHost);
                            table.Cell().Element(CellStyle).Text(district.CreatedBy);
                            table.Cell().Element(CellStyle).Text(district.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(district.UpdatedBy);
                            table.Cell().Element(CellStyle).Text(district.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(district.Status);
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
            var worksheet = workbook.Worksheets.Add("Districts");

            // Header
            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Code";
            worksheet.Cell(1, 3).Value = "Name";
            worksheet.Cell(1, 4).Value = "District Host";
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
                worksheet.Cell(row, 4).Value = district.DistrictHost;
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
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

namespace BusinessLogic.Services.Implementation
{
    public class MstAccessCctvService : IMstAccessCctvService
    {
        private readonly MstAccessCctvRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MstAccessCctvService(MstAccessCctvRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MstAccessCctvDto> GetByIdAsync(Guid id)
        {
            var accessCctv = await _repository.GetByIdAsync(id);
            return accessCctv == null ? null : _mapper.Map<MstAccessCctvDto>(accessCctv);
        }

        public async Task<IEnumerable<MstAccessCctvDto>> GetAllAsync()
        {
            var accessCctvs = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstAccessCctvDto>>(accessCctvs);
        }

        public async Task<MstAccessCctvDto> CreateAsync(MstAccessCctvCreateDto createDto)
        {
            var accessCctv = _mapper.Map<MstAccessCctv>(createDto);

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            accessCctv.Id = Guid.NewGuid();
            accessCctv.Status = 1;
            accessCctv.CreatedBy = username;
            accessCctv.CreatedAt = DateTime.UtcNow;
            accessCctv.UpdatedBy = username;
            accessCctv.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(accessCctv);
            return _mapper.Map<MstAccessCctvDto>(accessCctv);
        }

        public async Task UpdateAsync(Guid id, MstAccessCctvUpdateDto updateDto)
        {
            var accessCctv = await _repository.GetByIdAsync(id);
            if (accessCctv == null)
                throw new KeyNotFoundException("Access CCTV not found");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            accessCctv.UpdatedBy = username;
            accessCctv.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(updateDto, accessCctv);
            await _repository.UpdateAsync(accessCctv);
        }

        public async Task DeleteAsync(Guid id)
        {
            var accessCctv = await _repository.GetByIdAsync(id);
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            accessCctv.UpdatedBy = username;
            await _repository.SoftDeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name"};
            var validSortColumns = new[] { "Name", "CreatedAt", "UpdatedAt", "Status" };

            var filterService = new GenericDataTableService<MstAccessCctv, MstAccessCctvDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
            public async Task<byte[]> ExportPdfAsync()
        {
            var accessControls = await _repository.GetAllExportAsync();

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Access Control Report")
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
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("RTSP").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created At").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created By").SemiBold();
                        });

                        int index = 1;
                        foreach (var control in accessControls)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(control.Name);
                            table.Cell().Element(CellStyle).Text(control.Rtsp);
                            table.Cell().Element(CellStyle).Text(control.Status.ToString());
                            table.Cell().Element(CellStyle).Text(control?.CreatedAt.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(control?.CreatedBy);
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
            var accessControls = await _repository.GetAllExportAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Access Controls");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "RTSP";
            worksheet.Cell(1, 4).Value = "Status";
            worksheet.Cell(1, 5).Value = "Created At";
            worksheet.Cell(1, 6).Value = "Created By";

            int row = 2;
            int no = 1;

            foreach (var control in accessControls)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = control.Name;
                worksheet.Cell(row, 3).Value = control.Rtsp;
                worksheet.Cell(row, 5).Value = control.Status;
                worksheet.Cell(row, 6).Value = control.CreatedAt;
                worksheet.Cell(row, 7).Value = control.CreatedBy;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
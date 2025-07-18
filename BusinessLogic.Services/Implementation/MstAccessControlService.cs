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
    public class MstAccessControlService : IMstAccessControlService
    {
        private readonly MstAccessControlRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MstAccessControlService(MstAccessControlRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MstAccessControlDto> GetByIdAsync(Guid id)
        {
            var accessControl = await _repository.GetByIdAsync(id);
            return accessControl == null ? null : _mapper.Map<MstAccessControlDto>(accessControl);
        }

        public async Task<IEnumerable<MstAccessControlDto>> GetAllAsync()
        {
            var accessControls = await _repository
            .GetAllAsync();
            return _mapper.Map<IEnumerable<MstAccessControlDto>>(accessControls);
        }

        public async Task<MstAccessControlDto> CreateAsync(MstAccessControlCreateDto createDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var accessControl = _mapper.Map<MstAccessControl>(createDto);

            accessControl.Id = Guid.NewGuid();
            accessControl.Status = 1;
            accessControl.CreatedBy = username;
            accessControl.CreatedAt = DateTime.UtcNow;
            accessControl.UpdatedBy = username;
            accessControl.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(accessControl);
            return _mapper.Map<MstAccessControlDto>(accessControl);
        }

        public async Task UpdateAsync(Guid id, MstAccessControlUpdateDto updateDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var accessControl = await _repository.GetByIdAsync(id);
            if (accessControl == null)
                throw new KeyNotFoundException("Access Control not found");

            accessControl.UpdatedBy = username;
            accessControl.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(updateDto, accessControl);
            await _repository.UpdateAsync(accessControl);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.SoftDeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name", "Brand.Name", "Integration.Name" };
            var validSortColumns = new[] { "Name", "Brand.Name", "Type", "Channel", "DoorId", "Integration.Name", "CreatedAt", "UpdatedAt", "Status" };

            var filterService = new GenericDataTableService<MstAccessControl, MstAccessControlDto>(
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
                            header.Cell().Element(CellStyle).Text("Type").SemiBold();
                            header.Cell().Element(CellStyle).Text("Description").SemiBold();
                            header.Cell().Element(CellStyle).Text("Channel").SemiBold();
                            header.Cell().Element(CellStyle).Text("Door ID").SemiBold();
                            header.Cell().Element(CellStyle).Text("Raw").SemiBold();
                            header.Cell().Element(CellStyle).Text("Brand").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created At").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created By").SemiBold();
                        });

                        int index = 1;
                        foreach (var control in accessControls)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(control.Name);
                            table.Cell().Element(CellStyle).Text(control.Type);
                            table.Cell().Element(CellStyle).Text(control.Description);
                            table.Cell().Element(CellStyle).Text(control.Channel);
                            table.Cell().Element(CellStyle).Text(control.DoorId);
                            table.Cell().Element(CellStyle).Text(control.Raw);
                            table.Cell().Element(CellStyle).Text(control.Brand?.Name);
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
            worksheet.Cell(1, 3).Value = "Type";
            worksheet.Cell(1, 3).Value = "Description";
            worksheet.Cell(1, 4).Value = "Channel";
            worksheet.Cell(1, 5).Value = "Door ID";
            worksheet.Cell(1, 6).Value = "Raw";
            worksheet.Cell(1, 7).Value = "Brand";
            worksheet.Cell(1, 8).Value = "Status";
            worksheet.Cell(1, 9).Value = "Created At";
            worksheet.Cell(1, 10).Value = "Created By";

            int row = 2;
            int no = 1;

            foreach (var control in accessControls)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = control.Name;
                worksheet.Cell(row, 3).Value = control.Type;
                worksheet.Cell(row, 3).Value = control.Description;
                worksheet.Cell(row, 4).Value = control.Channel;
                worksheet.Cell(row, 5).Value = control.DoorId;
                worksheet.Cell(row, 6).Value = control.Raw;
                worksheet.Cell(row, 7).Value = control.Brand?.Name;
                worksheet.Cell(row, 8).Value = control.Status;
                worksheet.Cell(row, 9).Value = control.CreatedAt;
                worksheet.Cell(row, 10).Value = control.CreatedBy;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

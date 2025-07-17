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
    public class BleReaderNodeService : IBleReaderNodeService
    {
        private readonly BleReaderNodeRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BleReaderNodeService(
            BleReaderNodeRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BleReaderNodeDto> GetByIdAsync(Guid id)
        {
            var bleReaderNode = await _repository.GetByIdAsync(id);
            return bleReaderNode == null ? null : _mapper.Map<BleReaderNodeDto>(bleReaderNode);
        }

        public async Task<IEnumerable<BleReaderNodeDto>> GetAllAsync()
        {
            var bleReaderNodes = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<BleReaderNodeDto>>(bleReaderNodes);
        }

        public async Task<BleReaderNodeDto> CreateAsync(BleReaderNodeCreateDto createDto)
        {
            // Validasi DTO
            var validationContext = new ValidationContext(createDto);
            Validator.ValidateObject(createDto, validationContext, true);

            // Validasi ReaderId
            var reader = await _repository.GetReaderByIdAsync(createDto.ReaderId);
            if (reader == null)
                throw new ArgumentException($"Reader with ID {createDto.ReaderId} not found.");

            // Validasi ApplicationId
            var application = await _repository.GetApplicationByIdAsync(createDto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {createDto.ApplicationId} not found.");

            var bleReaderNode = _mapper.Map<BleReaderNode>(createDto);
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            bleReaderNode.CreatedBy = username;
            bleReaderNode.CreatedAt = DateTime.UtcNow;
            bleReaderNode.UpdatedBy = username;
            bleReaderNode.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(bleReaderNode);
            return _mapper.Map<BleReaderNodeDto>(bleReaderNode);
        }

        public async Task UpdateAsync(Guid id, BleReaderNodeUpdateDto updateDto)
        {
            // Validasi DTO
            var validationContext = new ValidationContext(updateDto);
            Validator.ValidateObject(updateDto, validationContext, true);

            // Validasi ReaderId
            var reader = await _repository.GetReaderByIdAsync(updateDto.ReaderId);
            if (reader == null)
                throw new ArgumentException($"Reader with ID {updateDto.ReaderId} not found.");

            // Validasi ApplicationId
            var application = await _repository.GetApplicationByIdAsync(updateDto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {updateDto.ApplicationId} not found.");

            var bleReaderNode = await _repository.GetByIdAsync(id);
            if (bleReaderNode == null)
                throw new KeyNotFoundException("BleReaderNode not found");

            _mapper.Map(updateDto, bleReaderNode);
            bleReaderNode.UpdatedBy = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            bleReaderNode.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(bleReaderNode);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name", "Reader.Name" };
            var validSortColumns = new[] { "Name", "Reader.Name", "CreatedAt", "UpdatedAt", "Status" };

            var filterService = new GenericDataTableService<BleReaderNode, BleReaderNodeDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
          public async Task<byte[]> ExportPdfAsync()
        {
            var records = await _repository.GetAllExportAsync();

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("BLE Reader Node Report")
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
                            header.Cell().Element(CellStyle).Text("Reader").SemiBold();
                            header.Cell().Element(CellStyle).Text("Start Position").SemiBold();
                            header.Cell().Element(CellStyle).Text("End Position").SemiBold();
                            header.Cell().Element(CellStyle).Text("Distance (px)").SemiBold();
                            header.Cell().Element(CellStyle).Text("Distance").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created At").SemiBold();
                        });

                        int index = 1;
                        foreach (var record in records)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(record.Reader?.Name);
                            table.Cell().Element(CellStyle).Text(record.StartPos);
                            table.Cell().Element(CellStyle).Text(record.EndPos);
                            table.Cell().Element(CellStyle).Text(record.DistancePx.ToString());
                            table.Cell().Element(CellStyle).Text(record.Distance.ToString());
                            table.Cell().Element(CellStyle).Text(record.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
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
            var records = await _repository.GetAllExportAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("BLE Reader Nodes");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Reader";
            worksheet.Cell(1, 3).Value = "Start Position";
            worksheet.Cell(1, 4).Value = "End Position";
            worksheet.Cell(1, 5).Value = "Distance (px)";
            worksheet.Cell(1, 6).Value = "Distance";
            worksheet.Cell(1, 7).Value = "Created At";

            int row = 2;
            int no = 1;

            foreach (var record in records)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = record.Reader?.Name;
                worksheet.Cell(row, 3).Value = record.StartPos;
                worksheet.Cell(row, 4).Value = record.EndPos;
                worksheet.Cell(row, 5).Value = record.DistancePx;
                worksheet.Cell(row, 6).Value = record.Distance;
                worksheet.Cell(row, 7).Value = record.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
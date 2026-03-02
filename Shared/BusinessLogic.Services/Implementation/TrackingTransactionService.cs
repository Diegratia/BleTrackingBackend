using AutoMapper;
using BusinessLogic.Services.Interface;
using ClosedXML.Excel;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementation
{
    public class TrackingTransactionService : BaseService, ITrackingTransactionService
    {
        private readonly TrackingTransactionRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAuditEmitter _audit;

        public TrackingTransactionService(
            TrackingTransactionRepository repository,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _audit = audit;
        }

        public async Task<TrackingTransactionRead?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<TrackingTransactionRead>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, TrackingTransactionFilter filter)
        {
            var (data, total, filtered) = await _repository.FilterAsync(filter);

            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data = data
            };
        }

        public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            var trackingTransactions = await _repository.GetAllWithIncludesAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Tracking Transaction Report")
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
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("TransTime").SemiBold();
                            header.Cell().Element(CellStyle).Text("Reader").SemiBold();
                            header.Cell().Element(CellStyle).Text("CardId").SemiBold();
                            header.Cell().Element(CellStyle).Text("FloorplanMaskedArea").SemiBold();
                            header.Cell().Element(CellStyle).Text("CoordinateX").SemiBold();
                            header.Cell().Element(CellStyle).Text("CoordinateY").SemiBold();
                            header.Cell().Element(CellStyle).Text("CoordinatePxX").SemiBold();
                            header.Cell().Element(CellStyle).Text("CoordinatePxY").SemiBold();
                            header.Cell().Element(CellStyle).Text("AlarmStatus").SemiBold();
                            header.Cell().Element(CellStyle).Text("Battery").SemiBold();
                        });

                        int index = 1;
                        foreach (var trackingtransaction in trackingTransactions)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(trackingtransaction.TransTime?.ToString("yyyy-MM-dd") ?? "");
                            table.Cell().Element(CellStyle).Text(trackingtransaction.Reader?.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(trackingtransaction.CardId?.ToString("") ?? "");
                            table.Cell().Element(CellStyle).Text(trackingtransaction.FloorplanMaskedArea?.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(trackingtransaction.CoordinateX?.ToString("") ?? "");
                            table.Cell().Element(CellStyle).Text(trackingtransaction.CoordinateY?.ToString("") ?? "");
                            table.Cell().Element(CellStyle).Text(trackingtransaction.CoordinatePxX?.ToString("") ?? "");
                            table.Cell().Element(CellStyle).Text(trackingtransaction.CoordinatePxY?.ToString("") ?? "");
                            table.Cell().Element(CellStyle).Text(trackingtransaction.AlarmStatus?.ToString("") ?? "");
                            table.Cell().Element(CellStyle).Text(trackingtransaction.Battery?.ToString("") ?? "");
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
            var trackingTransactions = await _repository.GetAllExportAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Tracking Transaction");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "TransTime";
            worksheet.Cell(1, 3).Value = "Reader Name";
            worksheet.Cell(1, 4).Value = "Card Id";
            worksheet.Cell(1, 5).Value = "Coordinate X";
            worksheet.Cell(1, 6).Value = "Coordinate Y";
            worksheet.Cell(1, 7).Value = "Coordinate Px X";
            worksheet.Cell(1, 8).Value = "Coordinate Px Y";
            worksheet.Cell(1, 9).Value = "Alarm Status";
            worksheet.Cell(1, 10).Value = "Battery";

            int row = 2;
            int no = 1;

            foreach (var trackingtransaction in trackingTransactions)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = trackingtransaction.TransTime;
                worksheet.Cell(row, 3).Value = trackingtransaction.Reader?.Name ?? "-";
                worksheet.Cell(row, 4).Value = trackingtransaction.Card?.Dmac ?? "-";
                worksheet.Cell(row, 5).Value = trackingtransaction.CoordinateX;
                worksheet.Cell(row, 6).Value = trackingtransaction.CoordinateY;
                worksheet.Cell(row, 7).Value = trackingtransaction.CoordinatePxX;
                worksheet.Cell(row, 8).Value = trackingtransaction.CoordinatePxY;
                worksheet.Cell(row, 9).Value = trackingtransaction.AlarmStatus.ToString();
                worksheet.Cell(row, 10).Value = trackingtransaction.Battery;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

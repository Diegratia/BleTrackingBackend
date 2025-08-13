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
    public class TrackingTransactionService : ITrackingTransactionService
    {
        private readonly TrackingTransactionRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TrackingTransactionService(TrackingTransactionRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        // public async Task<TrackingTransactionDto> CreateTrackingTransactionAsync(TrackingTransactionCreateDto createDto)
        // {
        //     if (createDto == null) throw new ArgumentNullException(nameof(createDto));

        //     var transaction = _mapper.Map<TrackingTransaction>(createDto);
        //     transaction.Id = Guid.NewGuid();

        //     await _repository.AddAsync(transaction);
        //     return _mapper.Map<TrackingTransactionDto>(transaction);
        // }

        public async Task<TrackingTransactionDto> GetTrackingTransactionByIdAsync(Guid id)
        {
            var transaction = await _repository.GetByIdWithIncludesAsync(id);
            return transaction == null ? null : _mapper.Map<TrackingTransactionDto>(transaction);
        }

        public async Task<IEnumerable<TrackingTransactionDto>> GetAllTrackingTransactionsAsync()
        {
            var transactions = await _repository.GetAllWithIncludesAsync();
            return _mapper.Map<IEnumerable<TrackingTransactionDto>>(transactions);
        }

        // public async Task UpdateTrackingTransactionAsync(Guid id, TrackingTransactionUpdateDto updateDto)
        // {
        //     if (updateDto == null) throw new ArgumentNullException(nameof(updateDto));

        //     var transaction = await _repository.GetByIdAsync(id);
        //     if (transaction == null)
        //         throw new KeyNotFoundException($"TrackingTransaction with ID {id} not found.");

        //     _mapper.Map(updateDto, transaction);
        //     await _repository.UpdateAsync(transaction);
        // }

        // public async Task DeleteTrackingTransactionAsync(Guid id)
        // {
        //     var transaction = await _repository.GetByIdAsync(id);
        //     if (transaction == null)
        //         throw new KeyNotFoundException($"TrackingTransaction with ID {id} not found.");

        //     await _repository.DeleteAsync(transaction);
        // }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] {  "Reader.Name", "FloorplanMaskedArea.Name" };
            var validSortColumns = new[] { "Reader.Name", "FloorplanMaskedArea.Name", "TransTime", "CardId", "AlarmStatus" };

            var filterService = new GenericDataTableService<TrackingTransaction, TrackingTransactionDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
        public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;
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

                        // Table body
                        int index = 1;
                        foreach (var trackingtransaction in trackingTransactions)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(trackingtransaction.TransTime?.ToString("yyyy-MM-dd") ?? "");
                            table.Cell().Element(CellStyle).Text(trackingtransaction.Reader.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(trackingtransaction.CardId);
                            table.Cell().Element(CellStyle).Text(trackingtransaction.FloorplanMaskedArea.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(trackingtransaction.CoordinateX?.ToString("") ?? "");
                            table.Cell().Element(CellStyle).Text(trackingtransaction.CoordinateY?.ToString("") ?? "");
                            table.Cell().Element(CellStyle).Text(trackingtransaction.CoordinatePxX?.ToString("") ?? "");
                            table.Cell().Element(CellStyle).Text(trackingtransaction.CoordinatePxY?.ToString("") ?? "");
                            table.Cell().Element(CellStyle).Text(trackingtransaction.AlarmStatus);
                            table.Cell().Element(CellStyle).Text(trackingtransaction.Battery);
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

            // Header
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
                worksheet.Cell(row, 3).Value = trackingtransaction.Reader.Name?? "-";
                worksheet.Cell(row, 4).Value = trackingtransaction.Card.Dmac ?? "-";
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

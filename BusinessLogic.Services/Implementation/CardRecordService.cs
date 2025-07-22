using AutoMapper;
using BusinessLogic.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;
using Repositories.DbContexts;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Repositories;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

namespace BusinessLogic.Services.Implementation
{
    public class CardRecordService : ICardRecordService
    {
        private readonly CardRecordRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CardRecordService(CardRecordRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CardRecordDto> GetByIdAsync(Guid id)
        {
            var cardRecord = await _repository.GetByIdAsync(id);
            return cardRecord == null ? null : _mapper.Map<CardRecordDto>(cardRecord);
        }

        public async Task<IEnumerable<CardRecordDto>> GetAllAsync()
        {
            var cardRecord = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<CardRecordDto>>(cardRecord);
        }

        public async Task<CardRecordDto> CreateAsync(CardRecordCreateDto createDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var card = await _repository.GetCardByIdAsync(createDto.CardId);
            if (card == null)
                throw new ArgumentException($"Card with ID {createDto.CardId} not found.");

            // var visitor = await _repository.GetVisitorByIdAsync(createDto.VisitorId);
            // if (application == null)
            //     throw new ArgumentException($"Visitor with ID {createDto.VisitorId} not found.");

            // var member = await _repository.GetMemberByIdAsync(createDto.MemberId);
            // if (application == null)
            //     throw new ArgumentException($"Member with ID {createDto.MemberId} not found.");

            var cardRecord = _mapper.Map<CardRecord>(createDto);
            cardRecord.Id = Guid.NewGuid();
            cardRecord.Timestamp = DateTime.UtcNow;
            cardRecord.CheckinBy = username ?? "";
            cardRecord.CheckoutBy = username ?? "";

            var createdcardRecord = await _repository.AddAsync(cardRecord);
            return _mapper.Map<CardRecordDto>(createdcardRecord);
        }

        public async Task UpdateAsync(Guid id, CardRecordUpdateDto updateDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var card = await _repository.GetCardByIdAsync(updateDto.CardId);
            if (card == null)
                throw new ArgumentException($"Card with ID {updateDto.CardId} not found.");

            // var visitor = await _repository.GetVisitorByIdAsync(createDto.VisitorId);
            // if (application == null)
            //     throw new ArgumentException($"Visitor with ID {createDto.VisitorId} not found.");

            // var member = await _repository.GetMemberByIdAsync(createDto.MemberId);
            // if (application == null)
            //     throw new ArgumentException($"Member with ID {createDto.MemberId} not found.");

            var cardRecord = await _repository.GetByIdAsync(id);
            if (cardRecord == null)
                throw new KeyNotFoundException("Card Record not found");

            _mapper.Map(updateDto, cardRecord);
            await _repository.UpdateAsync(cardRecord);
        }

        public async Task DeleteAsync(Guid id)
        {
            var cardRecord = await _repository.GetByIdAsync(id);
            if (cardRecord == null)
            {
                throw new KeyNotFoundException("Card Record Not Found");
            }
            await _repository.DeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name", "VisitorName" };
            var validSortColumns = new[] { "Name", "VisitorName", "Visitor.Name", "Member.Name", "CheckinAt", "CheckoutAt", "TimeStamp","VisitorType", "Status", "VisitorType" };

            var filterService = new GenericDataTableService<CardRecord, CardRecordDto>(
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
                        .Text("Card Records Report")
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
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Visitor Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Card Id").SemiBold();
                            header.Cell().Element(CellStyle).Text("Visitor").SemiBold();
                            header.Cell().Element(CellStyle).Text("Member").SemiBold();
                            header.Cell().Element(CellStyle).Text("Visitor Type").SemiBold();
                            header.Cell().Element(CellStyle).Text("Checkin At").SemiBold();
                            header.Cell().Element(CellStyle).Text("Checkin By").SemiBold();
                            header.Cell().Element(CellStyle).Text("Checkout At").SemiBold();
                            header.Cell().Element(CellStyle).Text("Checkout By").SemiBold();
                            header.Cell().Element(CellStyle).Text("Checkout Site").SemiBold();
                            header.Cell().Element(CellStyle).Text("Checkin Site").SemiBold();
                        });

                        int index = 1;
                        foreach (var record in records)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(record.Name);
                            table.Cell().Element(CellStyle).Text(record.VisitorCardId);
                            table.Cell().Element(CellStyle).Text(record.Visitor);
                            table.Cell().Element(CellStyle).Text(record.Member);
                            table.Cell().Element(CellStyle).Text(record.VisitorType.ToString());
                            table.Cell().Element(CellStyle).Text(record.CheckinAt?.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(record.CheckinBy);
                            table.Cell().Element(CellStyle).Text(record.CheckoutAt?.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(record.CheckoutBy);
                            table.Cell().Element(CellStyle).Text(record.CheckoutSiteId?.ToString());
                            table.Cell().Element(CellStyle).Text(record.CheckinSiteId?.ToString());
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
            var worksheet = workbook.Worksheets.Add("Card Records");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Visitor Name";
            worksheet.Cell(1, 3).Value = "Card Id";
            worksheet.Cell(1, 4).Value = "Visitor";
            worksheet.Cell(1, 5).Value = "Member";
            worksheet.Cell(1, 6).Value = "Visitor Type";
            worksheet.Cell(1, 7).Value = "Checkin At";
            worksheet.Cell(1, 8).Value = "Checkin By";
            worksheet.Cell(1, 9).Value = "Checkout At";
            worksheet.Cell(1, 10).Value = "Checkout By";
            worksheet.Cell(1, 11).Value = "Checkout Site";
            worksheet.Cell(1, 12).Value = "Checkin Site";

            int row = 2;
            int no = 1;

            foreach (var record in records)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = record.Name;
                worksheet.Cell(row, 3).Value = record.VisitorCardId.ToString();
                worksheet.Cell(row, 4).Value = record.Visitor.Name;
                worksheet.Cell(row, 5).Value = record.Member.Name;
                worksheet.Cell(row, 6).Value = record.VisitorType.ToString();
                worksheet.Cell(row, 7).Value = record.CheckinAt?.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell(row, 8).Value = record.CheckinBy;
                worksheet.Cell(row, 9).Value = record.CheckoutAt?.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell(row, 10).Value = record.CheckoutBy;
                worksheet.Cell(row, 11).Value = record.CheckoutSiteId?.ToString();
                worksheet.Cell(row, 12).Value = record.CheckinSiteId?.ToString();
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

    }
    
}
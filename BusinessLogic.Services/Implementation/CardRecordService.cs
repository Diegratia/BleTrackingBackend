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
using Bogus.DataSets;
using Helpers.Consumer;

namespace BusinessLogic.Services.Implementation
{
    public class CardRecordService : ICardRecordService
    {
        private readonly CardRecordRepository _repository;
        private readonly CardRepository _cardRepository;
        private readonly VisitorRepository _visitorRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CardRecordService(CardRecordRepository repository, CardRepository cardRepository, VisitorRepository visitorRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _cardRepository = cardRepository;
            _visitorRepository = visitorRepository;
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
            var cardRecord = _mapper.Map<CardRecord>(createDto);

            cardRecord.Id = Guid.NewGuid();

            cardRecord.CreatedBy = username;
            cardRecord.UpdatedBy = username;
            cardRecord.CreatedAt = DateTime.UtcNow;
            cardRecord.UpdatedAt = DateTime.UtcNow;
            cardRecord.Status = 1;
            cardRecord.CheckinBy = username;

           
            // var cardId = cardRecordMapper?.CardId;
            // var visitorId = cardRecordMapper?.VisitorId;\
            var card = await _cardRepository.GetByIdAsync(cardRecord.CardId!.Value); 
            var visitor = await _visitorRepository.GetByIdAsync(cardRecord.VisitorId!.Value);
            if (card.Id == null)
            {
                throw new ArgumentException("System GUID error: CardId is null");
            }
            if (visitor.Id == null)
            {
                throw new ArgumentException("System GUID error: CardId is null");
            }

           
        if (card.IsUsed == true && card.VisitorId != visitor.Id )
                throw new InvalidOperationException("Card already checked in by another visitor.");
        if (card.IsUsed == true)
                throw new InvalidOperationException("Card already used.");
            card.IsUsed = true;
            card.LastUsed = visitor.Name; 
            visitor.BleCardNumber = card.Dmac;
            visitor.CardNumber = card.CardNumber;
            card.VisitorId = visitor.Id;
            if (card.IsMultiMaskedArea == false)
            {
                cardRecord.CheckinMaskedArea = card.RegisteredMaskedAreaId;
            }
            else
            {
                cardRecord.CheckinMaskedArea = null;
            }
            // card.CheckinAt = visitor.TrxVisitors.FirstOrDefault()?.CheckedInAt;
                cardRecord.VisitorActiveStatus = VisitorActiveStatus.Active;
            // fallback jika null
            // Console.WriteLine("disini broo", visitor.TrxVisitors.FirstOrDefault()?.VisitorActiveStatus);
            cardRecord.Name = visitor.Name;
            card.CheckinAt = DateTime.UtcNow;
            cardRecord.CheckinAt = card.CheckinAt;
            await _cardRepository.UpdateAsync(card);
            await _visitorRepository.UpdateAsync(visitor);
            var createdCardRecord = await _repository.AddAsync(cardRecord);
            var cardRecordMapper = _mapper.Map<CardRecordDto>(createdCardRecord);
            return cardRecordMapper;   
        }

            public async Task CheckoutCard(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var cardRecord = await _repository.GetByIdAsync(id);
            var card = await _cardRepository.GetByIdAsync(cardRecord.CardId!.Value); 
            var visitor = await _visitorRepository.GetByIdAsync(cardRecord.VisitorId!.Value);
            if (cardRecord == null)
                throw new InvalidOperationException("No active session found");
            if (card.IsUsed == false )
                throw new InvalidOperationException("Card already checkout.");
            cardRecord.CheckoutAt = DateTime.UtcNow;
            cardRecord.CheckoutBy = username;
            cardRecord.CheckoutMaskedArea = card.RegisteredMaskedAreaId;
            // cardRecord.Status = 0;
            cardRecord.UpdatedAt = DateTime.UtcNow;
            cardRecord.UpdatedBy = username;
            cardRecord.VisitorActiveStatus = VisitorActiveStatus.Expired;

            card.CheckinAt = null;
            card.IsUsed = false;
            card.VisitorId = null;
            card.LastUsed = visitor.Name; 
            visitor.BleCardNumber = null;
            visitor.CardNumber = null;
            

            await _repository.UpdateAsync(cardRecord);
        }
        
        // public async Task<CardRecordDto> CreateAsync(CardRecordCreateDto createDto)
        // {
        //     return await _uow.ExecuteInTransactionAsync(async () =>
        //     {
        //         var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

        //         // Map & inisialisasi
        //         var cardRecord = _mapper.Map<CardRecord>(createDto);
        //         cardRecord.Id = Guid.NewGuid();
        //         cardRecord.CreatedBy = username;
        //         cardRecord.UpdatedBy = username;
        //         cardRecord.CreatedAt = DateTime.UtcNow;
        //         cardRecord.UpdatedAt = DateTime.UtcNow;
        //         cardRecord.Status = 1;

        //         // --- VALIDASI lebih dulu ---
        //         if (cardRecord.CardId == Guid.Empty) throw new ArgumentException("CardId wajib diisi");
        //         if (cardRecord.VisitorId == Guid.Empty) throw new ArgumentException("VisitorId wajib diisi");

        //         var card = await _cardRepository.GetByIdAsync(cardRecord.CardId!.Value);
        //         if (card is null) throw new InvalidOperationException("Card tidak ditemukan");

        //         var visitor = await _visitorRepository.GetByIdAsync(cardRecord.VisitorId!.Value);
        //         if (visitor is null) throw new InvalidOperationException("Visitor tidak ditemukan");

        //         // (opsional) aturan bisnis tambahan
        //         if (card.IsUsed == true && card.VisitorId != visitor.Id)
        //             throw new InvalidOperationException("Card sudah dipakai visitor lain");

        //         // --- Mutasi state di memori (belum save) ---
        //         await _cardRecordRepository.AddAsync(cardRecord, saveNow: false);

        //         card.IsUsed = true;
        //         card.LastUsed = visitor.Name;
        //         visitor.BleCardNumber = card.Dmac;
        //         visitor.CardNumber = card.CardNumber;
        //         card.VisitorId = visitor.Id;

        //         cardRecord.CheckinMaskedArea = (card.IsMultiMaskedArea == false)
        //             ? card.RegisteredMaskedAreaId
        //             : null;

        //         cardRecord.VisitorActiveStatus = visitor.TrxVisitors.FirstOrDefault()?.VisitorActiveStatus;
        //         card.CheckinAt = DateTime.UtcNow;

        //         _cardRepository.UpdateAsync(card, saveNow: false);
        //         _visitorRepository.UpdateAsync(visitor, saveNow: false);

        //         // Save + Commit dilakukan di UnitOfWork
        //         var dto = _mapper.Map<CardRecordDto>(cardRecord);
        //         return dto;
        //     });
        // }



        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name", "VisitorName" };
            var validSortColumns = new[] { "Name", "VisitorName", "Visitor.Name", "Member.Name", "CheckinAt", "CheckoutAt", "TimeStamp", "VisitorActiveStatus", "Status", "VisitorActiveStatus" };

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
                            table.Cell().Element(CellStyle).Text(record.CardId);
                            table.Cell().Element(CellStyle).Text(record.Visitor);
                            table.Cell().Element(CellStyle).Text(record.Member);
                            table.Cell().Element(CellStyle).Text(record.VisitorActiveStatus.ToString());
                            table.Cell().Element(CellStyle).Text(record.CheckinAt?.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(record.CheckinBy);
                            table.Cell().Element(CellStyle).Text(record.CheckoutAt?.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(record.CheckoutBy);
                            table.Cell().Element(CellStyle).Text(record.CheckoutMaskedArea?.ToString());
                            table.Cell().Element(CellStyle).Text(record.CheckinMaskedArea?.ToString());
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
                worksheet.Cell(row, 3).Value = record.CardId.ToString();
                worksheet.Cell(row, 4).Value = record.Visitor.Name;
                worksheet.Cell(row, 5).Value = record.Member.Name;
                worksheet.Cell(row, 6).Value = record.VisitorActiveStatus.ToString();
                worksheet.Cell(row, 7).Value = record.CheckinAt?.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell(row, 8).Value = record.CheckinBy;
                worksheet.Cell(row, 9).Value = record.CheckoutAt?.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell(row, 10).Value = record.CheckoutBy;
                worksheet.Cell(row, 11).Value = record.CheckoutMaskedArea?.ToString();
                worksheet.Cell(row, 12).Value = record.CheckinMaskedArea?.ToString();
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

    }
    
}
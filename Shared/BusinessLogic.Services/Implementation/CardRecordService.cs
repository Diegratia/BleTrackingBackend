using AutoMapper;
using BusinessLogic.Services.Background;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Extension.RootExtension;
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
using Shared.Contracts;
using Shared.Contracts.Read;
using Microsoft.Extensions.Logging;
using Repositories.Repository.RepoModel;
using DataView;

namespace BusinessLogic.Services.Implementation
{
    public class CardRecordService : BaseService, ICardRecordService
    {
        private readonly CardRecordRepository _repository;
        private readonly CardRepository _cardRepository;
        private readonly VisitorRepository _visitorRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CardRecord> _logger;
        private readonly IMqttPubQueue _mqttQueue;
        private readonly IAuditEmitter _audit;

        public CardRecordService(
            CardRecordRepository repository,
            CardRepository cardRepository,
            VisitorRepository visitorRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IMqttPubQueue mqttQueue,
            ILogger<CardRecord> logger,
            IAuditEmitter audit) : base(httpContextAccessor)
        {
            _repository = repository;
            _cardRepository = cardRepository;
            _visitorRepository = visitorRepository;
            _mapper = mapper;
            _logger = logger;
            _mqttQueue = mqttQueue;
            _audit = audit;
        }

        public async Task<CardRecordRead> GetByIdAsync(Guid id)
        {
            var cardRecord = await _repository.GetByIdAsync(id);
            if (cardRecord == null)
                throw new NotFoundException($"CardRecord with id {id} not found");
            return cardRecord; // Direct return, no mapper needed
        }

        public async Task<IEnumerable<CardRecordRead>> GetAllAsync()
        {
            var cardRecords = await _repository.GetAllAsync();
            return cardRecords; // Direct return
        }

      public async Task<CardRecordRead> CreateAsync(CardRecordCreateDto createDto)
        {
            var cardRecord = _mapper.Map<CardRecord>(createDto);

            // Ownership validation for Card
            if (cardRecord.CardId.HasValue)
            {
                var invalidCardIds = await _repository.CheckInvalidCardOwnershipAsync(
                    cardRecord.CardId.Value, AppId);
                if (invalidCardIds.Any())
                    throw new UnauthorizedException(
                        $"CardId does not belong to this Application: {string.Join(", ", invalidCardIds)}");
            }

            // Ownership validation for Visitor
            if (cardRecord.VisitorId.HasValue)
            {
                var invalidVisitorIds = await _repository.CheckInvalidVisitorOwnershipAsync(
                    cardRecord.VisitorId.Value, AppId);
                if (invalidVisitorIds.Any())
                    throw new UnauthorizedException(
                        $"VisitorId does not belong to this Application: {string.Join(", ", invalidVisitorIds)}");
            }

            // Ownership validation for Member
            if (cardRecord.MemberId.HasValue)
            {
                var invalidMemberIds = await _repository.CheckInvalidMemberOwnershipAsync(
                    cardRecord.MemberId.Value, AppId);
                if (invalidMemberIds.Any())
                    throw new UnauthorizedException(
                        $"MemberId does not belong to this Application: {string.Join(", ", invalidMemberIds)}");
            }

            cardRecord.Timestamp = DateTime.UtcNow;
            cardRecord.CheckinAt = DateTime.UtcNow;
            cardRecord.CheckinBy = UsernameFormToken;
            cardRecord.VisitorActiveStatus = VisitorActiveStatus.Active;
            cardRecord.ApplicationId = AppId;

            SetCreateAudit(cardRecord);

            var card = await _cardRepository.GetByIdEntityAsync(cardRecord.CardId!.Value);
            if (card == null)
                throw new NotFoundException("Card not found.");

            var visitor = cardRecord.VisitorId.HasValue
                ? await _visitorRepository.GetByIdAsync(cardRecord.VisitorId.Value)
                : null;

            if (card.IsUsed == true)
                throw new BusinessException("Card already checked in by another visitor.");

            card.IsUsed = true;
            card.CardStatus = CardStatus.Used;
            card.LastUsed = visitor?.Name ?? cardRecord.Name;
            card.VisitorId = cardRecord.VisitorId;
            card.CheckinAt = DateTime.UtcNow;

            if (visitor != null)
            {
                visitor.BleCardNumber = card.Dmac;
                visitor.CardNumber = card.CardNumber;
                await _visitorRepository.UpdateAsync(visitor);
            }

            cardRecord.Name = visitor?.Name ?? cardRecord.Name;
            cardRecord.CheckinMaskedArea = (card.IsMultiMaskedArea == false) ? card.RegisteredMaskedAreaId : (Guid?)null;

            await _cardRepository.UpdateAsync(card);
            var createdCardRecord = await _repository.AddAsync(cardRecord);
            _mqttQueue.Enqueue("engine/refresh/card-related", "");
            _audit.Created("CardRecord", createdCardRecord.Id, $"CardRecord {createdCardRecord.Name} created");

            return await _repository.GetByIdAsync(createdCardRecord.Id);
        }


        public async Task CheckoutCard(Guid id)
        {
            var cardRecord = await _repository.GetByIdEntityAsync(id);
            if (cardRecord is null)
                throw new NotFoundException("Card record not found.");

            var card = await _cardRepository.GetByIdEntityAsync(cardRecord.CardId!.Value)
                    ?? throw new NotFoundException("Card not found.");

            var visitor = cardRecord.VisitorId.HasValue
                ? await _visitorRepository.GetByIdAsync(cardRecord.VisitorId.Value)
                : null;

            if (card.IsUsed == false)
                throw new BusinessException("Card already checked out.");

            var now = DateTime.UtcNow;

            cardRecord.CheckoutAt = now;
            cardRecord.CheckoutBy = UsernameFormToken;
            cardRecord.CheckoutMaskedArea = (card.IsMultiMaskedArea == false) ? card.RegisteredMaskedAreaId : (Guid?)null;
            cardRecord.Timestamp = now;
            cardRecord.VisitorActiveStatus = VisitorActiveStatus.Expired;

            SetUpdateAudit(cardRecord);

            card.CheckinAt = null;
            card.IsUsed = false;
            card.CardStatus = CardStatus.Available;
            card.VisitorId = null;
            card.LastUsed = visitor?.Name ?? cardRecord.Name;

            if (visitor != null)
            {
                visitor.BleCardNumber = null;
                visitor.CardNumber = null;
                await _visitorRepository.UpdateAsync(visitor);
            }

            await _cardRepository.UpdateAsync(card);
            await _repository.UpdateAsync(cardRecord);
            _mqttQueue.Enqueue("engine/refresh/card-related", "");
            _audit.Updated("CardRecord", cardRecord.Id, $"CardRecord {cardRecord.Name} checked out");
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

        public async Task<IEnumerable<CardUsageSummaryRM>> GetCardUsageSummaryAsync(
        )
        {
            return await _repository.GetCardUsageSummaryAsync();
        }

        public async Task<IEnumerable<CardUsageHistoryRM>> GetCardUsageHistoryAsync(
        CardRecordRequestRM request    
        )
        {
            if (!request.CardId.HasValue)
                throw new BusinessException("CardId is required");
            return await _repository.GetCardUsageHistoryAsync(request);
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, Shared.Contracts.CardRecordFilter filter)
        {
            // Map Standard DataTables params
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "Timestamp";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            // Map Date Filters if present
            if (request.DateFilters != null && request.DateFilters.Count > 0)
            {
                if (request.DateFilters.TryGetValue("Timestamp", out var dateFilter))
                {
                    filter.DateFrom = dateFilter.DateFrom;
                    filter.DateTo = dateFilter.DateTo;
                }
                else if (request.DateFilters.TryGetValue("CheckinAt", out dateFilter))
                {
                    filter.DateFrom = dateFilter.DateFrom;
                    filter.DateTo = dateFilter.DateTo;
                }
            }

            // Call repository FilterAsync (using ProjectToRead)
            var (data, total, filtered) = await _repository.FilterAsync(filter);

            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data
            };
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
                            table.Cell().Element(CellStyle).Text(record.Name.ToString());
                            table.Cell().Element(CellStyle).Text(record.CardId.ToString());
                            table.Cell().Element(CellStyle).Text(record.Visitor.ToString());
                            table.Cell().Element(CellStyle).Text(record.Member.ToString());
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
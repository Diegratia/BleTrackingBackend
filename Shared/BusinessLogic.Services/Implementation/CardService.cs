using AutoMapper;
using BusinessLogic.Services.Background;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using System.IO;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;
using Shared.Contracts.Read;
using System.Linq.Dynamic.Core;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using LicenseType = QuestPDF.Infrastructure.LicenseType;
using Microsoft.Extensions.Logging;
using DataView;

namespace BusinessLogic.Services.Implementation
{
    public class CardService : BaseService, ICardService
    {
        private readonly CardRepository _repository;
        private readonly CardAccessRepository _cardAccessRepository;
        private readonly IMapper _mapper;
        private readonly MstMemberRepository _mstMemberRepository;
        private readonly ILogger<Card> _logger;
        private readonly IMqttPubQueue _mqttQueue;
        private readonly IAuditEmitter _audit;

        public CardService(
            CardRepository repository,
            CardAccessRepository cardAccessRepository,
            MstMemberRepository mstMemberRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<Card> logger,
            IMqttPubQueue mqttQueue,
            IAuditEmitter audit) : base(httpContextAccessor)
        {
            _repository = repository;
            _cardAccessRepository = cardAccessRepository;
            _mstMemberRepository = mstMemberRepository;
            _mapper = mapper;
            _logger = logger;
            _mqttQueue = mqttQueue;
            _audit = audit;
        }

        public async Task<CardRead?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<CardRead>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<CardRead>> GetAllUnUsedAsync()
        {
            return await _repository.GetUnUsedCardAsync();
        }

        public async Task<IEnumerable<OpenCardDto>> OpenGetAllUnUsedAsync()
        {
            var cards = await _repository.GetUnUsedCardAsync();
            return _mapper.Map<IEnumerable<OpenCardDto>>(cards);
        }
        
        public async Task<IEnumerable<OpenCardDto>> OpenGetAllAsync()
        {
            var cards = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<OpenCardDto>>(cards);
        }

        public async Task<CardRead> CreateAsync(CardCreateDto createDto)
        {
            var existingCard = await _repository.BaseEntityQuery()
            .FirstOrDefaultAsync(b =>
                                b.CardNumber == createDto.CardNumber ||
                                b.Dmac == createDto.Dmac);

            if (existingCard != null)
            {
                if (existingCard.CardNumber == createDto.CardNumber)
                {
                    throw new ArgumentException($"Card with Number {createDto.CardNumber} already exists.");
                }
                else if (existingCard.Dmac == createDto.Dmac)
                {
                    throw new ArgumentException($"Card with Mac {createDto.Dmac} already exists.");
                }
            }

            var card = _mapper.Map<Card>(createDto);

            // Ownership validation
            if (card.MemberId.HasValue)
            {
                var invalidMemberIds = await _repository.CheckInvalidMemberOwnershipAsync(card.MemberId.Value, AppId);
                if (invalidMemberIds.Any())
                    throw new UnauthorizedException($"MemberId does not belong to this Application: {string.Join(", ", invalidMemberIds)}");
            }

            if (card.VisitorId.HasValue)
            {
                var invalidVisitorIds = await _repository.CheckInvalidVisitorOwnershipAsync(card.VisitorId.Value, AppId);
                if (invalidVisitorIds.Any())
                    throw new UnauthorizedException($"VisitorId does not belong to this Application: {string.Join(", ", invalidVisitorIds)}");
            }

            if (card.CardGroupId.HasValue)
            {
                var invalidCardGroupIds = await _repository.CheckInvalidCardGroupOwnershipAsync(card.CardGroupId.Value, AppId);
                if (invalidCardGroupIds.Any())
                    throw new UnauthorizedException($"CardGroupId does not belong to this Application: {string.Join(", ", invalidCardGroupIds)}");
            }

            if (card.VisitorId != null && card.MemberId == null)
            {
                card.IsUsed = true;
                card.CardStatus = CardStatus.Used;
            }
            else if (card.VisitorId == null && card.MemberId != null)
            {
                card.IsUsed = true;
                card.CardStatus = CardStatus.Used;
            }
            else
            {
                card.IsUsed = false;
                card.CardStatus = CardStatus.Available;
            }

            if (card.IsMultiMaskedArea == true)
            {
                card.RegisteredMaskedArea = null;
            }

            SetCreateAudit(card);
            card.StatusCard = 1;
            card.IsUsed = false;
            card.CardStatus = CardStatus.Available;

            var createdCard = await _repository.AddAsync(card);
            card.QRCode = createdCard.CardNumber;
            _mqttQueue.Enqueue("engine/refresh/card-related","");
             _audit.Created("Card", card.Id, $"Card {card.CardNumber} created");
            return await _repository.GetByIdAsync(card.Id);
        }
        
        public async Task<CardRead> CreateMinimalAsync(CardAddDto dto)
        {
            var existingCard = await _repository.BaseEntityQuery()
            .FirstOrDefaultAsync(b =>
                                b.CardNumber == dto.CardNumber ||
                                b.Dmac == dto.Dmac);

            if (existingCard != null)
            {
                if (existingCard.CardNumber == dto.CardNumber)
                {
                    throw new ArgumentException($"Card with Number {dto.CardNumber} already exists.");
                }
                else if (existingCard.Dmac == dto.Dmac)
                {
                    throw new ArgumentException($"Card with Mac {dto.Dmac} already exists.");
                }
            }

            var entity = _mapper.Map<Card>(dto);
            SetCreateAudit(entity);
            entity.StatusCard = 1;
            entity.ApplicationId = AppId;

            // Ownership validation
            if (dto.MemberId.HasValue)
            {
                var invalidMemberIds = await _repository.CheckInvalidMemberOwnershipAsync(dto.MemberId.Value, AppId);
                if (invalidMemberIds.Any())
                    throw new UnauthorizedException($"MemberId does not belong to this Application: {string.Join(", ", invalidMemberIds)}");
            }

            if (dto.VisitorId.HasValue)
            {
                var invalidVisitorIds = await _repository.CheckInvalidVisitorOwnershipAsync(dto.VisitorId.Value, AppId);
                if (invalidVisitorIds.Any())
                    throw new UnauthorizedException($"VisitorId does not belong to this Application: {string.Join(", ", invalidVisitorIds)}");
            }

            if (dto.CardGroupId.HasValue)
            {
                var invalidCardGroupIds = await _repository.CheckInvalidCardGroupOwnershipAsync(dto.CardGroupId.Value, AppId);
                if (invalidCardGroupIds.Any())
                    throw new UnauthorizedException($"CardGroupId does not belong to this Application: {string.Join(", ", invalidCardGroupIds)}");
            }

            if (dto.CardAccessIds.Any())
            {
                var accesses = await _cardAccessRepository.GetAllQueryable()
                                    .Where(c => dto.CardAccessIds.Contains(c.Id))
                                    .ToListAsync();

                foreach (var access in accesses)
                {
                    if (access == null)
                    {
                        throw new KeyNotFoundException($"Card Access with id {dto.CardAccessIds.First()} not found");
                    }
                    entity.CardCardAccesses.Add(new CardCardAccess
                    {
                        CardId = entity.Id,
                        CardAccessId = access.Id,
                        ApplicationId = entity.ApplicationId
                    });
                }
            }
            var result = await _repository.AddAsync(entity);
            _mqttQueue.Enqueue("engine/refresh/card-related","");
             _audit.Created("Card", result.Id, $"Card {result.CardNumber} created");
            return await _repository.GetByIdAsync(result.Id);
        }

        public async Task<IEnumerable<CardRead>> BulkAddAsync(List<CardAddDto> dtos)
        {
            var result = new List<CardRead>();

            foreach (var dto in dtos)
            {
                var card = await CreateMinimalAsync(dto);
                result.Add(card);
            }

            _mqttQueue.Enqueue("engine/refresh/card-related", "");
            _audit.Created("Card", result.Count, $"Bulk created {result.Count} cards");

            return result;
        }

        public async Task UpdatesAsync(Guid id, CardEditDto dto)
        {
            var entity = await _repository.BaseEntityQuery()
                .Include(cg => cg.CardCardAccesses)
                .FirstOrDefaultAsync(cg => cg.Id == id);

            if (entity == null)
                throw new KeyNotFoundException("Card not found");

            // Ownership validation for updated values
            if (dto.MemberId.HasValue && dto.MemberId.Value != entity.MemberId)
            {
                var invalidMemberIds = await _repository.CheckInvalidMemberOwnershipAsync(dto.MemberId.Value, AppId);
                if (invalidMemberIds.Any())
                    throw new UnauthorizedException($"MemberId does not belong to this Application: {string.Join(", ", invalidMemberIds)}");
            }

            if (dto.VisitorId.HasValue && dto.VisitorId.Value != entity.VisitorId)
            {
                var invalidVisitorIds = await _repository.CheckInvalidVisitorOwnershipAsync(dto.VisitorId.Value, AppId);
                if (invalidVisitorIds.Any())
                    throw new UnauthorizedException($"VisitorId does not belong to this Application: {string.Join(", ", invalidVisitorIds)}");
            }

            if (dto.CardGroupId.HasValue && dto.CardGroupId.Value != entity.CardGroupId)
            {
                var invalidCardGroupIds = await _repository.CheckInvalidCardGroupOwnershipAsync(dto.CardGroupId.Value, AppId);
                if (invalidCardGroupIds.Any())
                    throw new UnauthorizedException($"CardGroupId does not belong to this Application: {string.Join(", ", invalidCardGroupIds)}");
            }

            // Update scalar
            entity.Name = dto.Name ?? entity.Name;
            entity.Remarks = dto.Remarks ?? entity.Remarks;
            entity.CardNumber = dto.CardNumber ?? entity.CardNumber;
            entity.Dmac = dto.Dmac ?? entity.Dmac;
            entity.IsMultiMaskedArea = dto.IsMultiMaskedArea ?? entity.IsMultiMaskedArea;
            entity.RegisteredMaskedAreaId = dto.RegisteredMaskedAreaId ?? entity.RegisteredMaskedAreaId;
            entity.VisitorId = dto.VisitorId ?? entity.VisitorId;
            entity.MemberId = dto.MemberId ?? entity.MemberId;
            entity.CardGroupId = dto.CardGroupId ?? entity.CardGroupId;
            entity.CardType = (CardType)Enum.Parse<CardType>(dto.CardType);

            SetUpdateAudit(entity);

            // Update CardAccesses (join table)
            var existingAccessIds = entity.CardCardAccesses.Select(ca => ca.CardAccessId).ToList();
            var newAccessIds = dto.CardAccessIds.Where(id => id.HasValue).Select(id => id.Value).ToList();

            // Remove yang tidak ada di request
            var toRemove = entity.CardCardAccesses
                .Where(ca => !newAccessIds.Contains(ca.CardAccessId))
                .ToList();

            foreach (var remove in toRemove)
            {
                entity.CardCardAccesses.Remove(remove);
            }

            // Tambah yang baru
            var toAdd = newAccessIds.Except(existingAccessIds).ToList();
            foreach (var addId in toAdd)
            {
                var access = await _cardAccessRepository.GetByIdAsync(addId);
                if (access == null)
                    throw new KeyNotFoundException($"Card Access with id {addId} not found");

                entity.CardCardAccesses.Add(new CardCardAccess
                {
                    CardId = entity.Id,
                    CardAccessId = addId,
                    ApplicationId = entity.ApplicationId
                });
            }

            await _repository.UpdateAsync(entity);
            _mqttQueue.Enqueue("engine/refresh/card-related","");
             _audit.Updated("Card", entity.Id, $"Card {entity.CardNumber} updated");
        }

        public async Task UpdateAccessAsync(Guid id, CardAccessEdit dto)
        {
            var entity = await _repository.BaseEntityQuery()
                .Include(cg => cg.CardCardAccesses)
                .FirstOrDefaultAsync(cg => cg.Id == id);

            if (entity == null)
                throw new KeyNotFoundException("Card not found");

            SetUpdateAudit(entity);

            // Update CardAccesses (join table)
            var existingAccessIds = entity.CardCardAccesses.Select(ca => ca.CardAccessId).ToList();
            var newAccessIds = dto.CardAccessIds.Where(id => id.HasValue).Select(id => id.Value).ToList();

            // Remove yang tidak ada di request
            var toRemove = entity.CardCardAccesses
                .Where(ca => !newAccessIds.Contains(ca.CardAccessId))
                .ToList();

            foreach (var remove in toRemove)
            {
                entity.CardCardAccesses.Remove(remove);
            }

            // Tambah yang baru
            var toAdd = newAccessIds.Except(existingAccessIds).ToList();
            foreach (var addId in toAdd)
            {
                var access = await _cardAccessRepository.GetByIdAsync(addId);
                if (access == null)
                    throw new KeyNotFoundException($"Card Access with id {addId} not found");

                entity.CardCardAccesses.Add(new CardCardAccess
                {
                    CardId = entity.Id,
                    CardAccessId = addId,
                    ApplicationId = entity.ApplicationId
                });
            }

            await _repository.UpdateAsync(entity);
             _audit.Updated("Card", entity.Id, $"Card {entity.CardNumber} access updated");
        }
        
        public async Task UpdateAccessByVMSAsync(string cardNumber, CardAccessEdit dto)
        {
            var entity = await _repository.BaseEntityQuery()
                .Include(cg => cg.CardCardAccesses)
                .FirstOrDefaultAsync(cg => cg.CardNumber == cardNumber);
            if (entity == null)
                throw new KeyNotFoundException("Card not found with card number " + cardNumber);

            SetUpdateAudit(entity);

            //update cardaccess - join table
            var existingAccessIds = entity.CardCardAccesses.Select(ca => ca.CardAccessId).ToList();
            var newAccessIds = dto.CardAccessIds.Where(id => id.HasValue).Select(id => id.Value).ToList();

            // Tambah yang baru
            var toAdd = newAccessIds.Except(existingAccessIds).ToList();
            foreach (var addId in toAdd)
            {
                var access = await _cardAccessRepository.GetByIdAsync(addId);
                if (access == null)
                    throw new KeyNotFoundException($"Card Access with id {addId} not found");

                entity.CardCardAccesses.Add(new CardCardAccess
                {
                    CardId = entity.Id,
                    CardAccessId = addId,
                    ApplicationId = entity.ApplicationId
                });
            }

            await _repository.UpdateAsync(entity);
             _audit.Updated("Card", entity.Id, $"Card {entity.CardNumber} access updated by VMS");
        }

        public async Task SwapCard(Guid id, CardAccessEdit dto)
        {
            var entity = await _repository.GetByIdEntityAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Card not found with card id " + id);

            SetUpdateAudit(entity);

            //update cardaccess - join table
            var existingAccessIds = entity.CardCardAccesses.Select(ca => ca.CardAccessId).ToList();
            var newAccessIds = dto.CardAccessIds.Where(id => id.HasValue).Select(id => id!.Value).ToList();

            // Tambah yang baru
            var toAdd = newAccessIds.Except(existingAccessIds).ToList();
            foreach (var addId in toAdd)
            {
                var access = await _cardAccessRepository.GetByIdAsync(addId);
                if (access == null)
                    throw new KeyNotFoundException($"Card Access with id {addId} not found");

                entity.CardCardAccesses.Add(new CardCardAccess
                {
                    CardId = entity.Id,
                    CardAccessId = addId,
                    ApplicationId = entity.ApplicationId
                });
            }

            await _repository.UpdateAsync(entity);
             _audit.Updated("Card", entity.Id, $"Card {entity.CardNumber} swapped");
        }

        public async Task UpdateAsync(Guid id, CardUpdateDto updateDto)
        {
            var card = await _repository.GetByIdEntityAsync(id);
            if (card == null)
                throw new KeyNotFoundException("Card not found");

            var existingCard = await _repository.BaseEntityQuery()
            .Where(b => b.Id != id)
            .FirstOrDefaultAsync(b =>
                                b.CardNumber == updateDto.CardNumber ||
                                b.Dmac == updateDto.Dmac);

            if (existingCard != null)
            {
                if (existingCard.CardNumber == updateDto.CardNumber)
                {
                    throw new ArgumentException($"Card with Number {updateDto.CardNumber} already exists.");
                }
                else if (existingCard.Dmac == updateDto.Dmac)
                {
                    throw new ArgumentException($"Card with Mac {updateDto.Dmac} already exists.");
                }
            }

            // Ownership validation for updated values
            if (updateDto.MemberId.HasValue && updateDto.MemberId.Value != card.MemberId)
            {
                var invalidMemberIds = await _repository.CheckInvalidMemberOwnershipAsync(updateDto.MemberId.Value, AppId);
                if (invalidMemberIds.Any())
                    throw new UnauthorizedException($"MemberId does not belong to this Application: {string.Join(", ", invalidMemberIds)}");
            }

            if (updateDto.VisitorId.HasValue && updateDto.VisitorId.Value != card.VisitorId)
            {
                var invalidVisitorIds = await _repository.CheckInvalidVisitorOwnershipAsync(updateDto.VisitorId.Value, AppId);
                if (invalidVisitorIds.Any())
                    throw new UnauthorizedException($"VisitorId does not belong to this Application: {string.Join(", ", invalidVisitorIds)}");
            }

            if (updateDto.CardGroupId.HasValue && updateDto.CardGroupId.Value != card.CardGroupId)
            {
                var invalidCardGroupIds = await _repository.CheckInvalidCardGroupOwnershipAsync(updateDto.CardGroupId.Value, AppId);
                if (invalidCardGroupIds.Any())
                    throw new UnauthorizedException($"CardGroupId does not belong to this Application: {string.Join(", ", invalidCardGroupIds)}");
            }

            if (updateDto.VisitorId != null && updateDto.MemberId == null)
            {
                updateDto.IsUsed = true;
                updateDto.CardStatus = CardStatus.Used;
            }
            else if (updateDto.VisitorId == null && updateDto.MemberId != null)
            {
                updateDto.IsUsed = true;
                updateDto.CardStatus = CardStatus.Used;
            }
            else
            {
                updateDto.IsUsed = false;
                updateDto.CardStatus = CardStatus.Available;
            }

            SetUpdateAudit(card);
            _mapper.Map(updateDto, card);
            await _repository.UpdateAsync(card);
            _mqttQueue.Enqueue("engine/refresh/card-related", "");
             _audit.Updated("Card", card.Id, $"Card {card.CardNumber} updated");
        }

        public async Task AssignToMemberAsync(Guid id, CardAssignDto dto)
        {
            var card = await _repository.GetByIdEntityAsync(id);
            var member = await _mstMemberRepository.GetByIdAsync(dto.MemberId!.Value);
            if (card == null)
                throw new KeyNotFoundException("Card not found");

            if (member == null)
                throw new KeyNotFoundException("Member not found");

            // Ownership validation
            var invalidMemberIds = await _repository.CheckInvalidMemberOwnershipAsync(dto.MemberId.Value, AppId);
            if (invalidMemberIds.Any())
                throw new UnauthorizedException($"MemberId does not belong to this Application: {string.Join(", ", invalidMemberIds)}");

            SetUpdateAudit(card);
            card.IsUsed = true;
            card.CardStatus = CardStatus.Used;
            card.LastUsed = member.Name;
            member.CardNumber = card.CardNumber;
            member.BleCardNumber = card.Dmac;
            _mapper.Map(dto, card);
            await _repository.UpdateAsync(card);
             _audit.Updated("Card", card.Id, $"Card {card.CardNumber} assigned to member {member.Name}");
        }

        public async Task DeleteAsync(Guid id)
        {
            var card = await _repository.GetByIdEntityAsync(id);
            if (card == null)
                throw new KeyNotFoundException("Card not found");

            SetDeleteAudit(card);
            card.StatusCard = 0;
            await _repository.DeleteAsync(id);
             _audit.Deleted("Card", id, $"Card {card.CardNumber} deleted");
        }

        public async Task<IEnumerable<CardRead>> ImportAsync(IFormFile file)
        {
            var cards = new List<Card>();

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1);

            int rowNumber = 2;
            foreach (var row in rows)
            {

               var maskedAreaStr = row.Cell(1).GetValue<string>();

                Guid? maskedAreaId = null;

                if (!string.IsNullOrWhiteSpace(maskedAreaStr))
                {
                    if (!Guid.TryParse(maskedAreaStr, out var parsed))
                        throw new ArgumentException($"Invalid maskedAreaId format at row {rowNumber}");

                    var maskedArea = await _repository.GetMaskedAreaByIdAsync(parsed);
                    if (maskedArea == null)
                        throw new ArgumentException($"maskedAreaId {parsed} not found at row {rowNumber}");

                    maskedAreaId = parsed;
                }


                var card = new Card
                {
                    RegisteredMaskedAreaId = maskedAreaId ?? null,
                    Name = row.Cell(2).GetValue<string>() ?? null,
                    Remarks = row.Cell(3).GetValue<string>() ?? null,
                    CardType = (CardType)Enum.Parse(typeof(CardType), row.Cell(4).GetValue<string>(), ignoreCase: true),
                    CardNumber = row.Cell(5).GetValue<string>() ?? null,
                    QRCode = row.Cell(6).GetValue<string>() ?? null,
                    IsMultiMaskedArea = row.Cell(7).GetValue<bool?>() ?? false,
                    Dmac = row.Cell(8).GetValue<string>() ?? null,
                    ApplicationId = AppId,
                    StatusCard = 1
                };

                SetCreateAudit(card);
                cards.Add(card);
                rowNumber++;
            }

            // Simpan ke database
            foreach (var card in cards)
            {
                await _repository.AddAsync(card);
            }
            _mqttQueue.Enqueue("engine/refresh/card-related","");
             _audit.Created("Card", cards.Count, $"Imported {cards.Count} cards");
            return await _repository.GetAllExportAsync();
        }

        public async Task<object> FilterAsync(
            DataTablesProjectedRequest request,
            CardFilter filter)
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            if (request.DateFilters != null)
            {
                if (request.DateFilters.TryGetValue("UpdatedAt", out var dateFilter))
                {
                    filter.DateFrom = dateFilter.DateFrom;
                    filter.DateTo = dateFilter.DateTo;
                }
            }

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
            QuestPDF.Settings.License = LicenseType.Community;
            var Cards = await _repository.GetAllAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Card Report")
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
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);

                        });

                        // Table header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Card Number").SemiBold();
                            header.Cell().Element(CellStyle).Text("Card Type").SemiBold();
                            header.Cell().Element(CellStyle).Text("Dmac").SemiBold();
                            header.Cell().Element(CellStyle).Text("Is Multi Masked Area").SemiBold();
                            header.Cell().Element(CellStyle).Text("Registered Masked Area").SemiBold();
                            header.Cell().Element(CellStyle).Text("Is Used").SemiBold();
                            header.Cell().Element(CellStyle).Text("Last Used").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                            header.Cell().Element(CellStyle).Text("Card Type").SemiBold();
                        });

                        // Table body
                        int index = 1;
                        foreach (var card in Cards)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(card.Name);
                            table.Cell().Element(CellStyle).Text(card.CardType.ToString());
                            table.Cell().Element(CellStyle).Text(card.CardNumber);
                            table.Cell().Element(CellStyle).Text(card.Dmac);
                            table.Cell().Element(CellStyle).Text(card.IsMultiMaskedArea == true ? "Yes" : "No");
                            table.Cell().Element(CellStyle).Text(card.RegisteredMaskedAreaId.ToString());
                            table.Cell().Element(CellStyle).Text(card.IsUsed == true ? "Yes" : "No");
                            table.Cell().Element(CellStyle).Text(card.LastUsed);
                            table.Cell().Element(CellStyle).Text(card.StatusCard.ToString());
                            table.Cell().Element(CellStyle).Text(card.CardType.ToString());
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
            var Cards = await _repository.GetAllAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Cards");

            // Header
            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Card Type";
            worksheet.Cell(1, 4).Value = "Card Number";
            worksheet.Cell(1, 5).Value = "Dmac";
            worksheet.Cell(1, 6).Value = "Is Multi Masked Area";
            worksheet.Cell(1, 7).Value = "Registered Masked Area";
            worksheet.Cell(1, 8).Value = "Is Used";
            worksheet.Cell(1, 9).Value = "Last Used";
            worksheet.Cell(1, 10).Value = "Status";
            worksheet.Cell(1, 11).Value = "Card Type";
            worksheet.Cell(1, 12).Value = "MaskedAreaId";
            worksheet.Cell(1, 13).Value = "VisitorId";
            worksheet.Cell(1, 14).Value = "MemberId";

            int row = 2;
            int no = 1;

            foreach (var card in Cards)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = card.Name;
                worksheet.Cell(row, 3).Value = card.CardType.ToString();
                worksheet.Cell(row, 4).Value = card.CardNumber;
                worksheet.Cell(row, 5).Value = card.Dmac;
                worksheet.Cell(row, 6).Value = card.IsMultiMaskedArea == true ? "Yes" : "No";
                worksheet.Cell(row, 7).Value = card.RegisteredMaskedAreaId.ToString();
                worksheet.Cell(row, 8).Value = card.IsUsed == true ? "Yes" : "No";
                worksheet.Cell(row, 9).Value = card.LastUsed;
                worksheet.Cell(row, 10).Value = card.StatusCard.ToString();
                worksheet.Cell(row, 11).Value = card.CardType.ToString();
                worksheet.Cell(row, 12).Value = card.RegisteredMaskedAreaId.ToString();
                worksheet.Cell(row, 13).Value = card.VisitorId.ToString();
                worksheet.Cell(row, 14).Value = card.MemberId.ToString();
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
    
}
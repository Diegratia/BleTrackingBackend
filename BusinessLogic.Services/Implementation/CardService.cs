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
using System.IO;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Helpers.Consumer;
using System.Linq.Dynamic.Core;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using LicenseType = QuestPDF.Infrastructure.LicenseType;

namespace BusinessLogic.Services.Implementation
{
    public class CardService : ICardService
    {
        private readonly CardRepository _repository;
        private readonly CardAccessRepository _cardAccessRepository;
        private readonly IMapper _mapper;
        private readonly MstMemberRepository _mstMemberRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CardService(CardRepository repository, CardAccessRepository cardAccessRepository, MstMemberRepository mstMemberRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _cardAccessRepository = cardAccessRepository;
            _mstMemberRepository = mstMemberRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CardDto> GetByIdAsync(Guid id)
        {
            var card = await _repository.GetByIdAsync(id);
            return card == null ? null : _mapper.Map<CardDto>(card);
        }
        public async Task<CardMinimalsDto> GetByIdAsyncV2(Guid id)
        {
            var card = await _repository.GetByIdAsync(id);
            return card == null ? null : _mapper.Map<CardMinimalsDto>(card);
        }

        public async Task<IEnumerable<CardDto>> GetAllAsync()
        {
            var cards = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<CardDto>>(cards);
        }
        public async Task<IEnumerable<CardMinimalsDto>> GetAllAsyncV2()
        {
            var cards = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<CardMinimalsDto>>(cards);
        }
        
                public async Task<IEnumerable<OpenCardDto>> OpenGetAllAsync()
        {
            var cards = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<OpenCardDto>>(cards);
        }
        
        //      public async Task<IEnumerable<CardDto>> GetAllAsync()
        // {
        //     var cards = await _repository.GetAllAsync();
        //     var mappedCards = new List<CardDto>();

        //     foreach (var card in cards)
        //     {
        //                     try
        //                     {
        //                         var dto = _mapper.Map<CardDto>(card);
        //                         mappedCards.Add(dto);
        //                     }
        //                     catch (Exception ex)
        //                     {

        //         }
        //     }

        //     return mappedCards;
        // }


        public async Task<CardDto> CreateAsync(CardCreateDto createDto)
        {
            var existingCard = await _repository.GetAllQueryable()
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

            if (card.VisitorId != null && card.MemberId == null)
            {
                card.IsUsed = true;
            }
            else if (card.VisitorId == null && card.MemberId != null)
            {
                card.IsUsed = true;
            }
            else
            {
                card.IsUsed = false;
            }

            if (card.IsMultiMaskedArea == true)
            {
                card.RegisteredMaskedArea = null;
            }

            card.Id = Guid.NewGuid();
            card.StatusCard = 1;

            card.CreatedAt = DateTime.UtcNow;
            card.CreatedBy = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            card.UpdatedAt = DateTime.UtcNow;
            card.UpdatedBy = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            var createdCard = await _repository.AddAsync(card);
            card.QRCode = createdCard.CardNumber;
            return _mapper.Map<CardDto>(createdCard);
        }
        
            public async Task<CardMinimalsDto> CreatesAsync(CardAddDto dto)
        {
            var existingCard = await _repository.GetAllQueryable()
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
            
            var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId");
            var entity = _mapper.Map<Card>(dto);

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            entity.Id = Guid.NewGuid();
            entity.CreatedBy = username;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.StatusCard = 1;

            if (applicationIdClaim != null)
                entity.ApplicationId = Guid.Parse(applicationIdClaim.Value);

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
                    // 🔹 Tambahkan baris di join table
                    entity.CardCardAccesses.Add(new CardCardAccess
                    {
                        CardId = entity.Id,
                        CardAccessId = access.Id,
                        ApplicationId = entity.ApplicationId
                    });
                }
            }
                var result = await _repository.AddAsync(entity);
                return _mapper.Map<CardMinimalsDto>(result);  
        }

        // public async Task BlockCardAsync(Guid id, CardBlockDto dto)
        // {
        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        //     var Card = await _repository.GetByIdAsync(id);
        //     if (Card.MemberId != null)
        //     {
        //         Card.IsBlock = dto.IsBlock;
        //         Card.UpdatedBy = username;
        //         Card.BlockAt = DateTime.UtcNow;
        //         Card.UpdatedAt = DateTime.UtcNow;
        //     }
        //     else if (Card.VisitorId != null)
        //     {
        //         Card.IsBlock = dto.IsBlock;
        //         Card.UpdatedBy = username;
        //         Card.BlockAt = DateTime.UtcNow;
        //         Card.UpdatedAt = DateTime.UtcNow;
        //     }

        //         await _repository.UpdateAsync(Card);
        // }
        
        public async Task UpdatesAsync(Guid id, CardEditDto dto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            var entity = await _repository.GetAllQueryable()
                .Include(cg => cg.CardCardAccesses)
                .FirstOrDefaultAsync(cg => cg.Id == id);

            if (entity == null)
                throw new KeyNotFoundException("Card Group not found");

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
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;

            // =============================
            // 🔹 Update CardAccesses (join table)
            // =============================
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

            // // Tambah yang baru
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
        }

        public async Task UpdateAccessAsync(Guid id, CardAccessEdit dto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            var entity = await _repository.GetAllQueryable()
                .Include(cg => cg.CardCardAccesses)
                .FirstOrDefaultAsync(cg => cg.Id == id);

            if (entity == null)
                throw new KeyNotFoundException("Card Group not found");

            // Update scalar
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;

            // =============================
            // 🔹 Update CardAccesses (join table)
            // =============================
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

                    // // Tambah yang baru
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
        }

        public async Task UpdateAsync(Guid id, CardUpdateDto updateDto)
        {
            var card = await _repository.GetByIdAsync(id);
            if (card == null)
                throw new KeyNotFoundException("Card not found");

            var existingCard = await _repository.GetAllQueryable()
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

            if (updateDto.VisitorId != null && updateDto.MemberId == null)
            {
                updateDto.IsUsed = true;
            }
            else if (updateDto.VisitorId == null && updateDto.MemberId != null)
            {
                updateDto.IsUsed = true;
            }
            else
            {
                updateDto.IsUsed = false;
            }

            card.UpdatedAt = DateTime.UtcNow;
            card.UpdatedBy = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";

            _mapper.Map(updateDto, card);
            await _repository.UpdateAsync(card);
        }

        public async Task AssignToMemberAsync(Guid id, CardAssignDto dto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var card = await _repository.GetByIdAsync(id);
            var member = await _mstMemberRepository.GetByIdAsync(dto.MemberId.Value);
            if (card == null)
                throw new KeyNotFoundException("Card not found");

            if (member == null)
                throw new KeyNotFoundException("Member not found");

            card.UpdatedAt = DateTime.UtcNow;
            card.IsUsed = true;
            card.LastUsed = member.Name;
            card.UpdatedBy = username;
            member.CardNumber = card.CardNumber;
            member.BleCardNumber = card.Dmac;
            _mapper.Map(dto, card);
            await _repository.UpdateAsync(card);
        }

        public async Task DeleteAsync(Guid id)
        {
            var card = await _repository.GetByIdAsync(id);
            card.UpdatedAt = DateTime.UtcNow;
            card.UpdatedBy = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            card.StatusCard = 0;
            await _repository.DeleteAsync(id);
        }

         public async Task<IEnumerable<CardDto>> ImportAsync(IFormFile file)
        {
            var cards = new List<Card>();
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var userApplicationId = _httpContextAccessor.HttpContext?.User.FindFirst("ApplicationId")?.Value;

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
                    Id = Guid.NewGuid(),
                    RegisteredMaskedAreaId = maskedAreaId,
                    Name = row.Cell(2).GetValue<string>(),
                    Remarks = row.Cell(3).GetValue<string>() ?? null,
                    CardType = (CardType)Enum.Parse(typeof(CardType), row.Cell(4).GetValue<string>(), ignoreCase: true),
                    CardNumber = row.Cell(5).GetValue<string>() ?? null,
                    QRCode = row.Cell(6).GetValue<string>() ?? null,
                    IsMultiMaskedArea = row.Cell(7).GetValue<bool?>() ?? false,
                    Dmac = row.Cell(8).GetValue<string>() ?? null,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    StatusCard = 1
                };

                cards.Add(card);
                rowNumber++;
            }

            // Simpan ke database
            foreach (var card in cards)
            {
                await _repository.AddAsync(card);
            }

            return _mapper.Map<IEnumerable<CardDto>>(cards);
        }

          public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name", "CardNumber", "QRCode" };
            var validSortColumns = new[] { "Name", "CardNumber", "QRCode", "CardType", "IsVisitor", "CreatedAt", "IsUsed", "RegisteredMaskedAreaId", "IsMultiMaskedArea" };

            var filterService = new GenericDataTableService<Card, CardMinimalsDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
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
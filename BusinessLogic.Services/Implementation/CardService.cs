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
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CardService(CardRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CardDto> GetByIdAsync(Guid id)
        {
            var card = await _repository.GetByIdAsync(id);
            return card == null ? null : _mapper.Map<CardDto>(card);
        }

        public async Task<IEnumerable<CardDto>> GetAllAsync()
        {
            var card = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<CardDto>>(card) ?? null;
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
            .FirstOrDefaultAsync(b => b.QRCode == createDto.QRCode ||
                                b.CardNumber == createDto.CardNumber ||
                                b.Dmac == createDto.Dmac);

            if (existingCard != null)
            {
                if (existingCard.QRCode == createDto.QRCode)
                {
                    throw new ArgumentException($"Card with QRCode {createDto.QRCode} already exists.");
                }
                else if (existingCard.CardNumber == createDto.CardNumber)
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
            return _mapper.Map<CardDto>(createdCard);
        }
        

        public async Task UpdateAsync(Guid id, CardUpdateDto updateDto)
        {
            var card = await _repository.GetByIdAsync(id);
            var updatecard = _mapper.Map<Card>(updateDto);
            if (card == null)
                throw new KeyNotFoundException("Card not found");

              var existingCard = await _repository.GetAllQueryable()
            .FirstOrDefaultAsync(b => b.QRCode == updateDto.QRCode ||
                                b.CardNumber == updateDto.CardNumber ||
                                b.Dmac == updateDto.Dmac);
        
        if (existingCard != null)
            {
                if (existingCard.QRCode == updateDto.QRCode)
                {
                    throw new ArgumentException($"Card with QRCode {updateDto.QRCode} already exists.");
                }
                else if (existingCard.CardNumber == updateDto.CardNumber)
                {
                    throw new ArgumentException($"Card with Number {updateDto.CardNumber} already exists.");
                }
                else if (existingCard.Dmac == updateDto.Dmac)
                {
                    throw new ArgumentException($"Card with Mac {updateDto.Dmac} already exists.");
                }
            }

            if (updatecard.VisitorId != null && updatecard.MemberId == null)
            {
                updatecard.IsUsed = true;
            }
            else if (updatecard.VisitorId == null && updatecard.MemberId != null)
            {
                updatecard.IsUsed = true;
            }
            else
            {
                updatecard.IsUsed = false;
            }

            card.UpdatedAt = DateTime.UtcNow;
            card.UpdatedBy = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _mapper.Map(updateDto, card);
            await _repository.UpdateAsync(card);
        }

        public async Task DeleteAsync(Guid id)
        {
            var card = await _repository.GetByIdAsync(id);
            card.UpdatedAt = DateTime.UtcNow;
            card.UpdatedBy = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
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
                if (!Guid.TryParse(maskedAreaStr, out var maskedAreaId))
                    throw new ArgumentException($"Invalid maskedAreaId format at row {rowNumber}");

                var maskedArea = await _repository.GetMaskedAreaByIdAsync(maskedAreaId);
                if (maskedArea == null)
                    throw new ArgumentException($"maskedAreaId {maskedAreaId} not found at row {rowNumber}");


                var card = new Card
                {
                    Id = Guid.NewGuid(),
                    RegisteredMaskedAreaId = maskedAreaId,
                    Name = row.Cell(2).GetValue<string>(),
                    Remarks = row.Cell(3).GetValue<string>() ?? null,
                    CardType = (CardType)Enum.Parse(typeof(CardType), row.Cell(4).GetValue<string>()),
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
            var validSortColumns = new[] { "Name", "CardNumber", "QRCode", "CardType", "IsVisitor", "CreatedAt" };

            var filterService = new GenericDataTableService<Card, CardDto>(
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
                            columns.RelativeColumn(1);
                        });

                        // Table header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Card Number").SemiBold();
                            header.Cell().Element(CellStyle).Text("Card Barcode").SemiBold();
                            header.Cell().Element(CellStyle).Text("Card Type").SemiBold();
                            header.Cell().Element(CellStyle).Text("Is Member").SemiBold();
                        });

                        // Table body
                        int index = 1;
                        foreach (var card in Cards)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(card.Name);
                            table.Cell().Element(CellStyle).Text(card.CardNumber);
                            table.Cell().Element(CellStyle).Text(card.QRCode);
                            table.Cell().Element(CellStyle).Text(card.CardType.ToString());
                            table.Cell().Element(CellStyle).Text(card.IsUsed == true ? "Yes" : "No");
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
            worksheet.Cell(1, 3).Value = "Card Number";
            worksheet.Cell(1, 4).Value = "Card Barcode";
            worksheet.Cell(1, 5).Value = "Card Type";
            worksheet.Cell(1, 6).Value = "Is Member";

            int row = 2;
            int no = 1;

            foreach (var card in Cards)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = card.Name;
                worksheet.Cell(row, 3).Value = card.CardNumber;
                worksheet.Cell(row, 4).Value = card.QRCode;
                worksheet.Cell(row, 5).Value = card.CardType.ToString();
                worksheet.Cell(row, 6).Value = card.IsUsed == true ? "Yes" : "No";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
    
}
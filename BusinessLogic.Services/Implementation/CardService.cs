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
            return _mapper.Map<IEnumerable<CardDto>>(card);
        }

        public async Task<CardDto> CreateAsync(CardCreateDto createDto)
        {
            var card = _mapper.Map<Card>(createDto);
            card.Id = Guid.NewGuid();
            card.StatusCard = true;

            var createdCard = await _repository.AddAsync(card);
            return _mapper.Map<CardDto>(createdCard);
        }

        public async Task UpdateAsync(Guid id, CardUpdateDto updateDto)
        {

            var card = await _repository.GetByIdAsync(id);
            if (card == null)
                throw new KeyNotFoundException("Card not found");

            _mapper.Map(updateDto, card);
            await _repository.UpdateAsync(card);
        }

        public async Task DeleteAsync(Guid id)
        {
            var card = await _repository.GetByIdAsync(id);
            card.StatusCard = false;
            await _repository.DeleteAsync(id);
        }

          public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name", "CardNumber", "CardBarcode" };
            var validSortColumns = new[] { "Name", "CardNumber", "CardBarcode", "CardType", "IsMember" };

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
                            table.Cell().Element(CellStyle).Text(card.CardBarcode);
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
                worksheet.Cell(row, 4).Value = card.CardBarcode;
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
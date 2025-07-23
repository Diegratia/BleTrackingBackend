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
    public class VisitorCardService : IVisitorCardService
    {
        private readonly VisitorCardRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VisitorCardService(VisitorCardRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<VisitorCardDto> GetByIdAsync(Guid id)
        {
            var visitorCard = await _repository.GetByIdAsync(id);
            return visitorCard == null ? null : _mapper.Map<VisitorCardDto>(visitorCard);
        }

        public async Task<IEnumerable<VisitorCardDto>> GetAllAsync()
        {
            var visitorCard = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<VisitorCardDto>>(visitorCard);
        }

        public async Task<VisitorCardDto> CreateAsync(VisitorCardCreateDto createDto)
        {
            var application = await _repository.GetApplicationByIdAsync(createDto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {createDto.ApplicationId} not found.");

        var cardtrue = await _repository.GetCardByIdAsyncTrue(createDto.CardId);
        if (cardtrue != null)
        {
            throw new ArgumentException($"Card with ID {createDto.CardId} is already used.");
        }

        var card = await _repository.GetCardByIdAsync(createDto.CardId);
        if (card == null)
        {
            throw new ArgumentException($"Card with ID {createDto.CardId} not found.");
        }
                

            var visitorCard = _mapper.Map<VisitorCard>(createDto);
            visitorCard.Id = Guid.NewGuid();
            visitorCard.CheckinStatus = 1;
            visitorCard.CreatedAt = DateTime.UtcNow;
            visitorCard.UpdatedAt = DateTime.UtcNow;
            visitorCard.CreatedBy = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value ?? "";
            visitorCard.UpdatedBy = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value ?? "";
            visitorCard.EnableStatus = 1;
            visitorCard.IsVisitor = 1;
            visitorCard.Status = 1;

            if (visitorCard.MemberId == Guid.Empty)
            {
                visitorCard.IsVisitor = 1;
            }
            else if (visitorCard.VisitorId == Guid.Empty)
            {
                visitorCard.IsVisitor = 0;
            }
            visitorCard.Card = card;
            visitorCard.Card.IsUsed = true;

            var createdvisitorCard = await _repository.AddAsync(visitorCard);
            return _mapper.Map<VisitorCardDto>(createdvisitorCard);
        }

        public async Task UpdateAsync(Guid id, VisitorCardUpdateDto updateDto)
        {
            var application = await _repository.GetApplicationByIdAsync(updateDto.ApplicationId);
            var card = await _repository.GetCardByIdAsync(updateDto.CardId);
            if (application == null)
                throw new ArgumentException($"Application with ID {updateDto.ApplicationId} not found.");
            if (card == null)
                throw new ArgumentException($"Card with ID {updateDto.CardId} not found.");
            if (card?.IsUsed == true)
                throw new ArgumentException($"Card with ID {updateDto.CardId} is already used.");

            var visitorCard = await _repository.GetByIdAsync(id);
            if (visitorCard == null)
                throw new KeyNotFoundException("VisitorCard not found");

            if (visitorCard.MemberId == Guid.Empty)
            {
                visitorCard.IsVisitor = 1;
            }
            else if (visitorCard.VisitorId == Guid.Empty)
            {
                visitorCard.IsVisitor = 0;
            }

            visitorCard.UpdatedAt = DateTime.UtcNow;
            visitorCard.UpdatedBy = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value ?? "";

            _mapper.Map(updateDto, visitorCard);
            await _repository.UpdateAsync(visitorCard);
        }

        public async Task DeleteAsync(Guid id)
        {
            var visitorCard = await _repository.GetByIdAsync(id);
            visitorCard.Status = 0;
            visitorCard.Card.IsUsed = false;
            await _repository.DeleteAsync(id);
        }

          public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name" }; 
            var validSortColumns = new[] { "Name" ,  "CardType", "IsVisitor" };

            var filterService = new GenericDataTableService<VisitorCard, VisitorCardDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }

         public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var visitorCards = await _repository.GetAllAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Visitor Card Report")
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
                            header.Cell().Element(CellStyle).Text("Number").SemiBold();
                            header.Cell().Element(CellStyle).Text("Card Type").SemiBold();
                            header.Cell().Element(CellStyle).Text("Is Member").SemiBold();
                        });

                        // Table body
                        int index = 1;
                        foreach (var card in visitorCards)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(card.Name);
                            table.Cell().Element(CellStyle).Text(card.Number);
                            table.Cell().Element(CellStyle).Text(card.CardType.ToString());
                            table.Cell().Element(CellStyle).Text(card.IsVisitor == 1 ? "Yes" : "No");
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
            var visitorCards = await _repository.GetAllAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Visitor Cards");

            // Header
            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Number";
            worksheet.Cell(1, 4).Value = "Card Type";
            worksheet.Cell(1, 5).Value = "Is Member";

            int row = 2;
            int no = 1;

            foreach (var card in visitorCards)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = card.Name;
                worksheet.Cell(row, 3).Value = card.Number;
                worksheet.Cell(row, 4).Value = card.CardType.ToString();
                worksheet.Cell(row, 5).Value = card.IsVisitor == 1 ? "Yes" : "No";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
    
}
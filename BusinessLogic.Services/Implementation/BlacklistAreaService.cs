using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Helpers.Consumer.DtoHelpers;

namespace BusinessLogic.Services.Implementation
{
    public class BlacklistAreaService : IBlacklistAreaService
    {
        private readonly BlacklistAreaRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public BlacklistAreaService(BlacklistAreaRepository repository, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<BlacklistAreaDto> CreateBlacklistAreaAsync(BlacklistAreaCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

           var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            var entity = _mapper.Map<BlacklistArea>(dto);
            entity.Id = Guid.NewGuid();
            entity.Status = 1;
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.CreatedBy = username;
            entity.CreatedAt = DateTime.UtcNow;

            await _repository.AddAsync(entity);
            return _mapper.Map<BlacklistAreaDto>(entity);
        }
        
        public async Task<IEnumerable<BlacklistAreaDto>> CreatesBlacklistAreaAsync(BlacklistAreaRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            var entities = new List<BlacklistArea>();
            foreach (var area in request.BlacklistAreas)
            {
                var entity = _mapper.Map<BlacklistArea>(area);
                entity.Id = Guid.NewGuid();
                entity.Status = 1;
                entity.UpdatedBy = username;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.CreatedBy = username;
                entity.CreatedAt = DateTime.UtcNow;
                entity.VisitorId = request.VisitorId;
                entities.Add(entity);
            }

            await _repository.AddRangeAsync(entities);
            return _mapper.Map<IEnumerable<BlacklistAreaDto>>(entities);
        }


        public async Task<List<BlacklistAreaDto>> CreateBatchBlacklistAreaAsync(List<BlacklistAreaCreateDto> dtos)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var result = new List<BlacklistAreaDto>();
            foreach (var dto in dtos)
            {
                var blacklistArea = _mapper.Map<BlacklistArea>(dto);
                blacklistArea.Id = Guid.NewGuid();
                blacklistArea.CreatedBy = username;
                blacklistArea.UpdatedBy = username;
                blacklistArea.CreatedAt = DateTime.UtcNow;
                blacklistArea.UpdatedAt = DateTime.UtcNow;
                blacklistArea.Status = 1;
                await _repository.AddAsync(blacklistArea);
                result.Add(_mapper.Map<BlacklistAreaDto>(blacklistArea));
            }
            return result;
        }

        public async Task<BlacklistAreaDto> GetBlacklistAreaByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return _mapper.Map<BlacklistAreaDto>(entity);
        }

        public async Task<IEnumerable<BlacklistAreaDto>> GetAllBlacklistAreasAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<BlacklistAreaDto>>(entities);
        }
        
                public async Task<IEnumerable<OpenBlacklistAreaDto>> OpenGetAllBlacklistAreasAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<OpenBlacklistAreaDto>>(entities);
        }

        public async Task UpdateBlacklistAreaAsync(Guid id, BlacklistAreaUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"BlacklistArea with ID {id} not found.");

            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(dto, entity);
            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteBlacklistAreaAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"BlacklistArea with ID {id} not found.");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repository.DeleteAsync(entity);
        }

         public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Visitor.Name", "FloorplanMaskedArea.Name" }; 
            var validSortColumns = new[] { "Visitor.Name", "FloorplanMaskedArea.Name" };

            var filterService = new GenericDataTableService<BlacklistArea, BlacklistAreaDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }

        // public async Task<object> MinimalFilterAsync(DataTablesRequest request)
        // {
        //     var query = _repository.GetAllQueryableMinimal();

        //     var searchableColumns = new[] { "Visitor.Name", "FloorplanMaskedArea.Name" }; 
        //     var validSortColumns = new[] { "Visitor.Name", "FloorplanMaskedArea.Name" };

        //     var filterService = new GenericDataTableService<BlacklistArea, BlacklistAreaDtoMinimal>(
        //         query,
        //         _mapper,
        //         searchableColumns,
        //         validSortColumns);

        //     return await filterService.FilterAsync(request);
        // }


          public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var BlacklistAreas = await _repository.GetAllAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Visitor Blacklist Area Report")
                        .SemiBold().FontSize(16).FontColor(Colors.Black).AlignCenter();

                    page.Content().Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        // Table header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Visitor").SemiBold();
                            header.Cell().Element(CellStyle).Text("Floorplan Masked Area").SemiBold();
                        });

                        // Table body
                        int index = 1;
                        foreach (var blacklistArea in BlacklistAreas)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(blacklistArea.Visitor?.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(blacklistArea.FloorplanMaskedArea?.Name ?? "-");
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
            var BlacklistAreas = await _repository.GetAllAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Visitor Blacklist Areas");

            // Header
            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Visitor";
            worksheet.Cell(1, 3).Value = "Floorplan Masked Area";

            int row = 2;
            int no = 1;

            foreach (var blacklistArea in BlacklistAreas)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = blacklistArea.Visitor?.Name ?? "-";
                worksheet.Cell(row, 3).Value = blacklistArea.FloorplanMaskedArea?.Name ?? "-";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

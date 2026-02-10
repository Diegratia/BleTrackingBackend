using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using ClosedXML.Excel;
using Data.ViewModels;
using Data.ViewModels.Shared.ExceptionHelper;
using DataView;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    public class MstBrandService : BaseService, IMstBrandService 
    {
        private readonly MstBrandRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<MstBrandService> _logger;
        private readonly IAuditEmitter _audit;

        public MstBrandService(MstBrandRepository repository,
        IMapper mapper,
        ILogger<MstBrandService> logger,
        IHttpContextAccessor httpContextAccessor,
        IAuditEmitter audit
        ) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _audit = audit;
        }

        public async Task<MstBrandRead> GetByIdAsync(Guid id)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null) throw new NotFoundException($"Brand with ID {id} not found");
            return _mapper.Map<MstBrandRead>(brand);
        }

        public async Task<IEnumerable<MstBrandRead>> GetAllAsync()
        {
            var brands = await _repository.GetAllAsync();
            return brands;
        }

        public async Task<IEnumerable<MstBrandRead>> OpenGetAllAsync()
        {
            var brands = await _repository.GetAllAsync();
            return brands;
        }

        public async Task<MstBrandRead> CreateAsync(MstBrandCreateDto createDto)
        {
            if (createDto == null) throw new BusinessException("Brand data cannot be null");

            var brand = _mapper.Map<MstBrand>(createDto);
            brand.Status = 1;
            // No audit fields to set

            await _repository.AddAsync(brand);
             _audit.Created(
                "Brand",
                brand.Id,
                "Created Brand",
                new { brand.Name }
            );
            return _mapper.Map<MstBrandRead>(brand);
        }

        public async Task<MstBrandRead> CreateInternalAsync(MstBrandCreateDto createDto)
        {
            if (createDto == null) throw new BusinessException("Brand data cannot be null");

            var brand = _mapper.Map<MstBrand>(createDto);
            brand.Status = 1;
            await _repository.RawAddAsync(brand);
            return _mapper.Map<MstBrandRead>(brand);
        }

        public async Task UpdateAsync(Guid id, MstBrandUpdateDto updateDto)
        {
            if (updateDto == null) throw new BusinessException("Update data cannot be null");
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null)
                throw new NotFoundException($"Brand with ID {id} not found");
            _mapper.Map(updateDto, brand);
             _audit.Updated(
                "Brand",
                brand.Id,
                "Updated Brand",
                new { brand.Name }
            );
            await _repository.UpdateAsync(brand);
        }

        public async Task DeleteAsync(Guid id)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null)
                throw new NotFoundException($"Brand with ID {id} not found");
             _audit.Deleted(
                "Brand",
                brand.Id,
                "Deleted Brand",
                new { brand.Name }
            );
            await _repository.DeleteAsync(brand.Id);
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, MstBrandFilter filter)
        {
            Console.WriteLine($"DEBUG: Filtering Brands with Page: {filter.Page}, PageSize: {filter.PageSize}");

            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn;
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            var (data, total, filtered) = await _repository.FilterAsync(filter);
            return new { draw = request.Draw, recordsTotal = total, recordsFiltered = filtered, data };
        }

        public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            var brands = await _repository.GetAllExportAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Brand Report")
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
                        });

                        // Table header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Tag").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        // Table body
                        int index = 1;
                        foreach (var brand in brands)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(brand.Name);
                            table.Cell().Element(CellStyle).Text(brand.Tag);
                            table.Cell().Element(CellStyle).Text(brand.Status.ToString());
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
            var brands = await _repository.GetAllExportAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Brands");

            // Header
            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Tag";
            worksheet.Cell(1, 4).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var brand in brands)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = brand.Name;
                worksheet.Cell(row, 3).Value = brand.Tag;
                worksheet.Cell(row, 4).Value = brand.Status;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

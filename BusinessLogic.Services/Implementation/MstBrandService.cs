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
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using Microsoft.Extensions.Logging;
namespace BusinessLogic.Services.Implementation
{
    public class MstBrandService : IMstBrandService
    {
        private readonly MstBrandRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<MstBrandService> _logger;

        public MstBrandService(MstBrandRepository repository, IMapper mapper, ILogger<MstBrandService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<MstBrandDto> GetByIdAsync(Guid id)
        {
            var brand = await _repository.GetByIdAsync(id);
            return brand == null ? null : _mapper.Map<MstBrandDto>(brand);
        }

        public async Task<IEnumerable<MstBrandDto>> GetAllAsync()

        {
            var brands = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstBrandDto>>(brands);
        }
        public async Task<IEnumerable<OpenMstBrandDto>> OpenGetAllAsync()

        {
            var brands = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<OpenMstBrandDto>>(brands);
        }

        public async Task<MstBrandDto> CreateAsync(MstBrandCreateDto createDto)
        {
            var brand = _mapper.Map<MstBrand>(createDto);
            brand.Status = 1;

            await _repository.AddAsync(brand);
            return _mapper.Map<MstBrandDto>(brand);
        }

        public async Task<MstBrandDto> CreateInternalAsync(MstBrandCreateDto createDto)
        {
            var brand = _mapper.Map<MstBrand>(createDto);
            brand.Status = 1;

            await _repository.RawAddAsync(brand);
            return _mapper.Map<MstBrandDto>(brand);
        }

        public async Task UpdateAsync(Guid id, MstBrandUpdateDto updateDto)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null)
                throw new KeyNotFoundException("Brand not found");
            _mapper.Map(updateDto, brand);

            await _repository.UpdateAsync(brand);
        }

        public async Task DeleteAsync(Guid id)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null)
                throw new KeyNotFoundException("Brand not found");

            brand.Status = 0;
            await _repository.DeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "Name", "Tag", "Status" };

            var filterService = new GenericDataTableService<MstBrand, MstBrandDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
        public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;
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
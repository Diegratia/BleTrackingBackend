using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using ClosedXML.Excel;
using Data.ViewModels;
using DataView; // For NotFoundException, BusinessException
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;


namespace BusinessLogic.Services.Implementation
{
    public class MstDistrictService : IMstDistrictService
    {
        private readonly MstDistrictRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;
        private readonly IAuditEmitter _audit;


        public MstDistrictService(MstDistrictRepository repository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IMemoryCache cache,
        IAuditEmitter audit
        )
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _audit = audit;
        }

        public async Task<MstDistrictRead> GetByIdAsync(Guid id)
        {
            var district = await _repository.GetByIdAsync(id);
            if (district == null) throw new NotFoundException($"District with id {id} not found");
            return _mapper.Map<MstDistrictRead>(district);
        }

        public async Task<IEnumerable<MstDistrictRead>> GetAllAsync()
        {
            const string cacheKey = "MstDistrictService_GetAll";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<MstDistrictRead> cachedData )&& cachedData != null)
                return cachedData;
            var districts = await _repository.GetAllAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(cacheKey, districts, cacheOptions);
            return districts;
        }

        public async Task<IEnumerable<MstDistrictRead>> OpenGetAllAsync()
        {
            var districts = await _repository.GetAllAsync();
            return districts;
        }

        public async Task<MstDistrictRead> CreateAsync(MstDistrictCreateDto createDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var district = _mapper.Map<MstDistrict>(createDto);

            district.Id = Guid.NewGuid();

            district.CreatedBy = username;
            district.UpdatedBy = username;
            district.CreatedAt = DateTime.UtcNow;
            district.UpdatedAt = DateTime.UtcNow;
            district.Status = 1;

            var createdDistrict = await _repository.AddAsync(district);
             _audit.Created(
                "District",
                createdDistrict.Id,
                "Created District",
                new { createdDistrict.Name }
            );
            _cache.Remove("MstDistrictService_GetAll");
            return _mapper.Map<MstDistrictRead>(createdDistrict);
        }

        public async Task<List<MstDistrictRead>> CreateBatchAsync(List<MstDistrictCreateDto> dtos)
        {
            var createdDistricts = new List<MstDistrictRead>();
            foreach (var dto in dtos)
            {
                var created = await CreateAsync(dto);
                createdDistricts.Add(created);
            }
            return createdDistricts;
        }

        public async Task UpdateAsync(Guid id, MstDistrictUpdateDto updateDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var district = await _repository.GetByIdAsync(id);

            // Note: If Repo GetByIdAsync throws KeyNotFound, it will be caught by middleware as 500 or 404 depending on implementation. 
            // Better to handle it here explicitly if Repo returns nullable, but currently Repo throws KeyNotFound. 
            // We will trust Repo or catch and rethrow if needed. 
            // ideally repo should return null, service throws NotFound.
            // But let's assume standard pattern:

            if (district == null)
                throw new NotFoundException($"District with id {id} not found");

            district.UpdatedAt = DateTime.UtcNow;
            district.UpdatedBy = username;
            _mapper.Map(updateDto, district);
            _cache.Remove("MstDistrictService_GetAll");
            await _repository.UpdateAsync(district);
             _audit.Updated(
                "District",
                district.Id,
                "Updated District",
                new { district.Name }
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            // Use GetByIdAsync to standard check
            var district = await _repository.GetByIdAsync(id);
            if (district == null) throw new NotFoundException($"District with id {id} not found");

            district.UpdatedAt = DateTime.UtcNow;
            district.UpdatedBy = username;
            _cache.Remove("MstDistrictService_GetAll");
            await _repository.DeleteAsync(id);
             _audit.Deleted(
                "District",
                district.Id,
                "Deleted District",
                new { district.Name }
            );
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, MstDistrictFilter filter)
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

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
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            var districts = await _repository.GetAllExportAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master District Report")
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
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        // Table header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Code").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("DistrictHost").SemiBold();
                            header.Cell().Element(CellStyle).Text("CreatedBy").SemiBold();
                            header.Cell().Element(CellStyle).Text("CreatedAt").SemiBold();
                            header.Cell().Element(CellStyle).Text("UpdatedBy").SemiBold();
                            header.Cell().Element(CellStyle).Text("UpdatedAt").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();

                        });


                        // Table body
                        int index = 1;
                        foreach (var district in districts)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(district.Code);
                            table.Cell().Element(CellStyle).Text(district.Name);
                            table.Cell().Element(CellStyle).Text(district.DistrictHost);
                            table.Cell().Element(CellStyle).Text(district.CreatedBy);
                            table.Cell().Element(CellStyle).Text(district.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(district.UpdatedBy);
                            table.Cell().Element(CellStyle).Text(district.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(district.Status.ToString());
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
            var districts = await _repository.GetAllExportAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Districts");

            // Header
            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Code";
            worksheet.Cell(1, 3).Value = "Name";
            worksheet.Cell(1, 4).Value = "District Host";
            worksheet.Cell(1, 5).Value = "Created By";
            worksheet.Cell(1, 6).Value = "Created At";
            worksheet.Cell(1, 7).Value = "Updated By";
            worksheet.Cell(1, 8).Value = "Updated At";
            worksheet.Cell(1, 9).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var district in districts)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = district.Code;
                worksheet.Cell(row, 3).Value = district.Name;
                worksheet.Cell(row, 4).Value = district.DistrictHost;
                worksheet.Cell(row, 5).Value = district.CreatedBy;
                worksheet.Cell(row, 6).Value = district.CreatedAt;
                worksheet.Cell(row, 7).Value = district.UpdatedBy;
                worksheet.Cell(row, 8).Value = district.UpdatedAt;
                worksheet.Cell(row, 9).Value = district.Status.ToString();
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<IEnumerable<MstDistrictRead>> ImportAsync(IFormFile file)
        {
            var districts = new List<MstDistrict>();
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // skip header

            int rowNumber = 2; // start dari baris ke 2
            foreach (var row in rows)
            {

                var district = new MstDistrict
                {
                    Id = Guid.NewGuid(),
                    Code = row.Cell(1).GetValue<string>(),
                    Name = row.Cell(2).GetValue<string>(),
                    DistrictHost = row.Cell(3).GetValue<string>() ?? "",
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                };

                districts.Add(district);
                rowNumber++;
            }

            foreach (var district in districts)
            {
                await _repository.AddAsync(district);
            }

            return _mapper.Map<List<MstDistrictRead>>(districts);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using ClosedXML.Excel;
using Data.ViewModels;
using Data.ViewModels.Shared.ExceptionHelper;
using DataView;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Repositories.Repository;
using Shared.Contracts;


namespace BusinessLogic.Services.Implementation
{
    public class MstOrganizationService : IMstOrganizationService
    {
        private readonly MstOrganizationRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;
        private readonly IAuditEmitter _audit;

        public MstOrganizationService(MstOrganizationRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IMemoryCache cache, IAuditEmitter audit)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _audit = audit;
        }

        public async Task<IEnumerable<MstOrganizationDto>> GetAllOrganizationsAsync()
        {
            const string cacheKey = "MstOrganizationService_GetAll";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<MstOrganizationDto> cachedData))
                return cachedData;
            var organizations = await _repository.GetAllAsync();
            var mapped = _mapper.Map<IEnumerable<MstOrganizationDto>>(organizations);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(cacheKey, mapped, cacheOptions);
            return mapped;
        }

        public async Task<IEnumerable<OpenMstOrganizationDto>> OpenGetAllOrganizationsAsync()
        {
            var organizations = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<OpenMstOrganizationDto>>(organizations);
        }

        public async Task<MstOrganizationDto> GetOrganizationByIdAsync(Guid id)
        {
            var organization = await _repository.GetByIdAsync(id);
            if (organization == null)
                throw new NotFoundException($"Organization with ID {id} not found");
            return _mapper.Map<MstOrganizationDto>(organization);
        }

        public async Task<MstOrganizationDto> CreateOrganizationAsync(MstOrganizationCreateDto dto)
        {
            if (dto == null) throw new BusinessException("Organization data cannot be null");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var organization = _mapper.Map<MstOrganization>(dto);
            organization.Id = Guid.NewGuid();
            organization.Status = 1;
            organization.CreatedBy = username;
            organization.CreatedAt = DateTime.UtcNow;
            organization.UpdatedBy = username;
            organization.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(organization);
             _audit.Created(
                "Organization",
                organization.Id,
                "Created organization",
                new { organization.Name }
            );
            _cache.Remove("MstOrganizationService_GetAll");

            return _mapper.Map<MstOrganizationDto>(organization);
        }

        public async Task UpdateOrganizationAsync(Guid id, MstOrganizationUpdateDto dto)
        {
            if (dto == null) throw new BusinessException("Update data cannot be null");

            var organization = await _repository.GetByIdAsync(id);
            if (organization == null || organization.Status == 0)
                throw new NotFoundException($"Organization with ID {id} not found");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            organization.UpdatedBy = username;
            organization.UpdatedAt = DateTime.UtcNow;
            _mapper.Map(dto, organization);

            _cache.Remove("MstOrganizationService_GetAll");
            await _repository.UpdateAsync(organization);
             _audit.Updated(
                "Organization",
                organization.Id,
                "Updated organization",
                new { organization.Name }
            );
        }

        public async Task DeleteOrganizationAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var organization = await _repository.GetByIdAsync(id);
            if (organization == null)
                throw new NotFoundException($"Organization with ID {id} not found");

            organization.UpdatedBy = username;
            organization.UpdatedAt = DateTime.UtcNow;

            _cache.Remove("MstOrganizationService_GetAll");
            await _repository.DeleteAsync(id);
             _audit.Deleted(
                "Organization",
                organization.Id,
                "Deleted organization",
                new { organization.Name }
            );
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, MstOrganizationFilter filter)
        {
            // Use the passed filter directly
            // Ensure Page and PageSize are set if not properly bound (though deserialization usually handles it, 
            // but DataTables request carries start/length which might override or sync with filter)

            // Sync DataTables params to Filter if needed (or rely on what's passed)
            // Typically if we use DataTablesProjectedRequest, the 'filter' object comes from the JSON 'Filters' prop.
            // But standard DataTables paging (Start, Length) is separate.
            // We should ensure the filter uses the DataTables paging if that's the intent, 
            // OR if the filter DTO already has them.
            // However, the Repository expects the filter DTO to have Page/PageSize.

            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            // If there's extra filtering needed from Columns, map it here:
            // if (request.Columns != null)
            // {
            //     foreach (var col in request.Columns)
            //     {
            //         if (!string.IsNullOrEmpty(col.Search.Value))
            //         {
            //              // Example: Mapping specific column search if needed
            //              // if (col.Name == "Status" && int.TryParse(col.Search.Value, out int status))
            //              //    filter.Status = status;
            //         }
            //     }
            // }

            var (data, total, filtered) = await _repository.FilterAsync(filter);
            return new { draw = request.Draw, recordsTotal = total, recordsFiltered = filtered, data };
        }

        public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            var organizations = await _repository.GetAllExportAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Organization Report")
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
                        });

                        // Table header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Code").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Organization Host").SemiBold();
                            header.Cell().Element(CellStyle).Text("CreatedBy").SemiBold();
                            header.Cell().Element(CellStyle).Text("CreatedAt").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        // Table body
                        int index = 1;
                        foreach (var organization in organizations)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(organization.Code);
                            table.Cell().Element(CellStyle).Text(organization.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(organization.OrganizationHost ?? "-");
                            table.Cell().Element(CellStyle).Text(organization.CreatedBy ?? "-");
                            table.Cell().Element(CellStyle).Text(organization.CreatedAt.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(organization.Status == 1 ? "Active" : "Inactive");
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
            var organizations = await _repository.GetAllExportAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Organizations");

            // Header
            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Code";
            worksheet.Cell(1, 3).Value = "Name";
            worksheet.Cell(1, 4).Value = "Organization Host";
            worksheet.Cell(1, 5).Value = "CreatedBy";
            worksheet.Cell(1, 6).Value = "CreatedAt";
            worksheet.Cell(1, 7).Value = "UpdatedBy";
            worksheet.Cell(1, 8).Value = "UpdatedAt";
            worksheet.Cell(1, 9).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var organization in organizations)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = organization.Code;
                worksheet.Cell(row, 3).Value = organization.Name ?? "-";
                worksheet.Cell(row, 4).Value = organization.OrganizationHost ?? "-";
                worksheet.Cell(row, 5).Value = organization.CreatedBy ?? "-";
                worksheet.Cell(row, 6).Value = organization.CreatedAt.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 7).Value = organization.UpdatedBy ?? "-";
                worksheet.Cell(row, 8).Value = organization.UpdatedAt.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 9).Value = organization.Status == 1 ? "Active" : "Inactive";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<IEnumerable<MstOrganizationDto>> ImportAsync(IFormFile file)
        {
            var organizations = new List<MstOrganization>();
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // skip header

            int rowNumber = 2; // start dari baris ke 2
            foreach (var row in rows)
            {

                var organization = new MstOrganization
                {
                    Id = Guid.NewGuid(),
                    Code = row.Cell(1).GetValue<string>(),
                    Name = row.Cell(2).GetValue<string>(),
                    OrganizationHost = row.Cell(3).GetValue<string>() ?? "",
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                };

                organizations.Add(organization);
                rowNumber++;
            }

            foreach (var organization in organizations)
            {
                await _repository.AddAsync(organization);
            }

            return _mapper.Map<IEnumerable<MstOrganizationDto>>(organizations);
        }

    }
}

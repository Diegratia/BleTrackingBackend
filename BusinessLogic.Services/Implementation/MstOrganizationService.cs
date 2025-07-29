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
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

namespace BusinessLogic.Services.Implementation
{
    public class MstOrganizationService : IMstOrganizationService
    {
        private readonly MstOrganizationRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MstOrganizationService(MstOrganizationRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<MstOrganizationDto>> GetAllOrganizationsAsync()
        {
            var organizations = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstOrganizationDto>>(organizations);
        }

        public async Task<MstOrganizationDto> GetOrganizationByIdAsync(Guid id)
        {
            var organization = await _repository.GetByIdAsync(id);
            return organization == null ? null : _mapper.Map<MstOrganizationDto>(organization);
        }

        public async Task<MstOrganizationDto> CreateOrganizationAsync(MstOrganizationCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var organization = _mapper.Map<MstOrganization>(dto);
            organization.Id = Guid.NewGuid();
            organization.Status = 1;
            organization.CreatedBy = username; 
            organization.CreatedAt = DateTime.UtcNow;
            organization.UpdatedBy = username;
            organization.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(organization);
            return _mapper.Map<MstOrganizationDto>(organization);
        }

        public async Task UpdateOrganizationAsync(Guid id, MstOrganizationUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var organization = await _repository.GetByIdAsync(id);
            if (organization == null || organization.Status == 0)
                throw new KeyNotFoundException($"Organization with ID {id} not found or has been deleted.");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            organization.UpdatedBy = username;
            organization.UpdatedAt = DateTime.UtcNow;
            _mapper.Map(dto, organization);

            await _repository.UpdateAsync(organization);
        }

        public async Task DeleteOrganizationAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var organization = await _repository.GetByIdAsync(id);
            if (organization == null || organization.Status == 0)
                throw new KeyNotFoundException($"Organization with ID {id} not found or already deleted.");

            organization.Status = 0;
            organization.UpdatedBy = username;
            organization.UpdatedAt = DateTime.UtcNow;

            await _repository.DeleteAsync(organization);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name" }; 
            var validSortColumns = new[] { "Name" , "Code", "OrganizationHost", "CreatedAt", "UpdatedAt", "Status" };

            var filterService = new GenericDataTableService<MstOrganization, MstOrganizationDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }

        public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var organizations = await _repository.GetAllAsync();

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
                            table.Cell().Element(CellStyle).Text(organization.CreatedAt.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(organization.CreatedBy ?? "-");
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
                worksheet.Cell(row, 3).Value = organization.Name?? "-";
                worksheet.Cell(row, 4).Value = organization.OrganizationHost?? "-";
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
    }
}

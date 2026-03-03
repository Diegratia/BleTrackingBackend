using AutoMapper;
using BusinessLogic.Services.Interface;
using ClosedXML.Excel;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementation
{
    public class MstIntegrationService : BaseService, IMstIntegrationService
    {
        private readonly MstIntegrationRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAuditEmitter _audit;

        public MstIntegrationService(
            MstIntegrationRepository repository,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _audit = audit;
        }

        public async Task<MstIntegrationRead?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<MstIntegrationRead>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<MstIntegrationDto> CreateAsync(MstIntegrationCreateDto createDto)
        {
            var brand = await _repository.GetBrandByIdAsync(createDto.BrandId);
            if (brand == null)
                throw new ArgumentException($"Brand with ID {createDto.BrandId} not found.");

            var application = await _repository.GetApplicationByIdAsync(createDto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {createDto.ApplicationId} not found.");

            var integration = _mapper.Map<MstIntegration>(createDto);
            integration.Id = Guid.NewGuid();
            integration.Status = 1;
            integration.CreatedBy = UsernameFormToken;
            integration.CreatedAt = DateTime.UtcNow;
            integration.UpdatedBy = UsernameFormToken;
            integration.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(integration);

            _audit.Created("MstIntegration", integration.Id, $"Integration created");

            var result = await _repository.GetByIdAsync(integration.Id);
            return _mapper.Map<MstIntegrationDto>(result);
        }

        public async Task<MstIntegrationDto> CreateRawAsync(MstIntegrationCreateDto createDto)
        {
            var brand = await _repository.GetBrandByIdAsync(createDto.BrandId);
            if (brand == null)
                throw new ArgumentException($"Brand with ID {createDto.BrandId} not found.");

            var application = await _repository.GetApplicationByIdAsync(createDto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {createDto.ApplicationId} not found.");

            var integration = _mapper.Map<MstIntegration>(createDto);
            integration.Id = Guid.NewGuid();
            integration.Status = 1;
            integration.CreatedBy = UsernameFormToken;
            integration.CreatedAt = DateTime.UtcNow;
            integration.UpdatedBy = UsernameFormToken;
            integration.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(integration);

            _audit.Created("MstIntegration", integration.Id, $"Integration created");

            var result = await _repository.GetByIdAsync(integration.Id);
            return _mapper.Map<MstIntegrationDto>(result);
        }

        public async Task UpdateAsync(Guid id, MstIntegrationUpdateDto updateDto)
        {
            var integration = await _repository.GetByIdEntityAsync(id);
            if (integration == null)
                throw new KeyNotFoundException("Integration not found");

            if (integration.BrandId != updateDto.BrandId)
            {
                var brand = await _repository.GetBrandByIdAsync(updateDto.BrandId);
                if (brand == null)
                    throw new ArgumentException($"Brand with ID {updateDto.BrandId} not found.");
                integration.BrandId = updateDto.BrandId;
            }

            integration.UpdatedBy = UsernameFormToken;
            integration.UpdatedAt = DateTime.UtcNow;
            _mapper.Map(updateDto, integration);

            await _repository.UpdateAsync(integration);

            _audit.Updated("MstIntegration", id, $"Integration updated");
        }

        public async Task DeleteAsync(Guid id)
        {
            var integration = await _repository.GetByIdEntityAsync(id);
            if (integration == null)
                throw new KeyNotFoundException("Integration not found");

            integration.UpdatedBy = UsernameFormToken;
            integration.UpdatedAt = DateTime.UtcNow;
            await _repository.SoftDeleteAsync(integration);

            _audit.Deleted("MstIntegration", id, $"Integration deleted");
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, MstIntegrationFilter filter)
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
            var integrations = await _repository.GetAllExportAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Integration Report")
                        .SemiBold().FontSize(16).FontColor(Colors.Black).AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Brand").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Integration Type").SemiBold();
                            header.Cell().Element(CellStyle).Text("API Type Auth").SemiBold();
                            header.Cell().Element(CellStyle).Text("Api Url").SemiBold();
                            header.Cell().Element(CellStyle).Text("Api Auth Username").SemiBold();
                            header.Cell().Element(CellStyle).Text("Api Auth Passwd").SemiBold();
                            header.Cell().Element(CellStyle).Text("Api Key Field").SemiBold();
                            header.Cell().Element(CellStyle).Text("Api Key Value").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created At").SemiBold();
                            header.Cell().Element(CellStyle).Text("Updated At").SemiBold();
                        });

                        int index = 1;
                        foreach (var integration in integrations)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(integration.Brand?.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(integration.IntegrationType.ToString() ?? "-");
                            table.Cell().Element(CellStyle).Text(integration.ApiTypeAuth.ToString() ?? "-");
                            table.Cell().Element(CellStyle).Text(integration.ApiUrl ?? "-");
                            table.Cell().Element(CellStyle).Text(integration.ApiAuthUsername ?? "-");
                            table.Cell().Element(CellStyle).Text(integration.ApiAuthPasswd ?? "-");
                            table.Cell().Element(CellStyle).Text(integration.ApiKeyField ?? "-");
                            table.Cell().Element(CellStyle).Text(integration.ApiKeyValue ?? "-");
                            table.Cell().Element(CellStyle).Text(integration.CreatedAt.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(integration.UpdatedAt.ToString("yyyy-MM-dd"));
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
            var integrations = await _repository.GetAllExportAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Integrations");

            worksheet.Cell(1, 1).Value = "No";
            worksheet.Cell(1, 2).Value = "Brand";
            worksheet.Cell(1, 3).Value = "Name";
            worksheet.Cell(1, 4).Value = "Integration Type";
            worksheet.Cell(1, 5).Value = "API Type Auth";
            worksheet.Cell(1, 6).Value = "Api Url";
            worksheet.Cell(1, 7).Value = "Api Auth Username";
            worksheet.Cell(1, 8).Value = "Api Auth Passwd";
            worksheet.Cell(1, 9).Value = "Api Key Field";
            worksheet.Cell(1, 10).Value = "Api Key Value";
            worksheet.Cell(1, 11).Value = "Created By";
            worksheet.Cell(1, 12).Value = "Created At";
            worksheet.Cell(1, 13).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var integration in integrations)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = integration.Brand?.Name ?? "-";
                worksheet.Cell(row, 3).Value = integration.IntegrationType.ToString();
                worksheet.Cell(row, 4).Value = integration.ApiTypeAuth.ToString();
                worksheet.Cell(row, 5).Value = integration.ApiUrl;
                worksheet.Cell(row, 6).Value = integration.ApiAuthUsername;
                worksheet.Cell(row, 7).Value = integration.ApiAuthPasswd;
                worksheet.Cell(row, 8).Value = integration.ApiKeyField;
                worksheet.Cell(row, 9).Value = integration.ApiKeyValue;
                worksheet.Cell(row, 10).Value = integration.CreatedBy;
                worksheet.Cell(row, 11).Value = integration.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cell(row, 12).Value = integration.Status == 1 ? "Active" : "Inactive";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

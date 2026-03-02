using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    public class MstAccessControlService : BaseService, IMstAccessControlService
    {
        private readonly MstAccessControlRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAuditEmitter _audit;

        public MstAccessControlService(
            MstAccessControlRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IAuditEmitter audit) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _audit = audit;
        }

        public async Task<MstAccessControlRead> GetByIdAsync(Guid id)
        {
            var accessControl = await _repository.GetByIdAsync(id);
            if (accessControl == null)
                throw new NotFoundException($"Access Control with id {id} not found");
            return accessControl;
        }

        public async Task<IEnumerable<MstAccessControlRead>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<MstAccessControlRead>> GetAllUnassignedAsync()
        {
            return await _repository.GetAllUnassignedAsync();
        }

        public async Task<IEnumerable<OpenMstAccessControlDto>> OpenGetAllAsync()
        {
            var accessControls = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<OpenMstAccessControlDto>>(accessControls);
        }

        public async Task<MstAccessControlRead> CreateAsync(MstAccessControlCreateDto createDto)
        {
            var invalidBrandIds = await _repository.CheckInvalidBrandOwnershipAsync(
                createDto.BrandId, AppId);
            if (invalidBrandIds.Any())
                throw new UnauthorizedException($"BrandId does not belong to this Application");

            if (createDto.IntegrationId.HasValue)
            {
                var invalidIntegrationIds = await _repository.CheckInvalidIntegrationOwnershipAsync(
                    createDto.IntegrationId.Value, AppId);
                if (invalidIntegrationIds.Any())
                    throw new UnauthorizedException($"IntegrationId does not belong to this Application");
            }

            var accessControl = _mapper.Map<MstAccessControl>(createDto);
            accessControl.ApplicationId = AppId;
            SetCreateAudit(accessControl);
            accessControl.Status = 1;

            await _repository.AddAsync(accessControl);
            _audit.Created("AccessControl", accessControl.Id, $"AccessControl {accessControl.Name} created");

            return await _repository.GetByIdAsync(accessControl.Id);
        }

        public async Task UpdateAsync(Guid id, MstAccessControlUpdateDto updateDto)
        {
            var accessControl = await _repository.GetByIdEntityAsync(id);
            if (accessControl == null)
                throw new NotFoundException($"Access Control not found");

            if (updateDto.BrandId != accessControl.BrandId)
            {
                var invalidBrandIds = await _repository.CheckInvalidBrandOwnershipAsync(
                    updateDto.BrandId, AppId);
                if (invalidBrandIds.Any())
                    throw new UnauthorizedException($"BrandId does not belong to this Application");
            }

            if (updateDto.IntegrationId.HasValue && updateDto.IntegrationId != accessControl.IntegrationId)
            {
                var invalidIntegrationIds = await _repository.CheckInvalidIntegrationOwnershipAsync(
                    updateDto.IntegrationId.Value, AppId);
                if (invalidIntegrationIds.Any())
                    throw new UnauthorizedException($"IntegrationId does not belong to this Application");
            }

            SetUpdateAudit(accessControl);
            _mapper.Map(updateDto, accessControl);
            await _repository.UpdateAsync(accessControl);
            _audit.Updated("AccessControl", accessControl.Id, $"AccessControl {accessControl.Name} updated");
        }

        public async Task DeleteAsync(Guid id)
        {
            var accessControl = await _repository.GetByIdEntityAsync(id);
            if (accessControl == null)
                throw new NotFoundException("Access Control not found");

            SetDeleteAudit(accessControl);
            await _repository.DeleteAsync(accessControl);
            _audit.Deleted("AccessControl", id, $"AccessControl {accessControl.Name} deleted");
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, MstAccessControlFilter filter)
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
            var accessControls = await _repository.GetAllQueryable().ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Access Control Report")
                        .SemiBold().FontSize(16).FontColor(Colors.Black).AlignCenter();

                    page.Content().Table(table =>
                    {
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
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Type").SemiBold();
                            header.Cell().Element(CellStyle).Text("Description").SemiBold();
                            header.Cell().Element(CellStyle).Text("Channel").SemiBold();
                            header.Cell().Element(CellStyle).Text("Door ID").SemiBold();
                            header.Cell().Element(CellStyle).Text("Raw").SemiBold();
                            header.Cell().Element(CellStyle).Text("Brand").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created At").SemiBold();
                        });

                        int index = 1;
                        foreach (var control in accessControls)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(control.Name);
                            table.Cell().Element(CellStyle).Text(control.Type);
                            table.Cell().Element(CellStyle).Text(control.Description);
                            table.Cell().Element(CellStyle).Text(control.Channel);
                            table.Cell().Element(CellStyle).Text(control.DoorId);
                            table.Cell().Element(CellStyle).Text(control.Raw);
                            table.Cell().Element(CellStyle).Text(control.Brand?.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(control.Status.ToString());
                            table.Cell().Element(CellStyle).Text(control.CreatedAt.ToString("yyyy-MM-dd"));
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
            var accessControls = await _repository.GetAllQueryable().ToListAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Access Controls");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Type";
            worksheet.Cell(1, 4).Value = "Description";
            worksheet.Cell(1, 5).Value = "Channel";
            worksheet.Cell(1, 6).Value = "Door ID";
            worksheet.Cell(1, 7).Value = "Raw";
            worksheet.Cell(1, 8).Value = "Brand";
            worksheet.Cell(1, 9).Value = "Status";
            worksheet.Cell(1, 10).Value = "Created At";

            int row = 2;
            int no = 1;

            foreach (var control in accessControls)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = control.Name;
                worksheet.Cell(row, 3).Value = control.Type;
                worksheet.Cell(row, 4).Value = control.Description;
                worksheet.Cell(row, 5).Value = control.Channel;
                worksheet.Cell(row, 6).Value = control.DoorId;
                worksheet.Cell(row, 7).Value = control.Raw;
                worksheet.Cell(row, 8).Value = control.Brand?.Name ?? "-";
                worksheet.Cell(row, 9).Value = control.Status;
                worksheet.Cell(row, 10).Value = control.CreatedAt.ToString("yyyy-MM-dd");
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

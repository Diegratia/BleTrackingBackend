using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

namespace BusinessLogic.Services.Implementation
{
    public class MstAccessCctvService : BaseService, IMstAccessCctvService
    {
        private readonly MstAccessCctvRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;

        public MstAccessCctvService(
            MstAccessCctvRepository repository,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _audit = audit;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MstAccessCctvRead> GetByIdAsync(Guid id)
        {
            var accessCctv = await _repository.GetByIdAsync(id);
            if (accessCctv == null)
                throw new NotFoundException($"Access CCTV with id {id} not found");
            return accessCctv;
        }

        public async Task<IEnumerable<MstAccessCctvRead>> GetAllAsync()
        {
            var accessCctvs = await _repository.GetAllAsync();
            return accessCctvs;
        }

        public async Task<IEnumerable<MstAccessCctvRead>> GetAllUnassignedAsync()
        {
            var accessCctvs = await _repository.GetAllUnassignedAsync();
            return accessCctvs;
        }

        public async Task<IEnumerable<OpenMstAccessCctvDto>> OpenGetAllAsync()
        {
            var accessCctvs = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<OpenMstAccessCctvDto>>(accessCctvs);
        }

        public async Task<MstAccessCctvDto> CreateAsync(MstAccessCctvCreateDto createDto)
        {
            var accessCctv = _mapper.Map<MstAccessCctv>(createDto);

            accessCctv.Id = Guid.NewGuid();
            accessCctv.Status = 1;
            accessCctv.IsAssigned = false;
            accessCctv.ApplicationId = AppId;

            SetCreateAudit(accessCctv);

            await _repository.AddAsync(accessCctv);

            await _audit.Created(
                "Access CCTV",
                accessCctv.Id,
                "Created Access CCTV",
                new { accessCctv.Name }
            );

            var result = await _repository.GetByIdAsync(accessCctv.Id);
            return _mapper.Map<MstAccessCctvDto>(result);
        }

        public async Task UpdateAsync(Guid id, MstAccessCctvUpdateDto updateDto)
        {
            var accessCctv = await _repository.GetByIdEntityAsync(id);
            if (accessCctv == null)
                throw new NotFoundException($"Access CCTV with id {id} not found");

            if (updateDto.Name != null)
                accessCctv.Name = updateDto.Name;

            if (updateDto.Rtsp != null)
                accessCctv.Rtsp = updateDto.Rtsp;

            if (updateDto.IntegrationId.HasValue)
                accessCctv.IntegrationId = updateDto.IntegrationId;

            SetUpdateAudit(accessCctv);

            await _repository.UpdateAsync(accessCctv);

            await _audit.Updated(
                "Access CCTV",
                accessCctv.Id,
                "Updated Access CCTV",
                new { accessCctv.Name }
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            var accessCctv = await _repository.GetByIdEntityAsync(id);
            if (accessCctv == null)
                throw new NotFoundException($"Access CCTV with id {id} not found");

            if (accessCctv.IsAssigned == true)
                throw new BusinessException("Access CCTV is still in use by floorplan device");

            SetDeleteAudit(accessCctv);
            accessCctv.Status = 0;

            await _audit.Deleted(
                "Access CCTV",
                accessCctv.Id,
                "Deleted Access CCTV",
                new { accessCctv.Name }
            );

            await _repository.SoftDeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, MstAccessCctvFilter filter)
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            // Map Date Filters (Generic Dictionary -> Specific Prop)
            if (request.DateFilters != null)
            {
                if (request.DateFilters.TryGetValue("UpdatedAt", out var dateFilter))
                {
                    filter.DateFrom = dateFilter.DateFrom;
                    filter.DateTo = dateFilter.DateTo;
                }
            }

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
            var accessControls = await _repository.GetAllExportAsync();

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Access CCTV Report")
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
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("RTSP").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created At").SemiBold();
                            header.Cell().Element(CellStyle).Text("Created By").SemiBold();
                        });

                        int index = 1;
                        foreach (var control in accessControls)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(control.Name);
                            table.Cell().Element(CellStyle).Text(control.Rtsp);
                            table.Cell().Element(CellStyle).Text(control.Status.ToString());
                            table.Cell().Element(CellStyle).Text(control.CreatedAt.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(control.CreatedBy ?? "");
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
            var accessControls = await _repository.GetAllExportAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Access CCTVs");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "RTSP";
            worksheet.Cell(1, 4).Value = "Status";
            worksheet.Cell(1, 5).Value = "Created At";
            worksheet.Cell(1, 6).Value = "Created By";

            int row = 2;
            int no = 1;

            foreach (var control in accessControls)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = control.Name;
                worksheet.Cell(row, 3).Value = control.Rtsp;
                worksheet.Cell(row, 4).Value = control.Status;
                worksheet.Cell(row, 5).Value = control.CreatedAt;
                worksheet.Cell(row, 6).Value = control.CreatedBy;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
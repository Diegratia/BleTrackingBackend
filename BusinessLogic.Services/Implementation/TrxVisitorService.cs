using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using System.ComponentModel.DataAnnotations;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using LicenseType = QuestPDF.Infrastructure.LicenseType;



namespace BusinessLogic.Services.Implementation
{
    public class TrxVisitorService : ITrxVisitorService
    {
        private readonly TrxVisitorRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TrxVisitorService(TrxVisitorRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<TrxVisitorDto>> GetAllTrxVisitorsAsync()
        {
            var trxvisitors = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<TrxVisitorDto>>(trxvisitors);
        }    

        public async Task<TrxVisitorDto> GetTrxVisitorByIdAsync(Guid id)
        {
            var trxvisitor = await _repository.GetByIdAsync(id);
            return trxvisitor == null ? null : _mapper.Map<TrxVisitorDto>(trxvisitor);
        }

        public async Task<TrxVisitorDto> CreateTrxVisitorAsync(TrxVisitorCreateDto createDto)
        {
            if (createDto == null) throw new ArgumentNullException(nameof(createDto));

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var trxvisitor = _mapper.Map<TrxVisitor>(createDto);
            trxvisitor.Id = Guid.NewGuid();

            trxvisitor.Status = VisitorStatus.Checkin;
            trxvisitor.CheckinBy = username;
            trxvisitor.CheckedInAt = DateTime.UtcNow;

            await _repository.AddAsync(trxvisitor);
            return _mapper.Map<TrxVisitorDto>(trxvisitor);
        }

        public async Task UpdateTrxVisitorAsync(Guid id, TrxVisitorUpdateDto updateDto)
        {
            if (updateDto == null) throw new ArgumentNullException(nameof(updateDto));

            var trxvisitor = await _repository.GetByIdAsync(id);
            if (trxvisitor == null)
                throw new KeyNotFoundException($"trxvisitor with ID {id} not found.");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            // if (updateDto.Status == "Checkout")
            // {
            //     trxvisitor.CheckoutBy = username;
            //     trxvisitor.CheckedOutAt = DateTime.UtcNow;
            // }
            // else if (updateDto.Status == "Deny")
            // {
            //     trxvisitor.DenyBy = username;
            //     trxvisitor.DenyAt = DateTime.UtcNow;
            // }
            // else if (updateDto.Status == "Block")
            // {
            //     trxvisitor.BlockBy = username;
            //     trxvisitor.BlockAt = DateTime.UtcNow;
            // }
            // else if (updateDto.Status == "Unblock")
            // {
            //     trxvisitor.UnblockAt = DateTime.UtcNow;
            // }

            trxvisitor.UpdatedBy = username;
            trxvisitor.UpdatedAt = DateTime.UtcNow;


            _mapper.Map(updateDto, trxvisitor);
            await _repository.UpdateAsync(trxvisitor);     
        }

        // public async Task DeleteTrxVisitorAsync(Guid id)
        // {
        //     var trxvisitor = await _repository.GetByIdAsync(id);
        //     if (trxvisitor == null)
        //         throw new KeyNotFoundException($"trxvisitor with ID {id} not found.");

        //     await _repository.DeleteAsync(id);
        // }

           public async Task CheckinTrxVisitorAsync(Guid trxVisitorId)
        {
            var trx = await _repository.GetByIdAsync(trxVisitorId);
            // var latestTrx = await _repository.GetLatestUnfinishedByVisitorIdAsync(visitorId);

            if (trx == null) throw new Exception("No active session found");

            if (trx.Status == VisitorStatus.Checkin)
                throw new InvalidOperationException("Already checked in");

                trx.CheckedInAt = DateTime.UtcNow;
                trx.Status = VisitorStatus.Checkin;
                trx.VisitorActiveStatus = VisitorActiveStatus.Active;
                trx.VisitorGroupCode = trx.VisitorGroupCode + 1;
                trx.VisitorNumber = $"VIS{trx.VisitorGroupCode}";
                trx.VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6);
                trx.UpdatedAt = DateTime.UtcNow;
                trx.UpdatedBy = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;          

            await _repository.UpdateAsync(trx);
        }

        public async Task CheckoutVisitorAsync(Guid trxVisitorId)
        {
            var trx = await _repository.GetByIdAsync(trxVisitorId);
            if (trx == null)
                throw new InvalidOperationException("No active session found");

            trx.CheckedOutAt = DateTime.UtcNow;
            trx.Status = VisitorStatus.Checkout;
            trx.UpdatedAt = DateTime.UtcNow;
            trx.UpdatedBy = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            await _repository.UpdateAsync(trx);
        }

            public async Task DeniedVisitorAsync(Guid trxVisitorId, DenyReasonDto denyReasonDto)
        {
            var trx = await _repository.GetByIdAsync(trxVisitorId);
            if (trx == null)
                throw new InvalidOperationException("No active session found");

            trx.DenyAt = DateTime.UtcNow;
            trx.Status = VisitorStatus.Denied;
            trx.UpdatedAt = DateTime.UtcNow;
            trx.UpdatedBy = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            _mapper.Map(denyReasonDto, trx);
            await _repository.UpdateAsync(trx);

        }

        public async Task BlockVisitorAsync(Guid trxVisitorId, BlockReasonDto blockVisitorDto)
        {
            var trx = await _repository.GetByIdAsync(trxVisitorId);
            if (trx == null)
                throw new InvalidOperationException("No active session found");

            trx.BlockAt = DateTime.UtcNow;
            trx.Status = VisitorStatus.Block;
            trx.UpdatedAt = DateTime.UtcNow;
            trx.UpdatedBy = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            _mapper.Map(blockVisitorDto, trx);
            await _repository.UpdateAsync(trx);
        }

        public async Task UnblockVisitorAsync(Guid trxVisitorId)
        {
            var trx = await _repository.GetByIdAsync(trxVisitorId);
            if (trx == null)
                throw new InvalidOperationException("No active session found");

            trx.UnblockAt = DateTime.UtcNow;
            trx.Status = VisitorStatus.Checkin;
            trx.UpdatedAt = DateTime.UtcNow;
            trx.UpdatedBy = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            await _repository.UpdateAsync(trx);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "VisitorNumber", "VisitorCode", "VehiclePlateNumber" };
            var validSortColumns = new[] { "CheckedInAt", "CheckedOutAt", "DenyAt", "BlockAt", "UnBlockAt", "InvitationCreatedAt", "Status", "VisitorNumber", "VisitorCode", "VehiclePlateNumber", "SiteId", "ParkingId" };

            var filterService = new GenericDataTableService<TrxVisitor, TrxVisitorDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }

        public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var trxvisitors = await _repository.GetAllAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Visitor Transaction Report")
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
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Visitor Number").SemiBold();
                            header.Cell().Element(CellStyle).Text("Visitor Code").SemiBold();
                            header.Cell().Element(CellStyle).Text("Vehicle Plate").SemiBold();
                            header.Cell().Element(CellStyle).Text("Checked In At").SemiBold();
                            header.Cell().Element(CellStyle).Text("Checked Out At").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        int index = 1;
                        foreach (var visitor in trxvisitors)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(visitor.VisitorNumber ?? "-");
                            table.Cell().Element(CellStyle).Text(visitor.VisitorCode ?? "-");
                            table.Cell().Element(CellStyle).Text(visitor.VehiclePlateNumber ?? "-");
                            table.Cell().Element(CellStyle).Text(visitor.CheckedInAt?.ToString("yyyy-MM-dd HH:mm"));
                            table.Cell().Element(CellStyle).Text(visitor.CheckedOutAt?.ToString("yyyy-MM-dd HH:mm"));
                            table.Cell().Element(CellStyle).Text(visitor.Status.ToString() ?? "-");
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
            var trxvisitors = await _repository.GetAllAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Visitors");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Visitor Number";
            worksheet.Cell(1, 3).Value = "Visitor Code";
            worksheet.Cell(1, 4).Value = "Vehicle Plate";
            worksheet.Cell(1, 5).Value = "Checked In At";
            worksheet.Cell(1, 6).Value = "Checked In By";
            worksheet.Cell(1, 7).Value = "Checked Out At";
            worksheet.Cell(1, 8).Value = "Checked Out By";
            worksheet.Cell(1, 9).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var visitor in trxvisitors)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = visitor.VisitorNumber ?? "-";
                worksheet.Cell(row, 3).Value = visitor.VisitorCode ?? "-";
                worksheet.Cell(row, 4).Value = visitor.VehiclePlateNumber ?? "-";
                worksheet.Cell(row, 5).Value = visitor.CheckedInAt?.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cell(row, 6).Value = visitor.CheckinBy ?? "-";
                worksheet.Cell(row, 7).Value = visitor.CheckedOutAt?.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cell(row, 8).Value = visitor.CheckoutBy ?? "-";
                worksheet.Cell(row, 9).Value = visitor.Status.ToString() ?? "-";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

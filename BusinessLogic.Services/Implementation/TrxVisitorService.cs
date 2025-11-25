using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Helpers.Consumer;
using Helpers.Consumer.DtoHelpers.MinimalDto;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using System.ComponentModel.DataAnnotations;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using LicenseType = QuestPDF.Infrastructure.LicenseType;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using  Data.ViewModels.Dto.Helpers.MinimalDto;
using Helpers.Consumer.Mqtt;
using Microsoft.Extensions.Logging;
// using Data.ViewModels.Dto.Helpers.MinimalDto;
// using Helpers.Consumer.Mqtt;


namespace BusinessLogic.Services.Implementation
{
    public class TrxVisitorService : ITrxVisitorService
    {
        private readonly TrxVisitorRepository _repository;
        private readonly ICardRecordService _cardRecordService;
        private readonly CardRepository _cardRepository;
        private readonly CardRecordRepository _cardRecordRepository;
        private readonly VisitorRepository _visitorRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TrxVisitor> _logger;
        private readonly IMqttClientService _mqttClient;

        public TrxVisitorService(
            TrxVisitorRepository repository,
            CardRecordRepository cardRecordRepository,
            CardRepository cardRepository,
            VisitorRepository visitorRepository,
            ICardRecordService cardRecordService,
            // IMqttClientService mqttClientService,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TrxVisitor> logger,
            IMqttClientService mqttClient)
        {
            _repository = repository;
            _cardRecordRepository = cardRecordRepository;
            _cardRecordService = cardRecordService;
            _visitorRepository = visitorRepository;
            _cardRepository = cardRepository;
            // _mqttClientService = mqttClientService;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _mqttClient = mqttClient;
        }

        public async Task<IEnumerable<TrxVisitorDto>> GetAllTrxVisitorsAsync()
        {
            var trxvisitors = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<TrxVisitorDto>>(trxvisitors);
        }  

              public async Task<IEnumerable<OpenTrxVisitorDto>> OpenGetAllTrxVisitorsAsync()
        {
            var trxvisitors = await _repository.OpenGetAllAsync();
            return _mapper.Map<IEnumerable<OpenTrxVisitorDto>>(trxvisitors);
        }     

        public async Task<IEnumerable<TrxVisitorDtoz>> GetAllTrxVisitorsAsyncMinimal()
        {
            return await _repository.GetAllQueryableMinimal().ToListAsync();
        }


        public async Task<TrxVisitorDto> GetTrxVisitorByIdAsync(Guid id)
        {
            var trxvisitor = await _repository.GetByIdAsync(id);
            return trxvisitor == null ? null : _mapper.Map<TrxVisitorDto>(trxvisitor);
        }

            public async Task<TrxVisitorDto> GetTrxVisitorByPublicIdAsync(Guid id)
        {
            var trxvisitor = await _repository.GetByPublicIdAsync(id);
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

        public async Task CheckinVisitorAsync(TrxVisitorCheckinDto dto)
        {
            if (dto.TrxVisitorId == Guid.Empty)
                throw new ArgumentException("TrxVisitorId is required.");
            if (dto.CardId == Guid.Empty)
                throw new ArgumentException("CardId is required.");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var trx = await _repository.GetByIdAsync(dto.TrxVisitorId);
            var card = await _cardRepository.GetByIdAsync(dto.CardId);

            if (trx == null)
                throw new Exception("No active session found");

            if (trx.Status == VisitorStatus.Checkin)
                throw new InvalidOperationException("Already checked in");

            if (!trx.VisitorId.HasValue)
                throw new InvalidOperationException("VisitorId is null");

            var allowedStatuses = new[] { VisitorStatus.Precheckin, VisitorStatus.Waiting }.Cast<VisitorStatus>().ToList();
            if (!allowedStatuses.Contains(trx.Status.Value))
                throw new InvalidOperationException($"Visitor status is not valid for checkin. Current status: {trx.Status}. Allowed statuses: {string.Join(", ", allowedStatuses)}.");

            var activeTrx = await _repository.GetAllQueryable()
                .Where(t => t.VisitorId == trx.VisitorId && t.Status == VisitorStatus.Checkin && t.CheckedOutAt == null && t.Id != trx.Id)
                .AnyAsync();
            if (activeTrx)
                throw new InvalidOperationException("Visitor already has an active transaction");

            using IDbContextTransaction transaction = await _repository.BeginTransactionAsync();
            try
            {
                // Update TrxVisitor
                trx.CheckedInAt = DateTime.UtcNow;
                trx.CheckinBy = username;
                trx.Status = VisitorStatus.Checkin;
                trx.VisitorActiveStatus = VisitorActiveStatus.Active;
                trx.UpdatedAt = DateTime.UtcNow;
                trx.UpdatedBy = username;
                trx.CardNumber = card?.CardNumber;

                await _repository.UpdateAsync(trx);

                var createDto = new CardRecordCreateDto
                {
                    CardId = dto.CardId,
                    VisitorId = trx.VisitorId,
                    // TrxVisitorId = dto.TrxVisitorId 
                };

                await _cardRecordService.CreateAsync(createDto);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            await _mqttClient.PublishAsync("engine/refresh/visitor-related","");
        }

        // public async Task CheckinVisitorAsync(TrxVisitorCheckinDto dto)
        // {
        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        //     var trx = await _repository.GetByIdAsync(dto.TrxVisitorId);

        //     if (trx == null)
        //         throw new Exception("No active session found");

        //     if (trx.Status == VisitorStatus.Checkin)
        //         throw new InvalidOperationException("Already checked in");

        //     // Validasi multiple TrxVisitor aktif
        //     var activeTrx = await _repository.GetAllQueryable()
        //         .Where(t => t.VisitorId == trx.VisitorId && t.Status == VisitorStatus.Checkin && t.CheckedOutAt == null && t.Id != trx.Id)
        //         .AnyAsync();
        //     if (activeTrx)
        //         throw new InvalidOperationException("Visitor already has an active transaction");

        //     // Update TrxVisitor
        //     trx.CheckedInAt = DateTime.UtcNow;
        //     trx.CheckinBy = username;
        //     trx.Status = VisitorStatus.Checkin;
        //     trx.VisitorActiveStatus = VisitorActiveStatus.Active;
        //     trx.UpdatedAt = DateTime.UtcNow;
        //     trx.UpdatedBy = username;

        //     await _repository.UpdateAsync(trx);

        //     // Create CardRecord
        //     var createDto = new CardRecordCreateDto
        //     {
        //         CardId = dto.CardId,
        //         VisitorId = trx.VisitorId
        //     };

        //     await _cardRecordService.CreateAsync(createDto);
        // }

        public async Task CheckoutVisitorAsync(Guid trxVisitorId)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var trx = await _repository.OpenGetByIdAsync(trxVisitorId);
            // var trx = await _repository.GetByIdAsync(trxVisitorId);

            if (trx == null)
                throw new Exception("No active session found");

            var allowedStatuses = new[] { VisitorStatus.Checkin, VisitorStatus.Unblock, VisitorStatus.Block }.Cast<VisitorStatus>().ToList();
            if (!allowedStatuses.Contains(trx.Status.Value))
                throw new InvalidOperationException($"Visitor status is not valid for checkout. Current status: {trx.Status}. Allowed statuses: {string.Join(", ", allowedStatuses)}.");

            // if (trx.Status != VisitorStatus.Checkin || trx.Status !=VisitorStatus.Block ||  trx.Status !=VisitorStatus.Denied || trx.Status !=VisitorStatus.Unblock)
            //     throw new InvalidOperationException("Visitor Status are not valid, can't checkout, visitor status now are " + trx.Status);

            if (!trx.VisitorId.HasValue)
                throw new InvalidOperationException("VisitorId is null");

            using IDbContextTransaction transaction = await _repository.BeginTransactionAsync();
            try
            {

                trx.CheckedOutAt = DateTime.UtcNow;
                trx.CheckoutBy = username;
                trx.Status = VisitorStatus.Checkout;
                trx.VisitorActiveStatus = VisitorActiveStatus.Expired;
                trx.UpdatedAt = DateTime.UtcNow;
                trx.UpdatedBy = username;

                await _repository.UpdateAsync(trx);

                var visitor = await _visitorRepository.GetByIdAsync(trx.VisitorId!.Value);
                if (visitor == null)
                    throw new KeyNotFoundException("Visitor not found");

                visitor.VisitorGroupCode = null;
                visitor.VisitorNumber = null;
                visitor.VisitorCode = null;

                await _visitorRepository.UpdateAsync(visitor);

                // Find and checkout the corresponding CardRecord
                var cardRecord = await _cardRecordRepository.GetAllQueryable()
                        .Where(cr => cr.VisitorId == trx.VisitorId && cr.CheckoutAt == null && cr.Status == 1)
                        .OrderByDescending(cr => cr.CheckinAt)
                        .FirstOrDefaultAsync();

                if (cardRecord != null)
                {
                    await _cardRecordService.CheckoutCard(cardRecord.Id);
                }
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
             await _mqttClient.PublishAsync("engine/refresh/visitor-related","");
        }

        public async Task CheckoutWithVisitorIdAsync(Guid visitorId)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            // Cari transaksi aktif berdasarkan VisitorId
            var trx = await _repository.GetAllQueryable()
                .Where(t => t.VisitorId == visitorId &&
                            (t.Status == VisitorStatus.Checkin ||
                             t.Status == VisitorStatus.Block ||
                             t.Status == VisitorStatus.Unblock))
                .OrderByDescending(t => t.CheckedInAt)
                .FirstOrDefaultAsync();

            if (trx == null)
                throw new Exception("No active transaction found for this visitor.");

            if (!trx.VisitorId.HasValue)
                throw new InvalidOperationException("VisitorId is null.");

            using IDbContextTransaction transaction = await _repository.BeginTransactionAsync();
            try
            {
                trx.CheckedOutAt = DateTime.UtcNow;
                trx.CheckoutBy = username;
                trx.Status = VisitorStatus.Checkout;
                trx.VisitorActiveStatus = VisitorActiveStatus.Expired;
                trx.UpdatedAt = DateTime.UtcNow;
                trx.UpdatedBy = username;

                await _repository.UpdateAsync(trx);

                // Update visitor info
                var visitor = await _visitorRepository.GetByIdAsync(visitorId);
                if (visitor == null)
                    throw new KeyNotFoundException("Visitor not found.");

                visitor.VisitorGroupCode = null;
                visitor.VisitorNumber = null;
                visitor.VisitorCode = null;

                await _visitorRepository.UpdateAsync(visitor);

                // Checkout kartu yang masih aktif
                var cardRecord = await _cardRecordRepository.GetAllQueryable()
                    .Where(cr => cr.VisitorId == visitorId && cr.CheckoutAt == null && cr.Status == 1)
                    .OrderByDescending(cr => cr.CheckinAt)
                    .FirstOrDefaultAsync();

                if (cardRecord != null)
                {
                    await _cardRecordService.CheckoutCard(cardRecord.Id);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        await _mqttClient.PublishAsync("engine/refresh/visitor-related","");
}


        public async Task DeniedVisitorAsync(Guid trxVisitorId, DenyReasonDto denyReasonDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var trx = await _repository.OpenGetByIdAsync(trxVisitorId);
            // var trx = await _repository.GetByIdAsync(trxVisitorId);
            if (trx == null)
                throw new InvalidOperationException("No active session found");

            trx.VisitorGroupCode = null;
            trx.VisitorNumber = null;
            trx.VisitorCode = null;
            trx.DenyAt = DateTime.UtcNow;
            trx.Status = VisitorStatus.Denied;
            trx.VisitorActiveStatus = VisitorActiveStatus.Cancelled;
            trx.UpdatedAt = DateTime.UtcNow;
            trx.DenyBy = username;
            trx.UpdatedBy = username;

            _mapper.Map(denyReasonDto, trx);
            await _repository.UpdateAsync(trx);
             await _mqttClient.PublishAsync("engine/refresh/visitor-related","");
        }

        public async Task BlockVisitorAsync(Guid trxVisitorId, BlockReasonDto blockVisitorDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var trx = await _repository.OpenGetByIdAsync(trxVisitorId);
            // var trx = await _repository.GetByIdAsync(trxVisitorId);
            if (trx == null)
                throw new InvalidOperationException("No active session found");

            trx.BlockAt = DateTime.UtcNow;
            trx.Status = VisitorStatus.Block;
            trx.BlockBy = username;
            trx.UpdatedAt = DateTime.UtcNow;
            trx.UpdatedBy = username;

            _mapper.Map(blockVisitorDto, trx);
            await _repository.UpdateAsync(trx);
             await _mqttClient.PublishAsync("engine/refresh/visitor-related","");
        }

        public async Task UnblockVisitorAsync(Guid trxVisitorId)
        {
            var trx = await _repository.OpenGetByIdAsync(trxVisitorId);
            // var trx = await _repository.GetByIdAsync(trxVisitorId);
            if (trx == null)
                throw new InvalidOperationException("No active session found");
            // var visitor = await _visitorRepository.GetByIdAsync(trx.VisitorId.Value);
            // var visitorCard = await _cardRepository.GetByCardNumberAsync(visitor.CardNumber);
            // 
            trx.UnblockAt = DateTime.UtcNow;
            trx.Status = VisitorStatus.Unblock;
            trx.UpdatedAt = DateTime.UtcNow;
            trx.UpdatedBy = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            // visitorCard.UpdatedAt = DateTime.UtcNow;
            // visitorCard.UpdatedBy = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            await _repository.UpdateAsync(trx);
            await _mqttClient.PublishAsync("engine/refresh/visitor-related","");
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var enumColumns = new Dictionary<string, Type>
            {
                { "Status", typeof(VisitorStatus)},
                { "Gender", typeof(Gender) },
                { "VisitorActiveStatus", typeof(VisitorActiveStatus) },
                { "IdentityType", typeof(IdentityType) }
            };

            var searchableColumns = new[] { "Visitor.Name", "Visitor.IdentityId", "Visitor.PersonId", "Visitor.BleCardNumber", "CardNumber" };
            var validSortColumns = new[] { "InvitationCreatedAt", "Visitor.Name", "CheckedInAt", "CheckedOutAt", "DenyAt", "BlockAt", "UnBlockAt", "Status", "VisitorNumber", "VisitorCode", "VehiclePlateNumber", "Member.Name", "MaskedArea.Name", "VisitorActiveStatus", "Gender", "IdentityType","VisitorActiveStatus", "EmailVerficationSendAt", "VisitorPeriodStart", "VisitorPeriodEnd" };

            var filterService = new GenericDataTableService<TrxVisitor, TrxVisitorDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns,
                enumColumns);
                

            return await filterService.FilterAsync(request);
        }
        
        
        
           public async Task<object> MinimalFilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryableMinimal();

            var enumColumns = new Dictionary<string, Type>
            {
                { "Status", typeof(VisitorStatus) },
                { "Gender", typeof(Gender) },
                { "VisitorActiveStatus", typeof(VisitorActiveStatus) },
                { "IdentityType", typeof(IdentityType) }
            };

            var searchableColumns = new[] { "Visitor.Name", "Visitor.IdentityId", "Visitor.PersonId", "Visitor.BleCardNumber" };
            var validSortColumns = new[] { "Visitor.Name", "CheckedInAt", "CheckedOutAt", "DenyAt", "BlockAt", "UnBlockAt", "InvitationCreatedAt", "Status", "VisitorNumber", "VisitorCode", "VehiclePlateNumber", "Member.Name", "MaskedArea.Name", "VisitorActiveStatus", "Gender", "IdentityType", "VisitorActiveStatus", "EmailVerficationSendAt", "VisitorPeriodStart", "VisitorPeriodEnd" };

            var filterService = new MinimalGenericDataTableService<TrxVisitorDtoz>(
              query,
              _mapper,
              searchableColumns,
              validSortColumns,
              enumColumns);

            return await filterService.FilterAsync(request);
        }

        public async Task<object> FilterRawAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryableRaw();

            var enumColumns = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "Status", typeof(VisitorStatus) },
                { "Gender", typeof(Gender) },
                { "VisitorActiveStatus", typeof(VisitorActiveStatus) },
                { "IdentityType", typeof(IdentityType) }
            };

            var searchableColumns = new[] { "Visitor.Name", "Visitor.IdentityId", "Visitor.PersonId", "Visitor.BleCardNumber", "CardNumber" };
            var validSortColumns = new[] { "Visitor.Name", "CheckedInAt", "CheckedOutAt", "DenyAt", "BlockAt", "UnBlockAt", "InvitationCreatedAt", "Status", "VisitorNumber", "VisitorCode", "VehiclePlateNumber", "Member.Name", "MaskedArea.Name", "VisitorActiveStatus", "Gender", "IdentityType", "VisitorActiveStatus", "EmailVerficationSendAt", "VisitorPeriodStart", "VisitorPeriodEnd" };

            var filterService = new GenericDataTableService<TrxVisitor, OpenTrxVisitorDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns,
                enumColumns);

            return await filterService.FilterAsync(request);
        }

        public async Task ExtendedVisitorTime(Guid trxVisitorId, ExtendedTimeDto dto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var trx = await _repository.OpenGetByTrxIdAsync(trxVisitorId);
            // var trx = await _repository.GetByIdAsync(trxVisitorId);

            if (trx == null)
                throw new Exception("No active session found");
            var additionalTime = TimeSpan.FromMinutes(dto.ExtendedVisitorTime);
            trx.ExtendedVisitorTime += dto.ExtendedVisitorTime;
            trx.VisitorPeriodEnd = trx.VisitorPeriodEnd.Value.Add(additionalTime);

            // trx.VisitorActiveStatus = VisitorActiveStatus.Extended;
            trx.UpdatedAt = DateTime.UtcNow;
            trx.UpdatedBy = username;
            trx.ExtendedVisitorTime = dto.ExtendedVisitorTime;
            await _repository.UpdateAsync(trx);
            await _mqttClient.PublishAsync("engine/refresh/visitor-related","");
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

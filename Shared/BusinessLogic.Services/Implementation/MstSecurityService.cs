using AutoMapper;
using BusinessLogic.Services.Background;
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
using Shared.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BusinessLogic.Services.Extension.FileStorageService;
using Shared.Contracts.Read;
using DataView;

namespace BusinessLogic.Services.Implementation
{
    public class MstSecurityService : BaseService, IMstSecurityService
    {
        private readonly MstSecurityRepository _repository;
        private readonly CardRepository _cardRepository;
        private readonly IMapper _mapper;
        private readonly string[] _allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
        private const long MaxFileSize = 5 * 1024 * 1024; // Max 5 MB
        private readonly ILogger<MstSecurityService> _logger;
        private readonly IMqttPubQueue _mqttQueue;
        private readonly IFileStorageService _fileStorageService;
        private readonly IAuditEmitter _audit;

        public MstSecurityService(
            MstSecurityRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            CardRepository cardRepository,
            ILogger<MstSecurityService> logger,
            IMqttPubQueue mqttQueue,
            IFileStorageService fileStorageService,
            IAuditEmitter audit) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _cardRepository = cardRepository;
            _logger = logger;
            _mqttQueue = mqttQueue;
            _fileStorageService = fileStorageService;
            _audit = audit;
        }

        public async Task<IEnumerable<MstSecurityRead>> GetAllSecuritiesAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<MstSecurityRead>> GetAllSecurityHeadsAsync()
        {
            return await _repository.GetAllSecurityHeadAsync();
        }

        public async Task<IEnumerable<MstSecurityLookUpRead>> GetAllLookUpAsync(bool? headsOnly = null)
        {
            return await _repository.GetAllLookUpAsync(headsOnly);
        }

        public async Task<IEnumerable<OpenMstSecurityDto>> OpenGetAllSecuritiesAsync()
        {
            var securities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<OpenMstSecurityDto>>(securities);
        }

        public async Task<MstSecurityRead> GetSecurityByIdAsync(Guid id)
        {
            var security = await _repository.GetByIdAsync(id);
            if (security == null)
                throw new NotFoundException($"Security {id} not found");
            if (security.Status == 0)
                throw new BusinessException("Security is inactive", "SECURITY_INACTIVE");
            return security;
        }

        public async Task<MstSecurityRead> CreateSecurityAsync(MstSecurityCreateDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (createDto.OrganizationId.HasValue)
            {
                var invalidOrgIds = await _repository.CheckInvalidOrganizationOwnershipAsync(
                    createDto.OrganizationId.Value, AppId);
                if (invalidOrgIds.Any())
                    throw new UnauthorizedException(
                        $"OrganizationId does not belong to this Application: {string.Join(", ", invalidOrgIds)}");
            }

            if (createDto.DepartmentId.HasValue)
            {
                var invalidDeptIds = await _repository.CheckInvalidDepartmentOwnershipAsync(
                    createDto.DepartmentId.Value, AppId);
                if (invalidDeptIds.Any())
                    throw new UnauthorizedException(
                        $"DepartmentId does not belong to this Application: {string.Join(", ", invalidDeptIds)}");
            }

            if (createDto.DistrictId.HasValue)
            {
                var invalidDistrictIds = await _repository.CheckInvalidDistrictOwnershipAsync(
                    createDto.DistrictId.Value, AppId);
                if (invalidDistrictIds.Any())
                    throw new UnauthorizedException(
                        $"DistrictId does not belong to this Application: {string.Join(", ", invalidDistrictIds)}");
            }

            var card = await _cardRepository.GetByIdEntityAsync(createDto.CardId!.Value);
            if (card == null)
                throw new NotFoundException($"Card {createDto.CardId} not found.");

            // Validate SecurityHead1Id and SecurityHead2Id must be heads
            var securityHeadIds = new List<Guid?>();
            if (createDto.SecurityHead1Id.HasValue)
                securityHeadIds.Add(createDto.SecurityHead1Id);
            if (createDto.SecurityHead2Id.HasValue)
                securityHeadIds.Add(createDto.SecurityHead2Id);

            if (securityHeadIds.Any())
            {
                var invalidHeadIds = await _repository.ValidateSecurityHeadsAsync(securityHeadIds);
                if (invalidHeadIds.Any())
                {
                    throw new BusinessException(
                        $"The following SecurityHead IDs are not valid heads (IsHead = false): {string.Join(", ", invalidHeadIds)}");
                }
            }

            var existingSecurity = await _repository.GetAllQueryable()
                .FirstOrDefaultAsync(b => b.Email == createDto.Email ||
                                         b.IdentityId == createDto.IdentityId ||
                                         b.PersonId == createDto.PersonId);

            if (existingSecurity != null)
            {
                if (existingSecurity.Email == createDto.Email)
                    throw new BusinessException($"Security with Email {createDto.Email} already exists.");
                if (existingSecurity.IdentityId == createDto.IdentityId)
                    throw new BusinessException($"Security with IdentityId {createDto.IdentityId} already exists.");
                if (existingSecurity.PersonId == createDto.PersonId)
                    throw new BusinessException($"Security with PersonId {createDto.PersonId} already exists.");
            }

            if (card.IsUsed == true)
                throw new BusinessException("Card already checked in by another security.");

            var security = _mapper.Map<MstSecurity>(createDto);
            security.ApplicationId = AppId;

            if (createDto.FaceImage != null && createDto.FaceImage.Length > 0)
            {
                try
                {
                    security.FaceImage = await _fileStorageService
                        .SaveImageAsync(createDto.FaceImage, "SecurityFaceImages", MaxFileSize, ImagePurpose.Photo);
                    security.UploadFr = 1;
                    security.UploadFrError = "Upload successful";
                }
                catch (Exception ex)
                {
                    security.UploadFr = 2;
                    security.UploadFrError = ex.Message;
                    security.FaceImage = null;
                }
            }
            else
            {
                security.UploadFr = 0;
                security.UploadFrError = "No file uploaded";
                security.FaceImage = null;
            }

            SetCreateAudit(security);
            security.BleCardNumber = card.Dmac;
            security.CardNumber = card.CardNumber;

            using var transaction = await _repository.BeginTransactionAsync();
            try
            {
                await _repository.AddAsync(security);
                card.IsUsed = true;
                card.CardStatus = CardStatus.Used;
                card.LastUsed = security.Name;
                card.SecurityId = security.Id;
                card.CheckinAt = DateTime.UtcNow;
                await _cardRepository.UpdateAsync(card);

                await transaction.CommitAsync();
                _mqttQueue.Enqueue("engine/refresh/card-related", "");
                _audit.Created("Security", security.Id, $"Security {security.Name} created");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await _repository.GetByIdAsync(security.Id);
        }

        public async Task<MstSecurityRead> UpdateSecurityAsync(Guid id, MstSecurityUpdateDto updateDto)
        {
            var security = await _repository.GetByIdEntityAsync(id);
            if (security == null)
                throw new NotFoundException($"Security with ID {id} not found or has been deleted.");

            if (updateDto.OrganizationId.HasValue && updateDto.OrganizationId != security.OrganizationId)
            {
                var invalidOrgIds = await _repository.CheckInvalidOrganizationOwnershipAsync(
                    updateDto.OrganizationId.Value, AppId);
                if (invalidOrgIds.Any())
                    throw new UnauthorizedException(
                        $"OrganizationId does not belong to this Application: {string.Join(", ", invalidOrgIds)}");
            }

            if (updateDto.DepartmentId.HasValue && updateDto.DepartmentId != security.DepartmentId)
            {
                var invalidDeptIds = await _repository.CheckInvalidDepartmentOwnershipAsync(
                    updateDto.DepartmentId.Value, AppId);
                if (invalidDeptIds.Any())
                    throw new UnauthorizedException(
                        $"DepartmentId does not belong to this Application: {string.Join(", ", invalidDeptIds)}");
            }

            if (updateDto.DistrictId.HasValue && updateDto.DistrictId != security.DistrictId)
            {
                var invalidDistrictIds = await _repository.CheckInvalidDistrictOwnershipAsync(
                    updateDto.DistrictId.Value, AppId);
                if (invalidDistrictIds.Any())
                    throw new UnauthorizedException(
                        $"DistrictId does not belong to this Application: {string.Join(", ", invalidDistrictIds)}");
            }

            var cardId = updateDto.CardId ?? Guid.Empty;
            var card = updateDto.CardId.HasValue ? await _cardRepository.GetByIdEntityAsync(cardId) : null;
            if (updateDto.CardId.HasValue && card == null)
                throw new NotFoundException($"Card with ID {updateDto.CardId} not found or has been deleted.");

            // Validate SecurityHead1Id and SecurityHead2Id must be heads
            var securityHeadIds = new List<Guid?>();
            if (updateDto.SecurityHead1Id.HasValue)
                securityHeadIds.Add(updateDto.SecurityHead1Id);
            if (updateDto.SecurityHead2Id.HasValue)
                securityHeadIds.Add(updateDto.SecurityHead2Id);

            if (securityHeadIds.Any())
            {
                var invalidHeadIds = await _repository.ValidateSecurityHeadsAsync(securityHeadIds);
                if (invalidHeadIds.Any())
                {
                    throw new BusinessException(
                        $"The following SecurityHead IDs are not valid heads (IsHead = false): {string.Join(", ", invalidHeadIds)}");
                }
            }

            if (updateDto.FaceImage != null && updateDto.FaceImage.Length > 0)
            {
                try
                {
                    await _fileStorageService.DeleteAsync(security.FaceImage!);
                    security.FaceImage = await _fileStorageService
                        .SaveImageAsync(updateDto.FaceImage, "SecurityFaceImages", MaxFileSize, ImagePurpose.Photo);
                    security.UploadFr = 1;
                    security.UploadFrError = "Upload successful";
                }
                catch (Exception ex)
                {
                    security.UploadFr = 2;
                    security.UploadFrError = ex.Message;
                }
            }

            using var transaction = await _repository.BeginTransactionAsync();
            try
            {
                var oldCard = await _cardRepository.GetAllQueryable()
                    .FirstOrDefaultAsync(c => c.SecurityId == security.Id && c.StatusCard != 0);

                if (oldCard != null && oldCard.Id != cardId)
                {
                    oldCard.IsUsed = false;
                    oldCard.CardStatus = CardStatus.Available;
                    oldCard.SecurityId = null;
                    oldCard.CheckinAt = null;
                    await _cardRepository.UpdateAsync(oldCard);
                }

                if (updateDto.CardId.HasValue)
                {
                    if (card!.SecurityId.HasValue && card.SecurityId != security.Id)
                        throw new BusinessException("This card is already assigned to another security.");

                    card.IsUsed = true;
                    card.CardStatus = CardStatus.Used;
                    card.LastUsed = security.Name;
                    card.SecurityId = security.Id;
                    card.CheckinAt = DateTime.UtcNow;
                    await _cardRepository.UpdateAsync(card);
                }

                SetUpdateAudit(security);
                _mapper.Map(updateDto, security);
                security.BleCardNumber = card?.Dmac;
                security.CardNumber = card?.CardNumber;

                await _repository.UpdateAsync(security);

                await transaction.CommitAsync();
                _mqttQueue.Enqueue("engine/refresh/card-related", "");
                _audit.Updated("Security", security.Id, $"Security {security.Name} updated");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await _repository.GetByIdAsync(security.Id);
        }

        public async Task DeleteSecurityAsync(Guid id)
        {
            var security = await _repository.GetByIdEntityAsync(id);
            if (security == null)
                throw new NotFoundException($"Security {id} not found");

            var (applicationId, isSystemAdmin) = _repository.GetApplicationIdAndRole();
            if (!isSystemAdmin && security.ApplicationId != applicationId)
                throw new UnauthorizedException("Cannot delete Security from a different application.");

            var oldCard = await _cardRepository.GetAllQueryable()
                .FirstOrDefaultAsync(c => c.SecurityId == security.Id && c.StatusCard != 0);

            if (oldCard != null)
            {
                oldCard.IsUsed = false;
                oldCard.CardStatus = CardStatus.Available;
                oldCard.SecurityId = null;
                oldCard.CheckinAt = null;
                await _cardRepository.UpdateAsync(oldCard);
            }

            SetDeleteAudit(security);
            await _repository.DeleteAsync(security);
            _mqttQueue.Enqueue("engine/refresh/card-related", "");
            _audit.Deleted("Security", id, $"Security {security.Name} deleted");
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, SecurityFilter filter)
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

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
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            var securities = await _repository.GetAllAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Security Report")
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
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("PersonId").SemiBold();
                            header.Cell().Element(CellStyle).Text("Organization").SemiBold();
                            header.Cell().Element(CellStyle).Text("Department").SemiBold();
                            header.Cell().Element(CellStyle).Text("District").SemiBold();
                            header.Cell().Element(CellStyle).Text("Identity").SemiBold();
                            header.Cell().Element(CellStyle).Text("Card Number").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Phone").SemiBold();
                            header.Cell().Element(CellStyle).Text("Email").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        int index = 1;
                        foreach (var security in securities)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(security.PersonId ?? "-");
                            table.Cell().Element(CellStyle).Text(security.Organization?.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(security.Department?.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(security.District?.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(security.IdentityId ?? "-");
                            table.Cell().Element(CellStyle).Text(security.CardNumber ?? "-");
                            table.Cell().Element(CellStyle).Text(security.Name);
                            table.Cell().Element(CellStyle).Text(security.Phone ?? "-");
                            table.Cell().Element(CellStyle).Text(security.Email ?? "-");
                            table.Cell().Element(CellStyle).Text(security.Status == 1 ? "Active" : "Inactive");
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
            var securities = await _repository.GetAllExportAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Securities");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "PersonId";
            worksheet.Cell(1, 3).Value = "Organization";
            worksheet.Cell(1, 4).Value = "Department";
            worksheet.Cell(1, 5).Value = "District";
            worksheet.Cell(1, 6).Value = "Identity";
            worksheet.Cell(1, 7).Value = "Card Number";
            worksheet.Cell(1, 8).Value = "Name";
            worksheet.Cell(1, 9).Value = "Phone";
            worksheet.Cell(1, 10).Value = "Email";
            worksheet.Cell(1, 11).Value = "Gender";
            worksheet.Cell(1, 12).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var security in securities)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = security.PersonId;
                worksheet.Cell(row, 3).Value = security.Organization?.Name ?? "-";
                worksheet.Cell(row, 4).Value = security.Department?.Name ?? "-";
                worksheet.Cell(row, 5).Value = security.District?.Name ?? "-";
                worksheet.Cell(row, 6).Value = security.IdentityId;
                worksheet.Cell(row, 7).Value = security.CardNumber;
                worksheet.Cell(row, 8).Value = security.Name;
                worksheet.Cell(row, 9).Value = security.Phone;
                worksheet.Cell(row, 10).Value = security.Email;
                worksheet.Cell(row, 11).Value = security.Gender.ToString() ?? "-";
                worksheet.Cell(row, 12).Value = security.Status == 1 ? "Active" : "Inactive";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<IEnumerable<MstSecurityDto>> ImportAsync(IFormFile file)
        {
            var securities = new List<MstSecurity>();

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1);

            int rowNumber = 2;
            foreach (var row in rows)
            {
                var organizationIdStr = row.Cell(1).GetValue<string>();
                if (!Guid.TryParse(organizationIdStr, out var organizationId))
                    throw new ArgumentException($"Invalid Organization Id format at row {rowNumber}");

                var organization = await _repository.GetOrganizationByIdAsync(organizationId);
                if (organization == null)
                    throw new ArgumentException($"OrganizationId {organizationId} not found at row {rowNumber}");

                var departmentIdStr = row.Cell(2).GetValue<string>();
                if (!Guid.TryParse(departmentIdStr, out var departmentId))
                    throw new ArgumentException($"Invalid Department Id format at row {rowNumber}");

                var department = await _repository.GetDepartmentByIdAsync(departmentId);
                if (department == null)
                    throw new ArgumentException($"Department Id {departmentId} not found at row {rowNumber}");

                var districtIdStr = row.Cell(3).GetValue<string>();
                if (!Guid.TryParse(districtIdStr, out var districtId))
                    throw new ArgumentException($"Invalid District Id format at row {rowNumber}");

                var district = await _repository.GetDistrictByIdAsync(districtId);
                if (district == null)
                    throw new ArgumentException($"districtId {districtId} not found at row {rowNumber}");

                var security = new MstSecurity
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    DepartmentId = departmentId,
                    DistrictId = districtId,
                    ApplicationId = AppId,
                    Name = row.Cell(4).GetValue<string>(),
                    PersonId = row.Cell(5).GetValue<string>(),
                    CardNumber = row.Cell(6).GetValue<string>(),
                    Gender = (Gender)Enum.Parse(typeof(Gender), row.Cell(7).GetValue<string>(), ignoreCase: true)
                };

                SetCreateAudit(security);
                securities.Add(security);
                rowNumber++;
            }

            foreach (var security in securities)
            {
                await _repository.AddAsync(security);
            }

            _audit.Created("Security", securities.Count, $"Imported {securities.Count} securities");
            return _mapper.Map<IEnumerable<MstSecurityDto>>(securities);
        }
    }
}

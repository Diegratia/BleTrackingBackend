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
using Bogus.DataSets;
using Helpers.Consumer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Helpers.Consumer.Mqtt;
using BusinessLogic.Services.Extension.FileStorageService;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MstSecurityService> _logger;
        private readonly IMqttClientService _mqttClient;
        private readonly IFileStorageService _fileStorageService;

        public MstSecurityService(MstSecurityRepository repository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        CardRepository cardRepository,
        ILogger<MstSecurityService> logger,
        IMqttClientService mqttClient,
        IFileStorageService fileStorageService
        ) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _cardRepository = cardRepository;
            _logger = logger;
            _mqttClient = mqttClient;
            _fileStorageService = fileStorageService;
        }

        public async Task<IEnumerable<MstSecurityDto>> GetAllSecuritiesAsync()
        {
            var securities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstSecurityDto>>(securities);
        }
        public async Task<IEnumerable<MstSecurityLookUpDto>> GetAllLookUpAsync()
        {
            var securities = await _repository.GetAllLookUpAsync();
            return _mapper.Map<IEnumerable<MstSecurityLookUpDto>>(securities);
        }

        public async Task<IEnumerable<OpenMstSecurityDto>> OpenGetAllSecuritiesAsync()
        {
            var securities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<OpenMstSecurityDto>>(securities);
        }

        public async Task<MstSecurityDto> GetSecurityByIdAsync(Guid id)
        {
            var security = await _repository.GetByIdAsync(id);
            if (security == null)
                throw new NotFoundException($"Security {id} not found");
            if (security.Status == 0)
                throw new BusinessException("Security is inactive", "BUILDING_INACTIVE");
            return security == null ? null : _mapper.Map<MstSecurityDto>(security);
        }

        public async Task<MstSecurityDto> CreateSecurityAsync(MstSecurityCreateDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));
            var username = UsernameFormToken;
            var card = await _cardRepository.GetByIdAsync(createDto.CardId.Value);
            if (card == null)
                throw new NotFoundException($"Card {createDto.CardId} not found.");

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
                throw new BusinessException("Card already checked in by another visitor.");
            // if (card == null)
            //     throw new InvalidOperationException("Card not found.");

            // Validasi relasi
            // var department = await _repository.GetDepartmentByIdAsync(createDto.DepartmentId);
            // if (department == null)
            //     throw new ArgumentException($"Department with ID {createDto.DepartmentId} not found.");

            // var organization = await _repository.GetOrganizationByIdAsync(createDto.OrganizationId);
            // if (organization == null)
            //     throw new ArgumentException($"Organization with ID {createDto.OrganizationId} not found.");

            // var district = await _repository.GetDistrictByIdAsync(createDto.DistrictId);
            // if (district == null)
            //     throw new ArgumentException($"District with ID {createDto.DistrictId} not found.");

            var security = _mapper.Map<MstSecurity>(createDto);

            // Tangani upload gambar
            if (createDto.FaceImage != null && createDto.FaceImage.Length > 0)
            {
                try
                {
                    
                    security.FaceImage = await _fileStorageService
                        .SaveImageAsync(createDto.FaceImage, "SecurityFaceImages", MaxFileSize, ImagePurpose.Photo);
                    

                        security.UploadFr = 1; // Sukses
                        security.UploadFrError = "Upload successful";
                }
                    catch (Exception ex)
                    {
                        security.UploadFr = 2; // Gagal
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

            security.Id = Guid.NewGuid();
            security.Status = 1;
            security.CreatedBy = username;
            security.CreatedAt = DateTime.UtcNow;
            security.UpdatedBy = username;
            security.UpdatedAt = DateTime.UtcNow;
            security.BleCardNumber = card.Dmac;
            security.CardNumber = card.CardNumber;

            // security.JoinDate = createDto.JoinDate;
            // security.BirthDate = createDto.BirthDate;

            using var transaction = await _repository.BeginTransactionAsync();
            try
            {
                await _repository.AddAsync(security);
                card.IsUsed = true;
                card.LastUsed = security.Name;
                card.SecurityId = security.Id;
                card.CheckinAt = DateTime.UtcNow;
                await _cardRepository.UpdateAsync(card);

                await transaction.CommitAsync();
                await _mqttClient.PublishAsync("engine/refresh/card-related", "");

            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return _mapper.Map<MstSecurityDto>(security);
        }

        public async Task<MstSecurityDto> UpdateSecurityAsync(Guid id, MstSecurityUpdateDto updateDto)
        {
            // if (updateDto == null)
            //     throw new ArgumentNullException(nameof(updateDto));

            var security = await _repository.GetByIdAsync(id);
            if (security == null)
                throw new NotFoundException($"Security with ID {id} not found or has been deleted.");

            var cardId = updateDto.CardId ?? Guid.Empty;
            var card = updateDto.CardId.HasValue ? await _cardRepository.GetByIdAsync(cardId) : null;
            if (updateDto.CardId.HasValue && card == null)
                throw new NotFoundException($"Card with ID {updateDto.CardId} not found or has been deleted.");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            if (updateDto.FaceImage != null && updateDto.FaceImage.Length > 0)
            {
                try
                {
                     await _fileStorageService.DeleteAsync(security.FaceImage);

                    security.FaceImage = await _fileStorageService
                        .SaveImageAsync(updateDto.FaceImage, "SecurityFaceImages", MaxFileSize, ImagePurpose.Photo);
                    security.UploadFr = 1;
                    security.UploadFrError = "Upload successful";
                }
                catch (Exception ex)
                {
                    security.UploadFr = 2;
                    security.UploadFrError = ex.Message;
                    // security.FaceImage = null;
                }
            }
            else
            {
                security.UploadFr = 0;
                security.UploadFrError = "No file uploaded";
                // security.FaceImage = null;
            }

            using var transaction = await _repository.BeginTransactionAsync();
            try
            {
                // Reset card lama (jika beda dengan card baru)
                var oldCard = await _cardRepository.GetAllQueryable()
                    .FirstOrDefaultAsync(c => c.SecurityId == security.Id && c.StatusCard != 0);

                if (oldCard != null && oldCard.Id != cardId)
                {
                    oldCard.IsUsed = false;
                    oldCard.SecurityId = null;
                    oldCard.CheckinAt = null;
                    await _cardRepository.UpdateAsync(oldCard);
                }

                // Assign card baru
                if (updateDto.CardId.HasValue)
                {
                    if (card!.SecurityId.HasValue && card.SecurityId != security.Id)
                        throw new BusinessException("This card is already assigned to another security.");

                    card.IsUsed = true;
                    card.LastUsed = security.Name;
                    card.SecurityId = security.Id;
                    card.CheckinAt = DateTime.UtcNow;
                    await _cardRepository.UpdateAsync(card);
                }

                // Update security
                _mapper.Map(updateDto, security);
                security.BleCardNumber = card.Dmac;
                security.CardNumber = card.CardNumber;
                security.UpdatedBy = username;
                security.UpdatedAt = DateTime.UtcNow;

                

                await _repository.UpdateAsync(security);

                await transaction.CommitAsync();
                await _mqttClient.PublishAsync("engine/refresh/card-related", "");

            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return _mapper.Map<MstSecurityDto>(security);
        }


        public async Task DeleteSecurityAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var security = await _repository.GetByIdAsync(id);
            if (security == null)
                throw new NotFoundException($"Security {id} not found");
            security.UpdatedBy = username;
            security.UpdatedAt = DateTime.UtcNow;
            security.Status = 0;
            security.CardNumber = null;
            security.BleCardNumber = null;

            var oldCard = await _cardRepository.GetAllQueryable()
                    .FirstOrDefaultAsync(c => c.SecurityId == security.Id && c.StatusCard != 0);
            // oldCard.IsUsed = false;
            // oldCard.SecurityId = null;
            // oldCard.CheckinAt = null;
            // await _cardRepository.UpdateAsync(oldCard);
                if (oldCard != null)
                        {
                            oldCard.IsUsed = false;
                            oldCard.SecurityId = null;
                            oldCard.CheckinAt = null;
                            await _cardRepository.UpdateAsync(oldCard);
                        }



            await _repository.DeleteAsync(id);
            await _mqttClient.PublishAsync("engine/refresh/card-related", "");

        }

        

        // public async Task<MstSecurityDto> SecurityBlacklistAsync(Guid id, BlacklistReasonDto dto)
        // {
        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        //     var Security = await _repository.GetByIdAsync(id);

        //     _mapper.Map(dto, Security);
        //     Security.UpdatedBy = username;
        //     Security.BlacklistAt = DateTime.UtcNow;
        //     Security.IsBlacklist = true;
        //     Security.UpdatedAt = DateTime.UtcNow;

        //     await _repository.UpdateAsync(Security);
        //     await _mqttClient.PublishAsync("engine/refresh/blacklist-related", "");
        //     return _mapper.Map<MstSecurityDto>(Security);
        // }

        // public async Task UnBlacklistSecurityAsync(Guid id)
        // {
        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        //     var security = await _repository.GetByIdAsync(id);
        //     if (security == null)
        //         throw new KeyNotFoundException($"Security with ID {id} not found.");

        //     security.UpdatedBy = username ?? "System";
        //     security.UpdatedAt = DateTime.UtcNow;
        //     security.IsBlacklist = false;

        //     await _repository.UpdateAsync(security);
        //     await _mqttClient.PublishAsync("engine/refresh/blacklist-related", "");
        // }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable().AsNoTracking();

            var enumColumns = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "Gender", typeof(Gender) },
                { "IdentityType", typeof(IdentityType) }
            };

            var searchableColumns = new[] { "Name", "Organization.Name", "Department.Name", "District.Name" };
            var validSortColumns = new[] { "UpdatedAt", "Name", "Organization.Name", "Department.Name", "District.Name", "CreatedAt", "BirthDate", "JoinDate", "ExitDate", "StatusEmployee", "HeadSecurity1", "HeadSecurity2", "Status", "Brand.Name", "CardNumber" };

            var filterService = new GenericDataTableService<MstSecurity, MstSecurityDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns,
                enumColumns);

            return await filterService.FilterAsync(request);
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
                            header.Cell().Element(CellStyle).Text("PersonId").SemiBold();
                            header.Cell().Element(CellStyle).Text("Organization").SemiBold();
                            header.Cell().Element(CellStyle).Text("Department").SemiBold();
                            header.Cell().Element(CellStyle).Text("District").SemiBold();
                            header.Cell().Element(CellStyle).Text("Identity").SemiBold();
                            header.Cell().Element(CellStyle).Text("Card Number").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Phone").SemiBold();
                            header.Cell().Element(CellStyle).Text("Email").SemiBold();
                            header.Cell().Element(CellStyle).Text("Gender").SemiBold();
                            header.Cell().Element(CellStyle).Text("Address").SemiBold();
                            header.Cell().Element(CellStyle).Text("FaceImage").SemiBold();
                            header.Cell().Element(CellStyle).Text("UploadFr").SemiBold();
                            header.Cell().Element(CellStyle).Text("UploadFrError").SemiBold();
                            header.Cell().Element(CellStyle).Text("BirthDate").SemiBold();
                            header.Cell().Element(CellStyle).Text("JoinDate").SemiBold();
                            header.Cell().Element(CellStyle).Text("ExitDate").SemiBold();
                            header.Cell().Element(CellStyle).Text("HeadSecurity1").SemiBold();
                            header.Cell().Element(CellStyle).Text("HeadSecurity2").SemiBold();
                            header.Cell().Element(CellStyle).Text("StatusEmployee").SemiBold();
                            header.Cell().Element(CellStyle).Text("CreatedBy").SemiBold();
                            header.Cell().Element(CellStyle).Text("CreatedAt").SemiBold();
                            header.Cell().Element(CellStyle).Text("UpdatedBy").SemiBold();
                            header.Cell().Element(CellStyle).Text("UpdatedAt").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        // Table body
                        int index = 1;
                        foreach (var security in securities)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(security.PersonId);
                            table.Cell().Element(CellStyle).Text(security.Organization.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(security.Department.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(security.District.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(security.IdentityId);
                            table.Cell().Element(CellStyle).Text(security.CardNumber);
                            table.Cell().Element(CellStyle).Text(security.Name);
                            table.Cell().Element(CellStyle).Text(security.Phone);
                            table.Cell().Element(CellStyle).Text(security.Email);
                            table.Cell().Element(CellStyle).Text(security.Gender.ToString() ?? "-");
                            table.Cell().Element(CellStyle).Text(security.Address);
                            table.Cell().Element(CellStyle).Text(security.FaceImage);
                            table.Cell().Element(CellStyle).Text(security.UploadFr.ToString());
                            table.Cell().Element(CellStyle).Text(security.UploadFrError);
                            table.Cell().Element(CellStyle).Text(security.BirthDate?.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(security.JoinDate?.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(security.ExitDate?.ToString("yyyy-MM-dd"));
                            // table.Cell().Element(CellStyle).Text(security.HeadSecurity1);
                            table.Cell().Element(CellStyle).Text(security.CreatedAt.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(security.CreatedBy ?? "-");
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


            // Header
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
            worksheet.Cell(1, 12).Value = "Address";
            worksheet.Cell(1, 13).Value = "FaceImage";
            worksheet.Cell(1, 14).Value = "UploadFr";
            worksheet.Cell(1, 15).Value = "UploadFrError";
            worksheet.Cell(1, 16).Value = "BirthDate";
            worksheet.Cell(1, 17).Value = "JoinDate";
            worksheet.Cell(1, 18).Value = "ExitDate";
            worksheet.Cell(1, 19).Value = "HeadSecurity1";
            worksheet.Cell(1, 20).Value = "HeadSecurity2";
            worksheet.Cell(1, 21).Value = "StatusEmployee";
            worksheet.Cell(1, 22).Value = "CreatedBy";
            worksheet.Cell(1, 23).Value = "CreatedAt";
            worksheet.Cell(1, 24).Value = "UpdatedBy";
            worksheet.Cell(1, 25).Value = "UpdatedAt";
            worksheet.Cell(1, 26).Value = "Status";

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
                worksheet.Cell(row, 12).Value = security.Address;
                worksheet.Cell(row, 13).Value = security.FaceImage;
                worksheet.Cell(row, 14).Value = security.UploadFr;
                worksheet.Cell(row, 15).Value = security.UploadFrError;
                worksheet.Cell(row, 16).Value = security.BirthDate?.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 17).Value = security.JoinDate?.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 18).Value = security.ExitDate?.ToString("yyyy-MM-dd");
                // worksheet.Cell(row, 19).Value = security.HeadSecurity1;
                // worksheet.Cell(row, 20).Value = security.HeadSecurity2;
                worksheet.Cell(row, 21).Value = security.StatusEmployee.ToString() ?? "-";
                worksheet.Cell(row, 22).Value = security.CreatedBy ?? "-";
                worksheet.Cell(row, 23).Value = security.CreatedAt.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 24).Value = security.UpdatedBy ?? "-";
                worksheet.Cell(row, 25).Value = security.UpdatedAt.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 26).Value = security.Status == 1 ? "Active" : "Inactive";
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
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // skip header

            int rowNumber = 2; // start dari baris ke 2
            foreach (var row in rows)
            {
                // validasi
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
                    Name = row.Cell(4).GetValue<string>(),
                    PersonId = row.Cell(5).GetValue<string>(),
                    CardNumber = row.Cell(6).GetValue<string>(),
                    Gender = (Gender)Enum.Parse(typeof(Gender), row.Cell(7).GetValue<string>(), ignoreCase: true),
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                };

                securities.Add(security);
                rowNumber++;
            }

            foreach (var security in securities)
            {
                await _repository.AddAsync(security);
            }

            return _mapper.Map<IEnumerable<MstSecurityDto>>(securities);
        }


    }
}
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

namespace BusinessLogic.Services.Implementation
{
    public class MstMemberService : IMstMemberService
    {
        private readonly MstMemberRepository _repository;
        private readonly CardRepository _cardRepository;
        private readonly IMapper _mapper;
        private readonly string[] _allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
        private const long MaxFileSize = 5 * 1024 * 1024; // Max 5 MB
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MstMemberService(MstMemberRepository repository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        CardRepository cardRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _cardRepository = cardRepository;
        }

        public async Task<IEnumerable<MstMemberDto>> GetAllMembersAsync()
        {
            var members = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstMemberDto>>(members);
        }

        public async Task<IEnumerable<OpenMstMemberDto>> OpenGetAllMembersAsync()
        {
            var members = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<OpenMstMemberDto>>(members);
        }

        public async Task<MstMemberDto> GetMemberByIdAsync(Guid id)
        {
            var member = await _repository.GetByIdAsync(id);
            return member == null ? null : _mapper.Map<MstMemberDto>(member);
        }

        public async Task<MstMemberDto> CreateMemberAsync(MstMemberCreateDto createDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var card = await _cardRepository.GetByIdAsync(createDto.CardId.Value);
            if (card == null)
                throw new InvalidOperationException("Card not found.");
            Console.WriteLine("Username: {0}, Card: {1}", username, card);
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var existingMember = await _repository.GetAllQueryable()
            .FirstOrDefaultAsync(b => b.Email == createDto.Email ||
                                     b.IdentityId == createDto.IdentityId ||
                                     b.PersonId == createDto.PersonId);
            
            if (existingMember != null)
        {
            if (existingMember.Email == createDto.Email)
                throw new ArgumentException($"Member with Email {createDto.Email} already exists.");
            if (existingMember.IdentityId == createDto.IdentityId)
                throw new ArgumentException($"Member with IdentityId {createDto.IdentityId} already exists.");
            if (existingMember.PersonId == createDto.PersonId)
                throw new ArgumentException($"Member with PersonId {createDto.PersonId} already exists.");
        }


            if (card.IsUsed == true)
                throw new InvalidOperationException("Card already checked in by another visitor.");
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

            var member = _mapper.Map<MstMember>(createDto);

            // Tangani upload gambar
            if (createDto.FaceImage != null && createDto.FaceImage.Length > 0)
            {
                try
                {
                    // Validasi tipe file
                    if (string.IsNullOrEmpty(createDto.FaceImage.ContentType) || !_allowedImageTypes.Contains(createDto.FaceImage.ContentType))
                        throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

                    // Validasi ukuran file
                    if (createDto.FaceImage.Length > MaxFileSize)
                        throw new ArgumentException("File size exceeds 5 MB limit.");

                    // Folder penyimpanan
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "MemberFaceImages");
                    Directory.CreateDirectory(uploadDir);

                    // Buat nama file unik
                    var fileExtension = Path.GetExtension(createDto.FaceImage.FileName)?.ToLower() ?? ".jpg";
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    // Simpan file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await createDto.FaceImage.CopyToAsync(stream);
                    }

                    member.FaceImage = $"/Uploads/MemberFaceImages/{fileName}";
                    member.UploadFr = 1; // Sukses
                    member.UploadFrError = "Upload successful";
                }
                catch (Exception ex)
                {
                    member.UploadFr = 2; // Gagal
                    member.UploadFrError = ex.Message;
                    member.FaceImage = null;
                }
            }
            else
            {
                member.UploadFr = 0; // Tidak ada file
                member.UploadFrError = "No file uploaded";
                member.FaceImage = null;
            }

                    member.Id = Guid.NewGuid();
                    member.Status = 1;
                    member.CreatedBy = username;
                    member.CreatedAt = DateTime.UtcNow;
                    member.UpdatedBy = username;
                    member.UpdatedAt = DateTime.UtcNow;
                    member.BleCardNumber = card.Dmac;
                    member.CardNumber = card.CardNumber;

            // member.JoinDate = createDto.JoinDate;
            // member.BirthDate = createDto.BirthDate;

            using var transaction = await _repository.BeginTransactionAsync();
                try
                {
                    await _repository.AddAsync(member);
                    card.IsUsed = true;
                    card.LastUsed = member.Name;
                    card.MemberId = member.Id;
                    card.CheckinAt = DateTime.UtcNow;
                    await _cardRepository.UpdateAsync(card);

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }

            return _mapper.Map<MstMemberDto>(member);
        }

            public async Task<MstMemberDto> UpdateMemberAsync(Guid id, MstMemberUpdateDto updateDto)
        {
            // if (updateDto == null)
            //     throw new ArgumentNullException(nameof(updateDto));

            var member = await _repository.GetByIdAsync(id);
            if (member == null)
                throw new KeyNotFoundException($"Member with ID {id} not found or has been deleted.");

            var cardId = updateDto.CardId ?? Guid.Empty;
            var card = updateDto.CardId.HasValue ? await _cardRepository.GetByIdAsync(cardId) : null;
            if (updateDto.CardId.HasValue && card == null)
                throw new KeyNotFoundException($"Card with ID {updateDto.CardId} not found or has been deleted.");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            using var transaction = await _repository.BeginTransactionAsync();
            try
            {
                // Reset card lama (jika beda dengan card baru)
                var oldCard = await _cardRepository.GetAllQueryable()
                    .FirstOrDefaultAsync(c => c.MemberId == member.Id && c.StatusCard != 0);

                if (oldCard != null && oldCard.Id != cardId)
                {
                    oldCard.IsUsed = false;
                    oldCard.MemberId = null;
                    oldCard.CheckinAt = null;
                    await _cardRepository.UpdateAsync(oldCard);
                }

                // Assign card baru
                if (updateDto.CardId.HasValue)
                {
                    if (card!.MemberId.HasValue && card.MemberId != member.Id)
                        throw new InvalidOperationException("This card is already assigned to another member.");

                    card.IsUsed = true;
                    card.LastUsed = member.Name;
                    card.MemberId = member.Id;
                    card.CheckinAt = DateTime.UtcNow;
                    await _cardRepository.UpdateAsync(card);
                }

                // Update member
                _mapper.Map(updateDto, member);
                member.UpdatedBy = username;
                member.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(member);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return _mapper.Map<MstMemberDto>(member);
        }


        public async Task DeleteMemberAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var member = await _repository.GetByIdAsync(id);
            member.UpdatedBy = username;
            member.UpdatedAt = DateTime.UtcNow;
            member.Status = 0;
            await _repository.DeleteAsync(id);
        }

         public async Task BlockCardAsync(Guid id, MemberBlockDto dto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var Member = await _repository.GetByIdAsync(id);
                Member.IsBlock = dto.IsBlock;
                Member.UpdatedBy = username;
                Member.BlockAt = DateTime.UtcNow;
                Member.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(Member);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var enumColumns = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "Gender", typeof(Gender) },
                { "IdentityType", typeof(IdentityType) }
            };

            var searchableColumns = new[] { "Name", "Organization.Name", "Department.Name", "District.Name" };
            var validSortColumns = new[] { "UpdatedAt", "Name", "Organization.Name", "Department.Name", "District.Name", "CreatedAt", "BirthDate", "JoinDate", "ExitDate", "StatusEmployee", "HeadMember1", "HeadMember2", "Status", "Brand.Name", "CardNumber" };

            var filterService = new GenericDataTableService<MstMember, MstMemberDto>(
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
            var members = await _repository.GetAllAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Member Report")
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
                            header.Cell().Element(CellStyle).Text("HeadMember1").SemiBold();
                            header.Cell().Element(CellStyle).Text("HeadMember2").SemiBold();
                            header.Cell().Element(CellStyle).Text("StatusEmployee").SemiBold();
                            header.Cell().Element(CellStyle).Text("CreatedBy").SemiBold();
                            header.Cell().Element(CellStyle).Text("CreatedAt").SemiBold();
                            header.Cell().Element(CellStyle).Text("UpdatedBy").SemiBold();
                            header.Cell().Element(CellStyle).Text("UpdatedAt").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        // Table body
                        int index = 1;
                        foreach (var member in members)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(member.PersonId);
                            table.Cell().Element(CellStyle).Text(member.Organization.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(member.Department.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(member.District.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(member.IdentityId);
                            table.Cell().Element(CellStyle).Text(member.CardNumber);
                            table.Cell().Element(CellStyle).Text(member.Name);
                            table.Cell().Element(CellStyle).Text(member.Phone);
                            table.Cell().Element(CellStyle).Text(member.Email);
                            table.Cell().Element(CellStyle).Text(member.Gender.ToString() ?? "-");
                            table.Cell().Element(CellStyle).Text(member.Address);
                            table.Cell().Element(CellStyle).Text(member.FaceImage);
                            table.Cell().Element(CellStyle).Text(member.UploadFr.ToString());
                            table.Cell().Element(CellStyle).Text(member.UploadFrError);
                            table.Cell().Element(CellStyle).Text(member.BirthDate?.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(member.JoinDate?.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(member.ExitDate?.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(member.HeadMember1);
                            table.Cell().Element(CellStyle).Text(member.CreatedAt.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(member.CreatedBy ?? "-");
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
            var members = await _repository.GetAllExportAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Members");


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
            worksheet.Cell(1, 19).Value = "HeadMember1";
            worksheet.Cell(1, 20).Value = "HeadMember2";
            worksheet.Cell(1, 21).Value = "StatusEmployee";
            worksheet.Cell(1, 22).Value = "CreatedBy";
            worksheet.Cell(1, 23).Value = "CreatedAt";
            worksheet.Cell(1, 24).Value = "UpdatedBy";
            worksheet.Cell(1, 25).Value = "UpdatedAt";
            worksheet.Cell(1, 26).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var member in members)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = member.PersonId;
                worksheet.Cell(row, 3).Value = member.Organization?.Name ?? "-";
                worksheet.Cell(row, 4).Value = member.Department?.Name ?? "-";
                worksheet.Cell(row, 5).Value = member.District?.Name ?? "-";
                worksheet.Cell(row, 6).Value = member.IdentityId;
                worksheet.Cell(row, 7).Value = member.CardNumber;
                worksheet.Cell(row, 8).Value = member.Name;
                worksheet.Cell(row, 9).Value = member.Phone;
                worksheet.Cell(row, 10).Value = member.Email;
                worksheet.Cell(row, 11).Value = member.Gender.ToString() ?? "-";
                worksheet.Cell(row, 12).Value = member.Address;
                worksheet.Cell(row, 13).Value = member.FaceImage;
                worksheet.Cell(row, 14).Value = member.UploadFr;
                worksheet.Cell(row, 15).Value = member.UploadFrError;
                worksheet.Cell(row, 16).Value = member.BirthDate?.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 17).Value = member.JoinDate?.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 18).Value = member.ExitDate?.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 19).Value = member.HeadMember1;
                worksheet.Cell(row, 20).Value = member.HeadMember2;
                worksheet.Cell(row, 21).Value = member.StatusEmployee.ToString() ?? "-";
                worksheet.Cell(row, 22).Value = member.CreatedBy ?? "-";
                worksheet.Cell(row, 23).Value = member.CreatedAt.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 24).Value = member.UpdatedBy ?? "-";
                worksheet.Cell(row, 25).Value = member.UpdatedAt.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 26).Value = member.Status == 1 ? "Active" : "Inactive";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        
            public async Task<IEnumerable<MstMemberDto>> ImportAsync(IFormFile file)
        {
            var members = new List<MstMember>();
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

                var member = new MstMember
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

                members.Add(member);
                rowNumber++;
            }

            foreach (var member in members)
            {
                await _repository.AddAsync(member);
            }

            return _mapper.Map<IEnumerable<MstMemberDto>>(members);
        }

    }
}
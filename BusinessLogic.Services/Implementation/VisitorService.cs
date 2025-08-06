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
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using Helpers.Consumer;
using DocumentFormat.OpenXml.ExtendedProperties;

namespace BusinessLogic.Services.Implementation
{

public class VisitorService : IVisitorService
{
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly VisitorRepository _visitorRepository;
    private readonly UserRepository _userRepository;
    private readonly TrxVisitorRepository _trxVisitorRepository;
    private readonly UserGroupRepository _userGroupRepository;
    private readonly IEmailService _emailService;
    private readonly string[] _allowedImageTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public VisitorService(
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        VisitorRepository visitorRepository,
        UserRepository userRepository,
        UserGroupRepository userGroupRepository,
        TrxVisitorRepository trxVisitorRepository,
        IEmailService emailService)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _visitorRepository = visitorRepository ?? throw new ArgumentNullException(nameof(visitorRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _userGroupRepository = userGroupRepository ?? throw new ArgumentNullException(nameof(userGroupRepository));
        _trxVisitorRepository = trxVisitorRepository ?? throw new ArgumentNullException(nameof(trxVisitorRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

        // create visitor, trx visitor dan user
        public async Task<VisitorDto> CreateVisitorAsync(VisitorCreateDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (string.IsNullOrWhiteSpace(createDto.Email))
                throw new ArgumentException("Email is required", nameof(createDto.Email));

            if (string.IsNullOrWhiteSpace(createDto.Name))
                throw new ArgumentException("Name is required", nameof(createDto.Name));

            var existingVisitor = await _visitorRepository.GetAllQueryable()
                .FirstOrDefaultAsync(b => b.Email == createDto.Email ||
                                    b.IdentityId == createDto.IdentityId ||
                                    b.PersonId == createDto.PersonId);

            if (existingVisitor != null)
            {
                if (existingVisitor.Email == createDto.Email)
                {
                    throw new ArgumentException($"Visitor with Email {createDto.Email} already exists.");
                }
                else if (existingVisitor.IdentityId == createDto.IdentityId)
                {
                    throw new ArgumentException($"Visitor with Email {createDto.IdentityId} already exists.");
                }
                else if (existingVisitor.PersonId == createDto.PersonId)
                {
                    throw new ArgumentException($"Visitor with PersonId {createDto.PersonId} already exists.");
                }
            }

            var username = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(currentUserId))
                throw new UnauthorizedAccessException("User not authenticated");

            var currentUser = await _userRepository.GetByIdAsync(Guid.Parse(currentUserId));
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found");

            if (currentUser.Group == null || currentUser.Group.ApplicationId == Guid.Empty)
                throw new InvalidOperationException("Current user has no valid group or application");

            var applicationId = currentUser.ApplicationId;

            // Cari atau buat grup dengan LevelPriority.UserCreated
            var userGroup = await _userGroupRepository.GetByApplicationIdAndPriorityAsync(applicationId, LevelPriority.UserCreated);
            if (userGroup == null)
            {
                userGroup = new UserGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "VisitorGroup",
                    LevelPriority = LevelPriority.UserCreated,
                    ApplicationId = applicationId,
                    Status = 1,
                    CreatedBy = username ?? "System",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username ?? "System",
                    UpdatedAt = DateTime.UtcNow
                };
                await _userGroupRepository.AddAsync(userGroup);
            }

            var visitor = _mapper.Map<Visitor>(createDto);
            if (visitor == null)
                throw new InvalidOperationException("Failed to map VisitorCreateDto to Visitor");

            var confirmationCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            visitor.Id = Guid.NewGuid();
            visitor.ApplicationId = applicationId;
            visitor.Status = 1;
            visitor.CreatedBy = username ?? "System";
            visitor.CreatedAt = DateTime.UtcNow;
            visitor.UpdatedBy = username ?? "System";
            visitor.UpdatedAt = DateTime.UtcNow;
            
            if (createDto.FaceImage != null && createDto.FaceImage.Length > 0)
            {
                try
                {
                    if (!_allowedImageTypes.Contains(createDto.FaceImage.ContentType))
                        throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

                    if (createDto.FaceImage.Length > MaxFileSize)
                        throw new ArgumentException("File size exceeds 5 MB limit.");

                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
                    Directory.CreateDirectory(uploadDir);

                    var fileName = $"{Guid.NewGuid()}_{createDto.FaceImage.FileName}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await createDto.FaceImage.CopyToAsync(stream);
                    }

                    visitor.FaceImage = $"/Uploads/visitorFaceImages/{fileName}";
                    visitor.UploadFr = 1;
                    visitor.UploadFrError = "Upload successful";
                }
                catch (Exception ex)
                {
                    visitor.UploadFr = 2;
                    visitor.UploadFrError = ex.Message;
                    visitor.FaceImage = "";
                }
            }
            else
            {
                visitor.UploadFr = 0;
                visitor.UploadFrError = "No file uploaded";
                visitor.FaceImage = "";
            }

            // Create User account
            if (await _userRepository.EmailExistsAsync(createDto.Email.ToLower()))
                throw new InvalidOperationException("Email is already registered");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = createDto.Email.ToLower(),
                Email = createDto.Email.ToLower(),
                Password = BCrypt.Net.BCrypt.HashPassword("P@ss0wrd"),
                IsCreatedPassword = 0,
                IsEmailConfirmation = 0,
                EmailConfirmationCode = confirmationCode,
                EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(7),
                EmailConfirmationAt = DateTime.UtcNow,
                LastLoginAt = DateTime.MinValue,
                StatusActive = StatusActive.NonActive,
                ApplicationId = applicationId,
                GroupId = userGroup.Id
            };

            var newTrx = _mapper.Map<TrxVisitor>(createDto);
            newTrx.VisitorId = visitor.Id;
            newTrx.Status = VisitorStatus.Preregist;
            newTrx.InvitationCode = confirmationCode;
            newTrx.IsInvitationAccepted = false;
            newTrx.VisitorGroupCode = visitor.TrxVisitors.Count + 1;
            newTrx.VisitorNumber = $"VIS{visitor.TrxVisitors.Count + 1}";
            newTrx.VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6);
            newTrx.InvitationCreatedAt = DateTime.UtcNow;
            newTrx.TrxStatus = 1;

            // var newTrx = new TrxVisitor
            //     {
            //         VisitorId = visitor.Id,
            //         CheckedInAt = DateTime.UtcNow,
            //         Status = VisitorStatus.Preregist,
            //         InvitationCode = confirmationCode,
            //         IsInvitationAccepted = false,
            //         VisitorGroupCode = visitor.TrxVisitors.Count + 1,
            //         VisitorNumber = $"VIS{visitor.TrxVisitors.Count + 1}",
            //         VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6),
            //         InvitationCreatedAt = DateTime.UtcNow
            //     };

            await _userRepository.AddAsync(newUser);
            await _visitorRepository.AddAsync(visitor);
            await _trxVisitorRepository.AddAsync(newTrx);

            // Send verification email
            await _emailService.SendConfirmationEmailAsync(visitor.Email, visitor.Name, confirmationCode);
            // await _emailService.SendConfirmationEmailAsync(newUser.Email, newUser.Username, confirmationCode);

            var result = _mapper.Map<VisitorDto>(visitor);
            if (result == null)
                throw new InvalidOperationException("Failed to map Visitor to VisitorDto");

            return result;
        }
        
        public async Task<VisitorDto> UpdateVisitorAsync(Guid id, VisitorUpdateDto updateDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            if (updateDto == null) throw new ArgumentNullException(nameof(updateDto));

            var visitor = await _visitorRepository.GetByIdAsync(id);
            if (visitor == null)
                throw new KeyNotFoundException($"Visitor with ID {id} not found.");

             var existingVisitor = await _visitorRepository.GetAllQueryable()
                .FirstOrDefaultAsync(b => b.Email == updateDto.Email ||
                                    b.IdentityId == updateDto.IdentityId ||
                                    b.PersonId == updateDto.PersonId);
                                
            if (existingVisitor != null)
            {
                if (existingVisitor.Email == updateDto.Email)
                {
                    throw new ArgumentException($"Visitor with Email {updateDto.Email} already exists.");
                }
                else if (existingVisitor.IdentityId == updateDto.IdentityId)
                {
                    throw new ArgumentException($"Visitor with Email {updateDto.IdentityId} already exists.");
                }
                else if (existingVisitor.PersonId == updateDto.PersonId)
                {
                    throw new ArgumentException($"Visitor with PersonId {updateDto.PersonId} already exists.");
                }
            }

            // Handle FaceImage upload
            if (updateDto.FaceImage != null && updateDto.FaceImage.Length > 0)
            {
                try
                {
                    if (!_allowedImageTypes.Contains(updateDto.FaceImage.ContentType))
                        throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

                    if (updateDto.FaceImage.Length > MaxFileSize)
                        throw new ArgumentException("File size exceeds 5 MB limit.");

                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
                    Directory.CreateDirectory(uploadDir);

                    var fileName = $"{Guid.NewGuid()}_{updateDto.FaceImage.FileName}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await updateDto.FaceImage.CopyToAsync(stream);
                    }

                    visitor.FaceImage = $"/Uploads/visitorFaceImages/{fileName}";
                    visitor.UploadFr = 1;
                    visitor.UploadFrError = "Upload successful";
                }
                catch (Exception ex)
                {
                    visitor.UploadFr = 2;
                    visitor.UploadFrError = ex.Message;
                    visitor.FaceImage = "";
                }
            }
            else
            {
                visitor.UploadFr = 0;
                visitor.UploadFrError = "No file uploaded";
                visitor.FaceImage = "";
            }

            // Cek jika email sudah digunakan user lain
            if (!string.IsNullOrWhiteSpace(updateDto.Email) &&
                await _userRepository.EmailExistsAsync(updateDto.Email.ToLower()))
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            visitor.UpdatedBy = username ?? "System";
            visitor.UpdatedAt = DateTime.UtcNow;

            // Mapping ke Visitor
            _mapper.Map(updateDto, visitor);
            await _visitorRepository.UpdateAsync(visitor);
            return _mapper.Map<VisitorDto>(visitor);
        }

        // konfirmasi email visitor
        public async Task ConfirmVisitorEmailAsync(ConfirmEmailDto confirmDto)
        {
            if (confirmDto == null)
                throw new ArgumentNullException(nameof(confirmDto));

            if (string.IsNullOrWhiteSpace(confirmDto.Email))
                throw new ArgumentException("Email is required", nameof(confirmDto.Email));

            if (string.IsNullOrWhiteSpace(confirmDto.ConfirmationCode))
                throw new ArgumentException("Confirmation code is required", nameof(confirmDto.ConfirmationCode));

            var user = await _userRepository.GetByEmailConfirmPasswordAsync(confirmDto.Email.ToLower());
            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (user.IsEmailConfirmation == 1)
                throw new InvalidOperationException("Email already confirmed");

            if (user.EmailConfirmationCode != confirmDto.ConfirmationCode)
                throw new InvalidOperationException("Invalid confirmation code");

            if (user.EmailConfirmationExpiredAt < DateTime.UtcNow)
                throw new InvalidOperationException("Confirmation code expired");

            user.IsEmailConfirmation = 1;
            user.EmailConfirmationAt = DateTime.UtcNow;
            user.StatusActive = StatusActive.Active;

            // update user status konfirmasi
            await _userRepository.UpdateConfirmAsync(user);

            var visitor = await _visitorRepository.GetByEmailAsync(confirmDto.Email.ToLower());
            var latestTrx = await _trxVisitorRepository.GetLatestUnfinishedByVisitorIdAsync(visitor.Id);

            latestTrx.IsInvitationAccepted = true;

            // visitor.TrxVisitors = visitor.TrxVisitors ?? new List<TrxVisitor>();
            // var trxVisitor = visitor.TrxVisitors.FirstOrDefault(t => t.Status == null);
            // if (trxVisitor == null)
            // {
            //     trxVisitor = new TrxVisitor
            //     {
            //         Status = VisitorStatus.Checkin,
            //         InvitationCreatedAt = DateTime.UtcNow,
            //         VisitorGroupCode = visitor.TrxVisitors.Count + 1,
            //         VisitorNumber = $"VIS{visitor.TrxVisitors.Count + 1}",
            //         VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid().ToString().Substring(0, 6)}",
            //         CheckedInAt = DateTime.UtcNow,
            //         VisitorId = visitor.Id
            //     };
            //     // visitor.TrxVisitors.Add(trxVisitor);
            //     await _trxVisitorRepository.AddAsync(trxVisitor);
            // }
            // else
            // {
            //     Console.WriteLine("Visitor already checked in");
            // }
            // jika visitor sudah ada
            if (visitor != null)
            {
                latestTrx.IsInvitationAccepted = true;
                latestTrx.Status = VisitorStatus.Precheckin;
                user.IsEmailConfirmation = 1;
                user.EmailConfirmationAt = DateTime.UtcNow;
                user.StatusActive = StatusActive.Active;

                await _visitorRepository.UpdateAsync(visitor);
            }
        }

        // send invitation ke visitor yang sudah terdaftar
        public async Task SendInvitationVisitorAsync(Guid id, CreateInvitationDto createInvitationDto)
        {
            var visitor = await _visitorRepository.GetByIdAsync(id);
            // var latestTrx = await _trxVisitorRepository.GetLatestUnfinishedByVisitorIdAsync(visitorId);

            // if (latestTrx != null && latestTrx.Status == VisitorStatus.Checkin)
            //     throw new InvalidOperationException("Visitor already checked in");
            var confirmationCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            var newTrx = _mapper.Map<TrxVisitor>(createInvitationDto);

            newTrx.VisitorId = visitor.Id;
            newTrx.Status = VisitorStatus.Preregist;
            newTrx.IsInvitationAccepted = false;
            newTrx.TrxStatus = 1;
            newTrx.VisitorGroupCode = visitor.TrxVisitors.Count + 1;
            newTrx.VisitorNumber = $"VIS{visitor.TrxVisitors.Count + 1}";
            newTrx.VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6);
            newTrx.InvitationCreatedAt = DateTime.UtcNow;
            newTrx.InvitationCode = confirmationCode;

            await _emailService.SendConfirmationEmailAsync(visitor.Email, visitor.Name, confirmationCode);
            await _trxVisitorRepository.AddAsync(newTrx);
        }

        // send invitation ke visitor yang blm terdaftar, klu dah ada cuma send doang + insert ke trx visitor
        public async Task SendInvitationByEmailAsync(SendEmailInvitationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email is required");

            // cek visitor base on email
            var existingVisitor = await _visitorRepository.GetByEmailAsync(dto.Email.ToLower());
            Visitor visitor;

            if (existingVisitor == null)
            {
                visitor = new Visitor
                {
                    Id = Guid.NewGuid(),
                    Email = dto.Email.ToLower(),
                    Name = dto.Name ?? "Guest",
                    Status = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "Invitation",
                    UpdatedBy = "Invitation"
                };
                await _visitorRepository.AddAsync(visitor);
            }
            else
            {
                visitor = existingVisitor;
            }

            var trxCount = await _trxVisitorRepository
            .CountByVisitorIdAsync(visitor.Id);

            // Buat undangan baru
            var confirmationCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId")?.Value;

            var newTrx = _mapper.Map<TrxVisitor>(dto);
            newTrx.VisitorId = visitor.Id;
            newTrx.Status = VisitorStatus.Preregist;
            newTrx.IsInvitationAccepted = false;
            newTrx.TrxStatus = 1;
            newTrx.VisitorGroupCode = trxCount + 1;
            newTrx.VisitorNumber = $"VIS{trxCount + 1}";
            newTrx.VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6);
            newTrx.InvitationCreatedAt = DateTime.UtcNow;
            newTrx.InvitationCode = confirmationCode;
            newTrx.InvitationTokenExpiredAt = DateTime.UtcNow.AddDays(3);
            

            var invitationUrl = $"http://192.168.1.116:10000/api/Visitor/fill-invitation-form?InvitationCode={confirmationCode}&ApplicationId={applicationIdClaim}";

            await _trxVisitorRepository.AddAsync(newTrx);
            await _emailService.SendVisitorInvitationEmailAsync(visitor.Email, visitor.Name ?? "Guest", confirmationCode, invitationUrl);
        }

        // fill invitation form
        public async Task<VisitorDto> FillInvitationFormAsync(VisitorInvitationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.InvitationCode))
                throw new ArgumentException("Invitation code is required.");

            var trx = await _trxVisitorRepository
                .GetByInvitationCodeAsync(dto.InvitationCode);

            if (trx == null)
                throw new KeyNotFoundException("Invitation not found or expired.");

            var visitor = trx.Visitor ?? throw new InvalidOperationException("Visitor not found.");

            // update visitor
            _mapper.Map(dto, visitor);
            visitor.UpdatedAt = DateTime.UtcNow;
            visitor.UpdatedBy = "VisitorForm";

            // upload faceimage
            if (dto.FaceImage != null && dto.FaceImage.Length > 0)
            {
                try
                {
                    var allowedImageTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
                    if (!allowedImageTypes.Contains(dto.FaceImage.ContentType))
                        throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

                    var maxFileSize = 5 * 1024 * 1024; // 5 MB
                    if (dto.FaceImage.Length > maxFileSize)
                        throw new ArgumentException("File size exceeds 5 MB limit.");

                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
                    Directory.CreateDirectory(uploadDir);

                    var fileName = $"{Guid.NewGuid()}_{dto.FaceImage.FileName}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.FaceImage.CopyToAsync(stream);
                    }

                    visitor.FaceImage = $"/Uploads/visitorFaceImages/{fileName}";
                    visitor.UploadFr = 1;
                    visitor.UploadFrError = "Upload successful";
                }
                catch (Exception ex)
                {
                    visitor.UploadFr = 2;
                    visitor.UploadFrError = ex.Message;
                    visitor.FaceImage = "";
                }
            }

            // upload trxvisitor
            _mapper.Map(dto, trx);
            trx.IsInvitationAccepted = true;
            trx.UpdatedAt = DateTime.UtcNow;
            trx.UpdatedBy = "VisitorForm";

            await _visitorRepository.UpdateAsyncRaw(visitor);
            await _trxVisitorRepository.UpdateAsyncRaw(trx);

            return _mapper.Map<VisitorDto>(visitor);
        }

        public async Task<VisitorDto> GetVisitorByIdAsync(Guid id)
        {
            var visitor = await _visitorRepository.GetByIdAsync(id);
            return _mapper.Map<VisitorDto>(visitor);
        }

        public async Task<IEnumerable<VisitorDto>> GetAllVisitorsAsync()
        {
            var visitors = await _visitorRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<VisitorDto>>(visitors);
        }

        public async Task DeleteVisitorAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var visitor = await _visitorRepository.GetByIdAsync(id);
            if (visitor == null)
            {
                throw new KeyNotFoundException($"Visitor with ID {id} not found.");
            }
            visitor.Status = 0;
            visitor.UpdatedBy = username;
            visitor.UpdatedAt = DateTime.UtcNow;
            await _visitorRepository.DeleteAsync(visitor);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _visitorRepository.GetAllQueryable();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "Name", "OrganizationName", "DistrictName", "DepartmentName", "Gender", "VisitorActiveStatus", "CardNumber", "Status", "EmailVerficationSendAt", "VisitorPeriodStart", "VisitorPeriodEnd", "PersonId", "CreatedAt", "UpdatedAt", "UpdatedBy", "CreatedBy" };

            var filterService = new GenericDataTableService<Visitor, VisitorDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);


            return await filterService.FilterAsync(request);
        }

        public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            var visitorBlacklistAreas = await _visitorRepository.GetAllAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Visitor Report")
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
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Person ID").SemiBold();
                            header.Cell().Element(CellStyle).Text("Identity ID").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Card Number").SemiBold();
                            header.Cell().Element(CellStyle).Text("BLE Card Number").SemiBold();
                            header.Cell().Element(CellStyle).Text("Phone").SemiBold();
                            header.Cell().Element(CellStyle).Text("Email").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        int index = 1;
                        foreach (var visitor in visitorBlacklistAreas)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(visitor.PersonId ?? "-");
                            table.Cell().Element(CellStyle).Text(visitor.IdentityId ?? "-");
                            table.Cell().Element(CellStyle).Text(visitor.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(visitor.CardNumber ?? "-");
                            table.Cell().Element(CellStyle).Text(visitor.BleCardNumber ?? "-");
                            table.Cell().Element(CellStyle).Text(visitor.Phone ?? "-");
                            table.Cell().Element(CellStyle).Text(visitor.Email ?? "-");
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
            var visitors = await _visitorRepository.GetAllAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Visitors");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Person ID";
            worksheet.Cell(1, 3).Value = "Identity ID";
            worksheet.Cell(1, 4).Value = "Name";
            worksheet.Cell(1, 5).Value = "Card Number";
            worksheet.Cell(1, 6).Value = "BLE Card Number";
            worksheet.Cell(1, 7).Value = "Phone";
            worksheet.Cell(1, 8).Value = "Email";
            worksheet.Cell(1, 9).Value = "Gender";
            worksheet.Cell(1, 10).Value = "Address";

            int row = 2;
            int no = 1;

            foreach (var visitor in visitors)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = visitor.PersonId ?? "-";
                worksheet.Cell(row, 3).Value = visitor.IdentityId ?? "-";
                worksheet.Cell(row, 4).Value = visitor.Name ?? "-";
                worksheet.Cell(row, 5).Value = visitor.CardNumber ?? "-";
                worksheet.Cell(row, 6).Value = visitor.BleCardNumber ?? "-";
                worksheet.Cell(row, 7).Value = visitor.Phone ?? "-";
                worksheet.Cell(row, 8).Value = visitor.Email ?? "-";
                worksheet.Cell(row, 9).Value = visitor.Gender.ToString() ?? "-";
                worksheet.Cell(row, 10).Value = visitor.Address ?? "-";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

    



































// using AutoMapper;
// using Microsoft.AspNetCore.Http;
// using System;
// using System.IO;
// using System.Threading.Tasks;
// using Entities.Models;
// using Data.ViewModels;
// using Repositories.Repository;
// using System.Collections.Generic;

// namespace BusinessLogic.Services.Implementation
// {
//     public interface IVisitorService
//     {
//         Task<VisitorDto> CreateVisitorAsync(VisitorCreateDto createDto);
//     }

//     public class VisitorService : IVisitorService
//     {
//         private readonly IMapper _mapper;
//         private readonly IHttpContextAccessor _httpContextAccessor;
//         private readonly VisitorRepository _visitorRepository;
//         private readonly UserRepository _userRepository;
//         private readonly UserGroupRepository _userGroupRepository;
//         private readonly IEmailService _emailService;
//         private readonly string[] _allowedImageTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
//         private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

//         public VisitorService(
//             IMapper mapper,
//             IHttpContextAccessor httpContextAccessor,
//             VisitorRepository visitorRepository,
//             UserRepository userRepository,
//             UserGroupRepository userGroupRepository,
//             IEmailService emailService)
//         {
//             _mapper = mapper;
//             _httpContextAccessor = httpContextAccessor;
//             _visitorRepository = visitorRepository;
//             _userRepository = userRepository;
//             _userGroupRepository = userGroupRepository;
//             _emailService = emailService;
//         }

//         public async Task<VisitorDto> CreateVisitorAsync(VisitorCreateDto createDto)
//         {
//             if (createDto == null)
//                 throw new ArgumentNullException(nameof(createDto));

//             var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
//             var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//             if (string.IsNullOrEmpty(currentUserId))
//                 throw new UnauthorizedAccessException("User not authenticated");

//             var currentUser = await _userRepository.GetByIdAsync(Guid.Parse(currentUserId));
//             if (currentUser == null)
//                 throw new UnauthorizedAccessException("Current user not found");

//             var applicationId = currentUser.Group.ApplicationId;
//             var userGroup = await _userGroupRepository.GetByApplicationIdAndPriorityAsync(applicationId, LevelPriority.UserCreated);
//             if (userGroup == null)
//                 throw new KeyNotFoundException("User group with UserCreated role not found for this application");

//             var visitor = _mapper.Map<Visitor>(createDto);
//             visitor.Id = Guid.NewGuid();
//             visitor.ApplicationId = applicationId;
//             visitor.Status = 1;
//             visitor.IsInvitationAccepted = false;
//             visitor.CreatedBy = username;
//             visitor.CreatedAt = DateTime.UtcNow;
//             visitor.UpdatedBy = username;
//             visitor.UpdatedAt = DateTime.UtcNow;

//             if (createDto.FaceImage != null && createDto.FaceImage.Length > 0)
//             {
//                 try
//                 {
//                     if (!_allowedImageTypes.Contains(createDto.FaceImage.ContentType))
//                         throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

//                     if (createDto.FaceImage.Length > MaxFileSize)
//                         throw new ArgumentException("File size exceeds 5 MB limit.");

//                     var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
//                     Directory.CreateDirectory(uploadDir);

//                     var fileName = $"{Guid.NewGuid()}_{createDto.FaceImage.FileName}";
//                     var filePath = Path.Combine(uploadDir, fileName);

//                     using (var stream = new FileStream(filePath, FileMode.Create))
//                     {
//                         await createDto.FaceImage.CopyToAsync(stream);
//                     }

//                     visitor.FaceImage = $"/Uploads/visitorFaceImages/{fileName}";
//                     visitor.UploadFr = 1;
//                     visitor.UploadFrError = "Upload successful";
//                 }
//                 catch (Exception ex)
//                 {
//                     visitor.UploadFr = 2;
//                     visitor.UploadFrError = ex.Message;
//                     visitor.FaceImage = "";
//                 }
//             }
//             else
//             {
//                 visitor.UploadFr = 0;
//                 visitor.UploadFrError = "No file uploaded";
//                 visitor.FaceImage = "";
//             }

//             // Create User account
//             if (await _userRepository.EmailExistsAsync(createDto.Email))
//                 throw new InvalidOperationException("Email is already registered");

//             var invitationToken = Guid.NewGuid().ToString("N");
//             var newUser = new User
//             {
//                 Id = Guid.NewGuid(),
//                 Username = createDto.Email.ToLower(), // Use email as username
//                 Email = createDto.Email.ToLower(),
//                 Password = null,
//                 IsCreatedPassword = 0,
//                 IsEmailConfirmation = 0,
//                 EmailConfirmationCode = invitationToken, // Reuse for invitation token
//                 EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(7),
//                 EmailConfirmationAt = DateTime.UtcNow,
//                 LastLoginAt = DateTime.MinValue,
//                 StatusActive = StatusActive.NonActive,
//                 GroupId = userGroup.Id
//             };

//             await _userRepository.AddAsync(newUser);
//             await _visitorRepository.AddAsync(visitor);

//             // Send invitation email
//             await _emailService.SendVisitorInvitationEmailAsync(visitor.Email, visitor.Name, invitationToken);

//             return _mapper.Map<VisitorDto>(visitor);
//         }
//     }
// }



  //  public async Task<VisitorDto> CreateVisitorWithTrxAsync(VisitorWithTrxCreateDto createDto)
        // {
        //     if (createDto == null)
        //         throw new ArgumentNullException(nameof(createDto));

        //     if (string.IsNullOrWhiteSpace(createDto.Email))
        //         throw new ArgumentException("Email is required", nameof(createDto.Email));

        //     if (string.IsNullOrWhiteSpace(createDto.Name))
        //         throw new ArgumentException("Name is required", nameof(createDto.Name));

        //     var username = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        //     var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //     if (string.IsNullOrWhiteSpace(currentUserId))
        //         throw new UnauthorizedAccessException("User not authenticated");

        //     var currentUser = await _userRepository.GetByIdAsync(Guid.Parse(currentUserId));
        //     if (currentUser == null)
        //         throw new UnauthorizedAccessException("Current user not found");

        //     if (currentUser.Group == null || currentUser.Group.ApplicationId == Guid.Empty)
        //         throw new InvalidOperationException("Current user has no valid group or application");

        //     var applicationId = currentUser.ApplicationId;

        //     // Cari atau buat grup dengan LevelPriority.UserCreated
        //     var userGroup = await _userGroupRepository.GetByApplicationIdAndPriorityAsync(applicationId, LevelPriority.UserCreated);
        //     if (userGroup == null)
        //     {
        //         userGroup = new UserGroup
        //         {
        //             Id = Guid.NewGuid(),
        //             Name = "VisitorGroup",
        //             LevelPriority = LevelPriority.UserCreated,
        //             ApplicationId = applicationId,
        //             Status = 1,
        //             CreatedBy = username ?? "System",
        //             CreatedAt = DateTime.UtcNow,
        //             UpdatedBy = username ?? "System",
        //             UpdatedAt = DateTime.UtcNow
        //         };
        //         await _userGroupRepository.AddAsync(userGroup);
        //     }

        //     var visitor = _mapper.Map<Visitor>(createDto);
        //     if (visitor == null)
        //         throw new InvalidOperationException("Failed to map VisitorCreateDto to Visitor");

        //     var confirmationCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

        //     visitor.Id = Guid.NewGuid();
        //     visitor.ApplicationId = applicationId;
        //     visitor.Status = 1;
        //     visitor.CreatedBy = username ?? "System";
        //     visitor.CreatedAt = DateTime.UtcNow;
        //     visitor.UpdatedBy = username ?? "System";
        //     visitor.UpdatedAt = DateTime.UtcNow;

        //     if (createDto.FaceImage != null && createDto.FaceImage.Length > 0)
        //     {
        //         try
        //         {
        //             if (!_allowedImageTypes.Contains(createDto.FaceImage.ContentType))
        //                 throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

        //             if (createDto.FaceImage.Length > MaxFileSize)
        //                 throw new ArgumentException("File size exceeds 5 MB limit.");

        //             var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
        //             Directory.CreateDirectory(uploadDir);

        //             var fileName = $"{Guid.NewGuid()}_{createDto.FaceImage.FileName}";
        //             var filePath = Path.Combine(uploadDir, fileName);

        //             using (var stream = new FileStream(filePath, FileMode.Create))
        //             {
        //                 await createDto.FaceImage.CopyToAsync(stream);
        //             }

        //             visitor.FaceImage = $"/Uploads/visitorFaceImages/{fileName}";
        //             visitor.UploadFr = 1;
        //             visitor.UploadFrError = "Upload successful";
        //         }
        //         catch (Exception ex)
        //         {
        //             visitor.UploadFr = 2;
        //             visitor.UploadFrError = ex.Message;
        //             visitor.FaceImage = "";
        //         }
        //     }
        //     else
        //     {
        //         visitor.UploadFr = 0;
        //         visitor.UploadFrError = "No file uploaded";
        //         visitor.FaceImage = "";
        //     }

        //     // Create User account
        //     if (await _userRepository.EmailExistsAsync(createDto.Email.ToLower()))
        //         throw new InvalidOperationException("Email is already registered");

        //     var newUser = new User
        //     {
        //         Id = Guid.NewGuid(),
        //         Username = createDto.Email.ToLower(),
        //         Email = createDto.Email.ToLower(),
        //         Password = null ?? string.Empty,
        //         IsCreatedPassword = 0,
        //         IsEmailConfirmation = 0,
        //         EmailConfirmationCode = confirmationCode,
        //         EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(7),
        //         EmailConfirmationAt = DateTime.UtcNow,
        //         LastLoginAt = DateTime.MinValue,
        //         StatusActive = StatusActive.NonActive,
        //         ApplicationId = applicationId,
        //         GroupId = userGroup.Id
        //     };

        //     var newTrx = new TrxVisitor
        //         {
        //             VisitorId = visitor.Id,

        //             CheckedInAt = DateTime.UtcNow,
        //             Status = VisitorStatus.Preregist,
        //             InvitationCode = confirmationCode,
        //             IsInvitationAccepted = false,
        //             VisitorGroupCode = visitor.TrxVisitors.Count + 1,
        //             VisitorNumber = $"VIS{visitor.TrxVisitors.Count + 1}",
        //             VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6),
        //             InvitationCreatedAt = DateTime.UtcNow
        //         };

        //         await _userRepository.AddAsync(newUser);
        //         await _visitorRepository.AddAsync(visitor);
        //         await _trxVisitorRepository.AddAsync(newTrx);

        //         // Send verification email
        //         await _emailService.SendConfirmationEmailAsync(visitor.Email, visitor.Name, confirmationCode);
        //         // await _emailService.SendConfirmationEmailAsync(newUser.Email, newUser.Username, confirmationCode);

        //     var result = _mapper.Map<VisitorDto>(visitor);
        //     if (result == null)
        //         throw new InvalidOperationException("Failed to map Visitor to VisitorDto");

        //     return result;
        // }


              // public async Task<VisitorDto> UpdateVisitorAsync(Guid id, VisitorUpdateDto updateDto)
        // {
        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        //     if (updateDto == null) throw new ArgumentNullException(nameof(updateDto));

        //     var visitor = await _visitorRepository.GetByIdAsync(id);
        //     if (visitor == null)
        //     {
        //         throw new KeyNotFoundException($"Visitor with ID {id} not found.");
        //     }

        //     // if (!await _repository.ApplicationExists(updateDto.ApplicationId))
        //     //     throw new ArgumentException($"Application with ID {updateDto.ApplicationId} not found.");

        //     if (updateDto.FaceImage != null && updateDto.FaceImage.Length > 0)
        //     {
        //         try
        //         {
        //             if (!_allowedImageTypes.Contains(updateDto.FaceImage.ContentType))
        //                 throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

        //             // Validasi ukuran file
        //             if (updateDto.FaceImage.Length > MaxFileSize)
        //                 throw new ArgumentException("File size exceeds 5 MB limit.");

        //             // folder penyimpanan di lokal server
        //             var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
        //             Directory.CreateDirectory(uploadDir); // akan membuat directory jika belum ada

        //             // buat nama file unik
        //             var fileName = $"{Guid.NewGuid()}_{updateDto.FaceImage.FileName}";
        //             var filePath = Path.Combine(uploadDir, fileName);

        //             using (var stream = new FileStream(filePath, FileMode.Create))
        //             {
        //                 await updateDto.FaceImage.CopyToAsync(stream);
        //             }

        //             visitor.FaceImage = $"/Uploads/visitorFaceImages/{fileName}";
        //             visitor.UploadFr = 1; // Sukses
        //             visitor.UploadFrError = "Upload successful";
        //         }
        //         catch (Exception ex)
        //         {
        //             visitor.UploadFr = 2;
        //             visitor.UploadFrError = ex.Message;
        //             visitor.FaceImage = "";
        //         }
        //     }
        //     else
        //     {
        //         visitor.UploadFr = 0;
        //         visitor.UploadFrError = "No file uploaded";
        //         visitor.FaceImage = "";
        //     }

        //     if (await _userRepository.EmailExistsAsync(updateDto.Email.ToLower()))
        //         throw new InvalidOperationException("Email is already registered");

        //     visitor.UpdatedBy = username;
        //     visitor.UpdatedAt = DateTime.UtcNow;

        //     _mapper.Map(updateDto, visitor);
        //     await _visitorRepository.UpdateAsync(visitor);
        //     return _mapper.Map<VisitorDto>(visitor);
        // }
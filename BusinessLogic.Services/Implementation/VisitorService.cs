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
using AutoMapper.Execution;
using Microsoft.EntityFrameworkCore.Storage;

namespace BusinessLogic.Services.Implementation
{

public class VisitorService : IVisitorService
{
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITrxVisitorService _trxVisitorService;
    private readonly VisitorRepository _visitorRepository;
    private readonly MstMemberRepository _mstmemberRepository;
    private readonly UserRepository _userRepository;
    private readonly TrxVisitorRepository _trxVisitorRepository;
    private readonly CardRepository _cardRepository;
    private readonly ICardRecordService _cardRecordService;
    private readonly CardRecordRepository _cardRecordRepository;
    private readonly UserGroupRepository _userGroupRepository;
    private readonly IEmailService _emailService;
    private readonly string[] _allowedImageTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public VisitorService(
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            VisitorRepository visitorRepository,
            ITrxVisitorService trxVisitorService,
            UserRepository userRepository,
            UserGroupRepository userGroupRepository,
            TrxVisitorRepository trxVisitorRepository,
            MstMemberRepository mstmemberRepository,
            CardRepository cardRepository,
            ICardRecordService cardRecordService,
            CardRecordRepository cardRecordRepository,
            IEmailService emailService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _visitorRepository = visitorRepository ?? throw new ArgumentNullException(nameof(visitorRepository));
            _trxVisitorService = trxVisitorService ?? throw new ArgumentNullException();
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _cardRecordRepository = cardRecordRepository ?? throw new ArgumentNullException(nameof(cardRecordRepository));
            _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
            _userGroupRepository = userGroupRepository ?? throw new ArgumentNullException(nameof(userGroupRepository));
            _trxVisitorRepository = trxVisitorRepository ?? throw new ArgumentNullException(nameof(trxVisitorRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _cardRecordService = cardRecordService ?? throw new ArgumentNullException();
            _mstmemberRepository = mstmemberRepository ?? throw new ArgumentNullException(nameof(mstmemberRepository));
    }


        //     public async Task<VisitorDto> CreateVisitorAsync(VisitorCreateDto createDto)
        // {
        //     if (createDto == null)
        //         throw new ArgumentNullException(nameof(createDto));

        //     if (string.IsNullOrWhiteSpace(createDto.Email))
        //         throw new ArgumentException("Email is required", nameof(createDto.Email));

        //     if (string.IsNullOrWhiteSpace(createDto.Name))
        //         throw new ArgumentException("Name is required", nameof(createDto.Name));

        //     var existingVisitor = await _visitorRepository.GetAllQueryable()
        //         .FirstOrDefaultAsync(b => b.Email == createDto.Email ||
        //                                  b.IdentityId == createDto.IdentityId ||
        //                                  b.PersonId == createDto.PersonId);

        //     if (existingVisitor != null)
        //     {
        //         if (existingVisitor.Email == createDto.Email)
        //             throw new ArgumentException($"Visitor with Email {createDto.Email} already exists.");
        //         if (existingVisitor.IdentityId == createDto.IdentityId)
        //             throw new ArgumentException($"Visitor with IdentityId {createDto.IdentityId} already exists.");
        //         if (existingVisitor.PersonId == createDto.PersonId)
        //             throw new ArgumentException($"Visitor with PersonId {createDto.PersonId} already exists.");
        //     }

        //     // Get ApplicationId and username
        //     Guid? applicationId = null;
        //     string username = "System";
        //     var isAuthenticated = _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        //     if (isAuthenticated)
        //     {
        //         var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //         if (string.IsNullOrWhiteSpace(currentUserId))
        //             throw new UnauthorizedAccessException("User not authenticated");

        //         var currentUser = await _userRepository.GetByIdAsync(Guid.Parse(currentUserId));
        //         if (currentUser == null)
        //             throw new UnauthorizedAccessException("Current user not found");

        //         applicationId = currentUser.ApplicationId;
        //         username = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        //     }
        //     else
        //     {
        //         var integration = _httpContextAccessor.HttpContext?.Items["MstIntegration"] as MstIntegration;
        //         if (integration?.ApplicationId == null)
        //             throw new UnauthorizedAccessException("ApplicationId not found in context");

        //         applicationId = integration.ApplicationId;
        //     }

        //     if (applicationId == Guid.Empty)
        //         throw new InvalidOperationException("Invalid ApplicationId");


        //     // Find or create UserGroup with LevelPriority.UserCreated
        //     var userGroup = await _userGroupRepository.GetByApplicationIdAndPriorityAsync(applicationId.Value, LevelPriority.UserCreated);
        //     if (userGroup == null)
        //     {
        //         userGroup = new UserGroup
        //         {
        //             Id = Guid.NewGuid(),
        //             Name = "VisitorGroup",
        //             LevelPriority = LevelPriority.UserCreated,
        //             ApplicationId = applicationId.Value,
        //             Status = 1,
        //             CreatedBy = username,
        //             CreatedAt = DateTime.UtcNow,
        //             UpdatedBy = username,
        //             UpdatedAt = DateTime.UtcNow
        //         };
        //         await _userGroupRepository.AddAsync(userGroup);
        //     }

        //     var visitor = _mapper.Map<Visitor>(createDto);
        //     if (visitor == null)
        //         throw new InvalidOperationException("Failed to map VisitorCreateDto to Visitor");

        //     var confirmationCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

        //     visitor.Id = Guid.NewGuid();
        //     visitor.ApplicationId = applicationId.Value;
        //     visitor.Status = 1;
        //     visitor.CreatedBy = username;
        //     visitor.CreatedAt = DateTime.UtcNow;
        //     visitor.UpdatedBy = username;
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
        //         Password = BCrypt.Net.BCrypt.HashPassword("P@ss0wrd"),
        //         IsCreatedPassword = 1,
        //         IsEmailConfirmation = 1,
        //         EmailConfirmationCode = confirmationCode,
        //         EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(7),
        //         EmailConfirmationAt = DateTime.UtcNow,
        //         LastLoginAt = DateTime.MinValue,
        //         StatusActive = StatusActive.Active,
        //         ApplicationId = applicationId.Value,
        //         GroupId = userGroup.Id
        //     };

        //     var newTrx = _mapper.Map<TrxVisitor>(createDto);
        //     newTrx.VisitorId = visitor.Id;
        //     newTrx.Status = VisitorStatus.Checkin;
        //     newTrx.InvitationCode = confirmationCode;
        //     newTrx.VisitorGroupCode = visitor.TrxVisitors.Count + 1;
        //     newTrx.VisitorNumber = $"VIS{visitor.TrxVisitors.Count + 1}";
        //     newTrx.VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6);
        //     newTrx.InvitationCreatedAt = DateTime.UtcNow;
        //     newTrx.TrxStatus = 1;

        //     newTrx.CheckedInAt = DateTime.UtcNow;
        //     newTrx.CheckinBy = username;

        //     //  var createDto = new CardRecordCreateDto
        //     //         {
        //     //             CardId = dto.CardId,
        //     //             VisitorId = trx.VisitorId,
        //     //             // TrxVisitorId = dto.TrxVisitorId 
        //     //         };

        //     await _cardRecordService.CreateAsync(createDto);
        //     await _userRepository.AddAsync(newUser);
        //     await _visitorRepository.AddAsync(visitor);
        //     await _trxVisitorRepository.AddAsync(newTrx);

        //     // Send verification email
        //     await _emailService.SendConfirmationEmailAsync(visitor.Email, visitor.Name, confirmationCode);

        //     var result = _mapper.Map<VisitorDto>(visitor);
        //     if (result == null)
        //         throw new InvalidOperationException("Failed to map Visitor to VisitorDto");

        //     return result;
        // }
        
        
         public async Task<VisitorDto> CreateVisitorAsync(OpenVisitorCreateDto createDto)
    {
        if (createDto == null)
            throw new ArgumentNullException(nameof(createDto));

        if (string.IsNullOrWhiteSpace(createDto.Email))
            throw new ArgumentException("Email is required", nameof(createDto.Email));

        if (string.IsNullOrWhiteSpace(createDto.Name))
            throw new ArgumentException("Name is required", nameof(createDto.Name));

        if (createDto.CardId == Guid.Empty)
            throw new ArgumentException("CardId is required for check-in", nameof(createDto.CardId));

        // Check for duplicate visitor
        var existingVisitor = await _visitorRepository.GetAllQueryable()
            .FirstOrDefaultAsync(b => b.Email == createDto.Email ||
                                     b.IdentityId == createDto.IdentityId ||
                                     b.PersonId == createDto.PersonId);

        if (existingVisitor != null)
        {
            if (existingVisitor.Email == createDto.Email)
                throw new ArgumentException($"Visitor with Email {createDto.Email} already exists.");
            if (existingVisitor.IdentityId == createDto.IdentityId)
                throw new ArgumentException($"Visitor with IdentityId {createDto.IdentityId} already exists.");
            if (existingVisitor.PersonId == createDto.PersonId)
                throw new ArgumentException($"Visitor with PersonId {createDto.PersonId} already exists.");
        }

        // Get ApplicationId and username
        Guid? applicationId = null;
        string username = "System";

        if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false)
        {
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(currentUserId))
                throw new UnauthorizedAccessException("User not authenticated");

            var currentUser = await _userRepository.GetByIdAsync(Guid.Parse(currentUserId));
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found");

            applicationId = currentUser.ApplicationId;
            username = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        }
        else
        {
            var integration = _httpContextAccessor.HttpContext?.Items["MstIntegration"] as MstIntegration;
            if (integration?.ApplicationId == null)
                throw new UnauthorizedAccessException("ApplicationId not found in context");

            applicationId = integration.ApplicationId;
        }

        if (!applicationId.HasValue || applicationId == Guid.Empty)
            throw new InvalidOperationException("Invalid ApplicationId");

        // Find or create UserGroup with LevelPriority.UserCreated
        var userGroup = await _userGroupRepository.GetByApplicationIdAndPriorityAsync(applicationId.Value, LevelPriority.UserCreated);
        if (userGroup == null)
        {
            userGroup = new UserGroup
            {
                Id = Guid.NewGuid(),
                Name = "VisitorGroup",
                LevelPriority = LevelPriority.UserCreated,
                ApplicationId = applicationId.Value,
                Status = 1,
                CreatedBy = username,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = username,
                UpdatedAt = DateTime.UtcNow
            };
            await _userGroupRepository.AddAsync(userGroup);
        }

        var visitor = _mapper.Map<Visitor>(createDto);
        if (visitor == null)
            throw new InvalidOperationException("Failed to map VisitorCreateDto to Visitor");

        var confirmationCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

        visitor.Id = Guid.NewGuid();
        visitor.ApplicationId = applicationId.Value;
        visitor.Status = 1;
        visitor.CreatedBy = username;
        visitor.CreatedAt = DateTime.UtcNow;
        visitor.UpdatedBy = username;
        visitor.UpdatedAt = DateTime.UtcNow;

        // Handle FaceImage upload
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
            IsCreatedPassword = 1,
            IsEmailConfirmation = 1,
            EmailConfirmationCode = confirmationCode,
            EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(7),
            EmailConfirmationAt = DateTime.UtcNow,
            LastLoginAt = DateTime.MinValue,
            StatusActive = StatusActive.Active,
            ApplicationId = applicationId.Value,
            GroupId = userGroup.Id
        };

        // Create TrxVisitor with check-in
        var newTrx = _mapper.Map<TrxVisitor>(createDto);
        newTrx.Id = Guid.NewGuid();
        newTrx.VisitorId = visitor.Id;
        newTrx.Status = VisitorStatus.Checkin;
        newTrx.VisitorActiveStatus = VisitorActiveStatus.Active;
        newTrx.InvitationCode = confirmationCode;
        newTrx.VisitorGroupCode = 1;
        newTrx.VisitorNumber = $"VIS{DateTime.UtcNow.Ticks}";
        newTrx.VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6);
        newTrx.InvitationCreatedAt = DateTime.UtcNow;
        newTrx.TrxStatus = 1;
        newTrx.CheckedInAt = DateTime.UtcNow;
        newTrx.CheckinBy = username;
        newTrx.ApplicationId = applicationId.Value;
        newTrx.CreatedBy = username;
        newTrx.CreatedAt = DateTime.UtcNow;
        newTrx.UpdatedBy = username;
        newTrx.UpdatedAt = DateTime.UtcNow;
        newTrx.IsInvitationAccepted = true;

        // Check for active transactions
        var activeTrx = await _trxVisitorRepository.GetAllQueryableRaw()
            .Where(t => t.VisitorId == visitor.Id && t.Status == VisitorStatus.Checkin && t.CheckedOutAt == null)
            .AnyAsync();
        if (activeTrx)
            throw new InvalidOperationException("Visitor already has an active transaction");

        // Create CardRecord
        var cardRecordDto = new CardRecordCreateDto
        {
            CardId = createDto.CardId,
            VisitorId = visitor.Id,
        };
    

        // Perform all database operations in a transaction
            using var transaction = await _visitorRepository.BeginTransactionAsync();
        try
        {
            await _userRepository.AddAsync(newUser);
            await _visitorRepository.AddAsync(visitor);
            await _trxVisitorRepository.AddAsync(newTrx);
            await _cardRecordService.CreateAsync(cardRecordDto);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        // Send verification email
        await _emailService.SendConfirmationEmailAsync(visitor.Email, visitor.Name, confirmationCode);

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
               .Where(v => v.Id != id)
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

            var visitor = await _visitorRepository.GetByEmailAsyncRaw(confirmDto.Email.ToLower());
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
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            // var latestTrx = await _trxVisitorRepository.GetLatestUnfinishedByVisitorIdAsync(visitorId);

            // if (latestTrx != null && latestTrx.Status == VisitorStatus.Checkin)
            //     throw new InvalidOperationException("Visitor already checked in");
            var confirmationCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            var newTrx = _mapper.Map<TrxVisitor>(createInvitationDto);

            newTrx.VisitorId = visitor.Id;
            newTrx.Status = VisitorStatus.Preregist;
            // newTrx.IsInvitationAccepted = false;
            newTrx.TrxStatus = 1;
            newTrx.VisitorGroupCode = visitor.TrxVisitors.Count + 1;
            newTrx.VisitorNumber = $"VIS{visitor.TrxVisitors.Count + 1}";
            newTrx.VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6);
            newTrx.InvitationCreatedAt = DateTime.UtcNow;
            newTrx.InvitationCode = confirmationCode;
            newTrx.UpdatedAt = DateTime.UtcNow;
            newTrx.UpdatedBy = username ?? "Invitation";
            newTrx.CreatedAt = DateTime.UtcNow;
            newTrx.CreatedBy = username ?? "Invitation";

            await _emailService.SendConfirmationEmailAsync(visitor.Email, visitor.Name, confirmationCode);
            await _trxVisitorRepository.AddAsync(newTrx);
        }

        // send invitation ke visitor yang blm terdaftar, klu dah ada cuma send doang + insert ke trx visitor
        public async Task SendInvitationByEmailAsync(SendEmailInvitationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email is required");
            if (dto.VisitorPeriodStart == null || dto.VisitorPeriodEnd == null)
                throw new ArgumentException($"Visitor period must be filled for {dto.Email}");
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
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
                    CreatedBy = username ?? "Invitation",
                    UpdatedBy = username ?? "Invitation"
                };
                await _visitorRepository.AddAsync(visitor);
            }
            else
            {
                visitor = existingVisitor;
            }
             bool exists = await _trxVisitorRepository.ExistsOverlappingTrxAsync(
                        existingVisitor.Id,
                            dto.VisitorPeriodStart.Value,
                            dto.VisitorPeriodEnd.Value
                    );
                    if (exists)
                        throw new InvalidOperationException($"Invitation already exists for {dto.Email} in that period");

            var trxCount = await _trxVisitorRepository
            .CountByVisitorIdAsync(visitor.Id);

            // Buat undangan baru
            var confirmationCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId")?.Value;
            // var memberEmail = await _mstmemberRepository.GetByEmailAsync(dto.Email.ToLower());
            // var userEmail = await _userRepository.GetByEmailAsync(dto.Email.ToLower());

            // var isMember = memberEmail != null || userEmail != null ? 1 : 0;

            var newTrx = _mapper.Map<TrxVisitor>(dto);
            newTrx.VisitorId = visitor.Id;
            newTrx.Status = VisitorStatus.Preregist;
            // newTrx.IsInvitationAccepted = false;
            newTrx.TrxStatus = 1;
            newTrx.VisitorGroupCode = trxCount + 1;
            newTrx.VisitorNumber = $"VIS{trxCount + 1}";
            newTrx.VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6);
            newTrx.InvitationCreatedAt = DateTime.UtcNow;
            newTrx.UpdatedAt = DateTime.UtcNow;
            newTrx.CreatedAt = DateTime.UtcNow;
            newTrx.UpdatedBy = username ?? "Invitation";
            newTrx.CreatedBy = username ?? "Invitation";
            newTrx.InvitationCode = confirmationCode;
            newTrx.InvitationTokenExpiredAt = DateTime.UtcNow.AddDays(3);
            // newTrx.IsMember = isMember;


            // var invitationUrl = $"http://192.168.1.116:10000/api/Visitor/fill-invitation-form?code={confirmationCode}&applicationId={applicationIdClaim}&visitorId={visitor.Id}&trxVisitorId={newTrx.Id}";
            var invitationUrl = $"http://192.168.1.173:3000/visitor-info?code={confirmationCode}&applicationId={applicationIdClaim}&visitorId={visitor.Id}&trxVisitorId={newTrx.Id}";
            // var memberInvitationUrl = $"http://192.168.1.173:3000/visitor-info?code={confirmationCode}&applicationId={applicationIdClaim}&trxVisitorId={newTrx.Id}";

            await _trxVisitorRepository.AddAsync(newTrx);
            // var memberId = newTrx.PurposePerson ?? Guid.Empty;
            // var member = await _mstmemberRepository.GetByIdAsync(memberId);
            // var memberName = member?.Name ?? "-";
            // var member = await _mstmemberRepository.GetByIdAsync(newTrx.PurposePerson.GetValueOrDefault());
            // var memberName = member?.Name ?? "-";
            var visitorPeriodStartDate = newTrx.VisitorPeriodStart?.ToString("yyyy-MM-dd") ?? "Unknown";
            var visitorPeriodEndDate   = newTrx.VisitorPeriodEnd?.ToString("yyyy-MM-dd") ?? "Unknown";
            var visitorPeriodStartTime = newTrx.VisitorPeriodStart?.ToString("HH:mm:ss") ?? "Unknown";
            var visitorPeriodEndTime   = newTrx.VisitorPeriodEnd?.ToString("HH:mm:ss") ?? "Unknown";
            var invitationAgenda = newTrx.Agenda;
            
            var savedTrx = await _trxVisitorRepository.GetByIdAsync(newTrx.Id);
            var maskedAreaName = savedTrx?.MaskedArea?.Name ?? "";
            var floorName = await _trxVisitorRepository.GetFloorNameByTrxIdAsync(newTrx.Id) ?? "";
            var buildingName = await _trxVisitorRepository.GetBuildingNameByTrxIdAsync(newTrx.Id) ?? "";
            var memberName = savedTrx?.Member?.Name ?? "-";

            // if (newTrx.IsMember == 1)
            // {
            //     await _emailService.SendMemberInvitationEmailAsync(visitor.Email, visitor.Name ?? "Member", confirmationCode, memberInvitationUrl, visitorPeriodStart, visitorPeriodEnd);
            // }
            await _emailService.SendVisitorInvitationEmailAsync(
                    visitor.Email,
                    visitor.Name ?? "Guest",
                    confirmationCode,
                    invitationUrl,
                    visitorPeriodStartDate,
                    visitorPeriodEndDate,
                    visitorPeriodStartTime,
                    visitorPeriodEndTime,
                     invitationAgenda,
                     maskedAreaName,
                     memberName,
                     floorName,
                     buildingName
                 );
        }

        //         public async Task SendBatchInvitationByEmailAsync(List<SendEmailInvitationDto> dtoList)
        // {
        //     if (dtoList == null || !dtoList.Any())
        //         throw new ArgumentException("Invitation list cannot be empty");

        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        //     var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId")?.Value;
        //     var loggedInUserEmail = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

        //     // Cek apakah user login adalah member
        //     var loggedInMember = !string.IsNullOrWhiteSpace(loggedInUserEmail)
        //         ? await _mstmemberRepository.GetByEmailAsyncRaw(loggedInUserEmail)
        //         : null;

        //     var firstVisitorEmail = dtoList.FirstOrDefault()?.Email?.ToLower();
        //     if (string.IsNullOrWhiteSpace(firstVisitorEmail))
        //         throw new ArgumentException("At least one valid email is required");

        //     var firstVisitor = await _visitorRepository.GetByEmailAsync(firstVisitorEmail)
        //         ?? new Visitor { Id = Guid.NewGuid(), Email = firstVisitorEmail };

        //     var baseGroupCode = await _trxVisitorRepository.CountByVisitorIdAsync(firstVisitor.Id) + 1;

        //     // Validasi semua data sebelum eksekusi
        //     foreach (var dto in dtoList)
        //     {
        //         if (string.IsNullOrWhiteSpace(dto.Email))
        //             throw new ArgumentException("Email is required");

        //         if (dto.VisitorPeriodStart == null || dto.VisitorPeriodEnd == null)
        //             throw new ArgumentException($"Visitor period must be filled for {dto.Email}");

        //         if (dto.VisitorPeriodStart > dto.VisitorPeriodEnd)
        //             throw new ArgumentException($"Invalid period for {dto.Email}");

        //         var existingVisitor = await _visitorRepository.GetByEmailAsync(dto.Email.ToLower());
        //         if (existingVisitor != null)
        //         {
        //             bool exists = await _trxVisitorRepository.ExistsOverlappingTrxAsync(
        //                 existingVisitor.Id,
        //                 dto.VisitorPeriodStart.Value,
        //                 dto.VisitorPeriodEnd.Value
        //             );

        //             if (exists)
        //                 throw new InvalidOperationException($"Invitation already exists for {dto.Email} in that period");
        //         }
        //     }

        //     // Proses insert & kirim email
        //     foreach (var dto in dtoList)
        //     {
        //         var email = dto.Email.ToLower();

        //         var existingVisitor = await _visitorRepository.GetByEmailAsync(email);
        //         Visitor visitor;

        //         if (existingVisitor == null)
        //         {
        //             visitor = new Visitor
        //             {
        //                 Id = Guid.NewGuid(),
        //                 Email = email,
        //                 Name = dto.Name,
        //                 Status = 1,
        //                 CreatedAt = DateTime.UtcNow,
        //                 UpdatedAt = DateTime.UtcNow,
        //                 CreatedBy = username ?? "Invitation",
        //                 UpdatedBy = username ?? "Invitation"
        //             };
        //             await _visitorRepository.AddAsync(visitor);
        //         }
        //         else
        //         {
        //             visitor = existingVisitor;
        //         }

        //         var confirmationCode = Guid.NewGuid().ToString("N")[..6].ToUpper();

        //         var newTrx = _mapper.Map<TrxVisitor>(dto);
        //         newTrx.VisitorId = visitor.Id;
        //         newTrx.Status = VisitorStatus.Preregist;
        //         newTrx.TrxStatus = 1;
        //         newTrx.VisitorGroupCode = baseGroupCode;
        //         newTrx.VisitorNumber = $"VIS{baseGroupCode}";
        //         newTrx.VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6);
        //         newTrx.InvitationCreatedAt = DateTime.UtcNow;
        //         newTrx.UpdatedAt = DateTime.UtcNow;
        //         newTrx.CreatedAt = DateTime.UtcNow;
        //         newTrx.UpdatedBy = username ?? "Invitation";
        //         newTrx.CreatedBy = username ?? "Invitation";
        //         newTrx.InvitationCode = confirmationCode;
        //         newTrx.InvitationTokenExpiredAt = DateTime.UtcNow.AddDays(3);

        //                 // Tentukan PurposePerson
        //                 if (loggedInMember != null)
        //                 {
        //                     // Member login → cek apakah yang diundang member
        //                     var invitedMember = await _mstmemberRepository.GetByEmailAsyncRaw(email);
        //                     newTrx.PurposePerson = loggedInMember.Id;
        //                     newTrx.MemberIdentity = invitedMember?.IdentityId;
        //                     newTrx.IsMember = 1;
        //         }
        //                 else
        //                 {
        //                     // Operator login → wajib isi PurposePerson
        //                     if (!dto.PurposePerson.HasValue)
        //                         throw new ArgumentException($"PurposePerson (member host) is required for {dto.Email}");

        //                     newTrx.PurposePerson = dto.PurposePerson.Value;
        //                 }

        //         await _trxVisitorRepository.AddAsync(newTrx);

        //         // Data untuk email
        //         var visitorPeriodStart = newTrx.VisitorPeriodStart?.ToString("yyyy-MM-dd") ?? "Unknown";
        //         var visitorPeriodEnd = newTrx.VisitorPeriodEnd?.ToString("yyyy-MM-dd") ?? "Unknown";
        //         var invitationAgenda = newTrx.Agenda;

        //         var savedTrx = await _trxVisitorRepository.GetByIdAsync(newTrx.Id);
        //         var maskedAreaName = savedTrx?.MaskedArea?.Name ?? "";
        //         var memberName = savedTrx?.Member?.Name ?? "";
        //         var floorName = await _trxVisitorRepository.GetFloorNameByTrxIdAsync(newTrx.Id) ?? "";
        //         var buildingName = await _trxVisitorRepository.GetBuildingNameByTrxIdAsync(newTrx.Id) ?? "";

        //         var invitationUrl =
        //             $"http://192.168.1.173:3000/visitor-info?code={confirmationCode}&applicationId={applicationIdClaim}&visitorId={visitor.Id}&trxVisitorId={newTrx.Id}";

        //         await _emailService.SendVisitorInvitationEmailAsync(
        //             visitor.Email,
        //             visitor.Name ?? "Guest",
        //             confirmationCode,
        //             invitationUrl,
        //             visitorPeriodStart,
        //             visitorPeriodEnd,
        //             invitationAgenda,
        //             maskedAreaName,
        //             memberName,
        //             floorName,
        //             buildingName
        //         );
        //     }
        // }



        //     public async Task SendBatchInvitationByEmailAsync(List<SendEmailInvitationDto> dtoList)
        // {
        //     if (dtoList == null || !dtoList.Any())
        //         throw new ArgumentException("Invitation list cannot be empty");

        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        //     var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId")?.Value;

        //     var firstVisitorEmail = dtoList.FirstOrDefault()?.Email?.ToLower();
        //     if (string.IsNullOrWhiteSpace(firstVisitorEmail))
        //         throw new ArgumentException("At least one valid email is required");

        //     var firstVisitor = await _visitorRepository.GetByEmailAsync(firstVisitorEmail)
        //         ?? new Visitor { Id = Guid.NewGuid(), Email = firstVisitorEmail };

        //     var baseGroupCode = await _trxVisitorRepository.CountByVisitorIdAsync(firstVisitor.Id) + 1;

        //     // Validasi semua data sebelum eksekusi
        //     foreach (var dto in dtoList)
        //     {
        //         if (string.IsNullOrWhiteSpace(dto.Email))
        //             throw new ArgumentException("Email is required");

        //         if (dto.VisitorPeriodStart == null || dto.VisitorPeriodEnd == null)
        //             throw new ArgumentException($"Visitor period must be filled for {dto.Email}");

        //         if (dto.VisitorPeriodStart > dto.VisitorPeriodEnd)
        //             throw new ArgumentException($"Invalid period for {dto.Email}");

        //         var existingVisitor = await _visitorRepository.GetByEmailAsync(dto.Email.ToLower());
        //         if (existingVisitor != null)
        //         {
        //             bool exists = await _trxVisitorRepository.ExistsOverlappingTrxAsync(
        //                 existingVisitor.Id,
        //                 dto.VisitorPeriodStart.Value,
        //                 dto.VisitorPeriodEnd.Value
        //             );

        //             if (exists)
        //                 throw new InvalidOperationException($"Invitation already exists for {dto.Email} in that period");
        //         }
        //     }

        //     // Kalau lolos semua → proses insert & kirim email
        //     foreach (var dto in dtoList)
        //     {
        //         var email = dto.Email.ToLower();

        //         var existingVisitor = await _visitorRepository.GetByEmailAsync(email);
        //         Visitor visitor;

        //         if (existingVisitor == null)
        //         {
        //             visitor = new Visitor
        //             {
        //                 Id = Guid.NewGuid(),
        //                 Email = email,
        //                 Name = dto.Name,
        //                 Status = 1,
        //                 CreatedAt = DateTime.UtcNow,
        //                 UpdatedAt = DateTime.UtcNow,
        //                 CreatedBy = username ?? "Invitation",
        //                 UpdatedBy = username ?? "Invitation"
        //             };
        //             await _visitorRepository.AddAsync(visitor);
        //         }
        //         else
        //         {
        //             visitor = existingVisitor;
        //         }

        //         var confirmationCode = Guid.NewGuid().ToString("N")[..6].ToUpper();

        //         var newTrx = _mapper.Map<TrxVisitor>(dto);
        //         newTrx.VisitorId = visitor.Id;
        //         newTrx.Status = VisitorStatus.Preregist;
        //         // newTrx.IsInvitationAccepted = false;
        //         newTrx.TrxStatus = 1;
        //         newTrx.VisitorGroupCode = baseGroupCode;
        //         newTrx.VisitorNumber = $"VIS{baseGroupCode}";
        //         newTrx.VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6);
        //         newTrx.InvitationCreatedAt = DateTime.UtcNow;
        //         newTrx.UpdatedAt = DateTime.UtcNow;
        //         newTrx.CreatedAt = DateTime.UtcNow;
        //         newTrx.UpdatedBy = username ?? "Invitation";
        //         newTrx.CreatedBy = username ?? "Invitation";
        //         newTrx.InvitationCode = confirmationCode;
        //         newTrx.InvitationTokenExpiredAt = DateTime.UtcNow.AddDays(3);

        //         await _trxVisitorRepository.AddAsync(newTrx);

        //         var visitorPeriodStart = newTrx.VisitorPeriodStart?.ToString("yyyy-MM-dd") ?? "Unknown";
        //         var visitorPeriodEnd = newTrx.VisitorPeriodEnd?.ToString("yyyy-MM-dd") ?? "Unknown";
        //         var invitationAgenda = newTrx.Agenda;

        //         var savedTrx = await _trxVisitorRepository.GetByIdAsync(newTrx.Id);
        //         var maskedAreaName = savedTrx?.MaskedArea?.Name ?? "";

        //         var memberName = savedTrx?.Member?.Name ?? "";
        //         var floorName = await _trxVisitorRepository.GetFloorNameByTrxIdAsync(newTrx.Id) ?? "";
        //         var buildingName = await _trxVisitorRepository.GetBuildingNameByTrxIdAsync(newTrx.Id) ?? "";


        //         var invitationUrl = $"http://192.168.1.173:3000/visitor-info?code={confirmationCode}&applicationId={applicationIdClaim}&visitorId={visitor.Id}&trxVisitorId={newTrx.Id}";

        //         await _emailService.SendVisitorInvitationEmailAsync(
        //             visitor.Email,
        //             visitor.Name ?? "Guest",
        //             confirmationCode,
        //             invitationUrl,
        //             visitorPeriodStart,
        //             visitorPeriodEnd,
        //             invitationAgenda,
        //             maskedAreaName,
        //             memberName,
        //             floorName,
        //             buildingName
        //         );
        //     }
        // }



        //   public async Task SendInvitationByEmailAsync(SendEmailInvitationDto dto)
        // {
        //     if (string.IsNullOrWhiteSpace(dto.Email))
        //         throw new ArgumentException("Email is required");

        //     var httpContext = _httpContextAccessor.HttpContext;
        //     var username = httpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        //     var applicationIdClaim = httpContext?.User.FindFirst("ApplicationId")?.Value;
        //     var userEmail = httpContext?.User.FindFirst(ClaimTypes.Email)?.Value 
        //                 ?? httpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

        //     // Cari member berdasarkan email user yang login (yang mengundang)
        //     var loggedInMember = await _mstmemberRepository.GetByEmailAsync(userEmail?.ToLower());

        //     // Cek apakah email target yang diundang adalah member
        //     var invitedMember = await _mstmemberRepository.GetByEmailAsync(dto.Email.ToLower());
        //     var invitedUser = await _userRepository.GetByEmailAsync(dto.Email.ToLower());
        //     var isMember = (invitedMember != null && invitedUser != null) ? 1 : 0;

        //     Guid visitorId = Guid.NewGuid();
        //     string recipientName;
        //     string recipientEmail = dto.Email.ToLower();

        //     if (isMember == 1)
        //     {
        //         // Target undangan adalah member → tidak buat Visitor
        //         // visitorId = Guid.NewGuid(); // dummy ID untuk VisitorId (wajib di schema)
        //         recipientName = invitedMember?.Name ?? "Member";
        //     }
        //     else
        //     {
        //         // Target adalah visitor biasa → cek/insert ke Visitor
        //         var existingVisitor = await _visitorRepository.GetByEmailAsync(recipientEmail);
        //         Visitor visitor;

        //         if (existingVisitor == null)
        //         {
        //             visitor = new Visitor
        //             {
        //                 Id = Guid.NewGuid(),
        //                 Email = recipientEmail,
        //                 Name = dto.Name ?? "Guest",
        //                 Status = 1,
        //                 CreatedAt = DateTime.UtcNow,
        //                 UpdatedAt = DateTime.UtcNow,
        //                 CreatedBy = username ?? "Invitation",
        //                 UpdatedBy = username ?? "Invitation"
        //             };

        //             await _visitorRepository.AddAsync(visitor);
        //         }
        //         else
        //         {
        //             visitor = existingVisitor;
        //         }

        //         visitorId = visitor.Id;
        //         recipientName = visitor.Name ?? "Guest";
        //     }

        //     // Hitung jumlah transaksi sebelumnya
        //     var trxCount = await _trxVisitorRepository.CountByVisitorIdAsync(visitorId);

        //     // Generate kode dan trx baru
        //     var confirmationCode = Guid.NewGuid().ToString("N")[..6].ToUpper();

        //     var newTrx = _mapper.Map<TrxVisitor>(dto);
        //     newTrx.VisitorId = visitorId;
        //     newTrx.PurposePerson = dto.PurposePerson ?? loggedInMember?.Id;
        //     newTrx.Status = VisitorStatus.Preregist;
        //     newTrx.IsInvitationAccepted = false;
        //     newTrx.TrxStatus = 1;
        //     newTrx.VisitorGroupCode = trxCount + 1;
        //     newTrx.VisitorNumber = $"VIS{trxCount + 1}";
        //     newTrx.VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}"[..6];
        //     newTrx.InvitationCreatedAt = DateTime.UtcNow;
        //     newTrx.InvitationTokenExpiredAt = DateTime.UtcNow.AddDays(3);
        //     newTrx.CreatedAt = DateTime.UtcNow;
        //     newTrx.UpdatedAt = DateTime.UtcNow;
        //     newTrx.CreatedBy = username ?? "Invitation";
        //     newTrx.UpdatedBy = username ?? "Invitation";
        //     newTrx.IsMember = isMember;

        //     await _trxVisitorRepository.AddAsync(newTrx);

        //     // Format tanggal untuk email
        //     var visitorPeriodStart = newTrx.VisitorPeriodStart?.ToString("yyyy-MM-dd") ?? "Unknown";
        //     var visitorPeriodEnd = newTrx.VisitorPeriodEnd?.ToString("yyyy-MM-dd") ?? "Unknown";

        //     // Buat link
        //     var baseUrl = "http://192.168.1.173:3000/visitor-info";
        //     var invitationUrl = isMember == 1
        //         ? $"{baseUrl}?code={confirmationCode}&applicationId={applicationIdClaim}&trxVisitorId={newTrx.Id}"
        //         : $"{baseUrl}?code={confirmationCode}&applicationId={applicationIdClaim}&visitorId={visitorId}&trxVisitorId={newTrx.Id}";

        //     // Kirim email sesuai tipe
        //     if (isMember == 1)
        //     {
        //         await _emailService.SendMemberInvitationEmailAsync(
        //             recipientEmail,
        //             recipientName,
        //             confirmationCode,
        //             invitationUrl,
        //             visitorPeriodStart,
        //             visitorPeriodEnd
        //         );
        //     }
        //     else
        //     {
        //         await _emailService.SendVisitorInvitationEmailAsync(
        //             recipientEmail,
        //             recipientName,
        //             confirmationCode,
        //             invitationUrl,
        //             visitorPeriodStart,
        //             visitorPeriodEnd
        //         );
        //     }
        // }
        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        
        

        //     public async Task SendBatchInvitationByEmailAsync(List<SendEmailInvitationDto> dtoList)
        // {
        //     if (dtoList == null || !dtoList.Any())
        //         throw new ArgumentException("Invitation list cannot be empty");

        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        //     var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId")?.Value;
        //     var loggedInUserEmail = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

        //     // Login sebagai member? (cocokkan via email)
        //     var loggedInMember = !string.IsNullOrWhiteSpace(loggedInUserEmail)
        //         ? await _mstmemberRepository.GetByEmailAsyncRaw(loggedInUserEmail)
        //         : null;

        //     // group code basis (tetap pakai visitor pertama; kalau semua undangan ke member, nilai ini tetap konsisten)
        //     var firstVisitorEmail = dtoList.First().Email?.ToLower();
        //     if (string.IsNullOrWhiteSpace(firstVisitorEmail))
        //         throw new ArgumentException("At least one valid email is required");

        //     var firstVisitor = await _visitorRepository.GetByEmailAsync(firstVisitorEmail)
        //         ?? new Visitor { Id = Guid.NewGuid(), Email = firstVisitorEmail };

        //     var baseGroupCode = await _trxVisitorRepository.CountByVisitorIdAsync(firstVisitor.Id) + 1;

        //     // VALIDASI awal (hanya untuk undangan ke VISITOR; undangan ke MEMBER dilewati pengecekan overlap visitor)
        //     foreach (var dto in dtoList)
        //     {
        //         if (string.IsNullOrWhiteSpace(dto.Email))
        //             throw new ArgumentException("Email is required");

        //         if (dto.VisitorPeriodStart == null || dto.VisitorPeriodEnd == null)
        //             throw new ArgumentException($"Visitor period must be filled for {dto.Email}");

        //         if (dto.VisitorPeriodStart > dto.VisitorPeriodEnd)
        //             throw new ArgumentException($"Invalid period for {dto.Email}");

        //         var inviteeEmail = dto.Email.ToLower();

        //         // kalau invitee adalah visitor (bukan member), barulah cek overlap trx visitor
        //         var inviteeAsMember = await _mstmemberRepository.GetByEmailAsyncRaw(inviteeEmail);
        //         if (inviteeAsMember == null)
        //         {
        //             var existingVisitor = await _visitorRepository.GetByEmailAsync(inviteeEmail);
        //             if (existingVisitor != null)
        //             {
        //                 bool exists = await _trxVisitorRepository.ExistsOverlappingTrxAsync(
        //                     existingVisitor.Id,
        //                     dto.VisitorPeriodStart.Value,
        //                     dto.VisitorPeriodEnd.Value
        //                 );
        //                 if (exists)
        //                     throw new InvalidOperationException($"Invitation already exists for {dto.Email} in that period");
        //             }
        //         }
        //         else
        //         {
        //             // invitee adalah MEMBER → tidak perlu cek overlap visitor
        //         }
        //     }

        //     // EKSEKUSI
        //     foreach (var dto in dtoList)
        //     {
        //         var email = dto.Email.ToLower();
        //         var confirmationCode = Guid.NewGuid().ToString("N")[..6].ToUpper();

        //         // Cek apakah email yang diundang adalah MEMBER
        //         var invitedMember = await _mstmemberRepository.GetByEmailAsyncRaw(email);

        //         // Buat entity TrxVisitor dari dto
        //         var newTrx = _mapper.Map<TrxVisitor>(dto);
        //         newTrx.Status = VisitorStatus.Preregist;
        //         newTrx.TrxStatus = 1;
        //         newTrx.VisitorGroupCode = baseGroupCode;
        //         newTrx.VisitorNumber = $"VIS{baseGroupCode}";
        //         newTrx.VisitorCode = GenerateRandomString(9);


        //         newTrx.InvitationCreatedAt = DateTime.UtcNow;
        //         newTrx.InvitationTokenExpiredAt = DateTime.UtcNow.AddDays(3);
        //         newTrx.CreatedAt = DateTime.UtcNow;
        //         newTrx.UpdatedAt = DateTime.UtcNow;
        //         newTrx.CreatedBy = username ?? "Invitation";
        //         newTrx.UpdatedBy = username ?? "Invitation";
        //         newTrx.InvitationCode = confirmationCode;

        //         // =========================
        //         // CASE A: MEMBER → undang MEMBER
        //         // =========================
        //         if (loggedInMember != null && invitedMember != null)
        //         {
        //             // TIDAK membuat Visitor
        //             // Set host & target member
        //             newTrx.PurposePerson = loggedInMember.Id;         // host = member yang login
        //             // newTrx.MemberIdentity = invitedMember.Id;               // pastikan field ada; sesuaikan nama jika berbeda
        //             newTrx.MemberIdentity = invitedMember.IdentityId; // jika ada
        //             newTrx.IsMember = 1;
        //             newTrx.VisitorId = null;                          // penting: jangan set visitor

        //             await _trxVisitorRepository.AddAsync(newTrx);

        //                 // Email ke MEMBER (pakai template member)

        //             var startMemberDate = newTrx.VisitorPeriodStart?.ToString("yyyy-MM-dd") ?? "Unknown";
        //             var endMemberDate   = newTrx.VisitorPeriodEnd?.ToString("yyyy-MM-dd") ?? "Unknown";
        //             var startMemberTime = newTrx.VisitorPeriodStart?.ToString("HH:mm:ss") ?? "Unknown";
        //             var endMemberTime   = newTrx.VisitorPeriodEnd?.ToString("HH:mm:ss") ?? "Unknown";
        //             var invitationAgendaMember  = newTrx.Agenda;

        //             var savedTrxMember = await _trxVisitorRepository.GetByIdAsync(newTrx.Id);
        //             var maskedAreaMemberName = savedTrxMember?.MaskedArea?.Name ?? "";
        //             var PurposePersonName     = savedTrxMember?.Member?.Name ?? (loggedInMember?.Name ?? ""); // fallback nama host
        //             var floorNameMember      = await _trxVisitorRepository.GetFloorNameByTrxIdAsync(newTrx.Id) ?? "";
        //             var buildingNameMember   = await _trxVisitorRepository.GetBuildingNameByTrxIdAsync(newTrx.Id) ?? "";

        //             var memberInvitationUrl =
        //                 $"http://192.168.1.173:3000/visitor-info?code={confirmationCode}&applicationId={applicationIdClaim}&trxVisitorId={newTrx.Id}&memberId={invitedMember.Id}&purposePersonId={loggedInMember.Id}";

        //             await _emailService.SendMemberInvitationEmailAsync(
        //                 invitedMember.Email,
        //                 invitedMember.Name ?? "Member",
        //                 confirmationCode,
        //                 memberInvitationUrl,
        //                 invitationAgendaMember,
        //                 startMemberDate,
        //                 endMemberDate,
        //                 startMemberTime,
        //                 endMemberTime,
        //                 maskedAreaMemberName,
        //                 PurposePersonName,
        //                 floorNameMember,
        //                 buildingNameMember
        //             );

        //             continue; // lanjut ke item berikutnya
        //         }

        //         // =========================
        //         // CASE B: MEMBER → undang VISITOR
        //         // CASE C: OPERATOR → undang VISITOR
        //         // =========================

        //         // (1) Pastikan VISITOR ada/terbentuk
        //         var existingVisitor = await _visitorRepository.GetByEmailAsync(email);
        //         Visitor visitor;
        //             if (existingVisitor == null)
        //             {
        //                 visitor = new Visitor
        //                 {
        //                     Id = Guid.NewGuid(),
        //                     Email = email,
        //                     Name = dto.Name ?? "Guest",
        //                     IsVip = dto.IsVip,
        //                     Status = 1,
        //                     VisitorGroupCode = baseGroupCode,
        //                     VisitorNumber = $"VIS{baseGroupCode}",
        //                     VisitorCode = newTrx.VisitorCode,
        //                     CreatedAt = DateTime.UtcNow,
        //                     UpdatedAt = DateTime.UtcNow,
        //                     CreatedBy = username ?? "Invitation",
        //                     UpdatedBy = username ?? "Invitation"
        //                 };
        //                 await _visitorRepository.AddAsync(visitor);
        //             }
        //             else
        //             {
        //                 visitor = existingVisitor;
        //                 existingVisitor.VisitorGroupCode = baseGroupCode;
        //                 existingVisitor.VisitorNumber = $"VIS{baseGroupCode}";
        //                 existingVisitor.VisitorCode = newTrx.VisitorCode;
        //             }

        //         newTrx.VisitorId = visitor.Id; // link ke visitor

        //         // (2) PurposePerson
        //         if (loggedInMember != null)
        //         {
        //             // member login → host = member login
        //             newTrx.PurposePerson = loggedInMember.Id;
        //             newTrx.IsMember = 0;
        //         }
        //         else
        //         {
        //             // operator login → wajib pilih host member
        //             if (!dto.PurposePerson.HasValue)
        //                 throw new ArgumentException($"PurposePerson (member host) is required for {dto.Email}");
        //             newTrx.PurposePerson = dto.PurposePerson.Value;
        //             newTrx.IsMember = 0;
        //         }

        //         await _trxVisitorRepository.AddAsync(newTrx);
        //         await _visitorRepository.UpdateAsyncRaw(visitor);

        //         // (3) Email ke VISITOR (pakai template visitor)
        //             var visitorPeriodStartDate = newTrx.VisitorPeriodStart?.ToString("yyyy-MM-dd") ?? "Unknown";
        //         var visitorPeriodEndDate   = newTrx.VisitorPeriodEnd?.ToString("yyyy-MM-dd") ?? "Unknown";
        //         var visitorPeriodStartTime = newTrx.VisitorPeriodStart?.ToString("HH:mm:ss") ?? "Unknown";
        //         var visitorPeriodEndTime   = newTrx.VisitorPeriodEnd?.ToString("HH:mm:ss") ?? "Unknown";
        //         var invitationAgenda   = newTrx.Agenda;


        //         var savedTrx = await _trxVisitorRepository.GetByIdAsync(newTrx.Id);
        //         var maskedAreaName = savedTrx?.MaskedArea?.Name ?? "";
        //         var memberName     = savedTrx?.Member?.Name ?? (loggedInMember?.Name ?? ""); // fallback nama host
        //         var floorName      = await _trxVisitorRepository.GetFloorNameByTrxIdAsync(newTrx.Id) ?? "";
        //         var buildingName   = await _trxVisitorRepository.GetBuildingNameByTrxIdAsync(newTrx.Id) ?? "";

        //         var invitationUrl =
        //             $"http://192.168.1.173:3000/visitor-info?code={confirmationCode}&applicationId={applicationIdClaim}&visitorId={visitor.Id}&trxVisitorId={newTrx.Id}";

        //         await _emailService.SendVisitorInvitationEmailAsync(
        //             visitor.Email,
        //             visitor.Name ?? "Guest",
        //             confirmationCode,
        //             invitationUrl,
        //             visitorPeriodStartDate,
        //             visitorPeriodEndDate,
        //             visitorPeriodStartTime,
        //             visitorPeriodEndTime,
        //             invitationAgenda,
        //             maskedAreaName,
        //             memberName,
        //             floorName,
        //             buildingName
        //         );
        //     }
        // }




        // public async Task AcceptInvitationAsync(Guid trxVisitorId)
        // {
        //     var trxVisitor = await _trxVisitorRepository.GetByIdAsync(trxVisitorId);
        //     if (trxVisitor == null)
        //         throw new KeyNotFoundException("Invitation not found");

        //     if (trxVisitor.IsInvitationAccepted == true)
        //         throw new InvalidOperationException("Invitation already accepted");

        //     trxVisitor.IsInvitationAccepted = true;
        //     // trxVisitor.InvitationAcceptedAt = DateTime.UtcNow;
        //     trxVisitor.Status = VisitorStatus.Preregist;

        //     await _trxVisitorRepository.UpdateAsync(trxVisitor);
        // }

        public async Task SendBatchInvitationByEmailAsync(List<SendEmailInvitationDto> dtoList)
        {
            if (dtoList == null || !dtoList.Any())
                throw new ArgumentException("Invitation list cannot be empty");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId")?.Value;
            var loggedInUserEmail = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

            //  var integration = _httpContextAccessor.HttpContext?.Items["MstIntegration"] as MstIntegration;
            // if (integration?.ApplicationId != null)
            // {
            //     return (integration.ApplicationId, false);
            // }
            

            // Login as member?
            var loggedInMember = !string.IsNullOrWhiteSpace(loggedInUserEmail)
                ? await _mstmemberRepository.GetByEmailAsyncRaw(loggedInUserEmail)
                : null;

            // Group code basis
            var firstVisitorEmail = dtoList.First().Email?.ToLower();
            if (string.IsNullOrWhiteSpace(firstVisitorEmail))
                throw new ArgumentException("At least one valid email is required");

            var firstVisitor = await _visitorRepository.GetByEmailAsync(firstVisitorEmail)
                ?? new Visitor { Id = Guid.NewGuid(), Email = firstVisitorEmail };

            var baseGroupCode = await _trxVisitorRepository.CountByVisitorIdAsync(firstVisitor.Id) + 1;

            // Validate all invitations upfront
            foreach (var dto in dtoList)
            {
                if (string.IsNullOrWhiteSpace(dto.Email))
                    throw new ArgumentException("Email is required");

                if (dto.VisitorPeriodStart == null || dto.VisitorPeriodEnd == null)
                    throw new ArgumentException($"Visitor period must be filled for {dto.Email}");

                if (dto.VisitorPeriodStart > dto.VisitorPeriodEnd)
                    throw new ArgumentException($"Invalid period for {dto.Email}");

                var inviteeEmail = dto.Email.ToLower();
                var inviteeAsMember = await _mstmemberRepository.GetByEmailAsyncRaw(inviteeEmail);
                if (inviteeAsMember == null)
                {
                    var existingVisitor = await _visitorRepository.GetByEmailAsync(inviteeEmail);
                    if (existingVisitor != null)
                    {
                        bool exists = await _trxVisitorRepository.ExistsOverlappingTrxAsync(
                            existingVisitor.Id,
                            dto.VisitorPeriodStart.Value,
                            dto.VisitorPeriodEnd.Value
                        );
                        if (exists)
                            throw new InvalidOperationException($"Invitation already exists for {dto.Email} in that period");
                    }
                }
            }

            // Process each invitation
            foreach (var dto in dtoList)
            {
                using IDbContextTransaction transaction = await _trxVisitorRepository.BeginTransactionAsync();
                try
                {
                    var email = dto.Email.ToLower();
                    var confirmationCode = Guid.NewGuid().ToString("N")[..6].ToUpper();
                    var newTrx = _mapper.Map<TrxVisitor>(dto);
                    newTrx.Status = VisitorStatus.Preregist;
                    newTrx.TrxStatus = 1;
                    newTrx.VisitorGroupCode = baseGroupCode;
                    newTrx.VisitorNumber = $"VIS{baseGroupCode}";
                    newTrx.VisitorCode = GenerateRandomString(9);
                    newTrx.InvitationCreatedAt = DateTime.UtcNow;
                    newTrx.InvitationTokenExpiredAt = DateTime.UtcNow.AddDays(3);
                    newTrx.CreatedAt = DateTime.UtcNow;
                    newTrx.UpdatedAt = DateTime.UtcNow;
                    newTrx.CreatedBy = username ?? "Invitation";
                    newTrx.UpdatedBy = username ?? "Invitation";
                    newTrx.InvitationCode = confirmationCode;

                    // Case: Member invites Member
                    if (loggedInMember != null && await _mstmemberRepository.GetByEmailAsyncRaw(email) is var invitedMember && invitedMember != null)
                    {
                        newTrx.PurposePerson = loggedInMember.Id;
                        newTrx.MemberIdentity = invitedMember.IdentityId;
                        newTrx.IsMember = 1;
                        newTrx.VisitorId = null;

                        await _trxVisitorRepository.AddAsync(newTrx);
                        await transaction.CommitAsync();

                        // Send email after commit
                        var startMemberDate = newTrx.VisitorPeriodStart?.ToString("yyyy-MM-dd") ?? "Unknown";
                        var endMemberDate = newTrx.VisitorPeriodEnd?.ToString("yyyy-MM-dd") ?? "Unknown";
                        var startMemberTime = newTrx.VisitorPeriodStart?.ToString("HH:mm:ss") ?? "Unknown";
                        var endMemberTime = newTrx.VisitorPeriodEnd?.ToString("HH:mm:ss") ?? "Unknown";
                        var invitationAgendaMember = newTrx.Agenda;

                        var savedTrxMember = await _trxVisitorRepository.GetByIdAsync(newTrx.Id);
                        var maskedAreaMemberName = savedTrxMember?.MaskedArea?.Name ?? "";
                        var purposePersonName = savedTrxMember?.Member?.Name ?? (loggedInMember?.Name ?? "");
                        var purposePersonEmail = savedTrxMember?.Member?.Email ?? (loggedInMember?.Email ?? "");
                        var floorNameMember = await _trxVisitorRepository.GetFloorNameByTrxIdAsync(newTrx.Id) ?? "";
                        var buildingNameMember = await _trxVisitorRepository.GetBuildingNameByTrxIdAsync(newTrx.Id) ?? "";

                        var dateTextMember = startMemberDate + " - " + endMemberDate;
                        var timeTextMember = startMemberTime + " - " + endMemberTime;
                        var locationMember = buildingNameMember + " - " + floorNameMember + " - " + maskedAreaMemberName;

                        var memberInvitationUrl = $"http://192.168.1.173:3000/visitor-info?code={confirmationCode}&applicationId={applicationIdClaim}&trxVisitorId={newTrx.Id}&memberId={invitedMember.Id}&purposePersonId={loggedInMember.Id}";

                        await _emailService.SendMemberInvitationEmailAsync(
                            invitedMember.Email,
                            invitedMember.Name ?? "Member",
                            confirmationCode,
                            memberInvitationUrl,
                            invitationAgendaMember,
                            startMemberDate,
                            endMemberDate,
                            startMemberTime,
                            endMemberTime,
                            maskedAreaMemberName,
                            purposePersonName,
                            floorNameMember,
                            buildingNameMember
                        );

                        await _emailService.SendMemberNotificationEmailAsync(
                            purposePersonEmail,
                            purposePersonName,
                            invitedMember.Name,
                            invitationAgendaMember,
                            dateTextMember,
                            timeTextMember,
                            locationMember,
                            purposePersonName,
                            confirmationCode,
                            memberInvitationUrl
                        );

                        continue;
                    }

                    // Case: Member/Operator invites Visitor
                    var existingVisitor = await _visitorRepository.GetByEmailAsync(email);
                    Visitor visitor;
                    var visitorJustCreated = false;
                    if (existingVisitor == null)
                    {
                        visitor = new Visitor
                        {
                            Id = Guid.NewGuid(),
                            Email = email,
                            Name = dto.Name ?? "Guest",
                            IsVip = dto.IsVip,
                            Status = 1,
                            VisitorGroupCode = baseGroupCode,
                            VisitorNumber = $"VIS{baseGroupCode}",
                            VisitorCode = newTrx.VisitorCode,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedBy = username ?? "Invitation",
                            UpdatedBy = username ?? "Invitation"
                        };
                        await _visitorRepository.AddAsync(visitor);
                        visitorJustCreated = true;
                    }
                    else
                    {
                        visitor = existingVisitor;
                        existingVisitor.VisitorGroupCode = baseGroupCode;
                        existingVisitor.VisitorNumber = $"VIS{baseGroupCode}";
                        existingVisitor.VisitorCode = newTrx.VisitorCode;
                        await _visitorRepository.UpdateAsyncRaw(existingVisitor);
                    }

                    // // === NEW: buat akun User jika belum ada ===
                    // var emailLower = email.ToLower();
                    // var userExists = await _userRepository.EmailExistsAsync(emailLower);
                    // if (!userExists)
                    // {
                    //     // Pastikan kita punya applicationId yang pasti. Karena repo Visitor menyetel ApplicationId,
                    //     // ambil dari visitor setelah tersimpan (aman untuk tenant).
                    //     var applicationId = visitor.ApplicationId;
                    //     if (applicationId == Guid.Empty && !string.IsNullOrWhiteSpace(applicationIdClaim))
                    //     {
                    //         Guid.TryParse(applicationIdClaim, out applicationId);
                    //     }
                    //     if (applicationId == Guid.Empty)
                    //         throw new InvalidOperationException("Cannot resolve ApplicationId for new user.");

                    //     // Ambil/buat group UserCreated
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

                    //     // Buat akun user default (password default + email confirmation)
                    //     var user = new User
                    //     {
                    //         Id = Guid.NewGuid(),
                    //         Username = emailLower,
                    //         Email = emailLower,
                    //         Password = BCrypt.Net.BCrypt.HashPassword("P@ss0wrd"),
                    //         IsCreatedPassword = 0,
                    //         IsEmailConfirmation = 0,
                    //         EmailConfirmationCode = confirmationCode,                 // pakai code yang sama
                    //         EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(7),
                    //         EmailConfirmationAt = DateTime.UtcNow,
                    //         LastLoginAt = DateTime.MinValue,
                    //         StatusActive = StatusActive.NonActive,
                    //         ApplicationId = applicationId,
                    //         GroupId = userGroup.Id
                    //     };
                    //     await _userRepository.AddAsync(user);

                    //     // Kirim email konfirmasi akun
                    //     await _emailService.SendConfirmationEmailAsync(user.Email, visitor.Name ?? user.Username, confirmationCode);
                    // }
                    // // === END NEW ===

                    newTrx.VisitorId = visitor.Id;
                    newTrx.PurposePerson = loggedInMember != null ? loggedInMember.Id : dto.PurposePerson ?? throw new ArgumentException($"PurposePerson (member host) is required for {dto.Email}");
                    newTrx.IsMember = 0;

                    await _trxVisitorRepository.AddAsync(newTrx);
                    await transaction.CommitAsync();

                    // Send email after commit
                    var visitorPeriodStartDate = newTrx.VisitorPeriodStart?.ToString("yyyy-MM-dd") ?? "Unknown";
                    var visitorPeriodEndDate = newTrx.VisitorPeriodEnd?.ToString("yyyy-MM-dd") ?? "Unknown";
                    var visitorPeriodStartTime = newTrx.VisitorPeriodStart?.ToString("HH:mm:ss") ?? "Unknown";
                    var visitorPeriodEndTime = newTrx.VisitorPeriodEnd?.ToString("HH:mm:ss") ?? "Unknown";

                    var dateText = visitorPeriodStartDate + " - " + visitorPeriodEndDate;
                    var timeText = visitorPeriodStartTime + " - " + visitorPeriodEndTime;

                    var invitationAgenda = newTrx.Agenda;
                    // date bisa gabung jadi text datetime jugabisa jadi text, terus building floor dan masked area bisa jadi text juga
                    var savedTrx = await _trxVisitorRepository.GetByIdAsync(newTrx.Id);
                    var maskedAreaName = savedTrx?.MaskedArea?.Name ?? "";
                    var memberName = savedTrx?.Member?.Name ?? (loggedInMember?.Name ?? "");
                    var memberEmail = savedTrx?.Member?.Email ?? (loggedInMember?.Email ?? "");
                    var floorName = await _trxVisitorRepository.GetFloorNameByTrxIdAsync(newTrx.Id) ?? "";
                    var buildingName = await _trxVisitorRepository.GetFloorNameByTrxIdAsync(newTrx.Id) ?? "";

                    var location = buildingName + " - " + floorName + " - " + maskedAreaName;

                    var invitationUrl = $"http://192.168.1.173:3000/visitor-info?code={confirmationCode}&applicationId={applicationIdClaim}&visitorId={visitor.Id}&trxVisitorId={newTrx.Id}";

                    await _emailService.SendVisitorInvitationEmailAsync(
                        visitor.Email,
                        visitor.Name ?? "Guest",
                        confirmationCode,
                        invitationUrl,
                        visitorPeriodStartDate,
                        visitorPeriodEndDate,
                        visitorPeriodStartTime,
                        visitorPeriodEndTime,
                        invitationAgenda,
                        maskedAreaName,
                        memberName,
                        floorName,
                        buildingName
                    );

                    await _emailService.SendMemberNotificationEmailAsync(
                        memberEmail,
                        memberName,
                        visitor.Name,
                        invitationAgenda,
                        dateText,
                        timeText,
                        location,
                        memberName,
                        confirmationCode,
                        invitationUrl
                    );
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw; // Let controller handle the error
                }
            }
        }


        public async Task<VisitorDto> AcceptInvitationFormAsync(MemberInvitationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.InvitationCode))
                throw new ArgumentException("Invitation code is required.");

            var trx = await _trxVisitorRepository
                .GetByInvitationCodeAsync(dto.InvitationCode);

            if (trx == null)
                throw new KeyNotFoundException("Invitation not found or expired.");
            var visitor = trx.Visitor ?? throw new InvalidOperationException("Visitor not found.");
            _mapper.Map(dto, trx);
            trx.IsInvitationAccepted = true;
            trx.UpdatedAt = DateTime.UtcNow;
            trx.UpdatedBy = "MemberForm";

            await _trxVisitorRepository.UpdateAsyncRaw(trx);

            return _mapper.Map<VisitorDto>(visitor);
        }

        //     public async Task<VisitorDto> DeclineInvitationFormAsync(MemberInvitationDto dto)
        // {
        //     if (string.IsNullOrWhiteSpace(dto.InvitationCode))
        //         throw new ArgumentException("Invitation code is required.");

        //     var trx = await _trxVisitorRepository
        //         .GetByInvitationCodeAsync(dto.InvitationCode);

        //     if (trx == null)
        //         throw new KeyNotFoundException("Invitation not found or expired.");
        //     var visitor = trx.Visitor ?? throw new InvalidOperationException("Visitor not found.");
        //     _mapper.Map(dto, trx);
        //     trx.IsInvitationAccepted = false;
        //     trx.UpdatedAt = DateTime.UtcNow;
        //     trx.UpdatedBy = "VisitorForm";

        //     await _trxVisitorRepository.UpdateAsyncRaw(trx);

        //     return _mapper.Map<VisitorDto>(visitor);
        // }

        public async Task DeclineInvitationAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var trxVisitor = await _trxVisitorRepository.GetByPublicIdAsync(id);
            if (trxVisitor == null)
                throw new KeyNotFoundException("Invitation not found");

            // Tidak boleh decline jika sudah accepted
            if (trxVisitor.IsInvitationAccepted == true)
                throw new InvalidOperationException("Invitation has already been accepted and cannot be declined.");

            // Jika sudah pernah decline, hentikan
            if (trxVisitor.IsInvitationAccepted == false && trxVisitor.VisitorActiveStatus == VisitorActiveStatus.Cancelled)
                throw new InvalidOperationException("Invitation already declined.");

            trxVisitor.IsInvitationAccepted = false;
            trxVisitor.VisitorActiveStatus = VisitorActiveStatus.Cancelled; // sesuai enum kamu
            trxVisitor.Status = VisitorStatus.Denied; // atau Cancelled, pilih satu yang konsisten di sistemmu
            // trxVisitor.InvitationDeclinedAt = DateTime.UtcNow; // tambahkan kolom ini jika ada
            trxVisitor.UpdatedAt = DateTime.UtcNow;
            trxVisitor.UpdatedBy = username;

            await _trxVisitorRepository.UpdateAsyncRaw(trxVisitor);
        }
        
        public async Task AcceptInvitationAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var trxVisitor = await _trxVisitorRepository.GetByPublicIdAsync(id);
            if (trxVisitor == null)
                throw new KeyNotFoundException("Invitation not found");

            // // Tidak boleh decline jika sudah decline sebelumnya
            // if (trxVisitor.IsInvitationAccepted == false)
            //     throw new InvalidOperationException("Invitation has already been declined and cannot be accepted.");

            // Tidak boleh decline jika sudah accepted
            if (trxVisitor.IsInvitationAccepted == true)
                throw new InvalidOperationException("Invitation has already been accepted and cannot be declined.");

            // Jika sudah pernah decline, hentikan
            if (trxVisitor.IsInvitationAccepted == false && trxVisitor.VisitorActiveStatus == VisitorActiveStatus.Cancelled)
                throw new InvalidOperationException("Invitation already declined.");

            trxVisitor.IsInvitationAccepted = false;
            trxVisitor.VisitorActiveStatus = VisitorActiveStatus.Cancelled; // sesuai enum kamu
            trxVisitor.Status = VisitorStatus.Denied; // atau Cancelled, pilih satu yang konsisten di sistemmu
            // trxVisitor.InvitationDeclinedAt = DateTime.UtcNow; // tambahkan kolom ini jika ada
            trxVisitor.UpdatedAt = DateTime.UtcNow;
            trxVisitor.UpdatedBy = username;

            await _trxVisitorRepository.UpdateAsyncRaw(trxVisitor);
        }


        // fill invitation form
        // public async Task<VisitorDto> FillInvitationFormAsync(VisitorInvitationDto dto)
        // {
        //     if (string.IsNullOrWhiteSpace(dto.InvitationCode))
        //         throw new ArgumentException("Invitation code is required.");

        //     if (string.IsNullOrWhiteSpace(dto.IdentityId))
        //         throw new ArgumentException("IdentityId is required.");

        //     if (string.IsNullOrWhiteSpace(dto.PersonId))
        //         throw new ArgumentException("PersonId is required.");

        //     if (string.IsNullOrWhiteSpace(dto.IdentityType))
        //         throw new ArgumentException("IdentityType is required.");

        //     var existingVisitor = await _visitorRepository.GetAllQueryable()
        //         .FirstOrDefaultAsync(b => b.Email == dto.Email ||
        //                             b.IdentityId == dto.IdentityId ||
        //                             b.PersonId == dto.PersonId);

        //     var trx = await _trxVisitorRepository
        //         .GetByInvitationCodeAsync(dto.InvitationCode);

        //     if (trx == null)
        //         throw new KeyNotFoundException("Invitation not found or expired.");


        //     if (trx.InvitationTokenExpiredAt < DateTime.UtcNow)
        //     {
        //         trx.VisitorActiveStatus = VisitorActiveStatus.Expired;
        //         throw new InvalidOperationException("Confirmation code expired");
        //     }

        //     if (trx.IsInvitationAccepted == true)
        //         throw new InvalidOperationException("Invitation already accepted.");

        //     var visitor = trx.Visitor ?? throw new InvalidOperationException("Visitor not found.");

        //     // update visitor
        //     _mapper.Map(dto, visitor);
        //     visitor.UpdatedAt = DateTime.UtcNow;
        //     visitor.UpdatedBy = "VisitorForm";

        //     // upload faceimage
        //     if (dto.FaceImage != null && dto.FaceImage.Length > 0)
        //     {
        //         try
        //         {
        //             var allowedImageTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
        //             if (!allowedImageTypes.Contains(dto.FaceImage.ContentType))
        //                 throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

        //             var maxFileSize = 5 * 1024 * 1024; // 5 MB
        //             if (dto.FaceImage.Length > maxFileSize)
        //                 throw new ArgumentException("File size exceeds 5 MB limit.");

        //             var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
        //             Directory.CreateDirectory(uploadDir);

        //             var fileName = $"{Guid.NewGuid()}_{dto.FaceImage.FileName}";
        //             var filePath = Path.Combine(uploadDir, fileName);

        //             using (var stream = new FileStream(filePath, FileMode.Create))
        //             {
        //                 await dto.FaceImage.CopyToAsync(stream);
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

        //     // upload trxvisitor
        //     _mapper.Map(dto, trx);
        //     trx.IsInvitationAccepted = true;
        //     trx.Status = VisitorStatus.Precheckin;
        //     trx.UpdatedAt = DateTime.UtcNow;
        //     trx.UpdatedBy = "VisitorForm";

        //     await _visitorRepository.UpdateAsyncRaw(visitor);
        //     await _trxVisitorRepository.UpdateAsyncRaw(trx);

        //     return _mapper.Map<VisitorDto>(visitor);
        // }

        public async Task<VisitorDto> FillInvitationFormAsync(VisitorInvitationDto dto)
        {

            var req = _httpContextAccessor.HttpContext?.Request;
            if (string.IsNullOrWhiteSpace(dto.InvitationCode))
                throw new ArgumentException("Invitation code is required.");


            var trx = await _trxVisitorRepository.GetByInvitationCodeAsync(dto.InvitationCode);
            if (trx == null)
                throw new KeyNotFoundException("Invitation not found or expired.");
            
            var visitor = trx.Visitor ?? throw new InvalidOperationException("Visitor not found.");
            
            var user = await _userRepository.GetByEmailConfirmPasswordAsyncRaw(visitor.Email.ToLower());
            Console.WriteLine("user", user);

            if (trx.InvitationTokenExpiredAt != null && trx.InvitationTokenExpiredAt < DateTime.UtcNow)
            {
                trx.VisitorActiveStatus = VisitorActiveStatus.Expired;
                await _trxVisitorRepository.UpdateAsyncRaw(trx);
                throw new InvalidOperationException("Confirmation code expired.");
            }

            // sudah accepted?
            if (trx.IsInvitationAccepted == true)
                throw new InvalidOperationException("Invitation already accepted.");

          

            // 2) Validasi unik (email/identity/person) terhadap visitor LAIN (bukan dirinya sendiri)
            if (!string.IsNullOrWhiteSpace(dto.Email) ||
                !string.IsNullOrWhiteSpace(dto.IdentityId) ||
                !string.IsNullOrWhiteSpace(dto.PersonId))
            {
                var duplicate = await _visitorRepository.GetByIdPublicDuplicateAsync(dto.Email, dto.IdentityId, dto.PersonId, visitor.Id);

                if (duplicate != null)
                    throw new InvalidOperationException("A visitor with the same, IdentityId, or PersonId already exists.");
            }

            // 3) Upload file (di luar transaksi DB) — siapkan hasil untuk dipakai setelah commit
            string? faceImagePath = null;
            int? uploadFr = null;
            string? uploadFrError = null;

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

                    faceImagePath = $"/Uploads/visitorFaceImages/{fileName}";
                    uploadFr = 1;
                    uploadFrError = "Upload successful";
                }
                catch (Exception ex)
                {
                    // jangan gagalkan proses accept hanya karena upload gagal — terserah kebijakan kamu.
                    uploadFr = 2;
                    uploadFrError = ex.Message;
                    faceImagePath = null;
                }
            }

                // ===== Buat/siapkan akun USER kalau belum ada (tanpa claims) =====
              

                // Ambil applicationId dari querystring
                string? appIdStr =
                    req?.Query["applicationId"].FirstOrDefault();

                Guid applicationId =
                    (Guid.TryParse(appIdStr, out var parsed) ? parsed :
                    (trx.ApplicationId != Guid.Empty ? trx.ApplicationId :
                    (visitor.ApplicationId != Guid.Empty ? visitor.ApplicationId : Guid.Empty)));
                
                Console.WriteLine("ApplicationId: {0}, {1}", appIdStr, applicationId);

                if (applicationId == Guid.Empty)
                    throw new InvalidOperationException("Cannot resolve ApplicationId (supply ?applicationId=... or set on trx/visitor).");

                var emailLower = visitor.Email?.Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(emailLower))
                    throw new InvalidOperationException("Visitor email is required to create user.");

                var usernameAudit = "VisitorForm";


                var expiredInvitation = trx.VisitorPeriodStart.Value.AddDays(3);
                var userConfirmationCode = trx.InvitationCode;
                if (user == null)
                {
                // Ambil/buat group UserCreated (pakai raw agar tidak kena tenant filter)
                // var userGroup = await _userGroupRepository.GetByApplicationIdAndPriorityAsyncRaw(applicationId, LevelPriority.UserCreated);
                // if (userGroup == null)
                // {
                //     userGroup = new UserGroup
                //     {
                //         Id = Guid.NewGuid(),
                //         Name = "VisitorGroup",
                //         LevelPriority = LevelPriority.UserCreated,
                //         ApplicationId = applicationId,
                //         Status = 1,
                //         CreatedBy = usernameAudit,
                //         CreatedAt = DateTime.UtcNow,
                //         UpdatedBy = usernameAudit,
                //         UpdatedAt = DateTime.UtcNow
                //     };
                //     await _userGroupRepository.AddAsyncRaw(userGroup);
                // }

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = emailLower,
                    Email = emailLower,
                    Password = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd"),
                    IsCreatedPassword = 1,
                    IsEmailConfirmation = 1,
                    EmailConfirmationCode = userConfirmationCode,
                    EmailConfirmationExpiredAt = expiredInvitation,
                    EmailConfirmationAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.MinValue,
                    StatusActive = StatusActive.NonActive,
                    ApplicationId = applicationId,
                    GroupId = new Guid("EA1A8B73-DCDB-4CC2-9F00-FB7A52CF7634"),
                };

                // simpan user (boleh pakai AddAsyncRaw jika AddAsync terkena tenant guard)
                await _userRepository.AddRawAsync(newUser);

            }
            else
            {
                // Perpanjang masa kadaluarsa konfirmasi kalau mau
                user.EmailConfirmationExpiredAt = expiredInvitation;
                user.EmailConfirmationCode = userConfirmationCode;
                await _userRepository.UpdateRawAsync(user);
            }
            

            // 4) Update VISITOR — hanya set field yang ada nilainya agar tidak menimpa jadi null

            // if (faceImagePath != null) visitor.FaceImage = faceImagePath;
            // if (uploadFr.HasValue) visitor.UploadFr = uploadFr.Value;
            // if (uploadFrError != null) visitor.UploadFrError = uploadFrError;
            using var transaction = await _trxVisitorRepository.BeginTransactionAsync();
            try
            { 
                _mapper.Map(dto, visitor);
                //         visitor.FaceImage = faceImagePath;
                //         visitor.UploadFr = uploadFr;
                //         visitor.UploadFrError = uploadFrError;
                //         visitor.UpdatedAt = DateTime.UtcNow;
                //         visitor.UpdatedBy = "VisitorForm";

                visitor.UpdatedAt = DateTime.UtcNow;
                visitor.UpdatedBy = "VisitorForm";

                // 5) Update TRX — hanya yang perlu
                // (kalau ada field lain di dto untuk trx — isi dengan guard null juga)
                // Update TrxVisitor
                _mapper.Map(dto, trx);
                trx.IsInvitationAccepted = true;
                trx.Status = VisitorStatus.Precheckin;
                trx.UpdatedAt = DateTime.UtcNow;
                trx.UpdatedBy = "VisitorForm";

                await _visitorRepository.UpdateAsyncRaw(visitor);
                await _trxVisitorRepository.UpdateAsyncRaw(trx);
                

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();

                // bersihkan file kalau perlu
                if (faceImagePath != null)
                {
                    var phys = Path.Combine(Directory.GetCurrentDirectory(), faceImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(phys)) File.Delete(phys);
                }
                throw;
            }

            // 6) Siapkan data untuk EMAIL setelah commit
            // Ambil lagi trx untuk memastikan navigasi (MaskedArea/Member) jika butuh
            // var savedTrx = await _trxVisitorRepository.GetByIdAsync(trx.Id);

            // var maskedAreaName = savedTrx?.MaskedArea?.Name ?? "";
            // var floorName = await _trxVisitorRepository.GetFloorNameByTrxIdAsync(trx.Id) ?? "";
            // var buildingName = await _trxVisitorRepository.GetBuildingNameByTrxIdAsync(trx.Id) ?? "";

            // // Tentukan HOST (purpose person) — pakai PurposePerson kalau ada; kalau tidak, pakai Member
            // Guid? hostMemberId = savedTrx?.PurposePerson;
            // string hostName = "";
            // string hostEmail = "";
            // if (hostMemberId.HasValue)
            // {
            //     var host = await _mstmemberRepository.GetByIdRawAsync(hostMemberId.Value);
            //     hostName = host?.Name ?? "";
            //     hostEmail = host?.Email ?? "";
            // }
            // else
            // {
            //     // fallback terakhir
            //     hostName = savedTrx?.Member?.Name ?? "";
            //     hostEmail = savedTrx?.Member?.Email ?? "";
            // }

            // // Format tanggal & jam
            // var start = savedTrx?.VisitorPeriodStart;
            // var end = savedTrx?.VisitorPeriodEnd;

            // string dateText = (start?.ToString("yyyy-MM-dd") ?? "-") + " - " + (end?.ToString("yyyy-MM-dd") ?? "-");
            // string timeText = (start?.ToString("HH:mm") ?? "-") + " - " + (end?.ToString("HH:mm") ?? "-");

            // string location = $"{buildingName} - {floorName} - {maskedAreaName}";
            // string agenda = savedTrx?.Agenda ?? "";

            // URL undangan (kalau kamu butuh kirim lagi linknya)
            // var applicationIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("ApplicationId")?.Value;
            // // karena AllowAnonymous, mungkin null — pakai applicationId dari trx
            // var appId = applicationIdClaim ?? (savedTrx?.ApplicationId.ToString() ?? "");
            // var invitationUrl = $"http://192.168.1.173:3000/visitor-form?code={savedTrx?.InvitationCode}&applicationId={appId}&visitorId={visitor.Id}&trxVisitorId={savedTrx?.Id}";

            // 7) Kirim EMAIL (2 pihak)

            // 7a) Ke HOST (purpose person) → notifikasi visitor sudah accept
            // if (!string.IsNullOrWhiteSpace(hostEmail))
            // {
            //     await _emailService.SendMemberVisitorAcceptNotificationEmailAsync(
            //         toEmail: hostEmail,
            //         hostName: hostName,
            //         visitorName: visitor.Name,
            //         invitationAgenda: agenda,
            //         dateText: dateText,
            //         timeText: timeText,
            //         location: location,
            //         memberName: hostName,                      
            //         confirmationCode: savedTrx?.InvitationCode // opsional ditampilkan
            //     );
            // }

            // // 7b) Ke VISITOR → konfirmasi berhasil menerima undangan
            // await _emailService.SendVisitorAcceptnotificationEmailAsync(
            //     toEmail: visitor.Email,
            //     hostName: hostName,
            //     visitorName: visitor.Name,
            //     invitationAgenda: agenda,
            //     dateText: dateText,
            //     timeText: timeText,
            //     location: location,
            //     memberName: hostName,                        
            //     confirmationCode: savedTrx?.InvitationCode
            // );

            

            return _mapper.Map<VisitorDto>(visitor);
        }


        // public async Task<VisitorDto> FillInvitationFormAsync(VisitorInvitationDto dto)
        // {
        //     // Input validation
        //     if (string.IsNullOrWhiteSpace(dto.InvitationCode))
        //         throw new ArgumentException("Invitation code is required.");
        //     if (string.IsNullOrWhiteSpace(dto.IdentityId))
        //         throw new ArgumentException("IdentityId is required.");
        //     if (string.IsNullOrWhiteSpace(dto.PersonId))
        //         throw new ArgumentException("PersonId is required.");
        //     if (string.IsNullOrWhiteSpace(dto.IdentityType))
        //         throw new ArgumentException("IdentityType is required.");

        //     var existingVisitor = await _visitorRepository.GetAllQueryable()
        //         .FirstOrDefaultAsync(b => b.Email == dto.Email ||
        //                             b.IdentityId == dto.IdentityId ||
        //                             b.PersonId == dto.PersonId);

        //     if (existingVisitor != null)
        //         throw new InvalidOperationException("A visitor with the same Email, IdentityId, or PersonId already exists.");

        //     var trx = await _trxVisitorRepository
        //         .GetByInvitationCodeAsync(dto.InvitationCode);

        //     if (trx == null)
        //         throw new KeyNotFoundException("Invitation not found or expired.");

        //     if (trx.InvitationTokenExpiredAt < DateTime.UtcNow)
        //     {
        //         trx.VisitorActiveStatus = VisitorActiveStatus.Expired;
        //         await _trxVisitorRepository.UpdateAsyncRaw(trx);
        //         throw new InvalidOperationException("Confirmation code expired");
        //     }

        //     if (trx.IsInvitationAccepted == true)
        //         throw new InvalidOperationException("Invitation already accepted.");

        //     var visitor = trx.Visitor ?? throw new InvalidOperationException("Visitor not found.");

        //     // Handle file upload outside transaction
        //     string faceImagePath = null;
        //     int uploadFr = 0;
        //     string uploadFrError = null;
        //     if (dto.FaceImage != null && dto.FaceImage.Length > 0)
        //     {
        //         try
        //         {
        //             var allowedImageTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
        //             if (!allowedImageTypes.Contains(dto.FaceImage.ContentType))
        //                 throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

        //             var maxFileSize = 5 * 1024 * 1024; // 5 MB
        //             if (dto.FaceImage.Length > maxFileSize)
        //                 throw new ArgumentException("File size exceeds 5 MB limit.");

        //             var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
        //             Directory.CreateDirectory(uploadDir);

        //             var fileName = $"{Guid.NewGuid()}_{dto.FaceImage.FileName}";
        //             var filePath = Path.Combine(uploadDir, fileName);

        //             using (var stream = new FileStream(filePath, FileMode.Create))
        //             {
        //                 await dto.FaceImage.CopyToAsync(stream);
        //             }

        //             faceImagePath = $"/Uploads/visitorFaceImages/{fileName}";
        //             uploadFr = 1;
        //             uploadFrError = "Upload successful";
        //         }
        //         catch (Exception ex)
        //         {
        //             uploadFr = 2;
        //             uploadFrError = ex.Message;
        //         }
        //     }

        //     using IDbContextTransaction transaction = await _trxVisitorRepository.BeginTransactionAsync();
        //     try
        //     {
        //         // Update visitor
        //         _mapper.Map(dto, visitor);
        //         visitor.FaceImage = faceImagePath;
        //         visitor.UploadFr = uploadFr;
        //         visitor.UploadFrError = uploadFrError;
        //         visitor.UpdatedAt = DateTime.UtcNow;
        //         visitor.UpdatedBy = "VisitorForm";

        //         // Update TrxVisitor
        //         _mapper.Map(dto, trx);
        //         trx.IsInvitationAccepted = true;
        //         trx.Status = VisitorStatus.Precheckin;
        //         trx.UpdatedAt = DateTime.UtcNow;
        //         trx.UpdatedBy = "VisitorForm";

        //         await _visitorRepository.UpdateAsyncRaw(visitor);
        //         await _trxVisitorRepository.UpdateAsyncRaw(trx);

        //         return _mapper.Map<VisitorDto>(visitor);
        //     }
        //     catch
        //     {
        //         await transaction.RollbackAsync();
        //         // Clean up uploaded file if it exists
        //         if (faceImagePath != null && File.Exists(Path.Combine(Directory.GetCurrentDirectory(), faceImagePath.TrimStart('/'))))
        //         {
        //             File.Delete(Path.Combine(Directory.GetCurrentDirectory(), faceImagePath.TrimStart('/')));
        //         }
        //         throw;
        //     }
        // }

        public async Task<VisitorDto> GetVisitorByIdAsync(Guid id)
        {
            var visitor = await _visitorRepository.GetByIdAsync(id);
            return _mapper.Map<VisitorDto>(visitor);
        }

            public async Task<VisitorDto> GetVisitorByIdPublicAsync(Guid id)
        {
            var visitor = await _visitorRepository.GetByIdPublicAsync(id);
            return _mapper.Map<VisitorDto>(visitor);
        }

        public async Task<IEnumerable<VisitorDto>> GetAllVisitorsAsync()
        {
            var visitors = await _visitorRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<VisitorDto>>(visitors);
        }

                public async Task<IEnumerable<OpenVisitorDto>> OpenGetAllVisitorsAsync()
        {
            var visitors = await _visitorRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<OpenVisitorDto>>(visitors);
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

            var enumColumns = new Dictionary<string, Type>
            {
                { "Status", typeof(VisitorStatus) },
                { "Gender", typeof(Gender) },
                { "VisitorActiveStatus", typeof(VisitorActiveStatus) },
                { "IdentityType", typeof(IdentityType) }
            };

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "Name", "OrganizationName", "DistrictName", "DepartmentName", "Gender", "CardNumber", "Status", "PersonId", "CreatedAt", "UpdatedAt", "UpdatedBy", "CreatedBy", "IdentityType" };

            var filterService = new GenericDataTableService<Visitor, VisitorDto>(
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




        //         // create visitor, trx visitor dan user
        // public async Task<VisitorDto> CreateVisitorAsync(VisitorCreateDto createDto)
        // {
        //     if (createDto == null)
        //         throw new ArgumentNullException(nameof(createDto));

        //     if (string.IsNullOrWhiteSpace(createDto.Email))
        //         throw new ArgumentException("Email is required", nameof(createDto.Email));

        //     if (string.IsNullOrWhiteSpace(createDto.Name))
        //         throw new ArgumentException("Name is required", nameof(createDto.Name));

        //     var existingVisitor = await _visitorRepository.GetAllQueryable()
        //         .FirstOrDefaultAsync(b => b.Email == createDto.Email ||
        //                             b.IdentityId == createDto.IdentityId ||
        //                             b.PersonId == createDto.PersonId);

        //     if (existingVisitor != null)
        //     {
        //         if (existingVisitor.Email == createDto.Email)
        //         {
        //             throw new ArgumentException($"Visitor with Email {createDto.Email} already exists.");
        //         }
        //         else if (existingVisitor.IdentityId == createDto.IdentityId)
        //         {
        //             throw new ArgumentException($"Visitor with Email {createDto.IdentityId} already exists.");
        //         }
        //         else if (existingVisitor.PersonId == createDto.PersonId)
        //         {
        //             throw new ArgumentException($"Visitor with PersonId {createDto.PersonId} already exists.");
        //         }
        //     }

        //     var username = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
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
        //         Password = BCrypt.Net.BCrypt.HashPassword("P@ss0wrd"),
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

        //     var newTrx = _mapper.Map<TrxVisitor>(createDto);
        //     newTrx.VisitorId = visitor.Id;
        //     newTrx.Status = VisitorStatus.Preregist;
        //     newTrx.InvitationCode = confirmationCode;
        //     // newTrx.IsInvitationAccepted = false;
        //     newTrx.VisitorGroupCode = visitor.TrxVisitors.Count + 1;
        //     newTrx.VisitorNumber = $"VIS{visitor.TrxVisitors.Count + 1}";
        //     newTrx.VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6);
        //     newTrx.InvitationCreatedAt = DateTime.UtcNow;
        //     newTrx.TrxStatus = 1;

        //     // var newTrx = new TrxVisitor
        //     //     {
        //     //         VisitorId = visitor.Id,
        //     //         CheckedInAt = DateTime.UtcNow,
        //     //         Status = VisitorStatus.Preregist,
        //     //         InvitationCode = confirmationCode,
        //     //         IsInvitationAccepted = false,
        //     //         VisitorGroupCode = visitor.TrxVisitors.Count + 1,
        //     //         VisitorNumber = $"VIS{visitor.TrxVisitors.Count + 1}",
        //     //         VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6),
        //     //         InvitationCreatedAt = DateTime.UtcNow
        //     //     };

        //     await _userRepository.AddAsync(newUser);
        //     await _visitorRepository.AddAsync(visitor);
        //     await _trxVisitorRepository.AddAsync(newTrx);

        //     // Send verification email
        //     await _emailService.SendConfirmationEmailAsync(visitor.Email, visitor.Name, confirmationCode);
        //     // await _emailService.SendConfirmationEmailAsync(newUser.Email, newUser.Username, confirmationCode);

        //     var result = _mapper.Map<VisitorDto>(visitor);
        //     if (result == null)
        //         throw new InvalidOperationException("Failed to map Visitor to VisitorDto");

        //     return result;
        // }
        
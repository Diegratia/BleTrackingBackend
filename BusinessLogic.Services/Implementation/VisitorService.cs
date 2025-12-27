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
using Microsoft.Extensions.Configuration;
using Helpers.Consumer.Mqtt;

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
    // private readonly CardRecordRepository _cardRecordRepository;
    private readonly CardAccessRepository _cardAccessRepository;
    private readonly UserGroupRepository _userGroupRepository;
    private readonly IEmailService _emailService;
    private readonly string[] _allowedImageTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
    private readonly string _invitationBaseUrl;
        private readonly IMqttClientService _mqttClient;

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
            CardAccessRepository cardAccessRepository,
            IEmailService emailService,
            IConfiguration configuration,
            IMqttClientService mqttClient)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _visitorRepository = visitorRepository ?? throw new ArgumentNullException(nameof(visitorRepository));
            _trxVisitorService = trxVisitorService ?? throw new ArgumentNullException();
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            // _cardRecordRepository = cardRecordRepository ?? throw new ArgumentNullException(nameof(cardRecordRepository));
            _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
            _userGroupRepository = userGroupRepository ?? throw new ArgumentNullException(nameof(userGroupRepository));
            _trxVisitorRepository = trxVisitorRepository ?? throw new ArgumentNullException(nameof(trxVisitorRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _cardRecordService = cardRecordService ?? throw new ArgumentNullException();
            _cardAccessRepository = cardAccessRepository ?? throw new ArgumentNullException(nameof(cardAccessRepository));
            _mstmemberRepository = mstmemberRepository ?? throw new ArgumentNullException(nameof(mstmemberRepository));
            _invitationBaseUrl = configuration["InvitationBaseUrl"] ?? "null";
            _mqttClient = mqttClient;
        }
    

        //latest vms create
        public async Task<VisitorDto> CreateVisitorAsync(VMSOpenVisitorCreateDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (string.IsNullOrWhiteSpace(createDto.Email))
                throw new ArgumentException("Email is required", nameof(createDto.Email));

            if (string.IsNullOrWhiteSpace(createDto.Name))
                throw new ArgumentException("Name is required", nameof(createDto.Name));

            if (string.IsNullOrWhiteSpace(createDto.CardNumber))
                throw new ArgumentException("Card Number is required for check-in", nameof(createDto.CardNumber));

            // cari visitor existing
            var existingVisitor = await _visitorRepository.GetAllQueryable()
                .FirstOrDefaultAsync(b => b.Email == createDto.Email ||
                                        b.IdentityId == createDto.IdentityId ||
                                        b.PersonId == createDto.PersonId);

            // ambil card berdasarkan nomor kartu
            var cardByCardNumber = await _cardRepository.GetByCardNumberAsync(createDto.CardNumber);
            if (cardByCardNumber == null)
                throw new KeyNotFoundException($"Card with CardNumber {createDto.CardNumber} not found.");

            // dapatkan ApplicationId dan username
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

            // find or create user group
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

            // kalau visitor sudah ada → skip pembuatan user & visitor baru
            Visitor visitor;
            bool isNewVisitor = false;

            if (existingVisitor != null)
            {
                visitor = existingVisitor;
            }
            else
            {
                visitor = _mapper.Map<Visitor>(createDto);
                visitor.Id = Guid.NewGuid();
                visitor.ApplicationId = applicationId.Value;
                visitor.Status = 1;
                visitor.CreatedBy = username;
                visitor.CreatedAt = DateTime.UtcNow;
                visitor.UpdatedBy = username;
                visitor.UpdatedAt = DateTime.UtcNow;
                isNewVisitor = true;

                // handle upload face image
                if (createDto.FaceImage != null && createDto.FaceImage.Length > 0)
                {
                    try
                    {
                        if (!_allowedImageTypes.Contains(createDto.FaceImage.ContentType))
                            throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

                        if (createDto.FaceImage.Length > MaxFileSize)
                            throw new ArgumentException("File size exceeds 5 MB limit.");

                        var basePath = AppContext.BaseDirectory;
                        var uploadDir = Path.Combine(basePath, "Uploads", "visitorFaceImages");
                        Directory.CreateDirectory(uploadDir);

                        // var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
                        // Directory.CreateDirectory(uploadDir);

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
            }

            // check active trx dulu biar tidak double checkin
            var hasActiveTrx = await _trxVisitorRepository.GetAllQueryableRaw()
                .AnyAsync(t => t.VisitorId == visitor.Id && t.Status == VisitorStatus.Checkin && t.CheckedOutAt == null);
            if (hasActiveTrx)
                throw new InvalidOperationException("Visitor already has an active transaction");

            // generate trx visitor baru
            var confirmationCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
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

            // assign card access
            if (createDto.MaskedAreaId.HasValue)
            {
                var cardAccess = await _cardAccessRepository.GetAllQueryable()
                    .FirstOrDefaultAsync(ca => ca.CardAccessMaskedAreas.Any(cam => cam.MaskedAreaId == createDto.MaskedAreaId.Value));

                if (cardAccess == null)
                    throw new KeyNotFoundException($"CardAccess for MaskedAreaId {createDto.MaskedAreaId} not found");

                var card = await _cardRepository.GetAllQueryable()
                    .FirstOrDefaultAsync(ca => ca.CardCardAccesses.Any(cam => cam.Card.CardNumber == createDto.CardNumber));

                if (card == null)
                    throw new KeyNotFoundException($"Card with CardNumber {createDto.CardNumber} not found");

                // sync card access
                var accessesToRemove = card.CardCardAccesses.Where(cca => cca.CardAccessId != cardAccess.Id).ToList();
                foreach (var access in accessesToRemove)
                    card.CardCardAccesses.Remove(access);

                if (!card.CardCardAccesses.Any(cca => cca.CardAccessId == cardAccess.Id))
                {
                    card.CardCardAccesses.Add(new CardCardAccess
                    {
                        CardId = card.Id,
                        CardAccessId = cardAccess.Id,
                        ApplicationId = card.ApplicationId
                    });
                }
            }

            var cardRecordDto = new CardRecordCreateDto
            {
                CardId = cardByCardNumber.Id,
                VisitorId = visitor.Id,
            };

            using var transaction = await _visitorRepository.BeginTransactionAsync();
            try
            {
                if (isNewVisitor)
                {
                    // buat user baru hanya untuk visitor baru
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

                    await _userRepository.AddAsync(newUser);
                    await _visitorRepository.AddAsync(visitor);
                }

                await _trxVisitorRepository.AddAsync(newTrx);
                await _cardRecordService.CreateAsync(cardRecordDto);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            
            // kirim email konfirmasi hanya kalau visitor baru
            if (isNewVisitor)
                await _emailService.SendConfirmationEmailAsync(visitor.Email, visitor.Name, confirmationCode);

            var result = _mapper.Map<VisitorDto>(visitor);
            if (result == null)
                throw new InvalidOperationException("Failed to map Visitor to VisitorDto");
            await _mqttClient.PublishAsync("engine/refresh/card-related", "");
            await _mqttClient.PublishAsync("engine/refresh/visitor-related", "");
            return result;
        }
        
        //VMS Latest + CardAccessIds
        public async Task<VisitorDto> CreateVisitorVMSAsync(VMSOpenVisitorCreateDto createDto)
            {
                if (createDto == null)
                    throw new ArgumentNullException(nameof(createDto));

                if (string.IsNullOrWhiteSpace(createDto.Email))
                    throw new ArgumentException("Email is required", nameof(createDto.Email));

                if (string.IsNullOrWhiteSpace(createDto.Name))
                    throw new ArgumentException("Name is required", nameof(createDto.Name));

                if (string.IsNullOrWhiteSpace(createDto.CardNumber))
                    throw new ArgumentException("Card Number is required for check-in", nameof(createDto.CardNumber));

                //cari visitor yang sudah ada atau buat baru
                var existingVisitor = await _visitorRepository.GetAllQueryable()
                    .FirstOrDefaultAsync(b => b.Email == createDto.Email ||
                                            b.IdentityId == createDto.IdentityId ||
                                            b.PersonId == createDto.PersonId);

                var cardByCardNumber = await _cardRepository.GetByCardNumberAsync(createDto.CardNumber);
                if (cardByCardNumber == null)
                    throw new KeyNotFoundException($"Card with CardNumber {createDto.CardNumber} not found.");

                //ambil user context dan application id
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

                //cek apakah user group sudah ada
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

                // buat visitor baru kalai belum ada
                Visitor visitor;
                bool isNewVisitor = false;

                if (existingVisitor != null)
                {
                    visitor = existingVisitor;
                }
                else
                {
                    visitor = _mapper.Map<Visitor>(createDto);
                    visitor.Id = Guid.NewGuid();
                    visitor.ApplicationId = applicationId.Value;
                    visitor.Status = 1;
                    visitor.CreatedBy = username;
                    visitor.CreatedAt = DateTime.UtcNow;
                    visitor.UpdatedBy = username;
                    visitor.UpdatedAt = DateTime.UtcNow;
                    isNewVisitor = true;

                    if (createDto.FaceImage != null && createDto.FaceImage.Length > 0)
                    {
                        try
                        {
                            if (!_allowedImageTypes.Contains(createDto.FaceImage.ContentType))
                                throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

                            if (createDto.FaceImage.Length > MaxFileSize)
                                throw new ArgumentException("File size exceeds 5 MB limit.");
                                
                            var basePath = AppContext.BaseDirectory;
                            var uploadDir = Path.Combine(basePath, "Uploads", "visitorFaceImages");
                            Directory.CreateDirectory(uploadDir);

                        // var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
                        // Directory.CreateDirectory(uploadDir);

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
                }

                //cek transaksi aktif
                var hasActiveTrx = await _trxVisitorRepository.GetAllQueryableRaw()
                    .AnyAsync(t => t.VisitorId == visitor.Id && 
                                t.Status == VisitorStatus.Checkin && 
                                t.CheckedOutAt == null);

                if (hasActiveTrx)
                    throw new InvalidOperationException("Visitor already has an active transaction");

                // buat trx baru
                var confirmationCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
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

                //asign cardaccess ke card
                if ((createDto.CardAccessIds != null && createDto.CardAccessIds.Any()) || createDto.MaskedAreaId.HasValue)
                {
                    var card = await _cardRepository.GetAllQueryable()
                        .Include(c => c.CardCardAccesses)
                        .FirstOrDefaultAsync(c => c.CardNumber == createDto.CardNumber);

                    if (card == null)
                        throw new KeyNotFoundException($"Card with CardNumber {createDto.CardNumber} not found");

                    var newCardAccessIds = new List<Guid>();

                    // kalau kirim card access id
                    if (createDto.CardAccessIds != null && createDto.CardAccessIds.Any())
                        newCardAccessIds.AddRange(createDto.CardAccessIds);

                    // kalau kirim maskedarea resolve ke access id
                    if (createDto.MaskedAreaId.HasValue)
                    {
                        var cardAccessFromArea = await _cardAccessRepository.GetAllQueryable()
                            .Where(ca => ca.CardAccessMaskedAreas.Any(cam => cam.MaskedAreaId == createDto.MaskedAreaId.Value))
                            .Select(ca => ca.Id)
                            .ToListAsync();

                        newCardAccessIds.AddRange(cardAccessFromArea);
                    }

                    newCardAccessIds = newCardAccessIds.Distinct().ToList();

                    // remove relasi lama
                    var toRemove = card.CardCardAccesses
                        .Where(ca => !newCardAccessIds.Contains(ca.CardAccessId))
                        .ToList();
                    foreach (var old in toRemove)
                        card.CardCardAccesses.Remove(old);

                    // add relasi baru
                    foreach (var accessId in newCardAccessIds)
                    {
                        if (!card.CardCardAccesses.Any(cca => cca.CardAccessId == accessId))
                        {
                            card.CardCardAccesses.Add(new CardCardAccess
                            {
                                CardId = card.Id,
                                CardAccessId = accessId,
                                ApplicationId = card.ApplicationId,
                                Status = 1
                            });
                        }
                    }

                    // Set TrxVisitor.MaskedAreaId dari CardAccess
                    if (newCardAccessIds.Any())
                    {
                        var firstAreaId = await _cardAccessRepository.GetAllQueryable()
                            .Where(ca => newCardAccessIds.Contains(ca.Id))
                            .SelectMany(ca => ca.CardAccessMaskedAreas.Select(cam => cam.MaskedAreaId))
                            .FirstOrDefaultAsync();

                        if (firstAreaId != Guid.Empty)
                            newTrx.MaskedAreaId = firstAreaId;
                    }
                }

                // create card record
                var cardRecordDto = new CardRecordCreateDto
                {
                    CardId = cardByCardNumber.Id,
                    VisitorId = visitor.Id,
                };

                // trans
                using var transaction = await _visitorRepository.BeginTransactionAsync();
                try
                {
                    if (isNewVisitor)
                    {
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

                        await _userRepository.AddAsync(newUser);
                        await _visitorRepository.AddAsync(visitor);
                    }

                    await _trxVisitorRepository.AddAsync(newTrx);
                    await _cardRecordService.CreateAsync(cardRecordDto);

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }

                if (isNewVisitor)
                    await _emailService.SendConfirmationEmailAsync(visitor.Email, visitor.Name, confirmationCode);

                var result = _mapper.Map<VisitorDto>(visitor);
                if (result == null)
                    throw new InvalidOperationException("Failed to map Visitor to VisitorDto");
                    
                await _mqttClient.PublishAsync("engine/refresh/card-related", "");
                await _mqttClient.PublishAsync("engine/refresh/visitor-related", "");
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

                    // var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
                    // Directory.CreateDirectory(uploadDir);
                        var basePath = AppContext.BaseDirectory;
                        var uploadDir = Path.Combine(basePath, "Uploads", "visitorFaceImages");
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
            // if (!string.IsNullOrWhiteSpace(updateDto.Email) &&
            //     await _userRepository.EmailExistsAsync(updateDto.Email.ToLower()))
            // {
            //     throw new InvalidOperationException("Email is already registered.");
            // }

            visitor.UpdatedBy = username ?? "System";
            visitor.UpdatedAt = DateTime.UtcNow;

            // Mapping ke Visitor
            _mapper.Map(updateDto, visitor);
            await _visitorRepository.UpdateAsync(visitor);
            await _mqttClient.PublishAsync("engine/refresh/card-related", "");
            await _mqttClient.PublishAsync("engine/refresh/visitor-related", "");
            return _mapper.Map<VisitorDto>(visitor);
        }

            public async Task<VisitorDto> BlacklistVisitorAsync(Guid id, BlacklistReasonDto dto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var visitor = await _visitorRepository.GetByIdAsync(id);
            if (visitor == null)
                throw new KeyNotFoundException($"Visitor with ID {id} not found.");

            visitor.UpdatedBy = username ?? "System";
            visitor.UpdatedAt = DateTime.UtcNow;
            visitor.IsBlacklist = true;

            _mapper.Map(dto, visitor);
            await _visitorRepository.UpdateAsync(visitor);
            await _mqttClient.PublishAsync("engine/refresh/blacklist-related", "");
            return _mapper.Map<VisitorDto>(visitor);
        }

        public async Task UnBlacklistVisitorAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var visitor = await _visitorRepository.GetByIdAsync(id);
            if (visitor == null)
                throw new KeyNotFoundException($"Visitor with ID {id} not found.");

            visitor.UpdatedBy = username ?? "System";
            visitor.UpdatedAt = DateTime.UtcNow;
            visitor.IsBlacklist = false;

            await _visitorRepository.UpdateAsync(visitor);
            await _mqttClient.PublishAsync("engine/refresh/blacklist-related", "");
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


            // var invitationUrl = $"http://localhost:5000/api/Visitor/fill-invitation-form?code={confirmationCode}&applicationId={applicationIdClaim}&visitorId={visitor.Id}&trxVisitorId={newTrx.Id}";
            var invitationUrl = $"{_invitationBaseUrl }/visitor-info?code={confirmationCode}&applicationId={applicationIdClaim}&visitorId={visitor.Id}&trxVisitorId={newTrx.Id}";
            // var memberInvitationUrl = $"http://localhost:3000/visitor-info?code={confirmationCode}&applicationId={applicationIdClaim}&trxVisitorId={newTrx.Id}";

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

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        
    
        public async Task SendBatchInvitationByEmailAsync(List<SendEmailInvitationDto> dtoList)
        {
            if (dtoList == null || !dtoList.Any())
                throw new ArgumentException("Invitation list cannot be empty");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId")?.Value;
            var loggedInUserEmail = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
            

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

            return result;
        }

        
        // public async Task<VisitorDto> CreateVisitorAsync(OpenVisitorCreateDto createDto)
        // {
        //     if (createDto == null)
        //         throw new ArgumentNullException(nameof(createDto));

        //     if (string.IsNullOrWhiteSpace(createDto.Email))
        //         throw new ArgumentException("Email is required", nameof(createDto.Email));

        //     if (string.IsNullOrWhiteSpace(createDto.Name))
        //         throw new ArgumentException("Name is required", nameof(createDto.Name));

        //     if (string.IsNullOrWhiteSpace(createDto.CardNumber))
        //         throw new ArgumentException("Card Number is required for check-in", nameof(createDto.CardNumber));

        //     // cari visitor existing
        //     var existingVisitor = await _visitorRepository.GetAllQueryable()
        //         .FirstOrDefaultAsync(b => b.Email == createDto.Email ||
        //                                 b.IdentityId == createDto.IdentityId ||
        //                                 b.PersonId == createDto.PersonId);

        //     // ambil card berdasarkan nomor kartu
        //     var cardByCardNumber = await _cardRepository.GetByCardNumberAsync(createDto.CardNumber);
        //     if (cardByCardNumber == null)
        //         throw new KeyNotFoundException($"Card with CardNumber {createDto.CardNumber} not found.");

        //     // dapatkan ApplicationId dan username
        //     Guid? applicationId = null;
        //     string username = "System";

        //     if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false)
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

        //     if (!applicationId.HasValue || applicationId == Guid.Empty)
        //         throw new InvalidOperationException("Invalid ApplicationId");

        //     // find or create user group
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

        //     // kalau visitor sudah ada → skip pembuatan user & visitor baru
        //     Visitor visitor;
        //     bool isNewVisitor = false;

        //     if (existingVisitor != null)
        //     {
        //         visitor = existingVisitor;
        //     }
        //     else
        //     {
        //         visitor = _mapper.Map<Visitor>(createDto);
        //         visitor.Id = Guid.NewGuid();
        //         visitor.ApplicationId = applicationId.Value;
        //         visitor.Status = 1;
        //         visitor.CreatedBy = username;
        //         visitor.CreatedAt = DateTime.UtcNow;
        //         visitor.UpdatedBy = username;
        //         visitor.UpdatedAt = DateTime.UtcNow;
        //         isNewVisitor = true;

        //         // handle upload face image
        //         if (createDto.FaceImage != null && createDto.FaceImage.Length > 0)
        //         {
        //             try
        //             {
        //                 if (!_allowedImageTypes.Contains(createDto.FaceImage.ContentType))
        //                     throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

        //                 if (createDto.FaceImage.Length > MaxFileSize)
        //                     throw new ArgumentException("File size exceeds 5 MB limit.");

        //                 var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
        //                 Directory.CreateDirectory(uploadDir);

        //                 var fileName = $"{Guid.NewGuid()}_{createDto.FaceImage.FileName}";
        //                 var filePath = Path.Combine(uploadDir, fileName);

        //                 using (var stream = new FileStream(filePath, FileMode.Create))
        //                 {
        //                     await createDto.FaceImage.CopyToAsync(stream);
        //                 }

        //                 visitor.FaceImage = $"/Uploads/visitorFaceImages/{fileName}";
        //                 visitor.UploadFr = 1;
        //                 visitor.UploadFrError = "Upload successful";
        //             }
        //             catch (Exception ex)
        //             {
        //                 visitor.UploadFr = 2;
        //                 visitor.UploadFrError = ex.Message;
        //                 visitor.FaceImage = "";
        //             }
        //         }
        //         else
        //         {
        //             visitor.UploadFr = 0;
        //             visitor.UploadFrError = "No file uploaded";
        //             visitor.FaceImage = "";
        //         }
        //     }

        //     // check active trx dulu biar tidak double checkin
        //     var hasActiveTrx = await _trxVisitorRepository.GetAllQueryableRaw()
        //         .AnyAsync(t => t.VisitorId == visitor.Id && t.Status == VisitorStatus.Checkin && t.CheckedOutAt == null);
        //     if (hasActiveTrx)
        //         throw new InvalidOperationException("Visitor already has an active transaction");

        //     // generate trx visitor baru
        //     var confirmationCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        //     var newTrx = _mapper.Map<TrxVisitor>(createDto);
        //     newTrx.Id = Guid.NewGuid();
        //     newTrx.VisitorId = visitor.Id;
        //     newTrx.Status = VisitorStatus.Checkin;
        //     newTrx.VisitorActiveStatus = VisitorActiveStatus.Active;
        //     newTrx.InvitationCode = confirmationCode;
        //     newTrx.VisitorGroupCode = 1;
        //     newTrx.VisitorNumber = $"VIS{DateTime.UtcNow.Ticks}";
        //     newTrx.VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6);
        //     newTrx.InvitationCreatedAt = DateTime.UtcNow;
        //     newTrx.TrxStatus = 1;
        //     newTrx.CheckedInAt = DateTime.UtcNow;
        //     newTrx.CheckinBy = username;
        //     newTrx.ApplicationId = applicationId.Value;
        //     newTrx.CreatedBy = username;
        //     newTrx.CreatedAt = DateTime.UtcNow;
        //     newTrx.UpdatedBy = username;
        //     newTrx.UpdatedAt = DateTime.UtcNow;
        //     newTrx.IsInvitationAccepted = true;

        //     //////////////////////////////////////////////

        //     // ===============================================
        //     // 🔹 Assign CardAccess ke Card (Visitor Check-in)
        //     // ===============================================
        //     if ((createDto.CardAccessIds != null && createDto.CardAccessIds.Any()) || createDto.MaskedAreaId.HasValue)
        //     {
        //         var card = await _cardRepository.GetAllQueryable()
        //             .Include(c => c.CardCardAccesses)
        //             .FirstOrDefaultAsync(c => c.CardNumber == createDto.CardNumber);

        //         if (card == null)
        //             throw new KeyNotFoundException($"Card with CardNumber {createDto.CardNumber} not found");

        //         var newCardAccessIds = new List<Guid>();

        //         // Case 1️⃣: Client kirim CardAccessIds langsung
        //         if (createDto.CardAccessIds != null && createDto.CardAccessIds.Any())
        //         {
        //             newCardAccessIds.AddRange(createDto.CardAccessIds);
        //         }

        //         // Case 2️⃣: Client masih pakai MaskedAreaId
        //         if (createDto.MaskedAreaId.HasValue)
        //         {
        //             var cardAccessFromArea = await _cardAccessRepository.GetAllQueryable()
        //                 .Where(ca => ca.CardAccessMaskedAreas.Any(cam => cam.MaskedAreaId == createDto.MaskedAreaId.Value))
        //                 .Select(ca => ca.Id)
        //                 .ToListAsync();

        //             newCardAccessIds.AddRange(cardAccessFromArea);
        //         }

        //         // Hilangkan duplikat
        //         newCardAccessIds = newCardAccessIds.Distinct().ToList();

        //         // Hapus relasi lama yang tidak lagi diperlukan
        //         var toRemove = card.CardCardAccesses.Where(ca => !newCardAccessIds.Contains(ca.CardAccessId)).ToList();
        //         foreach (var old in toRemove)
        //             card.CardCardAccesses.Remove(old);

        //         // Tambahkan relasi baru
        //         foreach (var accessId in newCardAccessIds)
        //         {
        //             if (!card.CardCardAccesses.Any(cca => cca.CardAccessId == accessId))
        //             {
        //                 card.CardCardAccesses.Add(new CardCardAccess
        //                 {
        //                     CardId = card.Id,
        //                     CardAccessId = accessId,
        //                     ApplicationId = card.ApplicationId,
        //                     Status = 1
        //                 });
        //             }
        //         }
        //     }


        //     // assign card access
        //     if (createDto.MaskedAreaId.HasValue)
        //     {
        //         var cardAccess = await _cardAccessRepository.GetAllQueryable()
        //             .FirstOrDefaultAsync(ca => ca.CardAccessMaskedAreas.Any(cam => cam.MaskedAreaId == createDto.MaskedAreaId.Value));

        //         if (cardAccess == null)
        //             throw new KeyNotFoundException($"CardAccess for MaskedAreaId {createDto.MaskedAreaId} not found");

        //         var card = await _cardRepository.GetAllQueryable()
        //             .FirstOrDefaultAsync(ca => ca.CardCardAccesses.Any(cam => cam.Card.CardNumber == createDto.CardNumber));

        //         if (card == null)
        //             throw new KeyNotFoundException($"Card with CardNumber {createDto.CardNumber} not found");

        //         // sync card access
        //         var accessesToRemove = card.CardCardAccesses.Where(cca => cca.CardAccessId != cardAccess.Id).ToList();
        //         foreach (var access in accessesToRemove)
        //             card.CardCardAccesses.Remove(access);

        //         if (!card.CardCardAccesses.Any(cca => cca.CardAccessId == cardAccess.Id))
        //         {
        //             card.CardCardAccesses.Add(new CardCardAccess
        //             {
        //                 CardId = card.Id,
        //                 CardAccessId = cardAccess.Id,
        //                 ApplicationId = card.ApplicationId
        //             });
        //         }
        //     }

        //     var cardRecordDto = new CardRecordCreateDto
        //     {
        //         CardId = cardByCardNumber.Id,
        //         VisitorId = visitor.Id,
        //     };

        //     using var transaction = await _visitorRepository.BeginTransactionAsync();
        //     try
        //     {
        //         if (isNewVisitor)
        //         {
        //             // buat user baru hanya untuk visitor baru
        //             var newUser = new User
        //             {
        //                 Id = Guid.NewGuid(),
        //                 Username = createDto.Email.ToLower(),
        //                 Email = createDto.Email.ToLower(),
        //                 Password = BCrypt.Net.BCrypt.HashPassword("P@ss0wrd"),
        //                 IsCreatedPassword = 1,
        //                 IsEmailConfirmation = 1,
        //                 EmailConfirmationCode = confirmationCode,
        //                 EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(7),
        //                 EmailConfirmationAt = DateTime.UtcNow,
        //                 LastLoginAt = DateTime.MinValue,
        //                 StatusActive = StatusActive.Active,
        //                 ApplicationId = applicationId.Value,
        //                 GroupId = userGroup.Id
        //             };

        //             await _userRepository.AddAsync(newUser);
        //             await _visitorRepository.AddAsync(visitor);
        //         }

        //         await _trxVisitorRepository.AddAsync(newTrx);
        //         await _cardRecordService.CreateAsync(cardRecordDto);
        //         await transaction.CommitAsync();
        //     }
        //     catch
        //     {
        //         await transaction.RollbackAsync();
        //         throw;
        //     }

        //     // kirim email konfirmasi hanya kalau visitor baru
        //     if (isNewVisitor)
        //         await _emailService.SendConfirmationEmailAsync(visitor.Email, visitor.Name, confirmationCode);

        //     var result = _mapper.Map<VisitorDto>(visitor);
        //     if (result == null)
        //         throw new InvalidOperationException("Failed to map Visitor to VisitorDto");

        //     return result;
        // }


        // public async Task<VisitorDto> CreateVisitorAsync(OpenVisitorCreateDto createDto)
        // {
        //     if (createDto == null)
        //         throw new ArgumentNullException(nameof(createDto));

        //     if (string.IsNullOrWhiteSpace(createDto.Email))
        //         throw new ArgumentException("Email is required", nameof(createDto.Email));

        //     if (string.IsNullOrWhiteSpace(createDto.Name))
        //         throw new ArgumentException("Name is required", nameof(createDto.Name));

        //     if (createDto.CardId == Guid.Empty)
        //         throw new ArgumentException("CardId is required for check-in", nameof(createDto.CardId));

        //     // Check for duplicate visitor
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

        //     if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false)
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

        //     if (!applicationId.HasValue || applicationId == Guid.Empty)
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

        //     // Handle FaceImage upload
        //         if (createDto.FaceImage != null && createDto.FaceImage.Length > 0)
        //         {
        //             try
        //             {
        //                 if (!_allowedImageTypes.Contains(createDto.FaceImage.ContentType))
        //                     throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

        //                 if (createDto.FaceImage.Length > MaxFileSize)
        //                     throw new ArgumentException("File size exceeds 5 MB limit.");

        //                 var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
        //                 Directory.CreateDirectory(uploadDir);

        //                 var fileName = $"{Guid.NewGuid()}_{createDto.FaceImage.FileName}";
        //                 var filePath = Path.Combine(uploadDir, fileName);

        //                 using (var stream = new FileStream(filePath, FileMode.Create))
        //                 {
        //                     await createDto.FaceImage.CopyToAsync(stream);
        //                 }

        //                 visitor.FaceImage = $"/Uploads/visitorFaceImages/{fileName}";
        //                 visitor.UploadFr = 1;
        //                 visitor.UploadFrError = "Upload successful";
        //             }
        //             catch (Exception ex)
        //             {
        //                 visitor.UploadFr = 2;
        //                 visitor.UploadFrError = ex.Message;
        //                 visitor.FaceImage = "";
        //             }
        //         }
        //         else
        //         {
        //             visitor.UploadFr = 0;
        //             visitor.UploadFrError = "No file uploaded";
        //             visitor.FaceImage = "";
        //         }

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

        //     // Create TrxVisitor with check-in
        //     var newTrx = _mapper.Map<TrxVisitor>(createDto);
        //     newTrx.Id = Guid.NewGuid();
        //     newTrx.VisitorId = visitor.Id;
        //     newTrx.Status = VisitorStatus.Checkin;
        //     newTrx.VisitorActiveStatus = VisitorActiveStatus.Active;
        //     newTrx.InvitationCode = confirmationCode;
        //     newTrx.VisitorGroupCode = 1;
        //     newTrx.VisitorNumber = $"VIS{DateTime.UtcNow.Ticks}";
        //     newTrx.VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6);
        //     newTrx.InvitationCreatedAt = DateTime.UtcNow;
        //     newTrx.TrxStatus = 1;
        //     newTrx.CheckedInAt = DateTime.UtcNow;
        //     newTrx.CheckinBy = username;
        //     newTrx.ApplicationId = applicationId.Value;
        //     newTrx.CreatedBy = username;
        //     newTrx.CreatedAt = DateTime.UtcNow;
        //     newTrx.UpdatedBy = username;
        //     newTrx.UpdatedAt = DateTime.UtcNow;
        //     newTrx.IsInvitationAccepted = true;

        //     // Check for active transactions
        //     var activeTrx = await _trxVisitorRepository.GetAllQueryableRaw()
        //         .Where(t => t.VisitorId == visitor.Id && t.Status == VisitorStatus.Checkin && t.CheckedOutAt == null)
        //         .AnyAsync();
        //     if (activeTrx)
        //         throw new InvalidOperationException("Visitor already has an active transaction");

        //     // Create CardRecord
        //     var cardRecordDto = new CardRecordCreateDto
        //     {
        //         CardId = createDto.CardId,
        //         VisitorId = visitor.Id,
        //     };

        //     // Perform all database operations in a transaction
        //         using var transaction = await _visitorRepository.BeginTransactionAsync();
        //     try
        //     {
        //         await _userRepository.AddAsync(newUser);
        //         await _visitorRepository.AddAsync(visitor);
        //         await _trxVisitorRepository.AddAsync(newTrx);
        //         await _cardRecordService.CreateAsync(cardRecordDto);
        //         await transaction.CommitAsync();
        //     }
        //     catch
        //     {
        //         await transaction.RollbackAsync();
        //         throw;
        //     }

        //     // Send verification email
        //     await _emailService.SendConfirmationEmailAsync(visitor.Email, visitor.Name, confirmationCode);

        //     var result = _mapper.Map<VisitorDto>(visitor);
        //     if (result == null)
        //         throw new InvalidOperationException("Failed to map Visitor to VisitorDto");

        //     return result;
        // }











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

        //vms before showing      
        //     public async Task<VisitorDto> CreateVisitorAsync(OpenVisitorCreateDto createDto)
        // {
        //     if (createDto == null)
        //         throw new ArgumentNullException(nameof(createDto));

        //     if (string.IsNullOrWhiteSpace(createDto.Email))
        //         throw new ArgumentException("Email is required", nameof(createDto.Email));

        //     if (string.IsNullOrWhiteSpace(createDto.Name))
        //         throw new ArgumentException("Name is required", nameof(createDto.Name));

        //     if (createDto.CardId == Guid.Empty)
        //         throw new ArgumentException("CardId is required for check-in", nameof(createDto.CardId));

        //     // Check for duplicate visitor
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

        //     if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false)
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

        //     if (!applicationId.HasValue || applicationId == Guid.Empty)
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

        //     // Handle FaceImage upload
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

        //     // Create TrxVisitor with check-in
        //     var newTrx = _mapper.Map<TrxVisitor>(createDto);
        //     newTrx.Id = Guid.NewGuid();
        //     newTrx.VisitorId = visitor.Id;
        //     newTrx.Status = VisitorStatus.Checkin;
        //     newTrx.VisitorActiveStatus = VisitorActiveStatus.Active;
        //     newTrx.InvitationCode = confirmationCode;
        //     newTrx.VisitorGroupCode = 1;
        //     newTrx.VisitorNumber = $"VIS{DateTime.UtcNow.Ticks}";
        //     newTrx.VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6);
        //     newTrx.InvitationCreatedAt = DateTime.UtcNow;
        //     newTrx.TrxStatus = 1;
        //     newTrx.CheckedInAt = DateTime.UtcNow;
        //     newTrx.CheckinBy = username;
        //     newTrx.ApplicationId = applicationId.Value;
        //     newTrx.CreatedBy = username;
        //     newTrx.CreatedAt = DateTime.UtcNow;
        //     newTrx.UpdatedBy = username;
        //     newTrx.UpdatedAt = DateTime.UtcNow;
        //     newTrx.IsInvitationAccepted = true;

        //     // Check for active transactions
        //     var activeTrx = await _trxVisitorRepository.GetAllQueryableRaw()
        //         .Where(t => t.VisitorId == visitor.Id && t.Status == VisitorStatus.Checkin && t.CheckedOutAt == null)
        //         .AnyAsync();
        //     if (activeTrx)
        //         throw new InvalidOperationException("Visitor already has an active transaction");

        //     // Assign CardAccess berdasarkan MaskedAreaId
        //     if (createDto.MaskedAreaId.HasValue)
        //     {
        //         // Cari CardAccess berdasarkan MaskedAreaId
        //         var cardAccess = await _cardAccessRepository.GetAllQueryable()
        //             .Where(ca => ca.CardAccessMaskedAreas.Any(cam => cam.MaskedAreaId == createDto.MaskedAreaId.Value))
        //             .FirstOrDefaultAsync();
        //         if (cardAccess == null)
        //             throw new KeyNotFoundException($"CardAccess for MaskedAreaId {createDto.MaskedAreaId} not found");

        //         // Ambil kartu
        //         var card = await _cardRepository.GetAllQueryable()
        //             .Where(ca => ca.CardCardAccesses.Any(cam => cam.CardId == createDto.CardId))
        //             .FirstOrDefaultAsync();
        //         if (card == null)
        //             throw new KeyNotFoundException($"Card with Id {createDto.CardId} not found");

        //         // Hapus semua CardAccess yang tidak terkait dengan CardAccess ini
        //         var accessesToRemove = card.CardCardAccesses
        //             .Where(cca => cca.CardAccessId != cardAccess.Id)
        //             .ToList();
        //         foreach (var access in accessesToRemove)
        //         {
        //             card.CardCardAccesses.Remove(access);
        //         }

        //         // Tambahkan CardAccess jika belum ada
        //         if (!card.CardCardAccesses.Any(cca => cca.CardAccessId == cardAccess.Id))
        //         {
        //             card.CardCardAccesses.Add(new CardCardAccess
        //             {
        //                 CardId = card.Id,
        //                 CardAccessId = cardAccess.Id,
        //                 ApplicationId = card.ApplicationId
        //             });
        //         }
        //     }

        //     // Create CardRecord
        //     var cardRecordDto = new CardRecordCreateDto
        //     {
        //         CardId = createDto.CardId,
        //         VisitorId = visitor.Id,
        //     };

        //     // Perform all database operations in a transaction
        //     using var transaction = await _visitorRepository.BeginTransactionAsync();
        //     try
        //     {
        //         await _userRepository.AddAsync(newUser);
        //         await _visitorRepository.AddAsync(visitor);
        //         await _trxVisitorRepository.AddAsync(newTrx);
        //         await _cardRecordService.CreateAsync(cardRecordDto); // Simpan perubahan CardAccess
        //         await transaction.CommitAsync();
        //     }
        //     catch
        //     {
        //         await transaction.RollbackAsync();
        //         throw;
        //     }

        //     // Send verification email
        //     await _emailService.SendConfirmationEmailAsync(visitor.Email, visitor.Name, confirmationCode);

        //     var result = _mapper.Map<VisitorDto>(visitor);
        //     if (result == null)
        //         throw new InvalidOperationException("Failed to map Visitor to VisitorDto");

        //     return result;
        // }

        // vms backup create
        // public async Task<VisitorDto> CreateVisitorAsync(VMSOpenVisitorCreateDto createDto)
        // {
        //     if (createDto == null)
        //         throw new ArgumentNullException(nameof(createDto));

        //     if (string.IsNullOrWhiteSpace(createDto.Email))
        //         throw new ArgumentException("Email is required", nameof(createDto.Email));

        //     if (string.IsNullOrWhiteSpace(createDto.Name))
        //         throw new ArgumentException("Name is required", nameof(createDto.Name));

        //     if (createDto.CardNumber == string.Empty)
        //         throw new ArgumentException("Card Number is required for check-in", nameof(createDto.CardNumber));

        //     // Check for duplicate visitor
        //     var existingVisitor = await _visitorRepository.GetAllQueryable()
        //         .FirstOrDefaultAsync(b => b.Email == createDto.Email ||
        //                                  b.IdentityId == createDto.IdentityId ||
        //                                  b.PersonId == createDto.PersonId);

        //     if (existingVisitor != null)
        //     {
        //         // if (existingVisitor.Email == createDto.Email)
        //         //     throw new ArgumentException($"Visitor with Email {createDto.Email} already exists.");
        //         if (existingVisitor.IdentityId == createDto.IdentityId)
        //             throw new ArgumentException($"Visitor with IdentityId {createDto.IdentityId} already exists.");
        //         if (existingVisitor.PersonId == createDto.PersonId)
        //             throw new ArgumentException($"Visitor with PersonId {createDto.PersonId} already exists.");
        //     }

        //     var CardByCardNumber = await _cardRepository.GetByCardNumberAsync(createDto.CardNumber);

        //     // if (CardByCardNumber == null)
        //     //     throw new KeyNotFoundException($"Card with CardNumber {createDto.CardNumber} not found.");

        //     // Get ApplicationId and username
        //     Guid? applicationId = null;
        //     string username = "System";

        //     if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false)
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

        //     if (!applicationId.HasValue || applicationId == Guid.Empty)
        //         throw new InvalidOperationException("Invalid ApplicationId");

        //     // find or create user group
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

        //     // handle face image
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


        //     // create account
        //     // if (await _userRepository.EmailExistsAsync(createDto.Email.ToLower()))
        //     //     throw new InvalidOperationException("Email is already registered");

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

        //     // create trx visitor checkin
        //     var newTrx = _mapper.Map<TrxVisitor>(createDto);
        //     newTrx.Id = Guid.NewGuid();
        //     newTrx.VisitorId = visitor.Id;
        //     newTrx.Status = VisitorStatus.Checkin;
        //     newTrx.VisitorActiveStatus = VisitorActiveStatus.Active;
        //     newTrx.InvitationCode = confirmationCode;
        //     newTrx.VisitorGroupCode = 1;
        //     newTrx.VisitorNumber = $"VIS{DateTime.UtcNow.Ticks}";
        //     newTrx.VisitorCode = $"V{DateTime.UtcNow.Ticks}{Guid.NewGuid():N}".Substring(0, 6);
        //     newTrx.InvitationCreatedAt = DateTime.UtcNow;
        //     newTrx.TrxStatus = 1;
        //     newTrx.CheckedInAt = DateTime.UtcNow;
        //     newTrx.CheckinBy = username;
        //     newTrx.ApplicationId = applicationId.Value;
        //     newTrx.CreatedBy = username;
        //     newTrx.CreatedAt = DateTime.UtcNow;
        //     newTrx.UpdatedBy = username;
        //     newTrx.UpdatedAt = DateTime.UtcNow;
        //     newTrx.IsInvitationAccepted = true;

        //     // check active trx
        //     var activeTrx = await _trxVisitorRepository.GetAllQueryableRaw()
        //         .Where(t => t.VisitorId == visitor.Id && t.Status == VisitorStatus.Checkin && t.CheckedOutAt == null)
        //         .AnyAsync();
        //     if (activeTrx)
        //         throw new InvalidOperationException("Visitor already has an active transaction");

        //     // aSSIGN card access berdasarkan maskedareaid
        //     if (createDto.MaskedAreaId.HasValue)
        //     {
        //         // cari card access berdasarkan maskedareaid
        //         var cardAccess = await _cardAccessRepository.GetAllQueryable()
        //             .Where(ca => ca.CardAccessMaskedAreas.Any(cam => cam.MaskedAreaId == createDto.MaskedAreaId.Value))
        //             .FirstOrDefaultAsync();
        //         if (cardAccess == null)
        //             throw new KeyNotFoundException($"CardAccess for MaskedAreaId {createDto.MaskedAreaId} not found");

        //         // get card
        //         var card = await _cardRepository.GetAllQueryable()
        //             .Where(ca => ca.CardCardAccesses.Any(cam => cam.Card.CardNumber == createDto.CardNumber))
        //             .FirstOrDefaultAsync();
        //         if (card == null)
        //             throw new KeyNotFoundException($"Card with Id {createDto.CardNumber} not found");

        //         // hapus card access yang tidak sesuai
        //         var accessesToRemove = card.CardCardAccesses
        //             .Where(cca => cca.CardAccessId != cardAccess.Id)
        //             .ToList();
        //         foreach (var access in accessesToRemove)
        //         {
        //             card.CardCardAccesses.Remove(access);
        //         }

        //         // add card access kalau ga ada
        //         if (!card.CardCardAccesses.Any(cca => cca.CardAccessId == cardAccess.Id))
        //         {
        //             card.CardCardAccesses.Add(new CardCardAccess
        //             {
        //                 CardId = card.Id,
        //                 CardAccessId = cardAccess.Id,
        //                 ApplicationId = card.ApplicationId
        //             });
        //         }
        //     }

        //     // create card record
        //     var cardRecordDto = new CardRecordCreateDto
        //     {
        //         CardId = CardByCardNumber.Id,
        //         VisitorId = visitor.Id,
        //     };

        //     // transaction
        //     using var transaction = await _visitorRepository.BeginTransactionAsync();
        //     try
        //     {
        //         await _userRepository.AddAsync(newUser);
        //         await _visitorRepository.AddAsync(visitor);
        //         await _trxVisitorRepository.AddAsync(newTrx);
        //         await _cardRecordService.CreateAsync(cardRecordDto);
        //         await transaction.CommitAsync();
        //     }
        //     catch
        //     {
        //         await transaction.RollbackAsync();
        //         throw;
        //     }

        //     // email verification
        //     await _emailService.SendConfirmationEmailAsync(visitor.Email, visitor.Name, confirmationCode);

        //     var result = _mapper.Map<VisitorDto>(visitor);
        //     if (result == null)
        //         throw new InvalidOperationException("Failed to map Visitor to VisitorDto");

        //     return result;
        // }






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
        
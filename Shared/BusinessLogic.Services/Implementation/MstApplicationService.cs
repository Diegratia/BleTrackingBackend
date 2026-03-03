using AutoMapper;
using BusinessLogic.Services.Interface;
using ClosedXML.Excel;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementation
{
    public class MstApplicationService : BaseService, IMstApplicationService
    {
        private readonly MstApplicationRepository _applicationRepository;
        private readonly UserGroupRepository _userGroupRepository;
        private readonly UserRepository _userRepository;
        private readonly IMstIntegrationService _integrationService;
        private readonly IMstBrandService _brandService;
        private readonly IMapper _mapper;
        private readonly IAuditEmitter _audit;

        public MstApplicationService(
            MstApplicationRepository applicationRepository,
            UserGroupRepository userGroupRepository,
            UserRepository userRepository,
            IMstIntegrationService integrationService,
            IMstBrandService brandService,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _applicationRepository = applicationRepository;
            _userGroupRepository = userGroupRepository;
            _userRepository = userRepository;
            _integrationService = integrationService;
            _brandService = brandService;
            _mapper = mapper;
            _audit = audit;
        }

        public async Task<IEnumerable<MstApplicationRead>> GetAllApplicationsAsync()
        {
            return await _applicationRepository.GetAllAsync();
        }

        public async Task<MstApplicationRead?> GetApplicationByIdAsync(Guid id)
        {
            return await _applicationRepository.GetByIdAsync(id);
        }

        public string RandomApiKeyGenerator(int length = 32)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<MstApplicationDto> CreateApplicationAsync(MstApplicationCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var application = _mapper.Map<MstApplication>(dto);
            var genetateApplicationId = Guid.NewGuid();
            application.Id = genetateApplicationId;
            application.ApplicationStatus = 1;

            var createdApplication = await _applicationRepository.AddAsync(application);

            var username = UsernameFormToken;
            using var transaction = await _applicationRepository.BeginTransactionAsync();
            try
            {
                var createdBrand = new MstBrandCreateDto
                {
                    ApplicationId = createdApplication.Id,
                    Name = $"BIO-{createdApplication.Id}",
                    Tag = $"BIO-Tag-{createdApplication.Id}",
                };
                var brandId = await _brandService.CreateInternalAsync(createdBrand);
                var createdBrandId = brandId.Id;

                var createIntegration = new MstIntegrationCreateDto
                {
                    BrandId = createdBrandId,
                    IntegrationType = IntegrationType.Api.ToString().ToLower(),
                    ApiTypeAuth = ApiTypeAuth.ApiKey.ToString().ToLower(),
                    ApiUrl = "bio-peopletracking.com",
                    ApiAuthUsername = $"bio-peopletracking-{createdApplication.Id}",
                    ApiAuthPasswd = RandomApiKeyGenerator(8),
                    ApiKeyField = "X-BIOPEOPLETRACKING-API-KEY",
                    ApiKeyValue = RandomApiKeyGenerator(32),
                    ApplicationId = createdApplication.Id,
                };

                var userGroups = new List<UserGroup>
            {
                new UserGroup
                {
                    Id = Guid.NewGuid(),
                    Name = $"Super Admin Group-{genetateApplicationId}",
                    LevelPriority = LevelPriority.SuperAdmin,
                    ApplicationId = genetateApplicationId,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                },
                new UserGroup
                {
                    Id = Guid.NewGuid(),
                    Name = $"Operator Admin Group-{genetateApplicationId}",
                    LevelPriority = LevelPriority.PrimaryAdmin,
                    ApplicationId = genetateApplicationId,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                },
                new UserGroup
                {
                    Id = Guid.NewGuid(),
                    Name = $"Operator Security Group-{genetateApplicationId}",
                    LevelPriority = LevelPriority.Primary,
                    ApplicationId = genetateApplicationId,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                },
                new UserGroup
                {
                    Id = Guid.NewGuid(),
                    Name = $"Other Primary Group-{genetateApplicationId}",
                    LevelPriority = LevelPriority.Primary,
                    ApplicationId = genetateApplicationId,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                }
            };

                foreach (var group in userGroups)
                {
                    await _userGroupRepository.AddAsyncRaw(group);
                }

                var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = $"SuperAdmin-{genetateApplicationId}",
                    Password = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd"),
                    IsCreatedPassword = 1,
                    Email = "superadmin@example.com",
                    IsEmailConfirmation = 1,
                    EmailConfirmationCode = Guid.NewGuid().ToString(),
                    EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(1),
                    EmailConfirmationAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    Status = 1,
                    GroupId = userGroups[0].Id,
                    ApplicationId = userGroups[0].ApplicationId
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = $"Operator Primary Admin-{genetateApplicationId}",
                    Password = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd"),
                    IsCreatedPassword = 1,
                    Email = "operator@example.com",
                    IsEmailConfirmation = 1,
                    EmailConfirmationCode = Guid.NewGuid().ToString(),
                    EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(1),
                    EmailConfirmationAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    Status = 1,
                    GroupId = userGroups[1].Id,
                    ApplicationId = userGroups[1].ApplicationId
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = $"Security Primary-{genetateApplicationId}",
                    Password = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd"),
                    IsCreatedPassword = 1,
                    Email = "securityuser@example.com",
                    IsEmailConfirmation = 1,
                    EmailConfirmationCode = Guid.NewGuid().ToString(),
                    EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(1),
                    EmailConfirmationAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    Status = 1,
                    GroupId = userGroups[2].Id,
                    ApplicationId = userGroups[2].ApplicationId
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = $"Other Primary User-{genetateApplicationId}",
                    Password = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd"),
                    IsCreatedPassword = 1,
                    Email = "otherprimary@example.com",
                    IsEmailConfirmation = 1,
                    EmailConfirmationCode = Guid.NewGuid().ToString(),
                    EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(1),
                    EmailConfirmationAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    Status = 1,
                    GroupId = userGroups[3].Id,
                    ApplicationId = userGroups[3].ApplicationId
                }
            };

                foreach (var user in users)
                {
                    await _userRepository.AddRawAsync(user);
                }

                await _integrationService.CreateAsync(createIntegration);
                await transaction.CommitAsync();

                _audit.Created("MstApplication", createdApplication.Id, $"Application created: {createdApplication.ApplicationName}");

                return _mapper.Map<MstApplicationDto>(createdApplication);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateApplicationAsync(Guid id, MstApplicationUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var application = await _applicationRepository.GetByIdEntityAsync(id);
            if (application == null)
                throw new KeyNotFoundException($"Application with ID {id} not found");

            _mapper.Map(dto, application);
            await _applicationRepository.UpdateAsync(application);

            _audit.Updated("MstApplication", id, $"Application updated: {application.ApplicationName}");
        }

        public async Task DeleteApplicationAsync(Guid id)
        {
            var application = await _applicationRepository.GetByIdEntityAsync(id);
            if (application == null)
                throw new KeyNotFoundException($"Application with ID {id} not found");

            await _applicationRepository.DeleteAsync(application);

            _audit.Deleted("MstApplication", id, $"Application deleted: {application.ApplicationName}");
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, MstApplicationFilter filter)
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "ApplicationName";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            var (data, total, filtered) = await _applicationRepository.FilterAsync(filter);

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
            var applications = await _applicationRepository.GetAllExportAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Application Report")
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
                            header.Cell().Element(CellStyle).Text("Application Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Organization Type").SemiBold();
                            header.Cell().Element(CellStyle).Text("Host Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("License Code").SemiBold();
                            header.Cell().Element(CellStyle).Text("License Type").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        int index = 1;
                        foreach (var application in applications)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(application.ApplicationName);
                            table.Cell().Element(CellStyle).Text(application.OrganizationType.ToString());
                            table.Cell().Element(CellStyle).Text(application.HostName);
                            table.Cell().Element(CellStyle).Text(application.LicenseCode);
                            table.Cell().Element(CellStyle).Text(application.LicenseType.ToString());
                            table.Cell().Element(CellStyle).Text(application.ApplicationStatus.ToString());
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
            var applications = await _applicationRepository.GetAllExportAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Applications");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Application Name";
            worksheet.Cell(1, 3).Value = "Organization Type";
            worksheet.Cell(1, 4).Value = "Host Name";
            worksheet.Cell(1, 5).Value = "License Code";
            worksheet.Cell(1, 6).Value = "License Type";
            worksheet.Cell(1, 7).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var application in applications)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = application.ApplicationName;
                worksheet.Cell(row, 3).Value = application.OrganizationType.ToString();
                worksheet.Cell(row, 4).Value = application.HostName;
                worksheet.Cell(row, 5).Value = application.LicenseCode;
                worksheet.Cell(row, 6).Value = application.LicenseType.ToString();
                worksheet.Cell(row, 7).Value = application.ApplicationStatus.ToString();
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

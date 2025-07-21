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

namespace BusinessLogic.Services.Implementation
{
    public class MstApplicationService : IMstApplicationService
    {
        private readonly MstApplicationRepository _applicationRepository;
        private readonly UserGroupRepository _userGroupRepository;
        private readonly UserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MstApplicationService(
            MstApplicationRepository applicationRepository,
            UserGroupRepository userGroupRepository,
            UserRepository userRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _applicationRepository = applicationRepository;
            _userGroupRepository = userGroupRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<MstApplicationDto>> GetAllApplicationsAsync()
        {
            var applications = await _applicationRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstApplicationDto>>(applications);
        }

        public async Task<MstApplicationDto> GetApplicationByIdAsync(Guid id)
        {
            var application = await _applicationRepository.GetByIdAsync(id);
            return application == null ? null : _mapper.Map<MstApplicationDto>(application);
        }

        public async Task<MstApplicationDto> CreateApplicationAsync(MstApplicationCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var application = _mapper.Map<MstApplication>(dto);
            // application.Id = Guid.NewGuid();
            application.ApplicationStatus = 1;

            var createdApplication = await _applicationRepository.AddAsync(application);

            // var userGroup = await _userGroupRepository.GetByApplicationIdAndPriorityAsync(
            //     createdApplication.Id, LevelPriority.Primary);

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            // if (userGroup == null)
            // {
            //     userGroup = new UserGroup
            //     {
            //         Id = Guid.NewGuid(),
            //         Name = $"Primary Group for {createdApplication.Id}",
            //         LevelPriority = LevelPriority.Primary,
            //         ApplicationId = createdApplication.Id,
            //         CreatedBy = username,
            //         CreatedAt = DateTime.UtcNow,
            //         UpdatedBy = username,
            //         UpdatedAt = DateTime.UtcNow,
            //         Status = 1
            //     };
            //     await _userGroupRepository.AddAsync(userGroup);
            // }

            var userGroups = new List<UserGroup>
            {
                new UserGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    LevelPriority = LevelPriority.PrimaryAdmin,
                    ApplicationId = application.Id,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                },
                new UserGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "Operator Admin",
                    LevelPriority = LevelPriority.PrimaryAdmin,
                    ApplicationId = application.Id,
                     CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                },
                new UserGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "Operator Security",
                    LevelPriority = LevelPriority.Primary,
                    ApplicationId = application.Id,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                },
                new UserGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "Other Primary",
                    LevelPriority = LevelPriority.Primary,
                    ApplicationId = application.Id,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                }
            };

            foreach (var group in userGroups)
            {
                await _userGroupRepository.AddAsync(group);
            }
            var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "TestPrimaryUser1",
                    Password = BCrypt.Net.BCrypt.HashPassword("testprimaryuser123@"),
                    IsCreatedPassword = 1,
                    Email = "testprimaryuser1@example.com",
                    IsEmailConfirmation = 0,
                    EmailConfirmationCode = Guid.NewGuid().ToString(),
                    EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(1),
                    EmailConfirmationAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    StatusActive = StatusActive.Active,
                    GroupId = userGroups[0].Id // Admin
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "TestPrimaryUser2",
                    Password = BCrypt.Net.BCrypt.HashPassword("testprimaryuser123@"),
                    IsCreatedPassword = 1,
                    Email = "testprimaryuser2@example.com",
                    IsEmailConfirmation = 0,
                    EmailConfirmationCode = Guid.NewGuid().ToString(),
                    EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(1),
                    EmailConfirmationAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    StatusActive = StatusActive.Active,
                    GroupId = userGroups[1].Id // Operator
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "TestPrimaryUser3",
                    Password = BCrypt.Net.BCrypt.HashPassword("testprimaryuser123@"),
                    IsCreatedPassword = 1,
                    Email = "testprimaryuser3@example.com",
                    IsEmailConfirmation = 0,
                    EmailConfirmationCode = Guid.NewGuid().ToString(),
                    EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(1),
                    EmailConfirmationAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    StatusActive = StatusActive.Active,
                    GroupId = userGroups[2].Id // Security
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "TestPrimaryUser4",
                    Password = BCrypt.Net.BCrypt.HashPassword("testprimaryuser123@"),
                    IsCreatedPassword = 1,
                    Email = "testprimaryuser4@example.com",
                    IsEmailConfirmation = 0,
                    EmailConfirmationCode = Guid.NewGuid().ToString(),
                    EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(1),
                    EmailConfirmationAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    StatusActive = StatusActive.Active,
                    GroupId = userGroups[3].Id // other primary
                }
            };

            foreach (var user in users)
            {
                await _userRepository.AddAsync(user);
            }

            // await _applicationRepository.AddAsync(application);

            return _mapper.Map<MstApplicationDto>(createdApplication);
        }

        public async Task UpdateApplicationAsync(Guid id, MstApplicationUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var application = await _applicationRepository.GetByIdAsync(id);
            if (application == null)
                throw new KeyNotFoundException($"Application with ID {id} not found");

            _mapper.Map(dto, application);

            await _applicationRepository.UpdateAsync(application);
        }

        public async Task DeleteApplicationAsync(Guid id)
        {
            await _applicationRepository.DeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _applicationRepository.GetAllQueryable();

            var searchableColumns = new[] { "ApplicationName" };
            var validSortColumns = new[] { "ApplicationName", "ApplicationType", "OrganizationType" ,"ApplicationRegistered", "ApplicationExpired", "ApplicationStatus", "HostName", "HostAddress", "ApplicationCustomName", "ApplicationCustomDomain", "LicenseCode" };
            var enumColumns = new Dictionary<string, Type> { { "ApplicationType", typeof(ApplicationType) } };

            var filterService = new GenericDataTableService<MstApplication, MstApplicationDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns,
                enumColumns);

            return await filterService.FilterAsync(request);
        }

          public async Task<byte[]> ExportPdfAsync()
        {
            var applications = await _applicationRepository.GetAllExportAsync();

            var document = QuestPDF.Fluent.Document.Create(container =>
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

            using var workbook = new ClosedXML.Excel.XLWorkbook();
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
using Bogus;
using Repositories.DbContexts;
using Entities.Models;
using System;
using System.Linq;
using Bogus.DataSets;
using Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Seeding
{
    public static class DatabaseSeeder
    {
        public static void Seed(BleTrackingDbContext context)
        {
            // If the database is completely empty, run the SQL dump from setup_people_tracking_db.sql
            if (!context.MstApplications.Any())
            {
                try 
                {
                    context.Database.ExecuteSqlRaw(SqlDumpData.SqlQuery);    
                    return; // Skip the rest of the generated dummy data since we just imported the full raw backup
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error migrating Seeder Dump: " + ex.Message);
                }
            }

            // Fallback: Default Seed Application
            if (!context.MstApplications.Any(a => a.ApplicationStatus != 0))
            {
                var app = new MstApplication
                {
                    Id = new Guid("c926d20b-a746-4492-9924-eb7eee76305c"),
                    ApplicationName = "BIO PEOPLE TRACKING",
                    OrganizationType = OrganizationType.Single,
                    ApplicationType = ApplicationType.Tracking,
                    ApplicationCustomDomain = "localhost",
                    ApplicationCustomPort = "8080",
                    ApplicationCustomName = "BioPeopleTracking",
                    ApplicationExpired = DateTime.UtcNow.AddYears(10),
                    ApplicationRegistered = DateTime.UtcNow,
                    OrganizationAddress = "Jl. Default No 1",
                    HostName = "Admin",
                    HostPhone = "08123456789",
                    HostEmail = "admin@example.com",
                    HostAddress = "Jl. Host No 1",
                    LicenseCode = "PERPETUAL-001",
                    LicenseType = LicenseType.Perpetual,
                    ApplicationStatus = 1
                };
                context.MstApplications.Add(app);
                context.SaveChanges();
            }

            var appId = context.MstApplications.First().Id;

            // Seed UserGroup
            if (!context.UserGroups.Any(ug => ug.Status != 0))
            {
                var groups = new[]
                {
                    new UserGroup { Id = Guid.NewGuid(), Name = "System", LevelPriority = LevelPriority.System, ApplicationId = appId, CreatedBy = "System", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, UpdatedBy = "System", Status = 1 },
                    new UserGroup { Id = Guid.NewGuid(), Name = "Super Admin", LevelPriority = LevelPriority.SuperAdmin, ApplicationId = appId, CreatedBy = "System", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, UpdatedBy = "System", Status = 1 }
                };
                context.UserGroups.AddRange(groups);
                context.SaveChanges();
            }

            // Seed User
            if (!context.Users.Any(u => u.Status != 0))
            {
                var superadminGroup = context.UserGroups.FirstOrDefault(ug => ug.LevelPriority == LevelPriority.SuperAdmin && ug.Status != 0);
                if (superadminGroup != null)
                {
                    var superadmin = new User
                    {
                        Id = new Guid("b53f464b-68b4-4831-ab9a-8f3e56d9fa33"),
                        Username = "superadmin",
                        Password = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd"),
                        IsCreatedPassword = 1,
                        Email = "superadmin@test.com",
                        IsEmailConfirmation = 1,
                        EmailConfirmationCode = "CONFIRMED",
                        EmailConfirmationExpiredAt = DateTime.UtcNow,
                        EmailConfirmationAt = DateTime.UtcNow,
                        LastLoginAt = DateTime.UtcNow,
                        Status = 1,
                        GroupId = superadminGroup.Id,
                        ApplicationId = appId,
                    };
                    context.Users.Add(superadmin);
                }

                var systemGroup = context.UserGroups.FirstOrDefault(ug => ug.LevelPriority == LevelPriority.System && ug.Status != 0);
                if (systemGroup != null)
                {
                    var systemadmin = new User
                    {
                        Id = Guid.NewGuid(),
                        Username = "systemadmin",
                        Password = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd"),
                        IsCreatedPassword = 1,
                        Email = "systemadmin@test.com",
                        IsEmailConfirmation = 1,
                        EmailConfirmationCode = "CONFIRMED",
                        EmailConfirmationExpiredAt = DateTime.UtcNow,
                        EmailConfirmationAt = DateTime.UtcNow,
                        LastLoginAt = DateTime.UtcNow,
                        Status = 1,
                        GroupId = systemGroup.Id,
                        ApplicationId = appId,
                    };
                    context.Users.Add(systemadmin);
                }

                context.SaveChanges();
            }
            // Seed Organization
            if (!context.MstOrganizations.Any(o => o.Status != 0))
            {
                var organization = new MstOrganization
                {
                    Id = new Guid("9AD17645-8D52-414B-A770-82C8FC1E187E"),
                    Code = "1",
                    Name = "BIO - Org",
                    OrganizationHost = "BIO - Host",
                    ApplicationId = appId,
                    Status = 1,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "System"
                };
                context.MstOrganizations.Add(organization);
                context.SaveChanges();
            }

            // Seed District
            if (!context.MstDistricts.Any(d => d.Status != 0))
            {
                var district = new MstDistrict
                {
                    Id = new Guid("0CF11396-56F5-4946-9DD8-F02B0EF6F1AD"),
                    Code = "1",
                    Name = "BIO - District",
                    DistrictHost = "BIO - Host",
                    ApplicationId = appId,
                    Status = 1,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "System"
                };
                context.MstDistricts.Add(district);
                context.SaveChanges();
            }

            // Seed Department
            if (!context.MstDepartments.Any(d => d.Status != 0))
            {
                var department = new MstDepartment
                {
                    Id = new Guid("F99CF1F7-789E-4C75-A044-BDB10C773881"),
                    Code = "1",
                    Name = "BIO - Department",
                    DepartmentHost = "BIO - Host",
                    ApplicationId = appId,
                    Status = 1,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "System"
                };
                context.MstDepartments.Add(department);
                context.SaveChanges();
            }

            // Seed Brand
            if (!context.MstBrands.Any(b => b.Status != 0))
            {
                var brand = new MstBrand
                {
                    Id = new Guid("824264AF-036F-4AB1-8179-469F1BCC8813"),
                    Name = "Bio - Initial",
                    Tag = "People Tracking Tag",
                    ApplicationId = appId,
                    Status = 1
                };
                context.MstBrands.Add(brand);
                context.SaveChanges();
            }
            
            // Seed Integration
            if (!context.MstIntegrations.Any(i => i.Status != 0))
            {
                var integration = new MstIntegration
                {
                    Id = Guid.NewGuid(),
                    BrandId = new Guid("824264AF-036F-4AB1-8179-469F1BCC8813"),
                    ApplicationId = appId,
                    IntegrationType = IntegrationType.Api,
                    ApiTypeAuth = ApiTypeAuth.ApiKey,
                    ApiKeyField = "X-BIOPEOPLETRACKING-API-KEY",
                    ApiKeyValue = "FujDuGTsyEXVwkKrtRgn52APwAVRGmPOiIRX8cffynDvIW35bJaGeH3NcH6HcSeK",
                    Status = 1,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "System"
                };
                context.MstIntegrations.Add(integration);
                context.SaveChanges();
            }

            // Seed Building and Floor
            if (!context.MstBuildings.Any(b => b.Status != 0))
            {
                 var building = new MstBuilding
                 {
                     Id = Guid.NewGuid(),
                     Name = "Main Building",
                     ApplicationId = appId,
                     Status = 1
                 };
                 context.MstBuildings.Add(building);
                 context.SaveChanges();

                 var floor = new MstFloor
                 {
                     Id = Guid.NewGuid(),
                     Name = "1st Floor",
                     BuildingId = building.Id,
                     ApplicationId = appId,
                     Status = 1
                 };
                 context.MstFloors.Add(floor);
                 context.SaveChanges();
            }

            // Seed Member
            if (!context.MstMembers.Any(m => m.Status != 0))
            {
                var member = new MstMember
                {
                    Id = Guid.NewGuid(),
                    PersonId = "EMP001",
                    Name = "Alice Employee",
                    ApplicationId = appId,
                    Status = 1
                };
                context.MstMembers.Add(member);
                context.SaveChanges();
            }

            // Seed Security
            if (!context.MstSecurities.Any(m => m.Status != 0))
            {
                var security = new MstSecurity
                {
                    Id = Guid.NewGuid(),
                    PersonId = "SEC001",
                    Name = "Bob Security",
                    ApplicationId = appId,
                    Status = 1
                };
                context.MstSecurities.Add(security);
                context.SaveChanges();
            }

            // Seed Visitor
            if (!context.Visitors.Any())
            {
                var visitor = new Visitor
                {
                    Id = Guid.NewGuid(),
                    PersonId = "VIS001",
                    Name = "Charlie Visitor",
                    ApplicationId = appId,
                    Status = 1
                };
                context.Visitors.Add(visitor);
                context.SaveChanges();
            }

            // Seed Floorplan
            if (!context.MstFloorplans.Any(f => f.Status != 0))
            {
                var floorplan = new MstFloorplan
                {
                    Id = Guid.NewGuid(),
                    Name = "1st Floorplan",
                    FloorId = context.MstFloors.First().Id,
                    ApplicationId = appId,
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow,
                    Status = 1
                };
                context.MstFloorplans.Add(floorplan);
                context.SaveChanges();
            }

            var floorplanId = context.MstFloorplans.First().Id;

            // Seed Masked Area
            if (!context.FloorplanMaskedAreas.Any(a => a.Status != 0))
            {
                var area = new FloorplanMaskedArea
                {
                    Id = Guid.NewGuid(),
                    FloorplanId = floorplanId,
                    FloorId = context.MstFloors.First().Id,
                    ApplicationId = appId,
                    Name = "Lobby Area",
                    AreaShape = "[{\"id\":\"1\",\"x\":100,\"y\":100,\"x_px\":100,\"y_px\":100},{\"id\":\"2\",\"x\":300,\"y\":100,\"x_px\":300,\"y_px\":100},{\"id\":\"3\",\"x\":300,\"y\":300,\"x_px\":300,\"y_px\":300},{\"id\":\"4\",\"x\":100,\"y\":300,\"x_px\":100,\"y_px\":300}]",
                    ColorArea = "#FF0000",
                    RestrictedStatus = RestrictedStatus.NonRestrict,
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow,
                    Status = 1
                };
                context.FloorplanMaskedAreas.Add(area);
                context.SaveChanges();
            }

            var areaId = context.FloorplanMaskedAreas.First().Id;

            // Seed Ble Reader
            if (!context.MstBleReaders.Any(r => r.Status != 0))
            {
                var reader = new MstBleReader
                {
                    Id = Guid.NewGuid(),
                    BrandId = context.MstBrands.First().Id,
                    ApplicationId = appId,
                    Name = "Lobby Reader 1",
                    Ip = "192.168.1.100",
                    Gmac = "0123456789ABCDEF",
                    ReaderType = ReaderType.Indoor, // Assuming default enum type
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow,
                    Status = 1
                };
                context.MstBleReaders.Add(reader);
                context.SaveChanges();
            }

            var readerId = context.MstBleReaders.First().Id;

            // Seed Floorplan Device
            if (!context.FloorplanDevices.Any(d => d.Status != 0))
            {
                var device = new FloorplanDevice
                {
                    Id = Guid.NewGuid(),
                    Name = "Lobby Reader Device",
                    Type = DeviceType.BleReader,
                    FloorplanId = floorplanId,
                    FloorplanMaskedAreaId = areaId,
                    ReaderId = readerId,
                    ApplicationId = appId,
                    PosX = 150,
                    PosY = 150,
                    PosPxX = 150,
                    PosPxY = 150,
                    DeviceStatus = DeviceStatus.Active,
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow,
                    Status = 1
                };
                context.FloorplanDevices.Add(device);
                context.SaveChanges();
            }
            // Seed Cards with specific MACs
            if (!context.Cards.Any())
            {
                var memberId = context.MstMembers.FirstOrDefault()?.Id;
                var securityId = context.MstSecurities.FirstOrDefault()?.Id;
                var visitorId = context.Visitors.FirstOrDefault()?.Id;

                var cards = new[]
                {
                    new Card
                    {
                        Id = Guid.NewGuid(),
                        Name = "Card Member",
                        CardNumber = "465757",
                        Dmac = "BC572905D5B9",
                        MemberId = memberId,
                        ApplicationId = appId,
                        StatusCard = 1
                    },
                    new Card
                    {
                        Id = Guid.NewGuid(),
                        Name = "Card Visitor",
                        CardNumber = "677028",
                        Dmac = "BC57291F5FD0",
                        VisitorId = visitorId, 
                        ApplicationId = appId,
                        StatusCard = 1
                    },
                    new Card
                    {
                        Id = Guid.NewGuid(),
                        Name = "Card Security",
                        CardNumber = "677013",
                        Dmac = "BC57291F5FC1",
                        SecurityId = securityId, 
                        ApplicationId = appId,
                        StatusCard = 1
                    }
                };
                context.Cards.AddRange(cards);
                context.SaveChanges();

                // Add TrxVisitor to simulate Visitor checkin
                if (visitorId != null && !context.TrxVisitors.Any())
                {
                    var trxVisitor = new TrxVisitor
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = appId,
                        VisitorId = visitorId,
                        CheckedInAt = DateTime.UtcNow,
                        CheckinBy = "System",
                        Status = VisitorStatus.Checkin,
                        VisitorActiveStatus = VisitorActiveStatus.Active,
                        InvitationCreatedAt = DateTime.UtcNow.AddDays(-1),
                        Remarks = "Auto-seeded checkin for demonstration",
                        VisitorPeriodStart = DateTime.UtcNow,
                        VisitorPeriodEnd = DateTime.UtcNow.AddDays(1),
                        IsInvitationAccepted = true,
                        CardNumber = "677028",
                        TrxStatus = 1,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = "System"
                    };
                    context.TrxVisitors.Add(trxVisitor);
                    context.SaveChanges();
                }

                // Add CardRecords to simulate card assignment / tapped
                if (!context.CardRecords.Any())
                {
                    var cardRecords = cards.Select(c => new CardRecord
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = appId,
                        CardId = c.Id,
                        Name = $"Assign to {c.Name}",
                        Timestamp = DateTime.UtcNow,
                        CheckinAt = DateTime.UtcNow,
                        CheckinBy = "System",
                        VisitorId = c.VisitorId,
                        MemberId = c.MemberId, // Reusing MemberId field for simplicity if it handles both or just to simulate. Let's map if appropriate. CardRecord has MemberId and VisitorId.
                        Status = 1,
                        VisitorActiveStatus = c.VisitorId != null ? VisitorActiveStatus.Active : null,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = "System"
                    }).ToList();

                    context.CardRecords.AddRange(cardRecords);
                    context.SaveChanges();
                }
            }         

             // // 14. MstSecurity
            // if (!context.MstSecurities.Any(m => m.Status != 0))
            // {
            //     var securityFaker = new Faker<MstSecurity>()
            //         .RuleFor(m => m.Id, f => Guid.NewGuid())
            //         .RuleFor(m => m.PersonId, f => "SEC" + f.Random.Number(1000, 9999))
            //         .RuleFor(m => m.OrganizationId, f => context.MstOrganizations
            //             .Where(o => o.Status != 0)
            //             .OrderBy(r => Guid.NewGuid())
            //             .First()
            //             .Id)
            //         .RuleFor(m => m.DepartmentId, f => context.MstDepartments
            //             .Where(d => d.Status != 0)
            //             .OrderBy(r => Guid.NewGuid())
            //             .First()
            //             .Id)
            //         .RuleFor(m => m.DistrictId, f => context.MstDistricts
            //             .Where(d => d.Status != 0)
            //             .OrderBy(r => Guid.NewGuid())
            //             .First()
            //             .Id)
            //         .RuleFor(m => m.IdentityId, f => "ID" + f.Random.Number(100, 999))
            //         .RuleFor(m => m.CardNumber, f => "CARD" + f.Random.Number(1000, 9999))
            //         .RuleFor(m => m.BleCardNumber, f => "BLE" + f.Random.Number(100, 999))
            //         .RuleFor(m => m.Name, f => f.Name.FullName())
            //         .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber())
            //         .RuleFor(m => m.Email, f => f.Internet.Email())
            //         .RuleFor(m => m.Gender, f => f.PickRandom<Gender>())
            //         .RuleFor(m => m.Address, f => f.Address.FullAddress())
            //         .RuleFor(m => m.FaceImage, f => $"https://example.com/faces/{f.Random.Word()}.jpg")
            //         .RuleFor(m => m.UploadFr, f => f.Random.Int(0, 2))
            //         .RuleFor(m => m.UploadFrError, f => f.Random.Bool() ? "" : "Upload failed")
            //         .RuleFor(m => m.BirthDate, f => DateOnly.FromDateTime(f.Date.Past(yearsToGoBack: 30, refDate: DateTime.Today.AddYears(-18))))
            //         .RuleFor(m => m.JoinDate, f => DateOnly.FromDateTime(DateTime.Today))
            //         .RuleFor(m => m.ExitDate, f => DateOnly.MaxValue)
            //         .RuleFor(m => m.ApplicationId, f => context.MstApplications
            //             .Where(a => a.ApplicationStatus != 0)
            //             .OrderBy(r => Guid.NewGuid())
            //             .First()
            //             .Id)
            //         .RuleFor(m => m.StatusEmployee, f => f.PickRandom<StatusEmployee>())
            //         .RuleFor(m => m.CreatedBy, f => "System")
            //         .RuleFor(m => m.CreatedAt, f => DateTime.UtcNow)
            //         .RuleFor(m => m.UpdatedBy, f => "System")
            //         .RuleFor(m => m.UpdatedAt, f => DateTime.UtcNow)
            //         .RuleFor(m => m.Status, f => 1);

            //     var securities = securityFaker.Generate(5);
            //     context.MstSecurities.AddRange(securities);
            //     context.SaveChanges();
            // }

        }
    }
}

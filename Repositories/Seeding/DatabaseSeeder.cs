using Bogus;
using Repositories.DbContexts;
using Entities.Models;
using System;
using System.Linq;
using Helpers.Consumer;

namespace Repositories.Seeding
{
    public static class DatabaseSeeder
    {
        public static void Seed(BleTrackingDbContext context)
        {

            // 1. MstBrand
            if (!context.MstBrands.Any(b => b.Status != 0))
            {
                var brandFaker = new Faker<MstBrand>()
                    .RuleFor(b => b.Id, f => Guid.NewGuid())
                    .RuleFor(b => b.Name, f => f.Company.CompanyName())
                    .RuleFor(b => b.Tag, f => f.Commerce.Product())
                    .RuleFor(b => b.Status, f => 1);

                var brands = brandFaker.Generate(3);
                context.MstBrands.AddRange(brands);
                context.SaveChanges();
            }

            // 2. MstApplication
            if (!context.MstApplications.Any(a => a.ApplicationStatus != 0))
            {
                var appFaker = new Faker<MstApplication>()
                    .RuleFor(a => a.Id, f => Guid.NewGuid())
                    .RuleFor(a => a.ApplicationName, f => f.Company.CompanyName() + " App")
                    .RuleFor(a => a.OrganizationType, f => f.PickRandom<OrganizationType>())
                    .RuleFor(a => a.OrganizationAddress, f => f.Address.FullAddress())
                    .RuleFor(a => a.ApplicationType, f => f.PickRandom<ApplicationType>())
                    .RuleFor(a => a.ApplicationRegistered, f => f.Date.Past(2))
                    .RuleFor(a => a.ApplicationExpired, f => f.Date.Future(2))
                    .RuleFor(a => a.HostName, f => f.Internet.DomainName())
                    .RuleFor(a => a.HostPhone, f => f.Phone.PhoneNumber())
                    .RuleFor(a => a.HostEmail, f => f.Internet.Email())
                    .RuleFor(a => a.HostAddress, f => f.Address.FullAddress())
                    .RuleFor(a => a.ApplicationCustomName, f => f.Commerce.ProductName())
                    .RuleFor(a => a.ApplicationCustomDomain, f => f.Internet.DomainName())
                    .RuleFor(a => a.ApplicationCustomPort, f => f.Random.Number(1000, 9999).ToString())
                    .RuleFor(a => a.LicenseCode, f => f.Random.AlphaNumeric(10))
                    .RuleFor(a => a.LicenseType, f => f.PickRandom<LicenseType>())
                    .RuleFor(a => a.ApplicationStatus, f => 1);

                var applications = appFaker.Generate(2);
                context.MstApplications.AddRange(applications);
                context.SaveChanges();
            }

            //UserGroup (3 role: System, Primary, UserCreated)
            if (!context.UserGroups.Any(ug => ug.Status != 0))
            {
                var applicationId = context.MstApplications
                    .Where(a => a.ApplicationStatus != 0)
                    .OrderBy(r => Guid.NewGuid())
                    .First()
                    .Id;

                var groups = new[]
                {
                    new UserGroup
                    {
                        Id = Guid.NewGuid(),
                        Name = "System Intial",
                        LevelPriority = LevelPriority.System,
                        ApplicationId = applicationId,
                        CreatedBy = "System",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedBy = "System",
                        UpdatedAt = DateTime.UtcNow,
                        Status = 1
                    },
                    new UserGroup
                    {
                        Id = Guid.NewGuid(),
                        Name = "Primary Admin Initial",
                        LevelPriority = LevelPriority.PrimaryAdmin,
                        ApplicationId = applicationId,
                        CreatedBy = "System",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedBy = "System",
                        UpdatedAt = DateTime.UtcNow,
                        Status = 1
                    },
                       new UserGroup
                    {
                        Id = Guid.NewGuid(),
                        Name = "Primary Initial",
                        LevelPriority = LevelPriority.Primary,
                        ApplicationId = applicationId,
                        CreatedBy = "System",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedBy = "System",
                        UpdatedAt = DateTime.UtcNow,
                        Status = 1
                    },
                    new UserGroup
                    {
                        Id = Guid.NewGuid(),
                        Name = "UserCreated Intial",
                        LevelPriority = LevelPriority.UserCreated,
                        ApplicationId = applicationId,
                        CreatedBy = "System",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedBy = "System",
                        UpdatedAt = DateTime.UtcNow,
                        Status = 1
                    }
                };

                context.UserGroups.AddRange(groups);
                context.SaveChanges();
            }

            // User
            if (!context.Users.Any(u => u.StatusActive != StatusActive.NonActive))
            {
                var userFaker = new Faker<User>()
                    .RuleFor(u => u.Id, f => Guid.NewGuid())
                    .RuleFor(u => u.Username, f => f.Internet.UserName())
                    .RuleFor(u => u.Password, f => BCrypt.Net.BCrypt.HashPassword("P@ssw0rd"))
                    .RuleFor(u => u.IsCreatedPassword, f => 1)
                    .RuleFor(u => u.Email, f => f.Internet.Email())
                    .RuleFor(u => u.IsEmailConfirmation, f => 1)
                    .RuleFor(u => u.EmailConfirmationCode, f => f.Random.AlphaNumeric(8))
                    .RuleFor(u => u.EmailConfirmationExpiredAt, f => DateTime.UtcNow.AddDays(1))
                    .RuleFor(u => u.EmailConfirmationAt, f => DateTime.UtcNow)
                    .RuleFor(u => u.LastLoginAt, f => DateTime.MinValue)
                    .RuleFor(u => u.StatusActive, f => StatusActive.Active)
                    .RuleFor(u => u.GroupId, f => context.UserGroups
                        .Where(ug => ug.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id);


                var users = userFaker.Generate(2);


                var superadminGroup = context.UserGroups
                    .FirstOrDefault(ug => ug.LevelPriority == LevelPriority.System && ug.Status != 0);
                if (superadminGroup != null)
                {
                    var superadmin = new User
                    {
                        Id = Guid.NewGuid(),
                        Username = "systemadmin",
                        Password = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd"),
                        IsCreatedPassword = 1,
                        Email = "systemadmin@test.com",
                        IsEmailConfirmation = 1,
                        EmailConfirmationCode = "ABC123",
                        EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(1),
                        EmailConfirmationAt = DateTime.UtcNow,
                        LastLoginAt = DateTime.MinValue,
                        StatusActive = StatusActive.Active,
                        GroupId = superadminGroup.Id
                    };
                    users.Add(superadmin);
                }

                context.Users.AddRange(users);
                context.SaveChanges();
            }

            // 3. MstBuilding
            if (!context.MstBuildings.Any(b => b.Status != 0))
            {
                var buildingFaker = new Faker<MstBuilding>()
                    .RuleFor(b => b.Id, f => Guid.NewGuid())
                    .RuleFor(b => b.Name, f => f.Address.City() + " Building")
                    .RuleFor(b => b.Image, f => $"https://example.com/buildings/{f.Random.Word()}.jpg")
                    .RuleFor(b => b.ApplicationId, f => context.MstApplications
                        .Where(a => a.ApplicationStatus != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(b => b.CreatedBy, f => "System")
                    .RuleFor(b => b.CreatedAt, f => DateTime.UtcNow)
                    .RuleFor(b => b.UpdatedBy, f => "System")
                    .RuleFor(b => b.UpdatedAt, f => DateTime.UtcNow)
                    .RuleFor(b => b.Status, f => 1);

                var buildings = buildingFaker.Generate(2);
                context.MstBuildings.AddRange(buildings);
                context.SaveChanges();
            }

            // 4. MstFloor 
            if (!context.MstFloors.Any(f => f.Status != 0))
            {
                var floorFaker = new Faker<MstFloor>()
                    .RuleFor(f => f.Id, f => Guid.NewGuid())
                    .RuleFor(f => f.BuildingId, f => context.MstBuildings
                        .Where(b => b.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(f => f.Name, f => f.Address.StreetName())
                    .RuleFor(f => f.FloorImage, f => $"https://example.com/floorplans/{f.Random.Word()}.png")
                    .RuleFor(f => f.PixelX, f => f.Random.Float(1280, 1920))
                    .RuleFor(f => f.PixelY, f => f.Random.Float(720, 1080))
                    .RuleFor(f => f.FloorX, f => f.Random.Float(20, 100))
                    .RuleFor(f => f.FloorY, f => f.Random.Float(20, 100))
                    .RuleFor(f => f.MeterPerPx, f => f.Random.Float())
                    .RuleFor(f => f.EngineFloorId, f => f.Random.Long(10000, 99999))
                    .RuleFor(f => f.CreatedBy, f => "System")
                    .RuleFor(f => f.CreatedAt, f => DateTime.UtcNow)
                    .RuleFor(f => f.UpdatedBy, f => "System")
                    .RuleFor(f => f.UpdatedAt, f => DateTime.UtcNow)
                    .RuleFor(f => f.Status, f => 1);

                var floors = floorFaker.Generate(2);
                context.MstFloors.AddRange(floors);
                context.SaveChanges();
            }

            // 5. MstFloorplan (bergantung pada MstFloor dan MstApplication)
            if (!context.MstFloorplans.Any(f => f.Status != 0))
            {
                var floorplanFaker = new Faker<MstFloorplan>()
                    .RuleFor(f => f.Id, f => Guid.NewGuid())
                    .RuleFor(f => f.Name, f => f.Address.StreetName() + " Floorplan")
                    .RuleFor(f => f.FloorId, f => context.MstFloors
                        .Where(fl => fl.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(f => f.ApplicationId, f => context.MstApplications
                        .Where(a => a.ApplicationStatus != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(f => f.CreatedBy, f => "System")
                    .RuleFor(f => f.CreatedAt, f => DateTime.UtcNow)
                    .RuleFor(f => f.UpdatedBy, f => "System")
                    .RuleFor(f => f.UpdatedAt, f => DateTime.UtcNow)
                    .RuleFor(f => f.Status, f => 1);

                var floorplans = floorplanFaker.Generate(2);
                context.MstFloorplans.AddRange(floorplans);
                context.SaveChanges();
            }

            // 6. MstIntegration
            if (!context.MstIntegrations.Any(i => i.Status != 0))
            {
                var intFaker = new Faker<MstIntegration>()
                    .RuleFor(i => i.Id, f => Guid.NewGuid())
                    .RuleFor(i => i.BrandId, f => context.MstBrands
                        .Where(b => b.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(i => i.IntegrationType, f => f.PickRandom<IntegrationType>())
                    .RuleFor(i => i.ApiTypeAuth, f => f.PickRandom<ApiTypeAuth>())
                    .RuleFor(i => i.ApiUrl, f => f.Internet.Url())
                    .RuleFor(i => i.ApiAuthUsername, f => f.Internet.UserName())
                    .RuleFor(i => i.ApiAuthPasswd, f => f.Internet.Password())
                    .RuleFor(i => i.ApiKeyField, f => "Key" + f.Random.Word())
                    .RuleFor(i => i.ApiKeyValue, f => f.Random.AlphaNumeric(20))
                    .RuleFor(i => i.ApplicationId, f => context.MstApplications
                        .Where(a => a.ApplicationStatus != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(i => i.CreatedBy, f => "System")
                    .RuleFor(i => i.CreatedAt, f => DateTime.UtcNow)
                    .RuleFor(i => i.UpdatedBy, f => "System")
                    .RuleFor(i => i.UpdatedAt, f => DateTime.UtcNow)
                    .RuleFor(i => i.Status, f => 1);

                var integrations = intFaker.Generate(2);
                context.MstIntegrations.AddRange(integrations);
                context.SaveChanges();
            }

            // 7. MstOrganization
            if (!context.MstOrganizations.Any(o => o.Status != 0))
            {
                var orgFaker = new Faker<MstOrganization>()
                    .RuleFor(o => o.Id, f => Guid.NewGuid())
                    .RuleFor(o => o.Code, f => "ORG" + f.Random.Number(100, 999))
                    .RuleFor(o => o.Name, f => f.Company.CompanyName())
                    .RuleFor(o => o.OrganizationHost, f => f.Internet.DomainName())
                    .RuleFor(o => o.ApplicationId, f => context.MstApplications
                        .Where(a => a.ApplicationStatus != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(o => o.CreatedBy, f => "System")
                    .RuleFor(o => o.CreatedAt, f => DateTime.UtcNow)
                    .RuleFor(o => o.UpdatedBy, f => "System")
                    .RuleFor(o => o.UpdatedAt, f => DateTime.UtcNow)
                    .RuleFor(o => o.Status, f => 1);

                var orgs = orgFaker.Generate(3);
                context.MstOrganizations.AddRange(orgs);
                context.SaveChanges();
            }

            // 8. MstDepartment
            if (!context.MstDepartments.Any(d => d.Status != 0))
            {
                var deptFaker = new Faker<MstDepartment>()
                    .RuleFor(d => d.Id, f => Guid.NewGuid())
                    .RuleFor(d => d.Code, f => "DEPT" + f.Random.Number(100, 999))
                    .RuleFor(d => d.Name, f => f.Commerce.Department())
                    .RuleFor(d => d.DepartmentHost, f => f.Internet.DomainName())
                    .RuleFor(d => d.ApplicationId, f => context.MstApplications
                        .Where(a => a.ApplicationStatus != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(d => d.CreatedBy, f => "System")
                    .RuleFor(d => d.CreatedAt, f => DateTime.UtcNow)
                    .RuleFor(d => d.UpdatedBy, f => "System")
                    .RuleFor(d => d.UpdatedAt, f => DateTime.UtcNow)
                    .RuleFor(d => d.Status, f => 1);

                var depts = deptFaker.Generate(4);
                context.MstDepartments.AddRange(depts);
                context.SaveChanges();
            }

            // 9. MstDistrict
            if (!context.MstDistricts.Any(d => d.Status != 0))
            {
                var distFaker = new Faker<MstDistrict>()
                    .RuleFor(d => d.Id, f => Guid.NewGuid())
                    .RuleFor(d => d.Code, f => "DIST" + f.Random.Number(100, 999))
                    .RuleFor(d => d.Name, f => f.Address.City())
                    .RuleFor(d => d.DistrictHost, f => f.Internet.DomainName())
                    .RuleFor(d => d.ApplicationId, f => context.MstApplications
                        .Where(a => a.ApplicationStatus != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(d => d.CreatedBy, f => "System")
                    .RuleFor(d => d.CreatedAt, f => DateTime.UtcNow)
                    .RuleFor(d => d.UpdatedBy, f => "System")
                    .RuleFor(d => d.UpdatedAt, f => DateTime.UtcNow)
                    .RuleFor(d => d.Status, f => 1);

                var districts = distFaker.Generate(4);
                context.MstDistricts.AddRange(districts);
                context.SaveChanges();
            }

            // 10. MstBleReader
            if (!context.MstBleReaders.Any(r => r.Status != 0))
            {
                var readerFaker = new Faker<MstBleReader>()
                    .RuleFor(r => r.Id, f => Guid.NewGuid())
                    .RuleFor(r => r.BrandId, f => context.MstBrands
                        .Where(b => b.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(r => r.Name, f => "Reader " + f.Random.Word())
                    // .RuleFor(r => r.Mac, f => f.Internet.Mac())
                    .RuleFor(r => r.Ip, f => f.Internet.Ip())
                    .RuleFor(r => r.Gmac, f => f.Random.String2(8, 8, "0123456789ABCDEF"))
                    // .RuleFor(r => r.LocationX, f => f.Random.Decimal(0, 100))
                    // .RuleFor(r => r.LocationY, f => f.Random.Decimal(0, 100))
                    // .RuleFor(r => r.LocationPxX, f => f.Random.Long(0, 1920))
                    // .RuleFor(r => r.LocationPxY, f => f.Random.Long(0, 1080))
                    .RuleFor(r => r.EngineReaderId, f => "RDR" + f.Random.Number(1000, 9999))
                    .RuleFor(r => r.CreatedBy, f => "System")
                    .RuleFor(r => r.CreatedAt, f => DateTime.UtcNow)
                    .RuleFor(r => r.UpdatedBy, f => "System")
                    .RuleFor(r => r.UpdatedAt, f => DateTime.UtcNow)
                    .RuleFor(r => r.Status, f => 1);

                var readers = readerFaker.Generate(2);
                context.MstBleReaders.AddRange(readers);
                context.SaveChanges();
            }

            // 11. FloorplanMaskedArea
            if (!context.FloorplanMaskedAreas.Any(a => a.Status != 0))
            {
                var areaFaker = new Faker<FloorplanMaskedArea>()
                    .RuleFor(a => a.Id, f => Guid.NewGuid())
                    .RuleFor(a => a.FloorplanId, f => context.MstFloorplans
                        .Where(fp => fp.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(a => a.FloorId, f => context.MstFloors
                        .Where(fl => fl.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(a => a.Name, f => f.Address.City() + " Area")
                    .RuleFor(a => a.AreaShape, f => f.PickRandom(new[]
                                {
                                    "[{\"id\":\"192\",\"x\":135.9945330296128,\"y\":186,\"x_px\":100,\"y_px\":50},{\"id\":\"193\",\"x\":284.898776681009,\"y\":134.88888888888889,\"x_px\":350,\"y_px\":50},{\"id\":\"194\",\"x\":386.00212604403947,\"y\":239.33333333333331,\"x_px\":400,\"y_px\":100},{\"id\":\"206\",\"x\":475.87217416443883,\"y\":451.09027438693573,\"x_px\":257.01652580537365,\"y_px\":267.7569410536024},{\"id\":\"196\",\"x\":135.9945330296128,\"y\":386,\"x_px\":100,\"y_px\":250}]",
                                    "[{\"id\":\"174\",\"x\":500,\"y\":50,\"x_px\":500,\"y_px\":50},{\"id\":\"175\",\"x\":550,\"y\":50,\"x_px\":550,\"y_px\":50},{\"id\":\"176\",\"x\":550,\"y\":200,\"x_px\":550,\"y_px\":200},{\"id\":\"177\",\"x\":525,\"y\":350,\"x_px\":525,\"y_px\":350},{\"id\":\"178\",\"x\":500,\"y\":200,\"x_px\":500,\"y_px\":200}]",
                                    "[{\"id\":\"170\",\"x\":150,\"y\":450,\"x_px\":150,\"y_px\":450},{\"id\":\"171\",\"x\":250,\"y\":450,\"x_px\":250,\"y_px\":450},{\"id\":\"172\",\"x\":250,\"y\":550,\"x_px\":250,\"y_px\":550},{\"id\":\"173\",\"x\":150,\"y\":550,\"x_px\":150,\"y_px\":550}]",
                                    "[{\"id\":\"165\",\"x\":100,\"y\":50,\"x_px\":100,\"y_px\":50},{\"id\":\"166\",\"x\":350,\"y\":50,\"x_px\":350,\"y_px\":50},{\"id\":\"167\",\"x\":400,\"y\":100,\"x_px\":400,\"y_px\":100},{\"id\":\"168\",\"x\":350,\"y\":250,\"x_px\":350,\"y_px\":250},{\"id\":\"169\",\"x\":100,\"y\":250,\"x_px\":100,\"y_px\":250}]"
                                }))
                    .RuleFor(a => a.ColorArea, f => f.Internet.Color())
                    .RuleFor(a => a.RestrictedStatus, f => f.PickRandom<RestrictedStatus>())
                    .RuleFor(a => a.EngineAreaId, f => "ENG" + f.Random.Number(100, 999))
                    // .RuleFor(a => a.WideArea, f => f.Random.Long(50, 200))
                    // .RuleFor(a => a.PositionPxX, f => f.Random.Long(0, 1920))
                    // .RuleFor(a => a.PositionPxY, f => f.Random.Long(0, 1080))
                    .RuleFor(a => a.CreatedBy, f => "System")
                    .RuleFor(a => a.CreatedAt, f => DateTime.UtcNow)
                    .RuleFor(a => a.UpdatedBy, f => "System")
                    .RuleFor(a => a.UpdatedAt, f => DateTime.UtcNow)
                    .RuleFor(a => a.Status, f => 1);

                var areas = areaFaker.Generate(2);
                context.FloorplanMaskedAreas.AddRange(areas);
                context.SaveChanges();
            }

            // 12. MstAccessCctv
            if (!context.MstAccessCctvs.Any(c => c.Status != 0))
            {
                var cctvFaker = new Faker<MstAccessCctv>()
                    .RuleFor(c => c.Id, f => Guid.NewGuid())
                    .RuleFor(c => c.Name, f => "CCTV " + f.Random.Word())
                    .RuleFor(c => c.Rtsp, f => $"rtsp://{f.Internet.Ip()}/live")
                    .RuleFor(c => c.IntegrationId, f => context.MstIntegrations
                        .Where(i => i.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(c => c.ApplicationId, f => context.MstApplications
                        .Where(a => a.ApplicationStatus != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(c => c.CreatedBy, f => "System")
                    .RuleFor(c => c.CreatedAt, f => DateTime.UtcNow)
                    .RuleFor(c => c.UpdatedBy, f => "System")
                    .RuleFor(c => c.UpdatedAt, f => DateTime.UtcNow)
                    .RuleFor(c => c.Status, f => 1);

                var cctvs = cctvFaker.Generate(2);
                context.MstAccessCctvs.AddRange(cctvs);
                context.SaveChanges();
            }

            // 13. MstAccessControl
            if (!context.MstAccessControls.Any(c => c.Status != 0))
            {
                var ctrlFaker = new Faker<MstAccessControl>()
                    .RuleFor(c => c.Id, f => Guid.NewGuid())
                    .RuleFor(c => c.ControllerBrandId, f => context.MstBrands
                        .Where(b => b.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(c => c.Name, f => "Control " + f.Random.Word())
                    .RuleFor(c => c.Type, f => f.PickRandom("Door", "Gate"))
                    .RuleFor(c => c.Description, f => f.Lorem.Sentence())
                    .RuleFor(c => c.Channel, f => "CH" + f.Random.Number(1, 10))
                    .RuleFor(c => c.DoorId, f => "DOOR" + f.Random.Number(100, 999))
                    .RuleFor(c => c.Raw, f => f.Lorem.Paragraph())
                    .RuleFor(c => c.IntegrationId, f => context.MstIntegrations
                        .Where(i => i.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(c => c.ApplicationId, f => context.MstApplications
                        .Where(a => a.ApplicationStatus != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(c => c.CreatedBy, f => "System")
                    .RuleFor(c => c.CreatedAt, f => DateTime.UtcNow)
                    .RuleFor(c => c.UpdatedBy, f => "System")
                    .RuleFor(c => c.UpdatedAt, f => DateTime.UtcNow)
                    .RuleFor(c => c.Status, f => 1);

                var controls = ctrlFaker.Generate(2);
                context.MstAccessControls.AddRange(controls);
                context.SaveChanges();
            }

            // 14. MstMember
            if (!context.MstMembers.Any(m => m.Status != 0))
            {
                var memberFaker = new Faker<MstMember>()
                    .RuleFor(m => m.Id, f => Guid.NewGuid())
                    .RuleFor(m => m.PersonId, f => "EMP" + f.Random.Number(1000, 9999))
                    .RuleFor(m => m.OrganizationId, f => context.MstOrganizations
                        .Where(o => o.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(m => m.DepartmentId, f => context.MstDepartments
                        .Where(d => d.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(m => m.DistrictId, f => context.MstDistricts
                        .Where(d => d.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(m => m.IdentityId, f => "ID" + f.Random.Number(100, 999))
                    .RuleFor(m => m.CardNumber, f => "CARD" + f.Random.Number(1000, 9999))
                    .RuleFor(m => m.BleCardNumber, f => "BLE" + f.Random.Number(100, 999))
                    .RuleFor(m => m.Name, f => f.Name.FullName())
                    .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber())
                    .RuleFor(m => m.Email, f => f.Internet.Email())
                    .RuleFor(m => m.Gender, f => f.PickRandom<Gender>())
                    .RuleFor(m => m.Address, f => f.Address.FullAddress())
                    .RuleFor(m => m.FaceImage, f => $"https://example.com/faces/{f.Random.Word()}.jpg")
                    .RuleFor(m => m.UploadFr, f => f.Random.Int(0, 2))
                    .RuleFor(m => m.UploadFrError, f => f.Random.Bool() ? "" : "Upload failed")
                    .RuleFor(m => m.BirthDate, f => DateOnly.FromDateTime(f.Date.Past(yearsToGoBack: 30, refDate: DateTime.Today.AddYears(-18))))
                    .RuleFor(m => m.JoinDate, f => DateOnly.FromDateTime(DateTime.Today))
                    .RuleFor(m => m.ExitDate, f => DateOnly.MaxValue)
                    .RuleFor(m => m.HeadMember1, f => f.Name.FullName())
                    .RuleFor(m => m.HeadMember2, f => f.Name.FullName())
                    .RuleFor(m => m.ApplicationId, f => context.MstApplications
                        .Where(a => a.ApplicationStatus != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(m => m.StatusEmployee, f => f.PickRandom<StatusEmployee>())
                    .RuleFor(m => m.CreatedBy, f => "System")
                    .RuleFor(m => m.CreatedAt, f => DateTime.UtcNow)
                    .RuleFor(m => m.UpdatedBy, f => "System")
                    .RuleFor(m => m.UpdatedAt, f => DateTime.UtcNow)
                    .RuleFor(m => m.Status, f => 1);

                var members = memberFaker.Generate(5);
                context.MstMembers.AddRange(members);
                context.SaveChanges();
            }

            // 15. Visitor
            if (!context.Visitors.Any())
            {
                var visitorFaker = new Faker<Visitor>()
                    .RuleFor(v => v.Id, f => Guid.NewGuid())
                    .RuleFor(v => v.PersonId, f => "VIS" + f.Random.Number(1000, 9999))
                    .RuleFor(v => v.IdentityId, f => "VID" + f.Random.Number(100, 999))
                    .RuleFor(v => v.CardNumber, f => "VCARD" + f.Random.Number(1000, 9999))
                    .RuleFor(v => v.BleCardNumber, f => "VBLE" + f.Random.Number(100, 999))
                    .RuleFor(v => v.VisitorType, f => f.PickRandom<VisitorType>())
                    .RuleFor(v => v.Name, f => f.Name.FullName())
                    .RuleFor(v => v.Phone, f => f.Phone.PhoneNumber())
                    .RuleFor(v => v.Email, f => f.Internet.Email())
                    .RuleFor(v => v.Gender, f => f.PickRandom<Gender>())
                    .RuleFor(v => v.Address, f => f.Address.FullAddress())
                    .RuleFor(v => v.OrganizationId, f => context.MstOrganizations
                        .Where(o => o.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(v => v.DistrictId, f => context.MstDistricts
                        .Where(d => d.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(v => v.DepartmentId, f => context.MstDepartments
                        .Where(d => d.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(v => v.IsVip, f => f.Random.Bool())
                    .RuleFor(v => v.IsEmailVerified, f => f.Random.Bool())
                    .RuleFor(v => v.EmailVerficationSendAt, f => f.Date.Recent(1))
                    .RuleFor(v => v.EmailVerificationToken, f => f.Random.AlphaNumeric(10))
                    .RuleFor(v => v.VisitorPeriodStart, f => f.Date.Recent(1))
                    .RuleFor(v => v.VisitorPeriodEnd, f => f.Date.Recent(1))
                    .RuleFor(v => v.IsEmployee, f => f.Random.Bool())
                    .RuleFor(v => v.Status, f => 1)
                    .RuleFor(v => v.FaceImage, f => $"https://example.com/faces/{f.Random.Word()}.jpg")
                    .RuleFor(v => v.UploadFr, f => f.Random.Int(0, 2))
                    .RuleFor(v => v.UploadFrError, f => f.Random.Bool() ? "" : "Upload failed")
                    .RuleFor(v => v.ApplicationId, f => context.MstApplications
                        .Where(a => a.ApplicationStatus != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id);

                var visitors = visitorFaker.Generate(2);
                context.Visitors.AddRange(visitors);
                context.SaveChanges();
            }

            // 16. TrackingTransaction
            if (!context.TrackingTransactions.Any())
            {
                var transFaker = new Faker<TrackingTransaction>()
                    .RuleFor(t => t.Id, f => Guid.NewGuid())
                    .RuleFor(t => t.TransTime, f => f.Date.Recent(1))
                    .RuleFor(t => t.ReaderId, f => context.MstBleReaders
                        .Where(r => r.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(t => t.CardId, f => f.Random.Long(1000, 9999))
                    .RuleFor(t => t.FloorplanMaskedAreaId, f => context.FloorplanMaskedAreas
                        .Where(a => a.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(t => t.CoordinateX, f => f.Random.Float(0, 100))
                    .RuleFor(t => t.CoordinateY, f => f.Random.Float(0, 100))
                    .RuleFor(t => t.CoordinatePxX, f => f.Random.Float(0, 1920))
                    .RuleFor(t => t.CoordinatePxY, f => f.Random.Float(0, 1080))
                    .RuleFor(t => t.AlarmStatus, f => f.PickRandom<AlarmStatus>())
                    .RuleFor(t => t.Battery, f => f.Random.Long(0, 100));

                var transactions = transFaker.Generate(20);
                context.TrackingTransactions.AddRange(transactions);
                context.SaveChanges();
            }

            // 17. VisitorBlacklistArea
            if (!context.VisitorBlacklistAreas.Any())
            {
                var blacklistFaker = new Faker<VisitorBlacklistArea>()
                    .RuleFor(v => v.Id, f => Guid.NewGuid())
                    .RuleFor(v => v.FloorplanMaskedAreaId, f => context.FloorplanMaskedAreas
                        .Where(a => a.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(v => v.VisitorId, f => context.Visitors
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id);

                var blacklists = blacklistFaker.Generate(2);
                context.VisitorBlacklistAreas.AddRange(blacklists);
                context.SaveChanges();
            }

            // 18. AlarmRecordTracking
            if (!context.AlarmRecordTrackings.Any())
            {
                var alarmFaker = new Faker<AlarmRecordTracking>()
                    .RuleFor(a => a.Id, f => Guid.NewGuid())
                    .RuleFor(a => a.Timestamp, f => f.Date.Recent(1))
                    .RuleFor(a => a.VisitorId, f => context.Visitors
                        .Where(r => r.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(a => a.ReaderId, f => context.MstBleReaders
                        .Where(r => r.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(a => a.FloorplanMaskedAreaId, f => context.FloorplanMaskedAreas
                        .Where(a => a.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(a => a.Alarm, f => f.PickRandom<AlarmRecordStatus>())
                    .RuleFor(a => a.Action, f => f.PickRandom<ActionStatus>())
                    .RuleFor(a => a.ApplicationId, f => context.MstApplications
                        .Where(a => a.ApplicationStatus != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(a => a.IdleTimestamp, f => f.Date.Recent(1))
                    .RuleFor(a => a.DoneTimestamp, f => f.Date.Recent(1))
                    .RuleFor(a => a.CancelTimestamp, f => f.Date.Recent(1))
                    .RuleFor(a => a.WaitingTimestamp, f => f.Date.Recent(1))
                    .RuleFor(a => a.InvestigatedTimestamp, f => f.Date.Recent(1))
                    .RuleFor(a => a.IdleBy, f => f.Name.FullName())
                    .RuleFor(a => a.DoneBy, f => f.Name.FullName())
                    .RuleFor(a => a.CancelBy, f => f.Name.FullName())
                    .RuleFor(a => a.WaitingBy, f => f.Name.FullName())
                    .RuleFor(a => a.InvestigatedBy, f => f.Name.FullName())
                    .RuleFor(a => a.InvestigatedResult, f => f.Lorem.Sentence())
                    .RuleFor(a => a.InvestigatedDoneAt, f => f.Date.Recent(1));

                var alarms = alarmFaker.Generate(1);
                context.AlarmRecordTrackings.AddRange(alarms);
                context.SaveChanges();
            }

            // 19. FloorplanDevice
            if (!context.FloorplanDevices.Any(d => d.Status != 0))
            {
                var deviceFaker = new Faker<FloorplanDevice>()
                    .RuleFor(d => d.Id, f => Guid.NewGuid())
                    .RuleFor(d => d.Name, f => "Device " + f.Random.Word())
                    .RuleFor(d => d.Type, f => f.PickRandom<DeviceType>())
                    .RuleFor(d => d.FloorplanId, f => context.MstFloorplans
                        .Where(fp => fp.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(d => d.AccessCctvId, f => context.MstAccessCctvs
                        .Where(c => c.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(d => d.ReaderId, f => context.MstBleReaders
                        .Where(r => r.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(d => d.AccessControlId, f => context.MstAccessControls
                        .Where(c => c.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(d => d.PosX, f => f.Random.Float(0, 1920))
                    .RuleFor(d => d.PosY, f => f.Random.Float(0, 1080))
                    .RuleFor(d => d.PosPxX, f => f.Random.Float(0, 100))
                    .RuleFor(d => d.PosPxY, f => f.Random.Float(0, 100))
                    .RuleFor(d => d.FloorplanMaskedAreaId, f => context.FloorplanMaskedAreas
                        .Where(a => a.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(d => d.ApplicationId, f => context.MstApplications
                        .Where(a => a.ApplicationStatus != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .First()
                        .Id)
                    .RuleFor(d => d.CreatedBy, f => "System")
                    .RuleFor(d => d.CreatedAt, f => DateTime.UtcNow)
                    .RuleFor(d => d.UpdatedBy, f => "System")
                    .RuleFor(d => d.UpdatedAt, f => DateTime.UtcNow)
                    .RuleFor(d => d.DeviceStatus, f => f.PickRandom<DeviceStatus>())
                    .RuleFor(d => d.Status, f => 1);

                var devices = deviceFaker.Generate(2);
                context.FloorplanDevices.AddRange(devices);
                context.SaveChanges();
            }
            // 20. BleReaderNode
            if (!context.BleReaderNodes.Any())
            {
                var blereadernodeFaker = new Faker<BleReaderNode>()
                    .RuleFor(v => v.Id, f => Guid.NewGuid())
                    .RuleFor(d => d.ReaderId, f => context.MstBleReaders
                        .Where(r => r.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .Select(r => r.Id)
                        .FirstOrDefault())
                    .RuleFor(d => d.StartPos, f => f.Random.Number(0, 99).ToString())
                    .RuleFor(d => d.EndPos, f => f.Random.Number(0, 99).ToString())
                    .RuleFor(d => d.DistancePx, f => f.Random.Float(0, 100))
                    .RuleFor(d => d.Distance, f => f.Random.Float(0, 100))
                    .RuleFor(b => b.ApplicationId, f => context.MstApplications
                        .Where(a => a.ApplicationStatus != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .Select(a => a.Id)
                        .FirstOrDefault())
                    .RuleFor(b => b.CreatedBy, f => "System")
                    .RuleFor(b => b.CreatedAt, f => DateTime.UtcNow)
                    .RuleFor(b => b.UpdatedBy, f => "System")
                    .RuleFor(b => b.UpdatedAt, f => DateTime.UtcNow);

                var blereadernodes = blereadernodeFaker.Generate(2);
                context.BleReaderNodes.AddRange(blereadernodes);
                context.SaveChanges();
            }

            // 21. MstEngine
            if (!context.MstEngines.Any())
            {
                var engineFaker = new Faker<MstEngine>()
                    .RuleFor(e => e.Id, f => Guid.NewGuid())
                    .RuleFor(e => e.Name, f => "Engine " + f.Random.Word())
                    .RuleFor(e => e.EngineId, f => "topic_" + Guid.NewGuid().ToString())
                    .RuleFor(e => e.Port, f => f.Random.Number(1000, 9999))
                    .RuleFor(e => e.Status, f => 1)
                    .RuleFor(e => e.IsLive, f => 1)
                    .RuleFor(e => e.LastLive, f => f.Date.Recent())
                    .RuleFor(e => e.ServiceStatus, f => f.PickRandom<ServiceStatus>());

                var engines = engineFaker.Generate(3);
                context.MstEngines.AddRange(engines);
                context.SaveChanges();
            }

            // 22. VisitorCard
            if (!context.VisitorCards.Any())
            {
                var visitorcardFaker = new Faker<VisitorCard>()
                    .RuleFor(e => e.Id, f => Guid.NewGuid())
                    .RuleFor(e => e.Name, f => "Card " + f.Random.Number(1000, 9999))
                    .RuleFor(e => e.Number, f => f.Random.Number(1000, 9999).ToString())
                    .RuleFor(a => a.CardType, f => f.PickRandom<CardType>())
                    .RuleFor(e => e.QRCode, f => f.Random.Number(1000, 9999).ToString())
                    .RuleFor(e => e.Mac, f => f.Internet.Mac()).RuleFor(e => e.Status, f => 1)
                    .RuleFor(e => e.CheckinStatus, f => 1)
                    .RuleFor(e => e.EnableStatus, f => 1)
                    .RuleFor(e => e.Status, f => 1)
                    .RuleFor(e => e.SiteId, f => Guid.NewGuid())
                    .RuleFor(e => e.IsMember, f => f.Random.Number(0, 1))
                    .RuleFor(b => b.ApplicationId, f => context.MstApplications
                        .Where(a => a.ApplicationStatus != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .Select(a => a.Id)
                        .FirstOrDefault());

                var visitorcards = visitorcardFaker.Generate(2);
                context.VisitorCards.AddRange(visitorcards);
                context.SaveChanges();
            }

            // 23. CardRecord
            if (!context.CardRecords.Any())
            {
                var cardrecordFaker = new Faker<CardRecord>()
                    .RuleFor(e => e.Id, f => Guid.NewGuid())
                    .RuleFor(e => e.VisitorName, f => f.Name.FullName())
                    .RuleFor(e => e.CardId, f => context.VisitorCards
                        .Where(v => v.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .Select(v => v.Id)
                        .FirstOrDefault())
                    .RuleFor(e => e.VisitorId, f => context.Visitors
                        .Where(v => v.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .Select(v => v.Id)
                        .FirstOrDefault())
                    .RuleFor(e => e.MemberId, f => context.MstMembers
                        .Where(v => v.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .Select(v => v.Id)
                        .FirstOrDefault())
                    .RuleFor(v => v.Timestamp, f => f.Date.Recent(1))
                    .RuleFor(v => v.CheckinAt, f => f.Date.Recent(1))
                    .RuleFor(v => v.CheckoutAt, f => f.Date.Future(1))
                    .RuleFor(b => b.CheckinBy, f => "System")
                    .RuleFor(b => b.CheckoutBy, f => "System")
                    .RuleFor(e => e.CheckoutSiteId, f => Guid.NewGuid())
                    .RuleFor(e => e.CheckinSiteId, f => Guid.NewGuid())
                    .RuleFor(a => a.VisitorType, f => f.PickRandom<VisitorType>());

                var cardrecords = cardrecordFaker.Generate(2);
                context.CardRecords.AddRange(cardrecords);
                context.SaveChanges();
            }

            // 24. TrxVisitor
            if (!context.TrxVisitors.Any())
            {
                var trxVisitorFaker = new Faker<TrxVisitor>()
                    .RuleFor(e => e.Id, f => Guid.NewGuid())
                    .RuleFor(v => v.CheckedInAt, f => f.Date.Recent(1))
                    .RuleFor(v => v.CheckedOutAt, f => f.Date.Future(1))
                    .RuleFor(v => v.DenyAt, f => f.Date.Future(1))
                    .RuleFor(v => v.BlockAt, f => f.Date.Future(1))
                    .RuleFor(v => v.UnblockAt, f => f.Date.Future(1))
                    .RuleFor(b => b.CheckinBy, f => "System")
                    .RuleFor(b => b.CheckoutBy, f => "System")
                    .RuleFor(b => b.DenyBy, f => "System")
                    .RuleFor(b => b.DenyReason, f => f.PickRandom(new[] { "No reason", "Card is not valid", "Card is expired", "Card is blocked", "Card is denied" }))

                    .RuleFor(e => e.VisitorId, f => context.Visitors
                        .Where(v => v.Status != 0)
                        .OrderBy(r => Guid.NewGuid())
                        .Select(v => v.Id)
                        .FirstOrDefault())
                    .RuleFor(e => e.Status, f => f.PickRandom<VisitorStatus>())
                    .RuleFor(e => e.InvitationCreatedAt, f => f.Date.Recent(1))
                    .RuleFor(e => e.VisitorGroupCode, f => f.Random.Number(1, 1000000))
                    .RuleFor(e => e.VisitorNumber, f => f.Random.Number(1, 100).ToString())
                    .RuleFor(e => e.VisitorCode, f => f.Random.String(10))
                    .RuleFor(e => e.VehiclePlateNumber, f => f.Random.String(10))
                    .RuleFor(e => e.Remarks, f => f.Random.String(255))
                    .RuleFor(e => e.SiteId, f => Guid.NewGuid().ToString())
                    .RuleFor(e => e.ParkingId, f => Guid.NewGuid().ToString());

                var trxVisitors = trxVisitorFaker.Generate(2);
                context.TrxVisitors.AddRange(trxVisitors);
                context.SaveChanges();
            }
            
               // 25. Card
            if (!context.Cards.Any())
            {
                var cardFaker = new Faker<Card>()
                    .RuleFor(e => e.Id, f => Guid.NewGuid())
                    .RuleFor(e => e.Name, f => f.Random.Word())
                    .RuleFor(e => e.Remarks, f => f.Random.String(255))
                    .RuleFor(e => e.CardType, f => f.PickRandom<CardType>())
                    .RuleFor(e => e.CardNumber, f => f.Random.Number(1000, 9999).ToString())
                    .RuleFor(e => e.CardBarcode, f => f.Random.String(10))
                    .RuleFor(e => e.IsMultiSite, f => f.Random.Bool())
                    .RuleFor(e => e.RegisteredSite, f => f.Random.Bool() ? (Guid?)null : Guid.NewGuid())
                    .RuleFor(e => e.IsUsed, f => f.Random.Bool())
                    .RuleFor(e => e.LastUsed, f => f.Person.FullName)
                    .RuleFor(e => e.StatusCard, f => f.Random.Bool());

                var cards = cardFaker.Generate(2);
                context.Cards.AddRange(cards);
                context.SaveChanges();
            }

               // 22. MstLogEngine
            // if (!context.MstLogTrackings.Any())
            // {
            //     var logTrackingFaker = new Faker<MstLogTrackings>()
            //         .RuleFor(e => e.Id, f => Guid.NewGuid())
            //         .RuleFor(b => b.FloorplanId, f => context.MstFloorplans
            //             .Where(a => a.Status != 0)
            //             .OrderBy(r => Guid.NewGuid())
            //             .Select(a => a.Id)
            //             .FirstOrDefault())
            //         .RuleFor(d => d.BeaconId, f => "BC572905DB80")
            //         .RuleFor(d => d.FirstReaderId, f => "282C02227F1A")
            //         .RuleFor(d => d.SecondReaderId, f => "282C02227F53")
            //         .RuleFor(d => d.FirstDistance, f => 9)
            //         .RuleFor(d => d.SecondDistance, f => 8)
            //         .RuleFor(d => d.DistancePx, f => 31.62m)
            //         .RuleFor(d => d.Distance, f => 31.62m)
            //         .RuleFor(d => d.PointX, f => 48)
            //         .RuleFor(d => d.PointY, f => 141)
            //         .RuleFor(d => d.FirstReaderX, f => 40)
            //         .RuleFor(d => d.FirstReaderY, f => 120)
            //         .RuleFor(d => d.SecondReaderX, f => 50)
            //         .RuleFor(d => d.SecondReaderY, f => 150)
            //         .RuleFor(d => d.Timestamp, f => DateTime.Parse("2025-05-29 05:48:05,580"));

            //     var logTrackings = logTrackingFaker.Generate(3);
            //     context.MstLogTrackings.AddRange(logTrackings);
            //     context.SaveChanges();
            // }            
        }
    }
}
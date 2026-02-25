using Bogus;
using Repositories.DbContexts;
using Entities.Models;
using System;
using System.Linq;
using Bogus.DataSets;
using Shared.Contracts;


namespace Repositories.Seeding
{
    public static class DatabaseSeeder
    {
        public static void Seed(BleTrackingDbContext context)
        {
            // Seed Application
            if (!context.MstApplications.Any(a => a.ApplicationStatus != 0))
            {
                var app = new MstApplication
                {
                    Id = new Guid("c926d20b-a746-4492-9924-eb7eee76305c"),
                    ApplicationName = "BIO PEOPLE TRACKING",
                    OrganizationType = OrganizationType.Single,
                    ApplicationType = ApplicationType.Tracking,
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
                    new UserGroup { Id = Guid.NewGuid(), Name = "System", LevelPriority = LevelPriority.System, ApplicationId = appId, CreatedBy = "System", CreatedAt = DateTime.UtcNow, Status = 1 },
                    new UserGroup { Id = Guid.NewGuid(), Name = "Super Admin", LevelPriority = LevelPriority.SuperAdmin, ApplicationId = appId, CreatedBy = "System", CreatedAt = DateTime.UtcNow, Status = 1 }
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
                        Status = 1,
                        GroupId = superadminGroup.Id,
                        ApplicationId = appId,
                    };
                    context.Users.Add(superadmin);
                    context.SaveChanges();
                }
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
                        Name = "Card 1",
                        CardNumber = "CARD-001",
                        Dmac = "BC572905D5B9",
                        MemberId = memberId,
                        ApplicationId = appId,
                        StatusCard = 1
                    },
                    new Card
                    {
                        Id = Guid.NewGuid(),
                        Name = "Card 2",
                        CardNumber = "CARD-002",
                        Dmac = "BC57291F5FD0",
                        VisitorId = visitorId, // assign to visitor instead (to cover all 3 types)
                        ApplicationId = appId,
                        StatusCard = 1
                    },
                    new Card
                    {
                        Id = Guid.NewGuid(),
                        Name = "Card 3",
                        CardNumber = "CARD-003",
                        Dmac = "BC57291F5FC1",
                        MemberId = securityId, // Assuming Security entity uses MemberId or creates a separate mapping, but let's just make it unassigned or simulate security
                        // Actually, looking at Card it has VisitorId and MemberId. Sec is often treated as Member in this context or it has its own mapping. We'll map to memberId for now if no SecurityId in Card. 
                        ApplicationId = appId,
                        StatusCard = 1
                    }
                };
                context.Cards.AddRange(cards);
                context.SaveChanges();
            }         


             // // 14. MstSecurity
            if (!context.MstSecurities.Any(m => m.Status != 0))
            {
                var securityFaker = new Faker<MstSecurity>()
                    .RuleFor(m => m.Id, f => Guid.NewGuid())
                    .RuleFor(m => m.PersonId, f => "SEC" + f.Random.Number(1000, 9999))
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

                var securities = securityFaker.Generate(5);
                context.MstSecurities.AddRange(securities);
                context.SaveChanges();
            }

        }
    }
}
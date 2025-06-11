using Microsoft.EntityFrameworkCore;
using Entities.Models;
using Helpers.Consumer;

namespace Repositories.DbContexts
{
    public class BleTrackingDbContext : DbContext
    {
        public BleTrackingDbContext(DbContextOptions<BleTrackingDbContext> dbContextOptions) 
            : base(dbContextOptions) { }

        public DbSet<MstAccessCctv> MstAccessCctvs { get; set; }
        public DbSet<MstApplication> MstApplications { get; set; }
        public DbSet<MstIntegration> MstIntegrations { get; set; }
        public DbSet<MstAccessControl> MstAccessControls { get; set; }
        public DbSet<MstBrand> MstBrands { get; set; }
        public DbSet<MstOrganization> MstOrganizations { get; set; }
        public DbSet<MstDepartment> MstDepartments { get; set; }
        public DbSet<MstDistrict> MstDistricts { get; set; }
        public DbSet<MstMember> MstMembers { get; set; }
        public DbSet<MstFloor> MstFloors { get; set; }
        public DbSet<FloorplanMaskedArea> FloorplanMaskedAreas { get; set; }
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<VisitorBlacklistArea> VisitorBlacklistAreas { get; set; }
        public DbSet<MstBleReader> MstBleReaders { get; set; }
        public DbSet<TrackingTransaction> TrackingTransactions { get; set; }
        public DbSet<AlarmRecordTracking> AlarmRecordTrackings { get; set; }
        public DbSet<MstFloorplan> MstFloorplans { get; set; }
        public DbSet<MstBuilding> MstBuildings { get; set; }
        public DbSet<FloorplanDevice> FloorplanDevices { get; set; }
        public DbSet<BleReaderNode> BleReaderNodes { get; set; }
        public DbSet<MstEngine> MstEngines { get; set; }
        
        public DbSet<MstTrackingLog> MstTrackingLogs { get; set; }
        public DbSet<RecordTrackingLog> RecordTrackingLogs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MstApplication>().ToTable("mst_application");
            modelBuilder.Entity<MstIntegration>().ToTable("mst_integration");
            modelBuilder.Entity<MstAccessCctv>().ToTable("mst_access_cctv");
            modelBuilder.Entity<MstAccessControl>().ToTable("mst_access_control");
            modelBuilder.Entity<MstBrand>().ToTable("mst_brand");
            modelBuilder.Entity<MstOrganization>().ToTable("mst_organization");
            modelBuilder.Entity<MstDepartment>().ToTable("mst_department");
            modelBuilder.Entity<MstDistrict>().ToTable("mst_district");
            modelBuilder.Entity<MstMember>().ToTable("mst_member");
            modelBuilder.Entity<MstFloor>().ToTable("mst_floor");
            modelBuilder.Entity<FloorplanMaskedArea>().ToTable("floorplan_masked_area");
            modelBuilder.Entity<Visitor>().ToTable("visitor");
            modelBuilder.Entity<VisitorBlacklistArea>().ToTable("visitor_blacklist_area");
            modelBuilder.Entity<MstBleReader>().ToTable("mst_ble_reader");
            modelBuilder.Entity<TrackingTransaction>().ToTable("tracking_transaction");
            modelBuilder.Entity<AlarmRecordTracking>().ToTable("alarm_record_tracking");
            modelBuilder.Entity<MstFloorplan>().ToTable("mst_floorplan");
            modelBuilder.Entity<MstBuilding>().ToTable("mst_building");
            modelBuilder.Entity<BleReaderNode>().ToTable("ble_reader_node");
            modelBuilder.Entity<MstEngine>().ToTable("mst_engine");
            modelBuilder.Entity<MstTrackingLog>().ToTable("mst_tracking_log");
            modelBuilder.Entity<RecordTrackingLog>().ToTable("record_tracking_log");
            modelBuilder.Entity<User>().ToTable("user");
            modelBuilder.Entity<UserGroup>().ToTable("user_group");
            modelBuilder.Entity<RefreshToken>().ToTable("refresh_token");
            




            // MstApplication
            modelBuilder.Entity<MstApplication>(entity =>
                {
                    entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                    entity.Property(e => e.OrganizationType)
                        .HasColumnType("nvarchar(255)")
                        .IsRequired()
                        .HasDefaultValue(OrganizationType.Single);
                    // .HasConversion(
                    //     v => v.ToString().ToLower(), // Simpan ke DB sebagai "single"
                    //     v => (OrganizationType)Enum.Parse(typeof(OrganizationType), v, true)
                    // );
                    entity.Property(e => e.ApplicationType)
                        .HasColumnType("nvarchar(255)")
                        .IsRequired()
                        .HasDefaultValue(ApplicationType.Empty)
                        .HasConversion(
                            v => v == ApplicationType.Empty ? "" : v.ToString().ToLower(),
                            v => string.IsNullOrEmpty(v) ? ApplicationType.Empty : (ApplicationType)Enum.Parse(typeof(ApplicationType), v, true)
                        );
                    entity.Property(e => e.LicenseType)
                        .HasColumnType("nvarchar(255)")
                        .IsRequired()
                        .HasConversion(
                            v => v.ToString().ToLower(),
                            v => (LicenseType)Enum.Parse(typeof(LicenseType), v, true)
                        );

                    entity.Property(e => e.ApplicationStatus)
                        .IsRequired()
                        .HasDefaultValue(1);
                });

            modelBuilder.Entity<MstApplication>()
                .HasQueryFilter(m => m.ApplicationStatus != 0);
            modelBuilder.Entity<MstIntegration>()
                .HasQueryFilter(m => m.Status != 0);
            modelBuilder.Entity<MstAccessControl>()
                .HasQueryFilter(m => m.Status != 0);
            modelBuilder.Entity<MstAccessCctv>()
                .HasQueryFilter(m => m.Status != 0);
            modelBuilder.Entity<FloorplanMaskedArea>()
                .HasQueryFilter(m => m.Status != 0);
            modelBuilder.Entity<MstBleReader>()
                .HasQueryFilter(m => m.Status != 0);
            modelBuilder.Entity<MstBrand>()
                .HasQueryFilter(m => m.Status != 0);
            modelBuilder.Entity<MstDepartment>()
                .HasQueryFilter(m => m.Status != 0);
            modelBuilder.Entity<MstDistrict>()
                .HasQueryFilter(m => m.Status != 0);
            modelBuilder.Entity<MstFloor>()
                .HasQueryFilter(m => m.Status != 0);
            modelBuilder.Entity<MstMember>()
                .HasQueryFilter(m => m.Status != 0);
            modelBuilder.Entity<MstOrganization>()
                .HasQueryFilter(m => m.Status != 0);
            modelBuilder.Entity<MstFloorplan>()
                .HasQueryFilter(m => m.Status != 0);
            modelBuilder.Entity<MstEngine>()
                .HasQueryFilter(m => m.Status != 0);

            // MstIntegration
            modelBuilder.Entity<MstIntegration>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.BrandId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.IntegrationType)
                    .HasColumnType("nvarchar(255)")
                    .IsRequired()
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (IntegrationType)Enum.Parse(typeof(IntegrationType), v, true)
                    );
                entity.Property(e => e.ApiTypeAuth)
                    .HasColumnType("nvarchar(255)")
                    .IsRequired()
                    .HasConversion(
                        v => v == ApiTypeAuth.ApiKey ? "apikey" : v.ToString().ToLower(),
                        v => v == "apikey" ? ApiTypeAuth.ApiKey : (ApiTypeAuth)Enum.Parse(typeof(ApiTypeAuth), v, true)
                    );

                entity.HasOne(m => m.Application)
                    .WithMany(a => a.Integrations)
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Brand)
                    .WithMany()
                    .HasForeignKey(m => m.BrandId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(m => m.Status)
                    .IsRequired()
                    .HasDefaultValue(1);
            });

            // MstAccessCctv
            modelBuilder.Entity<MstAccessCctv>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.IntegrationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();

                entity.HasOne(m => m.Integration)
                    .WithMany()
                    .HasForeignKey(m => m.IntegrationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(m => m.Status)
                    .IsRequired()
                    .HasDefaultValue(1);
            });

            // MstAccessControl
            modelBuilder.Entity<MstAccessControl>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.IntegrationId).HasMaxLength(36).IsRequired();

                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Integration)
                    .WithMany()
                    .HasForeignKey(m => m.IntegrationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(m => m.Status)
                   .IsRequired()
                   .HasDefaultValue(1);
            });

            // MstBrand
            modelBuilder.Entity<MstBrand>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
            });

            // MstOrganization
            modelBuilder.Entity<MstOrganization>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();

                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(m => m.Status)
                    .IsRequired()
                    .HasDefaultValue(1);
            });

            // MstDepartment
            modelBuilder.Entity<MstDepartment>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();

                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(m => m.Status)
                   .IsRequired()
                   .HasDefaultValue(1);
            });

            // MstDistrict
            modelBuilder.Entity<MstDistrict>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();

                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(m => m.Status)
                    .IsRequired()
                    .HasDefaultValue(1);
            });

            // MstMember
            modelBuilder.Entity<MstMember>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.OrganizationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.DepartmentId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.DistrictId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.Gender)
                    .HasColumnType("nvarchar(255)")
                    .IsRequired()
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (Gender)Enum.Parse(typeof(Gender), v, true)
                    );
                entity.Property(e => e.StatusEmployee)
                    .HasColumnType("nvarchar(255)")
                    .IsRequired()
                    .HasConversion(
                        v => v == StatusEmployee.NonActive ? "non-active" : v.ToString().ToLower(),
                        v => v == "non-active" ? StatusEmployee.NonActive : (StatusEmployee)Enum.Parse(typeof(StatusEmployee), v, true)
                    );

                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Organization)
                    .WithMany()
                    .HasForeignKey(m => m.OrganizationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Department)
                    .WithMany()
                    .HasForeignKey(m => m.DepartmentId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.District)
                    .WithMany()
                    .HasForeignKey(m => m.DistrictId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(m => m.Status)
                    .IsRequired()
                    .HasDefaultValue(1);

                entity.HasIndex(m => m.PersonId);
                entity.HasIndex(m => m.Email);
            });

            // MstFloor
            modelBuilder.Entity<MstFloor>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.BuildingId).HasMaxLength(36);
                entity.Property(m => m.Status)
                    .IsRequired()
                    .HasDefaultValue(1);

                entity.HasOne(m => m.Building)
                    .WithMany()
                    .HasForeignKey(m => m.BuildingId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // FloorplanMaskedArea
            modelBuilder.Entity<FloorplanMaskedArea>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.FloorId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.RestrictedStatus)
                    .HasColumnType("nvarchar(255)")
                    .IsRequired()
                    .HasConversion(
                        v => v == RestrictedStatus.NonRestrict ? "non-restrict" : v.ToString().ToLower(),
                        v => v == "non-restrict" ? RestrictedStatus.NonRestrict : (RestrictedStatus)Enum.Parse(typeof(RestrictedStatus), v, true)
                    );

                entity.HasOne(m => m.Floor)
                    .WithMany()
                    .HasForeignKey(m => m.FloorId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(m => m.Status)
                    .IsRequired()
                    .HasDefaultValue(1);
            });

            // Visitor
            modelBuilder.Entity<Visitor>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.Gender)
                    .HasColumnType("nvarchar(255)")
                    .IsRequired()
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (Gender)Enum.Parse(typeof(Gender), v, true)
                    );
                entity.Property(e => e.Status)
                    .HasColumnType("nvarchar(255)")
                    .IsRequired()
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (VisitorStatus)Enum.Parse(typeof(VisitorStatus), v, true)
                    );

                entity.HasOne(v => v.Application)
                    .WithMany()
                    .HasForeignKey(v => v.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(v => v.PersonId);
                entity.HasIndex(v => v.Email);
            });

            // VisitorBlacklistArea
            modelBuilder.Entity<VisitorBlacklistArea>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.FloorplanMaskedAreaId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.VisitorId).HasMaxLength(36).IsRequired();

                entity.HasOne(v => v.FloorplanMaskedArea)
                    .WithMany()
                    .HasForeignKey(v => v.FloorplanMaskedAreaId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(v => v.Visitor)
                    .WithMany()
                    .HasForeignKey(v => v.VisitorId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // MstBleReader
            modelBuilder.Entity<MstBleReader>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.BrandId).HasMaxLength(36).IsRequired();

                entity.HasOne(m => m.Brand)
                    .WithMany()
                    .HasForeignKey(m => m.BrandId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(m => m.Status)
                    .IsRequired()
                    .HasDefaultValue(1);

            });

            // TrackingTransaction
            modelBuilder.Entity<TrackingTransaction>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ReaderId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.FloorplanMaskedAreaId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.AlarmStatus)
                    .HasColumnType("nvarchar(255)")
                    .IsRequired()
                    .HasConversion(
                        v => v == AlarmStatus.NonActive ? "non-active" : v.ToString().ToLower(),
                        v => v == "non-active" ? AlarmStatus.NonActive : (AlarmStatus)Enum.Parse(typeof(AlarmStatus), v, true)
                    );

                entity.HasOne(t => t.Reader)
                    .WithMany()
                    .HasForeignKey(t => t.ReaderId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.FloorplanMaskedArea)
                    .WithMany()
                    .HasForeignKey(t => t.FloorplanMaskedAreaId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            //AlarmRecordTracking
            modelBuilder.Entity<AlarmRecordTracking>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.VisitorId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ReaderId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.FloorplanMaskedAreaId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();

                entity.Property(e => e.Alarm)
                    .HasColumnName("alarm_record_status")
                    .HasColumnType("nvarchar(255)")
                    .IsRequired()
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (AlarmRecordStatus)Enum.Parse(typeof(AlarmRecordStatus), v, true)
                    );

                entity.Property(e => e.Action)
                    .HasColumnType("nvarchar(255)")
                    .IsRequired()
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (ActionStatus)Enum.Parse(typeof(ActionStatus), v, true)
                    );

                // Relasi one-to-one dengan Visitor
                entity.HasOne(a => a.Visitor)
                    .WithOne(v => v.AlarmRecordTracking)
                    .HasForeignKey<AlarmRecordTracking>(a => a.VisitorId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Relasi one-to-one dengan MstBleReader
                entity.HasOne(a => a.Reader)
                    .WithOne(r => r.AlarmRecordTracking)
                    .HasForeignKey<AlarmRecordTracking>(a => a.ReaderId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Relasi one-to-one dengan FloorplanMaskedArea
                entity.HasOne(a => a.FloorplanMaskedArea)
                    .WithOne(f => f.AlarmRecordTracking)
                    .HasForeignKey<AlarmRecordTracking>(a => a.FloorplanMaskedAreaId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Relasi one-to-one dengan MstApplication
                entity.HasOne(a => a.Application)
                    .WithOne(m => m.AlarmRecordTracking)
                    .HasForeignKey<AlarmRecordTracking>(a => a.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(a => a.Generate)
                    .IsUnique()
                    .HasDatabaseName("alarm_record_tracking__generate_unique");
            });

            // FloorplanDevice
            modelBuilder.Entity<FloorplanDevice>(entity =>
            {
                entity.ToTable("floorplan_device");
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Type).HasConversion(v => v.ToString().ToLower(), v => (DeviceType)Enum.Parse(typeof(DeviceType), v, true));
                entity.Property(e => e.FloorplanId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.AccessCctvId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ReaderId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.AccessControlId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.FloorplanMaskedAreaId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.CreatedBy).HasMaxLength(255).IsRequired();
                entity.Property(e => e.UpdatedBy).HasMaxLength(255).IsRequired();
                entity.Property(e => e.DeviceStatus).HasConversion(v => v.ToString().ToLower(), v => (DeviceStatus)Enum.Parse(typeof(DeviceStatus), v, true));
                entity.Property(e => e.Status).IsRequired();

                entity.HasOne(d => d.Floorplan).WithMany(f => f.FloorplanDevices).HasForeignKey(d => d.FloorplanId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(d => d.AccessCctv).WithMany().HasForeignKey(d => d.AccessCctvId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(d => d.Reader).WithMany().HasForeignKey(d => d.ReaderId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(d => d.AccessControl).WithMany().HasForeignKey(d => d.AccessControlId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(d => d.FloorplanMaskedArea).WithMany().HasForeignKey(d => d.FloorplanMaskedAreaId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(d => d.Application).WithMany().HasForeignKey(d => d.ApplicationId).OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(d => d.Generate).IsUnique();
                entity.HasQueryFilter(d => d.Status != 0);
            });

            // MstBuilding
            modelBuilder.Entity<MstBuilding>(entity =>
            {
                entity.ToTable("mst_building");
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Image).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.CreatedBy).HasMaxLength(255);
                entity.Property(e => e.UpdatedBy).HasMaxLength(255);
                entity.Property(e => e.Status).IsRequired().HasDefaultValue(1);

                entity.HasOne(b => b.Application).WithMany().HasForeignKey(b => b.ApplicationId).OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(b => b.Generate).IsUnique();
                entity.HasQueryFilter(b => b.Status != 0);
            });

            // MstFloorplan
            modelBuilder.Entity<MstFloorplan>(entity =>
            {
                entity.ToTable("mst_floorplan");
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
                entity.Property(e => e.FloorId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.CreatedBy).HasMaxLength(255);
                entity.Property(e => e.UpdatedBy).HasMaxLength(255);
                entity.Property(e => e.Status).IsRequired().HasDefaultValue(1);

                entity.HasOne(f => f.Floor).WithMany().HasForeignKey(f => f.FloorId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(f => f.Application).WithMany().HasForeignKey(f => f.ApplicationId).OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(f => f.Generate).IsUnique();
                entity.HasQueryFilter(f => f.Status != 0);

                entity.Property(m => m.Status)
                    .IsRequired()
                    .HasDefaultValue(1);
            });

            modelBuilder.Entity<User>()
               .ToTable("user")
               .HasOne(u => u.Group)
               .WithMany()
               .HasForeignKey(u => u.GroupId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserGroup>()
               .ToTable("user_group");

            modelBuilder.Entity<BleReaderNode>(entity =>
            {
                entity.ToTable("ble_reader_node");
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ReaderId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.StartPos).HasMaxLength(255).IsRequired();
                entity.Property(e => e.EndPos).HasMaxLength(255).IsRequired();
                entity.Property(e => e.DistancePx).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Distance).HasMaxLength(255).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.CreatedBy).HasMaxLength(255);
                entity.Property(e => e.UpdatedBy).HasMaxLength(255);

                entity.HasOne(t => t.Reader)
                     .WithMany()
                     .HasForeignKey(t => t.ReaderId)
                     .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Application)
                     .WithMany()
                     .HasForeignKey(m => m.ApplicationId)
                     .OnDelete(DeleteBehavior.NoAction);


                entity.HasIndex(f => f.Generate).IsUnique();

            });

            // MstEngine
            modelBuilder.Entity<MstEngine>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
                entity.Property(e => e.EngineId).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Port).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasDefaultValue(1); // untuk delete
                entity.Property(e => e.IsLive).IsRequired().HasDefaultValue(1); // untuk monitoring status
                entity.Property(e => e.LastLive).IsRequired();
                entity.Property(e => e.ServiceStatus).HasMaxLength(50)
                    .HasColumnType("nvarchar(255)")
                    .IsRequired()
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (ServiceStatus)Enum.Parse(typeof(ServiceStatus), v, true)
                    );
            });

             modelBuilder.Entity<MstTrackingLog>(entity =>
            {
                entity.ToTable("mst_tracking_log");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("uuid")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.BeaconId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("beacon_id")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Pair)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("pair")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.FirstReaderId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("first_reader_id")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.SecondReaderId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("second_reader_id")
                    .HasColumnType("varchar(50)");

                // entity.Property(e => e.FirstDist)
                //     .IsRequired()
                //     .HasColumnName("first_dist")
                //     .HasColumnType("decimal(18,6)");

                // entity.Property(e => e.SecondDist)
                //     .IsRequired()
                //     .HasColumnName("second_dist")
                //     .HasColumnType("decimal(18,6)");

                // entity.Property(e => e.JarakMeter)
                //     .IsRequired()
                //     .HasColumnName("jarak_meter")
                //     .HasColumnType("decimal(18,6)");

                entity.Property(e => e.PointX)
                    .IsRequired()
                    .HasColumnName("point_x")
                    .HasColumnType("decimal(18,6)");

                entity.Property(e => e.PointY)
                    .IsRequired()
                    .HasColumnName("point_y")
                    .HasColumnType("decimal(18,6)");

                entity.Property(e => e.FirstReaderX)
                    .IsRequired()
                    .HasColumnName("first_reader_x")
                    .HasColumnType("decimal(18,6)");

                entity.Property(e => e.FirstReaderY)
                    .IsRequired()
                    .HasColumnName("first_reader_y")
                    .HasColumnType("decimal(18,6)");

                entity.Property(e => e.SecondReaderX)
                    .IsRequired()
                    .HasColumnName("second_reader_x")
                    .HasColumnType("decimal(18,6)");

                entity.Property(e => e.SecondReaderY)
                    .IsRequired()
                    .HasColumnName("second_reader_y")
                    .HasColumnType("decimal(18,6)");

                entity.Property(e => e.Time)
                    .IsRequired()
                    .HasColumnName("time")
                    .HasColumnType("timestamp");

                entity.Property(e => e.FloorplanId)
                    .IsRequired()
                    .HasColumnName("floorplan_id")
                    .HasColumnType("uuid");

                entity.HasOne(e => e.FloorplanDevices)
                    .WithMany()
                    .HasForeignKey(e => e.FloorplanId)
                    .HasConstraintName("fk_mst_tracking_log_floorplan_device")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure RecordTrackingLog entity
            modelBuilder.Entity<RecordTrackingLog>(entity =>
            {
                entity.ToTable("record_tracking_log");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("uuid")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.TableName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("table_name")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.FloorplanId)
                    .IsRequired()
                    .HasColumnName("floorplan_id")
                    .HasColumnType("uuid");

                entity.Property(e => e.FloorplanTimestamp)
                    .IsRequired()
                    .HasColumnName("floorplan_timestamp")
                    .HasColumnType("timestamp");

                entity.HasOne(e => e.FloorplanDevices)
                    .WithMany()
                    .HasForeignKey(e => e.FloorplanId)
                    .HasConstraintName("fk_record_tracking_log_floorplan_device")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RefreshToken>(Entity =>
            {
                Entity.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId);
            });   
           

            
        }
    }
}
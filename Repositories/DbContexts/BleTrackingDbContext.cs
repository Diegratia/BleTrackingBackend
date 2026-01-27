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
        public DbSet<MstSecurity> MstSecurities { get; set; }
        public DbSet<MstFloor> MstFloors { get; set; }
        public DbSet<FloorplanMaskedArea> FloorplanMaskedAreas { get; set; }
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<MstBleReader> MstBleReaders { get; set; }
        public DbSet<TrackingTransaction> TrackingTransactions { get; set; }
        public DbSet<AlarmRecordTracking> AlarmRecordTrackings { get; set; }
        public DbSet<MstFloorplan> MstFloorplans { get; set; }
        public DbSet<MstBuilding> MstBuildings { get; set; }
        public DbSet<FloorplanDevice> FloorplanDevices { get; set; }
        public DbSet<MstEngine> MstEngines { get; set; }
        public DbSet<CardRecord> CardRecords{ get; set; }
        public DbSet<TrxVisitor> TrxVisitors{ get; set; }
        public DbSet<Card> Cards{ get; set; }
        public DbSet<AlarmTriggers> AlarmTriggers{ get; set; }
        public DbSet<AlarmCategorySettings> AlarmCategorySettings{ get; set; }
        
        // CardAccessService
        public DbSet<CardAccessMaskedArea> CardAccessMaskedAreas { get; set; }
        public DbSet<CardAccessTimeGroups> CardAccessTimeGroups { get; set; }
        public DbSet<CardGroup> CardGroups{ get; set; }
        public DbSet<CardAccess> CardAccesses{ get; set; }
        // public DbSet<CardGroupCardAccess> CardGroupCardAccesses{ get; set; }
        public DbSet<CardCardAccess> CardCardAccesses{ get; set; }
        public DbSet<TimeBlock> TimeBlocks{ get; set; }
        public DbSet<TimeGroup> TimeGroups{ get; set; }
        public DbSet<MonitoringConfig> MonitoringConfigs{ get; set; }
        public DbSet<Geofence> Geofences{ get; set; }
        //patrol
        public DbSet<PatrolArea> PatrolAreas{ get; set; }
        public DbSet<PatrolRoute> PatrolRoutes{ get; set; }
        public DbSet<PatrolRouteAreas> PatrolRouteAreas{ get; set; }
        public DbSet<PatrolRouteTimeGroups> PatrolRouteTimeGroups{ get; set; }
        public DbSet<PatrolAssignment> PatrolAssignments{ get; set; }
        public DbSet<PatrolAssignmentSecurity> PatrolAssignmentSecurities{ get; set; }
        public DbSet<SecurityGroup> SecurityGroups{ get; set; }
        public DbSet<SecurityGroupMember> SecurityGroupMembers{ get; set; }
        
        public DbSet<PatrolCase> PatrolCases { get; set; }
        public DbSet<PatrolSession> PatrolSessions{ get; set; }
        public DbSet<PatrolCheckpointLog> PatrolCheckpointLogs{ get; set; }    
        public DbSet<PatrolCaseAttachment> PatrolCaseAttachments{ get; set; }    
        //
        public DbSet<StayOnArea> StayOnAreas { get; set; }
        public DbSet<Boundary> Boundarys{ get; set; }
        public DbSet<Overpopulating> Overpopulatings{ get; set; }
        public DbSet<TrackingReportPreset> TrackingReportPresets{ get; set; }
        
        // public DbSet<MstTrackingLog> MstTrackingLogs { get; set; }
        // public DbSet<RecordTrackingLog> RecordTrackingLogs { get; set; }
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
            modelBuilder.Entity<MstSecurity>().ToTable("mst_security");
            modelBuilder.Entity<MstFloor>().ToTable("mst_floor");
            modelBuilder.Entity<FloorplanMaskedArea>().ToTable("floorplan_masked_area");
            modelBuilder.Entity<Visitor>().ToTable("visitor");
            modelBuilder.Entity<MstBleReader>().ToTable("mst_ble_reader");
            modelBuilder.Entity<TrackingTransaction>().ToTable("tracking_transaction");
            modelBuilder.Entity<AlarmRecordTracking>().ToTable("alarm_record_tracking");
            modelBuilder.Entity<MstFloorplan>().ToTable("mst_floorplan");
            modelBuilder.Entity<MstBuilding>().ToTable("mst_building");
            modelBuilder.Entity<MstEngine>().ToTable("mst_engine");
            modelBuilder.Entity<CardRecord>().ToTable("card_record");
            modelBuilder.Entity<TrxVisitor>().ToTable("trx_visitor");
            modelBuilder.Entity<Card>().ToTable("card");
            modelBuilder.Entity<Geofence>().ToTable("geofence");
            modelBuilder.Entity<PatrolArea>().ToTable("patrol_area");
            modelBuilder.Entity<PatrolRoute>().ToTable("patrol_route");
            modelBuilder.Entity<PatrolRouteAreas>().ToTable("patrol_route_areas");
            modelBuilder.Entity<PatrolRouteTimeGroups>().ToTable("patrol_route_time_groups");
            modelBuilder.Entity<PatrolAssignment>().ToTable("patrol_assignment");
            modelBuilder.Entity<PatrolAssignmentSecurity>().ToTable("patrol_assignment_security");
            modelBuilder.Entity<PatrolCase>().ToTable("patrol_case");
            modelBuilder.Entity<PatrolSession>().ToTable("patrol_session");
            modelBuilder.Entity<PatrolCheckpointLog>().ToTable("patrol_checkpoint_log");
            modelBuilder.Entity<PatrolCaseAttachment>().ToTable("patrol_case_attachment");
            modelBuilder.Entity<SecurityGroup>().ToTable("security_group");
            modelBuilder.Entity<SecurityGroupMember>().ToTable("security_group_member");
            modelBuilder.Entity<StayOnArea>().ToTable("stay_on_area");
            modelBuilder.Entity<Overpopulating>().ToTable("overpopulating");
            modelBuilder.Entity<Boundary>().ToTable("boundary");
            modelBuilder.Entity<MonitoringConfig>().ToTable("monitoring_config");
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
            //            modelBuilder.Entity<AlarmRecordTracking>()
            // .HasQueryFilter(m => m.FloorplanMaskedAreaId != null   // pakai FK, bukan nav
            //               && m.ReaderId != null                    // kalau ada FK-nya
            //               && m.VisitorId != null);
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
            modelBuilder.Entity<MstSecurity>()
                .HasQueryFilter(m => m.Status != 0);
            modelBuilder.Entity<MstOrganization>()
                .HasQueryFilter(m => m.Status != 0);
            modelBuilder.Entity<MstFloorplan>()
                .HasQueryFilter(m => m.Status != 0);
            modelBuilder.Entity<MstEngine>()
                .HasQueryFilter(m => m.Status != 0); ;
            // modelBuilder.Entity<VisitorCard>()
            //     .HasQueryFilter(m => m.Status != 0);



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
                entity.Property(e => e.IntegrationId).HasMaxLength(36);
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
                entity.Property(e => e.IntegrationId).HasMaxLength(36);
                entity.Property(e => e.BrandId).HasMaxLength(36);

                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Integration)
                    .WithMany()
                    .HasForeignKey(m => m.IntegrationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Brand)
                    .WithMany()
                    .HasForeignKey(m => m.BrandId)
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

            // MonitoringConfig
            modelBuilder.Entity<MonitoringConfig>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();

                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            // TrackingReportPreset
            modelBuilder.Entity<TrackingReportPreset>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();

                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // MstMember
            modelBuilder.Entity<MstMember>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.OrganizationId).HasMaxLength(36);
                entity.Property(e => e.DepartmentId).HasMaxLength(36);
                entity.Property(e => e.DistrictId).HasMaxLength(36);
                entity.Property(e => e.Gender)
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (Gender)Enum.Parse(typeof(Gender), v, true)
                    );
                entity.Property(e => e.StatusEmployee)
                    .HasColumnType("nvarchar(255)")
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

            // MstSecurity
            modelBuilder.Entity<MstSecurity>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.OrganizationId).HasMaxLength(36);
                entity.Property(e => e.DepartmentId).HasMaxLength(36);
                entity.Property(e => e.DistrictId).HasMaxLength(36);
                entity.Property(e => e.Gender)
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (Gender)Enum.Parse(typeof(Gender), v, true)
                    );
                entity.Property(e => e.StatusEmployee)
                    .HasColumnType("nvarchar(255)")
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

                entity.HasMany(m => m.PatrolAssignmentSecurities)
                    .WithOne(m => m.Security)
                    .HasForeignKey(m => m.SecurityId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(m => m.SecurityGroupMembers)
                    .WithOne(m => m.Security)
                    .HasForeignKey(m => m.SecurityId)
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
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.BuildingId).HasMaxLength(36);
                entity.Property(m => m.Status)
                    .IsRequired()
                    .HasDefaultValue(1);

                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Building)
                    .WithMany()
                    .HasForeignKey(m => m.BuildingId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasMany(f => f.PatrolAreas)
                    .WithOne()
                    .HasForeignKey(f => f.FloorplanId)
                    .OnDelete(DeleteBehavior.NoAction);


            });

            // FloorplanMaskedArea
            modelBuilder.Entity<FloorplanMaskedArea>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.FloorId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.FloorplanId).HasMaxLength(36).IsRequired();
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

                entity.HasOne(m => m.Floorplan)
                    .WithMany(m => m.FloorplanMaskedAreas)
                    .HasForeignKey(m => m.FloorplanId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(m => m.Status)
                    .IsRequired()
                    .HasDefaultValue(1);

                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Visitor
            modelBuilder.Entity<Visitor>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.Gender)
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (Gender)Enum.Parse(typeof(Gender), v, true)
                    );

                entity.Property(e => e.IdentityType)
                    .HasColumnName("identity_type")
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (IdentityType)Enum.Parse(typeof(IdentityType), v, true)
                    );
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(v => v.PersonId);
                entity.HasIndex(v => v.Email);
            });

            // MstBleReader
            modelBuilder.Entity<MstBleReader>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.BrandId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ReaderType)
                    .HasColumnName("reader_type")
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (ReaderType)Enum.Parse(typeof(ReaderType), v, true)
                    );

                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.HasOne(m => m.Application)
                    .WithMany()
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

            // TrackingTransaction
            modelBuilder.Entity<TrackingTransaction>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ReaderId).HasMaxLength(36);
                entity.Property(e => e.VisitorId).HasMaxLength(36);
                entity.Property(e => e.MemberId).HasMaxLength(36);
                entity.Property(e => e.FloorplanMaskedAreaId).HasMaxLength(36);
                entity.Property(e => e.CardId).HasMaxLength(36);
                entity.Property(e => e.AlarmStatus)
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v == AlarmStatus.NonActive ? "non-active" : v.ToString().ToLower(),
                        v => v == "non-active" ? AlarmStatus.NonActive : (AlarmStatus)Enum.Parse(typeof(AlarmStatus), v, true)
                    );
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.Reader)
                    .WithMany()
                    .HasForeignKey(t => t.ReaderId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.Visitor)
                    .WithMany()
                    .HasForeignKey(t => t.VisitorId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.Member)
                    .WithMany()
                    .HasForeignKey(t => t.MemberId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.FloorplanMaskedArea)
                    .WithMany()
                    .HasForeignKey(t => t.FloorplanMaskedAreaId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.Card)
                    .WithMany()
                    .HasForeignKey(t => t.CardId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            //AlarmRecordTracking
            modelBuilder.Entity<AlarmRecordTracking>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.VisitorId).HasMaxLength(36);
                entity.Property(e => e.MemberId).HasMaxLength(36);
                entity.Property(e => e.ReaderId).HasMaxLength(36);
                entity.Property(e => e.FloorplanMaskedAreaId).HasMaxLength(36);
                entity.Property(e => e.AlarmTriggersId).HasMaxLength(36);
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();

                entity.Property(e => e.Alarm)
                    .HasColumnName("alarm_record_status")
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (AlarmRecordStatus)Enum.Parse(typeof(AlarmRecordStatus), v, true)
                    );

                entity.Property(e => e.Action)
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (ActionStatus)Enum.Parse(typeof(ActionStatus), v, true)
                    );

                // Relasi one-to-many dengan Visitor
                entity.HasOne(a => a.Visitor)
                    .WithMany()
                    .HasForeignKey(a => a.VisitorId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(a => a.Member)
                    .WithMany(b => b.AlarmRecordTrackings)
                    .HasForeignKey(a => a.MemberId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Relasi one-to-many dengan MstBleReader
                entity.HasOne(a => a.Reader)
                   .WithMany()
                    .HasForeignKey(a => a.ReaderId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Relasi one-to-many dengan FloorplanMaskedArea
                entity.HasOne(a => a.FloorplanMaskedArea)
                   .WithMany()
                    .HasForeignKey(a => a.FloorplanMaskedAreaId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(a => a.AlarmTriggers)
                   .WithMany()
                    .HasForeignKey(a => a.AlarmTriggersId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Relasi one-to-many dengan MstApplication
                entity.HasOne(a => a.Application)
                    .WithMany()
                    .HasForeignKey(a => a.ApplicationId)
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
                entity.Property(e => e.Type).HasConversion(v => v.ToString().ToLower(), v => (DeviceType)Enum.Parse(typeof(DeviceType), v, true));
                entity.Property(e => e.FloorplanId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.AccessCctvId).HasMaxLength(36);
                entity.Property(e => e.ReaderId).HasMaxLength(36);
                entity.Property(e => e.AccessControlId).HasMaxLength(36);
                entity.Property(e => e.FloorplanMaskedAreaId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.DeviceStatus).HasConversion(v => v.ToString().ToLower(), v => (DeviceStatus)Enum.Parse(typeof(DeviceStatus), v, true));
                entity.Property(e => e.Status).IsRequired();

                entity.HasOne(d => d.Floorplan).WithMany(f => f.FloorplanDevices).HasForeignKey(d => d.FloorplanId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(d => d.AccessCctv).WithMany().HasForeignKey(d => d.AccessCctvId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(d => d.Reader).WithMany().HasForeignKey(d => d.ReaderId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(d => d.AccessControl).WithMany().HasForeignKey(d => d.AccessControlId).OnDelete(DeleteBehavior.NoAction);
                // entity.HasOne(d => d.Reader)
                //     .WithOne(r => r.FloorplanDevices)
                //     .HasForeignKey<FloorplanDevice>(d => d.ReaderId)
                //     .OnDelete(DeleteBehavior.NoAction);
                //     // ⭐ CCTV = ONE-TO-ONE
                //     entity.HasOne(d => d.AccessCctv)
                //         .WithOne(c => c.FloorplanDevice)
                //         .HasForeignKey<FloorplanDevice>(d => d.AccessCctvId)
                //         .OnDelete(DeleteBehavior.NoAction);

                //     // ⭐ ACCESS CONTROL = ONE-TO-ONE
                //     entity.HasOne(d => d.AccessControl)
                //         .WithOne(ac => ac.FloorplanDevice)
                //         .HasForeignKey<FloorplanDevice>(d => d.AccessControlId)
                //         .OnDelete(DeleteBehavior.NoAction);
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
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
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
                entity.Property(e => e.FloorId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.EngineId).HasMaxLength(36);
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasDefaultValue(1);

                // entity.HasOne(f => f.Engine).WithMany().HasForeignKey(f => f.EngineId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(f => f.Floor).WithMany().HasForeignKey(f => f.FloorId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(f => f.Engine).WithMany(f => f.Floorplans).HasForeignKey(f => f.EngineId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(f => f.Application).WithMany().HasForeignKey(f => f.ApplicationId).OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(f => f.PatrolAreas).WithOne().HasForeignKey(f => f.FloorplanId).OnDelete(DeleteBehavior.NoAction);

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

            // MstEngine
            modelBuilder.Entity<MstEngine>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.EngineTrackingId).HasMaxLength(255);
                entity.Property(e => e.Status);
                entity.Property(e => e.IsLive);
                entity.Property(e => e.LastLive);

                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(e => e.Floorplans)
                    .WithOne(e => e.Engine)
                    .HasForeignKey(e => e.EngineId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(e => e.ServiceStatus).HasMaxLength(50)
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (ServiceStatus)Enum.Parse(typeof(ServiceStatus), v, true)

                    );
            });
            // Geofence
            modelBuilder.Entity<Geofence>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();

                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.FloorId).HasMaxLength(36);
                entity.Property(e => e.FloorplanId).HasMaxLength(36);
                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Floor)
                    .WithMany()
                    .HasForeignKey(m => m.FloorId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Floorplan)
                    .WithMany()
                    .HasForeignKey(m => m.FloorplanId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            // PatrolArea
            modelBuilder.Entity<PatrolArea>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.FloorId).HasMaxLength(36);
                entity.Property(e => e.FloorplanId).HasMaxLength(36);
                entity.HasOne(m => m.Application)
                    .WithMany(m => m.PatrolAreas)
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Floor)
                    .WithMany(m => m.PatrolAreas)
                    .HasForeignKey(m => m.FloorId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Floorplan)
                    .WithMany(m => m.PatrolAreas)
                    .HasForeignKey(m => m.FloorplanId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(e => e.PatrolRouteAreas)
                    .WithOne(e => e.PatrolArea)
                    .HasForeignKey(e => e.PatrolAreaId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // PatrolRoute
            modelBuilder.Entity<PatrolRoute>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.HasOne(m => m.Application)
                    .WithMany(m => m.PatrolRoutes)
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasMany(e => e.PatrolRouteAreas)
                    .WithOne(e => e.PatrolRoute)
                    .HasForeignKey(e => e.PatrolRouteId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasMany(e => e.PatrolRouteTimeGroups)
                    .WithOne(e => e.PatrolRoutes)
                    .HasForeignKey(e => e.PatrolRouteId)
                    .OnDelete(DeleteBehavior.NoAction);

            });

            // PatrolRouteAreas
            modelBuilder.Entity<PatrolRouteAreas>(entity =>
            {
                entity.ToTable("patrol_route_areas");

                entity.HasKey(e => new { e.PatrolAreaId, e.PatrolRouteId });

                entity.HasOne(e => e.PatrolArea)
                    .WithMany(e => e.PatrolRouteAreas)
                    .HasForeignKey(e => e.PatrolAreaId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.PatrolRoute)
                    .WithMany(ma => ma.PatrolRouteAreas)
                    .HasForeignKey(e => e.PatrolRouteId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            //PatrolAssignment
            modelBuilder.Entity<PatrolAssignment>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();

                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.PatrolRouteId).HasMaxLength(36);
                entity.HasOne(m => m.Application)
                    .WithMany(m => m.PatrolAssignments)
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.PatrolRoute)
                    .WithMany(m => m.PatrolAssignments)
                    .HasForeignKey(m => m.PatrolRouteId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(e => e.PatrolAssignmentSecurities)
                    .WithOne(e => e.PatrolAssignment)
                    .HasForeignKey(e => e.PatrolAssignmentId)
                    .OnDelete(DeleteBehavior.NoAction);

            });
            //PatrolAssignmentSecurity
            modelBuilder.Entity<PatrolAssignmentSecurity>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.PatrolAssignmentId).HasMaxLength(36);
                entity.Property(e => e.SecurityId).HasMaxLength(36);
                entity.HasOne(m => m.Application)
                    .WithMany(m => m.PatrolAssignmentSecurities)
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.PatrolAssignment)
                    .WithMany(m => m.PatrolAssignmentSecurities)
                    .HasForeignKey(m => m.PatrolAssignmentId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Security)
                    .WithMany(m => m.PatrolAssignmentSecurities)
                    .HasForeignKey(m => m.SecurityId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            //SecurityGroupMember
            modelBuilder.Entity<SecurityGroupMember>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.SecurityGroupId).HasMaxLength(36);
                entity.Property(e => e.SecurityId).HasMaxLength(36);
                entity.HasOne(m => m.Application)
                    .WithMany(m => m.SecurityGroupMembers)
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.SecurityGroup)
                    .WithMany(m => m.SecurityGroupMembers)
                    .HasForeignKey(m => m.SecurityGroupId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Security)
                    .WithMany(m => m.SecurityGroupMembers)
                    .HasForeignKey(m => m.SecurityId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            //SecurityGroup
            modelBuilder.Entity<SecurityGroup>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.HasOne(m => m.Application)
                    .WithMany(m => m.SecurityGroups)
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(m => m.SecurityGroupMembers)
                    .WithOne(m => m.SecurityGroup)
                    .HasForeignKey(m => m.SecurityGroupId)
                    .OnDelete(DeleteBehavior.NoAction);

            });
            //PatrolCase
            modelBuilder.Entity<PatrolCase>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.PatrolSessionId).HasMaxLength(36);
                entity.Property(e => e.PatrolRouteId).HasMaxLength(36);
                entity.Property(e => e.SecurityId).HasMaxLength(36);
                entity.Property(e => e.ApprovedByHeadId).HasMaxLength(36);

                entity.HasOne(m => m.Application)
                    .WithMany(m => m.PatrolCases)
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(m => m.PatrolSession)
                    .WithMany(m => m.PatrolCases)
                    .HasForeignKey(m => m.PatrolSessionId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(m => m.PatrolRoute)
                    .WithMany(m => m.PatrolCases)
                    .HasForeignKey(m => m.PatrolRouteId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(m => m.Security)
                    .WithMany(m => m.SecuritiesPatrolCases)
                    .HasForeignKey(m => m.SecurityId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(m => m.ApprovedByHead)
                    .WithMany(m => m.HeadPatrolCases)
                    .HasForeignKey(m => m.ApprovedByHeadId)
                    .OnDelete(DeleteBehavior.NoAction);
        });

            //PatrolSession
            modelBuilder.Entity<PatrolSession>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.PatrolRouteId).HasMaxLength(36);
                entity.Property(e => e.PatrolAssignmentId).HasMaxLength(36);
                entity.Property(e => e.SecurityId).HasMaxLength(36);
                entity.HasOne(m => m.Application)
                    .WithMany(m => m.PatrolSessions)
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(m => m.PatrolRoute)
                    .WithMany(m => m.PatrolSessions)
                    .HasForeignKey(m => m.PatrolRouteId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(m => m.PatrolAssignment)
                    .WithMany(m => m.PatrolSessions)
                    .HasForeignKey(m => m.PatrolAssignmentId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(m => m.Security)
                    .WithMany(m => m.PatrolSessions)
                    .HasForeignKey(m => m.SecurityId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            //PatrolCheckpointLog
            modelBuilder.Entity<PatrolCheckpointLog>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.PatrolSessionId).HasMaxLength(36);
                entity.HasOne(m => m.Application)
                    .WithMany(m => m.PatrolCheckpointLogs)
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(m => m.PatrolSession)
                    .WithMany(m => m.PatrolCheckpointLogs)
                    .HasForeignKey(m => m.PatrolSessionId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            //PatrolCaseAttachment
            modelBuilder.Entity<PatrolCaseAttachment>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.PatrolCaseId).HasMaxLength(36);
                entity.Property(e => e.FileType)
                    .HasColumnName("file_type")
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (FileType)Enum.Parse(typeof(FileType), v, true)
                    );
                entity.HasOne(m => m.Application)
                    .WithMany(m => m.PatrolCaseAttachments)
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(m => m.PatrolCase)
                    .WithMany(m => m.PatrolCaseAttachments)
                    .HasForeignKey(m => m.PatrolCaseId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


             // Boundary
                modelBuilder.Entity<Boundary>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();

                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.FloorId).HasMaxLength(36);
                entity.Property(e => e.FloorplanId).HasMaxLength(36);
                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);
                
                entity.HasOne(m => m.Floor)
                    .WithMany()
                    .HasForeignKey(m => m.FloorId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Floorplan)
                    .WithMany()
                    .HasForeignKey(m => m.FloorplanId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

             // StayOnArea
            modelBuilder.Entity<StayOnArea>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();

                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.FloorId).HasMaxLength(36);
                entity.Property(e => e.FloorplanId).HasMaxLength(36);
                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);
                
                entity.HasOne(m => m.Floor)
                    .WithMany()
                    .HasForeignKey(m => m.FloorId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Floorplan)
                    .WithMany()
                    .HasForeignKey(m => m.FloorplanId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

             // Overpopulating
            modelBuilder.Entity<Overpopulating>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();

                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.FloorId).HasMaxLength(36);
                entity.Property(e => e.FloorplanId).HasMaxLength(36);
                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);
                
                entity.HasOne(m => m.Floor)
                    .WithMany()
                    .HasForeignKey(m => m.FloorId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.Floorplan)
                    .WithMany()
                    .HasForeignKey(m => m.FloorplanId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

                modelBuilder.Entity<AlarmTriggers>(entity =>
            {
                entity.ToTable("alarm_triggers");
                // entity.ToTable("alarm_triggers");
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();

                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.FloorplanId).HasMaxLength(36);
                entity.Property(e => e.VisitorId).HasMaxLength(36);
                entity.Property(e => e.MemberId).HasMaxLength(36);
                entity.Property(e => e.SecurityId).HasMaxLength(36);
                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                 entity.Property(e => e.Alarm)
                    .HasColumnName("alarm_record_status")
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (AlarmRecordStatus)Enum.Parse(typeof(AlarmRecordStatus), v, true)
                    );

                entity.Property(e => e.Action)
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (ActionStatus)Enum.Parse(typeof(ActionStatus), v, true)
                    );

                // Relasi one-to-many dengan Visitor
                entity.HasOne(a => a.Floorplan)
                    .WithMany()
                    .HasForeignKey(a => a.FloorplanId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(a => a.Visitor)
                    .WithMany()
                    .HasForeignKey(a => a.VisitorId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(a => a.Member)
                    .WithMany()
                    .HasForeignKey(a => a.MemberId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(a => a.Security)
                    .WithMany()
                    .HasForeignKey(a => a.SecurityId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<CardRecord>(entity =>
            {
                entity.ToTable("card_record");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .HasColumnName("name");

                entity.Property(e => e.CardId)
                    .HasColumnName("card_id");

                entity.Property(e => e.VisitorId)
                    .HasColumnName("visitor_id");

                entity.Property(e => e.MemberId)
                    .HasColumnName("member_id");

                entity.Property(e => e.Timestamp)
                    .HasColumnName("timestamp");

                entity.Property(e => e.CheckinAt)
                    .HasColumnName("checkin_at");

                entity.Property(e => e.CheckoutAt)
                    .HasColumnName("checkout_at");

                entity.Property(e => e.CheckinBy)
                    .HasColumnName("checkin_by");

                entity.Property(e => e.CheckoutBy)
                    .HasColumnName("checkout_by");

                entity.Property(e => e.CheckinMaskedArea)
                    .HasColumnName("checkin_masked_area");

                entity.Property(e => e.CheckoutMaskedArea)
                    .HasColumnName("checkout_masked_area");

                entity.Property(e => e.VisitorActiveStatus)
                    .HasColumnName("visitor_type")
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (VisitorActiveStatus)Enum.Parse(typeof(VisitorActiveStatus), v, true)
                    );

                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Visitor)
                    .WithMany() // atau .WithMany(v => v.CardRecords) kalau ada
                    .HasForeignKey(e => e.VisitorId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Member)
                    .WithMany() // atau .WithMany(m => m.CardRecords)
                    .HasForeignKey(e => e.MemberId)
                    .OnDelete(DeleteBehavior.NoAction); 
                    
                entity.HasOne(e => e.Card)
                    .WithMany() // atau .WithMany(m => m.CardRecords)
                    .HasForeignKey(e => e.CardId)
                    .OnDelete(DeleteBehavior.NoAction); 
            });

            modelBuilder.Entity<TimeGroup>(entity =>
            {
                entity.ToTable("time_group");

                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();

                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);
                    
                entity.Property(e => e.ScheduleType)
                .HasColumnName("schedule_type")
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (ScheduleType)Enum.Parse(typeof(ScheduleType), v, true)
                    );
                entity.HasMany(e => e.CardAccessTimeGroups)
                    .WithOne(e => e.TimeGroup)
                    .HasForeignKey(e => e.TimeGroupId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

                modelBuilder.Entity<TimeBlock>(entity =>
            {
                entity.ToTable("time_block");

                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();

                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(e => e.TimeGroupId).HasColumnName("time_group_id").HasMaxLength(36);
                entity.HasOne(m => m.TimeGroup)
                    .WithMany(g => g.TimeBlocks) 
                    .HasForeignKey(m => m.TimeGroupId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            //  modelBuilder.Entity<MstTrackingLog>(entity =>
            // {
            //     entity.ToTable("mst_tracking_log");

            //     entity.HasKey(e => e.Id);

            //     entity.Property(e => e.Id)
            //         .HasColumnName("id")
            //         .HasColumnType("uuid")
            //         .ValueGeneratedOnAdd();

            //     entity.Property(e => e.BeaconId)
            //         .IsRequired()
            //         .HasMaxLength(50)
            //         .HasColumnName("beacon_id")
            //         .HasColumnType("varchar(50)");

            //     entity.Property(e => e.Pair)
            //         .IsRequired()
            //         .HasMaxLength(100)
            //         .HasColumnName("pair")
            //         .HasColumnType("varchar(100)");

            //     entity.Property(e => e.FirstReaderId)
            //         .IsRequired()
            //         .HasMaxLength(50)
            //         .HasColumnName("first_reader_id")
            //         .HasColumnType("varchar(50)");

            //     entity.Property(e => e.SecondReaderId)
            //         .IsRequired()
            //         .HasMaxLength(50)
            //         .HasColumnName("second_reader_id")
            //         .HasColumnType("varchar(50)");

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

            //     entity.Property(e => e.PointX)
            //         .IsRequired()
            //         .HasColumnName("point_x")
            //         .HasColumnType("decimal(18,6)");

            //     entity.Property(e => e.PointY)
            //         .IsRequired()
            //         .HasColumnName("point_y")
            //         .HasColumnType("decimal(18,6)");

            //     entity.Property(e => e.FirstReaderX)
            //         .IsRequired()
            //         .HasColumnName("first_reader_x")
            //         .HasColumnType("decimal(18,6)");

            //     entity.Property(e => e.FirstReaderY)
            //         .IsRequired()
            //         .HasColumnName("first_reader_y")
            //         .HasColumnType("decimal(18,6)");

            //     entity.Property(e => e.SecondReaderX)
            //         .IsRequired()
            //         .HasColumnName("second_reader_x")
            //         .HasColumnType("decimal(18,6)");

            //     entity.Property(e => e.SecondReaderY)
            //         .IsRequired()
            //         .HasColumnName("second_reader_y")
            //         .HasColumnType("decimal(18,6)");

            //     entity.Property(e => e.Time)
            //         .IsRequired()
            //         .HasColumnName("time")
            //         .HasColumnType("timestamp");

            //     entity.Property(e => e.FloorplanId)
            //         .IsRequired()
            //         .HasColumnName("floorplan_id")
            //         .HasColumnType("uuid");

            //     entity.HasOne(e => e.FloorplanDevice)
            //         .WithMany()
            //         .HasForeignKey(e => e.FloorplanDeviceId)
            //         .HasConstraintName("fk_mst_tracking_log_floorplan_device")
            //         .OnDelete(DeleteBehavior.Cascade);
            // });




            // Configure RecordTrackingLog entity
            // modelBuilder.Entity<RecordTrackingLog>(entity =>
            // {
            //     entity.ToTable("record_tracking_log");

            //     entity.HasKey(e => e.Id);

            //     entity.Property(e => e.Id)
            //         .HasColumnName("id")
            //         .HasColumnType("uuid")
            //         .ValueGeneratedOnAdd();

            //     entity.Property(e => e.TableName)
            //         .IsRequired()
            //         .HasMaxLength(255)
            //         .HasColumnName("table_name")
            //         .HasColumnType("varchar(255)");

            //     entity.Property(e => e.FloorplanId)
            //         .IsRequired()
            //         .HasColumnName("floorplan_id")
            //         .HasColumnType("uuid");

            //     entity.Property(e => e.FloorplanTimestamp)
            //         .IsRequired()
            //         .HasColumnName("floorplan_timestamp")
            //         .HasColumnType("timestamp");

            //     entity.HasOne(e => e.Floorplan)
            //         .WithMany()
            //         .HasForeignKey(e => e.FloorplanId)
            //         .HasConstraintName("fk_record_tracking_log_floorplan")
            //         .OnDelete(DeleteBehavior.NoAction);
            // });

            //TrxVisitor
            modelBuilder.Entity<TrxVisitor>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.VisitorId).HasMaxLength(36);
                entity.Property(e => e.PurposePerson).HasMaxLength(36);
                entity.Property(e => e.Status)
                    .HasColumnName("visitor_status")
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (VisitorStatus)Enum.Parse(typeof(VisitorStatus), v, true)
                    );
                entity.Property(e => e.VisitorActiveStatus)
                    .HasColumnName("visitor_active_status")
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (VisitorActiveStatus)Enum.Parse(typeof(VisitorActiveStatus), v, true)
                    );
                entity.Property(e => e.PersonType)
                    .HasColumnName("person_type")
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (PersonType)Enum.Parse(typeof(PersonType), v, true)
                    );
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(v => v.Visitor)
                   .WithMany()
                   .HasForeignKey(v => v.VisitorId)
                   .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(m => m.Member)
                    .WithMany()
                    .HasForeignKey(m => m.PurposePerson)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(v => v.MaskedArea)
                    .WithMany()
                    .HasForeignKey(v => v.MaskedAreaId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

             // Visitor
            modelBuilder.Entity<Visitor>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.Gender)
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (Gender)Enum.Parse(typeof(Gender), v, true)
                    );
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(v => v.Application)
                    .WithMany()
                    .HasForeignKey(v => v.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(v => v.PersonId);
                entity.HasIndex(v => v.Email);
            });

            // CardGroup
            modelBuilder.Entity<CardGroup>(entity =>
            {
                entity.ToTable("card_groups");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.ApplicationId).IsRequired();

                entity.HasOne(e => e.Application)
                    .WithMany()
                    .HasForeignKey(e => e.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                // entity.HasMany(e => e.CardGroupCardAccesses)
                //     .WithOne(e => e.CardGroup)
                //     .HasForeignKey(e => e.CardGroupId)
                //     .OnDelete(DeleteBehavior.NoAction);
                
                
            });

            // CardAccess
            modelBuilder.Entity<CardAccess>(entity =>
            {
                entity.ToTable("card_accesses");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.ApplicationId).IsRequired();

                entity.HasOne(e => e.Application)
                    .WithMany()
                    .HasForeignKey(e => e.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(e => e.CardAccessMaskedAreas)
                    .WithOne(e => e.CardAccess)
                    .HasForeignKey(e => e.CardAccessId)
                    .OnDelete(DeleteBehavior.NoAction);
                
                entity.HasMany(e => e.CardAccessTimeGroups)
                    .WithOne(e => e.CardAccess)
                    .HasForeignKey(e => e.CardAccessId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(e => e.AccessScope)
                .HasConversion(
                        v => v.ToString().ToLower(), // Simpan ke DB sebagai "single"
                        v => (AccessScope)Enum.Parse(typeof(AccessScope), v, true)
                    );

              
            });

            // CardAccessMaskedArea (pivot CardAccess <-> FloorplanMaskedArea)
            modelBuilder.Entity<CardAccessMaskedArea>(entity =>
            {
                entity.ToTable("card_access_masked_areas");

                entity.HasKey(e => new { e.CardAccessId, e.MaskedAreaId });

                entity.HasOne(e => e.CardAccess)
                    .WithMany(e => e.CardAccessMaskedAreas)
                    .HasForeignKey(e => e.CardAccessId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.MaskedArea)
                    .WithMany(ma => ma.CardAccessMaskedAreas)
                    .HasForeignKey(e => e.MaskedAreaId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            
             // CardAccessTimeGroups (pivot CardAccess <-> TimeGroups)
            modelBuilder.Entity<CardAccessTimeGroups>(entity =>
            {
                entity.ToTable("card_access_time_groups");

                entity.HasKey(e => new { e.CardAccessId, e.TimeGroupId });

                entity.HasOne(e => e.CardAccess)
                    .WithMany(e => e.CardAccessTimeGroups)
                    .HasForeignKey(e => e.CardAccessId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.TimeGroup)
                    .WithMany(ma => ma.CardAccessTimeGroups)
                    .HasForeignKey(e => e.TimeGroupId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
             // PatrolRouteTimeGroups (pivot PatrolRoute <-> TimeGroups)
            modelBuilder.Entity<PatrolRouteTimeGroups>(entity =>
            {
                entity.ToTable("patrol_route_time_groups");

                entity.HasKey(e => new { e.PatrolRouteId, e.TimeGroupId });

                entity.HasOne(e => e.PatrolRoutes)
                    .WithMany(e => e.PatrolRouteTimeGroups)
                    .HasForeignKey(e => e.PatrolRouteId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.TimeGroup)
                    .WithMany(ma => ma.PatrolRouteTimeGroups)
                    .HasForeignKey(e => e.TimeGroupId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            //pivot card dan card access
            modelBuilder.Entity<CardCardAccess>(entity =>
            {
                entity.ToTable("card_card_accesses");

                entity.HasKey(e => new { e.CardId, e.CardAccessId });

                entity.HasOne(e => e.Card)
                    .WithMany(e => e.CardCardAccesses)
                    .HasForeignKey(e => e.CardId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.CardAccess)
                    .WithMany()
                    .HasForeignKey(e => e.CardAccessId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            
            modelBuilder.Entity<AlarmCategorySettings>(entity =>
            {
                entity.ToTable("alarm_category_settings");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();

                entity.Property(e => e.AlarmCategory)
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (AlarmRecordStatus)Enum.Parse(typeof(AlarmRecordStatus), v, true)
                    );
                entity.Property(e => e.AlarmLevelPriority)
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (AlarmLevelPriority)Enum.Parse(typeof(AlarmLevelPriority), v, true)
                    );
            });

            // Card
            modelBuilder.Entity<Card>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.CardType)
                    .HasColumnType("nvarchar(255)")
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => (CardType)Enum.Parse(typeof(CardType), v, true)
                    );
                entity.Property(e => e.ApplicationId).HasMaxLength(36).IsRequired();
                entity.HasOne(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(e => e.MemberId).HasMaxLength(36);
                entity.HasOne(m => m.Member)
                    .WithMany()
                    .HasForeignKey(m => m.MemberId)
                    .OnDelete(DeleteBehavior.NoAction);
                    
                entity.Property(e => e.SecurityId).HasMaxLength(36);
                entity.HasOne(m => m.Security)
                    .WithMany()
                    .HasForeignKey(m => m.SecurityId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(e => e.RegisteredMaskedAreaId).HasMaxLength(36);
                entity.HasOne(m => m.RegisteredMaskedArea)
                    .WithMany()
                    .HasForeignKey(m => m.RegisteredMaskedAreaId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(e => e.CardGroupId).HasMaxLength(36);
                entity.HasOne(m => m.CardGroup)
                    .WithMany(g => g.Cards)
                    .HasForeignKey(m => m.CardGroupId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(e => e.VisitorId).HasMaxLength(36);
                entity.HasOne(m => m.Visitor)
                    .WithMany()
                    .HasForeignKey(m => m.VisitorId)
                    .OnDelete(DeleteBehavior.NoAction);
                    
                entity.HasMany(e => e.CardCardAccesses)
                    .WithOne(e => e.Card)
                    .HasForeignKey(e => e.CardId)
                    .OnDelete(DeleteBehavior.NoAction);
                
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
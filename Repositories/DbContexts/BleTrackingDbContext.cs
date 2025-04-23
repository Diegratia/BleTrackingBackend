using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Entities.Models;

namespace Repositories.DbContexts
{
    public class BleTrackingDbContext : DbContext
    {
        public BleTrackingDbContext(DbContextOptions<BleTrackingDbContext> dbContextOptions)
            : base(dbContextOptions) { }

        public DbSet<MstBrand> MstBrands { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<MstBrand>().ToTable("mst_brand");

            modelBuilder.Entity<MstBrand>()
                .HasQueryFilter(m => m.Status != 0);

            modelBuilder.Entity<MstBrand>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
            });
        }
    }
}


   
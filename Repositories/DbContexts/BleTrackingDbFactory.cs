using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Repositories.Extensions;
namespace Repositories.DbContexts;

    public class AppDbContextFactory 
        : IDesignTimeDbContextFactory<BleTrackingDbContext>
    {
        public BleTrackingDbContext CreateDbContext(string[] args)
    {
            
            // 🔥 INI KUNCI-NYA
            EnvExtension.LoadEnvWithTryCatch();
            var connectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__BleTrackingDbConnection")
                ?? throw new Exception("DB_CONNECTION not found in env");

            var optionsBuilder = new DbContextOptionsBuilder<BleTrackingDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new BleTrackingDbContext(optionsBuilder.Options);
        }
    }

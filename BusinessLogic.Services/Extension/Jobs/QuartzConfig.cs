using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using BusinessLogic.Services.JobsScheduler;


namespace BusinessLogic.Services.Jobs
{
    public class QuartzConfig
    {

        public static void AddQuartzServices(IServiceCollection services)
        {
            services.AddQuartz(q =>
            {
                q.SchedulerId = "TrackingScheduler";
                q.SchedulerName = "Tracking Scheduler";

                // Job untuk create table setiap hari jam 00:01
                var createJobKey = new JobKey("CreateDailyTrackingTable", "Tracking");
                q.AddJob<CreateDailyTrackingTableJob>(opts => opts.WithIdentity(createJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(createJobKey)
                    .WithIdentity("CreateDailyTrackingTableTrigger", "Tracking")
                    .WithCronSchedule("0 1 0 * * ?")); // Setiap hari jam 00:01

                // Job untuk drop table lama setiap hari jam 00:05
                var dropJobKey = new JobKey("DropOldTrackingTables", "Tracking");
                q.AddJob<DropOldTrackingTablesJob>(opts => opts.WithIdentity(dropJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(dropJobKey)
                    .WithIdentity("DropOldTrackingTablesTrigger", "Tracking")
                    .WithCronSchedule("0 5 0 * * ?")); // Setiap hari jam 00:05
            });

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        }
    }
}


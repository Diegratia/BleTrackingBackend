using System;
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

                // Job untuk create table setiap hari jam 00:01 WIB (17:01 UTC)
                var createJobKey = new JobKey("CreateDailyTrackingTable", "Tracking");
                q.AddJob<CreateDailyTrackingTableJob>(opts => opts.WithIdentity(createJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(createJobKey)
                    .WithIdentity("CreateDailyTrackingTableTrigger", "Tracking")
                    .WithCronSchedule("0 1 17 * * ?")); // 17:01 UTC = 00:01 WIB

                // Job untuk drop table lama setiap hari jam 00:05 WIB (17:05 UTC)
                var dropJobKey = new JobKey("DropOldTrackingTables", "Tracking");
                q.AddJob<DropOldTrackingTablesJob>(opts => opts.WithIdentity(dropJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(dropJobKey)
                    .WithIdentity("DropOldTrackingTablesTrigger", "Tracking")
                    .WithCronSchedule("0 5 17 * * ?")); // 17:05 UTC = 00:05 WIB
            });

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        }
    }
}
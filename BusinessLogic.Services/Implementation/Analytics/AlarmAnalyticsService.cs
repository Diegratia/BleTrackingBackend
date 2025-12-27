using AutoMapper;
using BusinessLogic.Services.Interface.Analytics;
using Data.ViewModels;
using Data.ViewModels.AlarmAnalytics;
using Repositories.Repository.Analytics;
using Repositories.Repository.RepoModel;


namespace BusinessLogic.Services.Implementation.Analytics
{
    public class AlarmAnalyticsService : IAlarmAnalyticsService
    {
        private readonly AlarmAnalyticsRepository _repo;
        private readonly IMapper _mapper;

        public AlarmAnalyticsService(AlarmAnalyticsRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // ===================================================================
        // 1️⃣ Daily Summary
        // ===================================================================
        public async Task<ResponseCollection<AlarmDailySummaryVM>> GetDailySummaryAsync(AlarmAnalyticsRequest request)
        {
            var filter = _mapper.Map<AlarmAnalyticsRequestRM>(request);
            var data = await _repo.GetDailySummaryAsync(filter);

            var result = data.Select(x => new AlarmDailySummaryVM
            {
                Date = x.Date,
                Total = x.Total,
                Done = x.Done,
                Cancelled = x.Cancelled,
                AvgResponseSeconds = x.AvgResponseSeconds
            });

            return ResponseCollection<AlarmDailySummaryVM>
                .Ok(result, "Daily alarm summary retrieved successfully");
        }

        // ===================================================================
        // 2️⃣ Area Summary
        // ===================================================================
        public async Task<ResponseCollection<AlarmAreaSummaryVM>> GetAreaSummaryAsync(AlarmAnalyticsRequest request)
        {
            var filter = _mapper.Map<AlarmAnalyticsRequestRM>(request);
            var data = await _repo.GetAreaSummaryAsync(filter);

            var result = data.Select(x => new AlarmAreaSummaryVM
            {
                FloorplanMaskedAreaId = x.AreaId,
                AreaName = x.AreaName,
                Total = x.Total,
                Done = x.Done,
                AvgResponseSeconds = x.AvgResponseSeconds
            });

            return ResponseCollection<AlarmAreaSummaryVM>
                .Ok(result, "Area summary retrieved successfully");
        }

        // ===================================================================
        // 3️⃣ Operator Summary
        // ===================================================================
        public async Task<ResponseCollection<AlarmOperatorSummaryVM>> GetOperatorSummaryAsync(AlarmAnalyticsRequest request)
        {
            var filter = _mapper.Map<AlarmAnalyticsRequestRM>(request);
            var data = await _repo.GetOperatorSummaryAsync(filter);

            var result = data.Select(x => new AlarmOperatorSummaryVM
            {
                OperatorName = x.OperatorName,
                TotalHandled = x.TotalHandled,
                AvgResponseSeconds = x.AvgResponseSeconds
            });

            return ResponseCollection<AlarmOperatorSummaryVM>
                .Ok(result, "Operator summary retrieved successfully");
        }

        // ===================================================================
        // 4️⃣ Status Summary
        // ===================================================================
        public async Task<ResponseCollection<AlarmStatusSummaryVM>> GetStatusSummaryAsync(AlarmAnalyticsRequest request)
        {
            var filter = _mapper.Map<AlarmAnalyticsRequestRM>(request);
            var data = await _repo.GetStatusSummaryAsync(filter);

            var result = data.Select(x => new AlarmStatusSummaryVM
            {
                Status = x.Status,
                Total = x.Total
            });

            return ResponseCollection<AlarmStatusSummaryVM>
                .Ok(result, "Status summary retrieved successfully");
        }

        // ===================================================================
        // 5️⃣ Building Summary
        // ===================================================================
        public async Task<ResponseCollection<AlarmBuildingSummaryVM>> GetBuildingSummaryAsync(AlarmAnalyticsRequest request)
        {
            var filter = _mapper.Map<AlarmAnalyticsRequestRM>(request);
            var data = await _repo.GetBuildingSummaryAsync(filter);

            var result = data.Select(x => new AlarmBuildingSummaryVM
            {
                BuildingId = x.BuildingId,
                BuildingName = x.BuildingName,
                Total = x.Total,
                Done = x.Done,
                AvgResponseSeconds = x.AvgResponseSeconds
            });

            return ResponseCollection<AlarmBuildingSummaryVM>
                .Ok(result, "Building summary retrieved successfully");
        }

        // ===================================================================
        // 6️⃣ Visitor Summary
        // ===================================================================
        public async Task<ResponseCollection<AlarmVisitorSummaryVM>> GetVisitorSummaryAsync(AlarmAnalyticsRequest request)
        {
            var filter = _mapper.Map<AlarmAnalyticsRequestRM>(request);
            var data = await _repo.GetVisitorSummaryAsync(filter);

            var result = data.Select(x => new AlarmVisitorSummaryVM
            {
                VisitorId = x.VisitorId,
                VisitorName = x.VisitorName,
                TotalTriggered = x.TotalTriggered,
                Done = x.Done
            });

            return ResponseCollection<AlarmVisitorSummaryVM>
                .Ok(result, "Visitor summary retrieved successfully");
        }

        // ===================================================================
        // 7️⃣ Time of Day Summary
        // ===================================================================
        public async Task<ResponseCollection<AlarmTimeOfDaySummaryVM>> GetTimeOfDaySummaryAsync(AlarmAnalyticsRequest request)
        {
            var filter = _mapper.Map<AlarmAnalyticsRequestRM>(request);
            var data = await _repo.GetTimeOfDaySummaryAsync(filter);

            var result = data.Select(x => new AlarmTimeOfDaySummaryVM
            {
                Hour = x.Hour,
                Total = x.Total
            });

            return ResponseCollection<AlarmTimeOfDaySummaryVM>
                .Ok(result, "Time of day summary retrieved successfully");
        }

        // ===================================================================
        // 8️⃣ Weekly Trend
        // ===================================================================
        public async Task<ResponseCollection<AlarmWeeklyTrendVM>> GetWeeklyTrendAsync(AlarmAnalyticsRequest request)
        {
            var filter = _mapper.Map<AlarmAnalyticsRequestRM>(request);
            var data = await _repo.GetWeeklyTrendAsync(filter);

            var result = data.Select(x => new AlarmWeeklyTrendVM
            {
                // ✅ Ambil dari DateTime-nya
                DayOfWeek = x.Date.ToString("dddd"), // contoh: "Monday"
                Total = x.Total
            });

            return ResponseCollection<AlarmWeeklyTrendVM>.Ok(result, "Weekly trend retrieved successfully");
        }


        // ===================================================================
        // 9️⃣ Floor Summary
        // ===================================================================
        public async Task<ResponseCollection<AlarmFloorSummaryVM>> GetFloorSummaryAsync(AlarmAnalyticsRequest request)
        {
            var filter = _mapper.Map<AlarmAnalyticsRequestRM>(request);
            var data = await _repo.GetFloorSummaryAsync(filter);

            var result = data.Select(x => new AlarmFloorSummaryVM
            {
                FloorId = x.FloorId,
                FloorName = x.FloorName,
                Total = x.Total,
                Done = x.Done,
                AvgResponseSeconds = x.AvgResponseSeconds
            });

            return ResponseCollection<AlarmFloorSummaryVM>
                .Ok(result, "Floor summary retrieved successfully");
        }

        // ===================================================================
        // 🔟 Duration Summary
        // ===================================================================
        public async Task<ResponseCollection<AlarmDurationSummaryVM>> GetDurationSummaryAsync(AlarmAnalyticsRequest request)
        {
            var filter = _mapper.Map<AlarmAnalyticsRequestRM>(request);
            var data = await _repo.GetDurationSummaryAsync(filter);

            var result = data.Select(x => new AlarmDurationSummaryVM
            {
                DurationRange = x.DurationRange,
                Count = x.Count
            });

            return ResponseCollection<AlarmDurationSummaryVM>
                .Ok(result, "Duration summary retrieved successfully");
        }

        // ===================================================================
        // 1️⃣1️⃣ Trend by Action
        // ===================================================================
        public async Task<ResponseCollection<AlarmTrendByActionVM>> GetTrendByActionAsync(AlarmAnalyticsRequest request)
        {
            var filter = _mapper.Map<AlarmAnalyticsRequestRM>(request);
            var data = await _repo.GetTrendByActionAsync(filter);

            var result = data.Select(x => new AlarmTrendByActionVM
            {
                Date = x.Date,
                ActionStatus = x.ActionStatus,
                Total = x.Total
            });

            return ResponseCollection<AlarmTrendByActionVM>
                .Ok(result, "Trend by action retrieved successfully");
        }
    }
}




// using BusinessLogic.Services.Interface;
// using BusinessLogic.Services.Interface.Analytics;
// using Repositories.Repository;
// using Repositories.Repository.Analytics;
// using System;
// using System.Linq;
// using System.Threading.Tasks;

// namespace BusinessLogic.Services.Implementation.Analytics
// {
//     public class AlarmAnalyticsService : IAlarmAnalyticsService
//     {
//         private readonly AlarmAnalyticsRepository _repository;

//         public AlarmAnalyticsService(AlarmAnalyticsRepository repository)
//         {
//             _repository = repository;
//         }

//         public async Task<object> GetDailySummaryAsync(DateTime from, DateTime to)
//         {
//             var data = await _repository.GetDailySummaryAsync(from, to);
//             var result = data.Select(x => new
//             {
//                 date = x.Date,
//                 total = x.Total,
//                 done = x.Done,
//                 cancelled = x.Cancelled,
//                 avgResponseSeconds = x.AvgResponseSeconds
//             });
//             return new { data = result };
//         }

//         public async Task<object> GetAreaSummaryAsync(DateTime from, DateTime to)
//         {
//             var data = await _repository.GetAreaSummaryAsync(from, to);
//             var result = data.Select(x => new
//             {
//                 floorplanMaskedAreaId = x.AreaId,
//                 areaName = x.AreaName,
//                 total = x.Total,
//                 done = x.Done,
//                 avgResponseSeconds = x.AvgResponseSeconds
//             });
//             return new { data = result };
//         }

//         public async Task<object> GetOperatorSummaryAsync(DateTime from, DateTime to)
//         {
//             var data = await _repository.GetOperatorSummaryAsync(from, to);
//             var result = data.Select(x => new
//             {
//                 operatorName = x.OperatorName,
//                 totalHandled = x.TotalHandled,
//                 avgResponseSeconds = x.AvgResponseSeconds
//             });
//             return new { data = result };
//         }
//     }
// }

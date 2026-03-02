using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Repositories.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Repositories.Repository.RepoModel;
using AutoMapper;
using Azure;

namespace BusinessLogic.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly ILogger<DashboardService> _logger;
        private readonly CardRepository _cardRepo;
        private readonly FloorplanDeviceRepository _deviceRepo;
        private readonly AlarmTriggersRepository _alarmRepo;
        private readonly FloorplanMaskedAreaRepository _areaRepo;
        private readonly VisitorRepository _visitorRepo;
        private readonly MstMemberRepository _memberRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;


        public DashboardService(
            CardRepository cardRepo,
            FloorplanDeviceRepository deviceRepo,
            AlarmTriggersRepository alarmRepo,
            FloorplanMaskedAreaRepository areaRepo,
            VisitorRepository visitorRepo,
            MstMemberRepository memberRepo,
            IHttpContextAccessor httpContextAccessor,
            ILogger<DashboardService> logger,
            IMapper mapper
            )
        {
            _cardRepo = cardRepo;
            _deviceRepo = deviceRepo;
            _alarmRepo = alarmRepo;
            _areaRepo = areaRepo;
            _visitorRepo = visitorRepo;
            _memberRepo = memberRepo;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ResponseSingle<DashboardSummaryDto>> GetSummaryAsync()
        {
            try
            {
                var appIdString = _httpContextAccessor.HttpContext?
                    .User.FindFirst("ApplicationId")?.Value;

                Guid appId = Guid.Empty;
                if (!string.IsNullOrEmpty(appIdString))
                {
                    Guid.TryParse(appIdString, out appId);
                }

                var activeBeaconCount = await _cardRepo.GetCountAsync();
                var nonActiveBeaconCount = await _cardRepo.GetNonActiveCountAsync();

                var topNonActiveBeaconDto = (await _cardRepo.GetTopUnUsedCardAsync(5))
                    .Select(rm => new CardDashboardDto
                    {
                        Id = rm.Id,
                        Dmac = rm.Dmac,
                        CardNumber = rm.CardNumber,
                    })
                    .ToList();

                var topActiveBeaconDto = (await _cardRepo.GetTopUsedCardAsync(5))
                    .Select(rm => new CardDashboardDto
                    {
                        Id = rm.Id,
                        Dmac = rm.Dmac,
                        CardNumber = rm.CardNumber,
                    })
                    .ToList();

                var activeGatewayCount = await _deviceRepo.GetCountAsync();

                var topReadersDto = (await _deviceRepo.GetTopReadersAsync())
                    .Select(rm => new ReaderSummaryDto
                    {
                        Id = rm.Id,
                        Name = rm.Name,
                    })
                    .ToList();

                var alarmCount = await _alarmRepo.GetCountAsync();

                var topTriggersDto = (await _alarmRepo.GetTopTriggersAsync(5))
                    .Select(rm => new AlarmTriggersSummary
                    {
                        Id = rm.Id,
                        BeaconId = rm.BeaconId,
                    })
                    .ToList();

                var areaCount = await _areaRepo.GetCountAsync();

                var topAreasDto = (await _areaRepo.GetTopAreasAsync(5))
                    .Select(rm => new AreaSummaryDto
                    {
                        Id = rm.Id,
                        Name = rm.Name,
                    })
                    .ToList();

                var visitorBlacklistCount = await _visitorRepo.GetBlacklistedCountAsync();
                var memberBlacklistCount = await _memberRepo.GetBlacklistedCountAsync();

                var dashboard = new DashboardSummaryDto
                {
                    ActiveBeaconCount = activeBeaconCount,
                    NonActiveBeaconCount = nonActiveBeaconCount,
                    ActiveGatewayCount = activeGatewayCount,
                    AreaCount = areaCount,
                    AlarmCount = alarmCount,
                    BlacklistedCount = visitorBlacklistCount + memberBlacklistCount,

                    TopActiveBeacon = topActiveBeaconDto,
                    TopNonActiveBeacon = topNonActiveBeaconDto,
                    TopReaders = topReadersDto,
                    TopTriggers = topTriggersDto,
                    TopAreas = topAreasDto,

                    ApplicationId = appId
                };

                return ResponseSingle<DashboardSummaryDto>.Ok(
                    dashboard,
                    "Dashboard summary retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting dashboard summary");

                return ResponseSingle<DashboardSummaryDto>.Error(
                    "Internal server error"
                );
            }
        }

        public async Task<ResponseSingle<List<AreaSummaryDto>>> GetTopAreasAsync(int topCount = 5)
        {
            try
            {
                var dataRM = await _areaRepo.GetTopAreasAsync(topCount);

                // Manual mapping
                var dataDto = dataRM.Select(rm => new AreaSummaryDto
                {
                    Id = rm.Id,
                    Name = rm.Name,
                }).ToList();
                return ResponseSingle<List<AreaSummaryDto>>.Ok(dataDto, $"Success get top {topCount} areas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top areas");
                return ResponseSingle<List<AreaSummaryDto>>.Error($"Internal error: {ex.Message}");
            }
        }

        public async Task<ResponseSingle<CardUsageCountDto>> GetCardStatsAsync()
        {
            var appIdString = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId")?.Value;
            Guid appId = Guid.Empty;
            if (!string.IsNullOrEmpty(appIdString))
            {
                Guid.TryParse(appIdString, out appId);
            }

            try
            {
                var data = await _cardRepo.CardUsageCountAsync();
                var dto = _mapper.Map<CardUsageCountDto>(data);
                return ResponseSingle<CardUsageCountDto>.Ok(dto, "Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting area count");
                return ResponseSingle<CardUsageCountDto>.Error($"Internal error: {ex.Message}");
            }
        }

        public async Task<List<BlacklistLogRM>> GetBlacklistLogsAsync()
        {
            var logs = await _memberRepo.GetBlacklistLogsAsync(); 
            return logs;
        }
    }

}
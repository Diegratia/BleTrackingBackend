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

namespace BusinessLogic.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly ILogger<DashboardService> _logger;
        private readonly CardRepository _cardRepo;
        private readonly FloorplanDeviceRepository _deviceRepo;
        private readonly AlarmTriggersRepository _alarmRepo;
        private readonly FloorplanMaskedAreaRepository _areaRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        


        public DashboardService(
            CardRepository cardRepo,
            FloorplanDeviceRepository deviceRepo,
            AlarmTriggersRepository alarmRepo,
            FloorplanMaskedAreaRepository areaRepo,
            IHttpContextAccessor httpContextAccessor,
            ILogger<DashboardService> logger,
            IMapper mapper
            )
        {
            _cardRepo = cardRepo;
            _deviceRepo = deviceRepo;
            _alarmRepo = alarmRepo;
            _areaRepo = areaRepo;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var appIdString = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId")?.Value;

            Guid appId = Guid.Empty;
            if (!string.IsNullOrEmpty(appIdString))
            {
                Guid.TryParse(appIdString, out appId);
            }

            var activeBeaconCount = await _cardRepo.GetCountAsync();
            var nonActiveBeaconCount = await _cardRepo.GetNonActiveCountAsync();
            var topNonActiveBeaconRM = await _cardRepo.GetTopUnUsedCardAsync(5);
            var topNonActiveBeaconDto = topNonActiveBeaconRM.Select(rm => new CardDashboardDto
                {
                        Id = rm.Id,
                        Dmac = rm.Dmac,
                        CardNumber = rm.CardNumber,
            }).ToList();
            var topActiveBeaconRM = await _cardRepo.GetTopUsedCardAsync(5);
            var topActiveBeaconDto = topActiveBeaconRM.Select(rm => new CardDashboardDto
            {
                Id = rm.Id,
                Dmac = rm.Dmac,
                CardNumber = rm.CardNumber,
            }).ToList();
            var activeGatewayCount = await _deviceRepo.GetCountAsync();
            var topReadersRM = await _deviceRepo.GetTopReadersAsync();
            var topReadersDto = topReadersRM.Select(rm => new ReaderSummaryDto
                {
                        Id = rm.Id,
                        Name = rm.Name,
            }).ToList();
            var alarmCount = await _alarmRepo.GetCountAsync();
            var topTriggersRM = await _alarmRepo.GetTopTriggersAsync(5);
            var topTriggersDto = topTriggersRM.Select(rm => new AlarmTriggersSummary
                {
                        Id = rm.Id,
                        BeaconId = rm.BeaconId,
            }).ToList();
            var areaCount = await _areaRepo.GetCountAsync(); // TOTAL COUNT
            var topAreasRM = await _areaRepo.GetTopAreasAsync(5);
            var topAreasDto = topAreasRM.Select(rm => new AreaSummaryDto
                    {
                        Id = rm.Id,
                        Name = rm.Name,
                    }).ToList();
            return new DashboardSummaryDto
            {
                ActiveBeaconCount = activeBeaconCount,
                TopActiveBeacon = topActiveBeaconDto,
                NonActiveBeaconCount = nonActiveBeaconCount,
                TopNonActiveBeacon = topNonActiveBeaconDto,
                ActiveGatewayCount = activeGatewayCount,
                TopReaders = topReadersDto,
                AlarmCount = alarmCount,
                TopTriggers = topTriggersDto, 
                AreaCount = areaCount, // TOTAL semua area
                TopAreas = topAreasDto,
                ApplicationId = appId
            };
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
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Repositories.Repository;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services.Implementation
{
        public class DashboardService : IDashboardService
    {
        private readonly CardRepository _cardRepo;
        private readonly FloorplanDeviceRepository _deviceRepo;
        private readonly AlarmTriggersRepository _alarmRepo;
        private readonly BlacklistAreaRepository _blacklistRepo;
        private readonly FloorplanMaskedAreaRepository _areaRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DashboardService(
            CardRepository cardRepo,
            FloorplanDeviceRepository deviceRepo,
            AlarmTriggersRepository alarmRepo,
            BlacklistAreaRepository blacklistRepo,
            FloorplanMaskedAreaRepository areaRepo,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _cardRepo = cardRepo;
            _deviceRepo = deviceRepo;
            _alarmRepo = alarmRepo;
            _blacklistRepo = blacklistRepo;
            _areaRepo = areaRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var appIdString  = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId")?.Value;

                Guid appId = Guid.Empty;
                if (!string.IsNullOrEmpty(appIdString))
                {
                    Guid.TryParse(appIdString, out appId);
                }

            var activeBeaconCount = await _cardRepo.GetCountAsync();
            var nonActiveBeaconCount = await _cardRepo.GetNonActiveCountAsync();
            var activeGatewayCount = await _deviceRepo.GetCountAsync();
            var blacklistCount = await _blacklistRepo.GetCountAsync();
            var alarmCount = await _alarmRepo.GetCountAsync();
            var areaCount = await _areaRepo.GetCountAsync();

            return new DashboardSummaryDto
            {
                ActiveBeaconCount = activeBeaconCount,
                NonActiveBeaconCount = nonActiveBeaconCount,
                ActiveGatewayCount = activeGatewayCount,
                BlacklistCount = blacklistCount,
                AlarmCount = alarmCount,
                AreaCount = areaCount,
                ApplicationId = appId
            };
        }
    }

}
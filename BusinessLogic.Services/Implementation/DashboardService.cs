using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Repositories.Repository;

namespace BusinessLogic.Services.Implementation
{
        public class DashboardService : IDashboardService
    {
        private readonly CardRepository _cardRepo;
        private readonly FloorplanDeviceRepository _deviceRepo;
        private readonly AlarmTriggersRepository _alarmRepo;
        private readonly BlacklistAreaRepository _blacklistRepo;
        private readonly FloorplanMaskedAreaRepository _areaRepo;

        public DashboardService(
            CardRepository cardRepo,
            FloorplanDeviceRepository deviceRepo,
            AlarmTriggersRepository alarmRepo,
            BlacklistAreaRepository blacklistRepo,
            FloorplanMaskedAreaRepository areaRepo)
        {
            _cardRepo = cardRepo;
            _deviceRepo = deviceRepo;
            _alarmRepo = alarmRepo;
            _blacklistRepo = blacklistRepo;
            _areaRepo = areaRepo;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
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
                AreaCount = areaCount
            };
        }
    }

}
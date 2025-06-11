using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;

namespace BusinessLogic.Services.Implementation
{
    public class MstTrackingLogService : IMstTrackingLogService
    {
        private readonly MstTrackingLogRepository _repository;
        private readonly IMapper _mapper;

        public MstTrackingLogService(MstTrackingLogRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MstTrackingLogDto>> GetMstTrackingLogsAsync()
        {
            var logs = await _repository.GetMstTrackingLogsAsync();
            return _mapper.Map<IEnumerable<MstTrackingLogDto>>(logs);
        }

        public async Task<MstTrackingLogDto> GetMstTrackingLogByIdAsync(Guid id)
        {
            var log = await _repository.GetMstTrackingLogByIdAsync(id);
            return log == null ? null : _mapper.Map<MstTrackingLogDto>(log);
        }

        public async Task DeleteMstTrackingLogAsync(Guid id)
        {
            await _repository.DeleteMstTrackingLogAsync(id);
        }
    }
}
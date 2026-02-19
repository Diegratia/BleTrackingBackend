using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using Repositories.DbContexts;

namespace BusinessLogic.Services.Implementation
{
    public class TimeBlockService : BaseService, ITimeBlockService
    {
        private readonly TimeBlockRepository _repository;
        private readonly TimeGroupRepository _timeGroupRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BleTrackingDbContext _context;
        private readonly IAuditEmitter _audit;

        public TimeBlockService(
            TimeBlockRepository repository,
            TimeGroupRepository timeGroupRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            BleTrackingDbContext context,
            IAuditEmitter audit
        ) : base(httpContextAccessor)
        {
            _repository = repository;
            _timeGroupRepository = timeGroupRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _audit = audit;
        }

        public async Task<TimeBlockDto> CreateAsync(TimeBlockCreateDto dto)
        {
            // Validate TimeGroup exists and belongs to same Application
            if (dto.TimeGroupId.HasValue)
            {
                var invalidGroupIds = await _timeGroupRepository
                    .CheckInvalidTimeGroupOwnershipAsync(
                        new List<Guid?> { dto.TimeGroupId }, AppId);

                if (invalidGroupIds.Any())
                    throw new UnauthorizedException(
                        $"TimeGroupId does not belong to this Application");
            }

            var timeBlock = _mapper.Map<TimeBlock>(dto);
            SetCreateAudit(timeBlock);

            await _repository.AddAsync(timeBlock);
            _audit.Created("TimeBlock", timeBlock.Id, $"TimeBlock created");

            return _mapper.Map<TimeBlockDto>(timeBlock);
        }
    }
}
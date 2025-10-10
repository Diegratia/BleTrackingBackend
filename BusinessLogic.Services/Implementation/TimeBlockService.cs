using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;

namespace BusinessLogic.Services.Implementation
{
    public class TimeBlockService : ITimeBlockService
    {
        private readonly TimeBlockRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TimeBlockService(TimeBlockRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

         public async Task<TimeBlockDto> CreateAsync(TimeBlockCreateDto dto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var timeBlock = _mapper.Map<TimeBlock>(dto);
            timeBlock.Status = 1;
            timeBlock.CreatedAt = DateTime.UtcNow;
            timeBlock.UpdatedAt = DateTime.UtcNow;
            timeBlock.CreatedBy = username;
            timeBlock.UpdatedBy = username;

            await _repository.AddAsync(timeBlock);
            return _mapper.Map<TimeBlockDto>(timeBlock);
        }
    }
}
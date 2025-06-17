using AutoMapper;
using BusinessLogic.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;
using Repositories.DbContexts;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Repositories;

namespace BusinessLogic.Services.Implementation
{
    public class VisitorCardService : IVisitorCardService
    {
        private readonly VisitorCardRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VisitorCardService(VisitorCardRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<VisitorCardDto> GetByIdAsync(Guid id)
        {
            var visitorCard = await _repository.GetByIdAsync(id);
            return visitorCard == null ? null : _mapper.Map<VisitorCardDto>(visitorCard);
        }

        public async Task<IEnumerable<VisitorCardDto>> GetAllAsync()
        {
            var visitorCard = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<VisitorCardDto>>(visitorCard);
        }

        public async Task<VisitorCardDto> CreateAsync(VisitorCardCreateDto createDto)
        {
            var application = await _repository.GetApplicationByIdAsync(createDto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {createDto.ApplicationId} not found.");

            var visitorCard = _mapper.Map<VisitorCard>(createDto);
            visitorCard.Id = Guid.NewGuid();
            visitorCard.CheckinStatus = 1;
            visitorCard.EnableStatus = 1;
            visitorCard.IsMember = 1;
            visitorCard.Status = 1;

            var createdvisitorCard = await _repository.AddAsync(visitorCard);
            return _mapper.Map<VisitorCardDto>(createdvisitorCard);
        }

         public async Task UpdateAsync(Guid id, VisitorCardUpdateDto updateDto)
        {
            var application = await _repository.GetApplicationByIdAsync(updateDto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {updateDto.ApplicationId} not found.");

            var visitorCard = await _repository.GetByIdAsync(id);
            if (visitorCard == null)
                throw new KeyNotFoundException("VisitorCard not found");

            _mapper.Map(updateDto, visitorCard);
            await _repository.UpdateAsync(visitorCard);
        }

        public async Task DeleteAsync(Guid id)
        {
            var visitorCard = await _repository.GetByIdAsync(id);
            visitorCard.Status = 0;
            await _repository.DeleteAsync(id);
        }

    }
    
}
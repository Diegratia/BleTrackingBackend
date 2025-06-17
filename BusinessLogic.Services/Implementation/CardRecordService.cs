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
    public class CardRecordService : ICardRecordService
    {
        private readonly CardRecordRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CardRecordService(CardRecordRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CardRecordDto> GetByIdAsync(Guid id)
        {
            var cardRecord = await _repository.GetByIdAsync(id);
            return cardRecord == null ? null : _mapper.Map<CardRecordDto>(cardRecord);
        }

        public async Task<IEnumerable<CardRecordDto>> GetAllAsync()
        {
            var cardRecord = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<CardRecordDto>>(cardRecord);
        }

        public async Task<CardRecordDto> CreateAsync(CardRecordCreateDto createDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ;
            var card = await _repository.GetCardByIdAsync(createDto.CardId);
            if (card == null)
                throw new ArgumentException($"Card with ID {createDto.CardId} not found.");

            // var visitor = await _repository.GetVisitorByIdAsync(createDto.VisitorId);
            // if (application == null)
            //     throw new ArgumentException($"Visitor with ID {createDto.VisitorId} not found.");

            // var member = await _repository.GetMemberByIdAsync(createDto.MemberId);
            // if (application == null)
            //     throw new ArgumentException($"Member with ID {createDto.MemberId} not found.");

            var cardRecord = _mapper.Map<CardRecord>(createDto);
            cardRecord.Id = Guid.NewGuid();
            cardRecord.Timestamp = DateTime.UtcNow;
            cardRecord.CheckinBy = username ?? "";
            cardRecord.CheckoutBy = username ?? "";

            var createdcardRecord = await _repository.AddAsync(cardRecord);
            return _mapper.Map<CardRecordDto>(createdcardRecord);
        }

         public async Task UpdateAsync(Guid id, CardRecordUpdateDto updateDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ;
            var card = await _repository.GetCardByIdAsync(updateDto.CardId);
            if (card == null)
                throw new ArgumentException($"Card with ID {updateDto.CardId} not found.");

            // var visitor = await _repository.GetVisitorByIdAsync(createDto.VisitorId);
            // if (application == null)
            //     throw new ArgumentException($"Visitor with ID {createDto.VisitorId} not found.");

            // var member = await _repository.GetMemberByIdAsync(createDto.MemberId);
            // if (application == null)
            //     throw new ArgumentException($"Member with ID {createDto.MemberId} not found.");

            var cardRecord = await _repository.GetByIdAsync(id);
            if (cardRecord == null)
                throw new KeyNotFoundException("Card Record not found");

            _mapper.Map(updateDto, cardRecord);
            await _repository.UpdateAsync(cardRecord);
        }

        public async Task DeleteAsync(Guid id)
        {
            var cardRecord = await _repository.GetByIdAsync(id);
            if (cardRecord == null)
            {
                throw new KeyNotFoundException("Card Record Not Found");
            }
            await _repository.DeleteAsync(id);
        }

    }
    
}
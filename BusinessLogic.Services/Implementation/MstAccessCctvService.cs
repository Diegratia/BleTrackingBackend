using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;

namespace BusinessLogic.Services.Implementation
{
    public class MstAccessCctvService : IMstAccessCctvService
    {
        private readonly MstAccessCctvRepository _repository;
        private readonly IMapper _mapper;

        public MstAccessCctvService(MstAccessCctvRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<MstAccessCctvDto> GetByIdAsync(Guid id)
        {
            var accessCctv = await _repository.GetByIdAsync(id);
            return accessCctv == null ? null : _mapper.Map<MstAccessCctvDto>(accessCctv);
        }

        public async Task<IEnumerable<MstAccessCctvDto>> GetAllAsync()
        {
            var accessCctvs = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstAccessCctvDto>>(accessCctvs);
        }

        public async Task<MstAccessCctvDto> CreateAsync(MstAccessCctvCreateDto createDto)
        {
            var accessCctv = _mapper.Map<MstAccessCctv>(createDto);

            accessCctv.Id = Guid.NewGuid();
            accessCctv.Status = 1;
            accessCctv.CreatedBy = "System";
            accessCctv.CreatedAt = DateTime.UtcNow;
            accessCctv.UpdatedBy = "System";
            accessCctv.UpdatedAt = DateTime.UtcNow;

            // notes untuk nanti, jika ingin memisahkan service dan repository, bisa memisahkan proses logika bisnis
            // dengan service, dan proses database dengan repository, contohnya pada _context
            await _repository.AddAsync(accessCctv);
            return _mapper.Map<MstAccessCctvDto>(accessCctv);
        }

        public async Task UpdateAsync(Guid id, MstAccessCctvUpdateDto updateDto)
        {
            var accessCctv = await _repository.GetByIdAsync(id);
            if (accessCctv == null)
                throw new KeyNotFoundException("Access CCTV not found");

            accessCctv.UpdatedBy = "System";
            accessCctv.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(updateDto, accessCctv);
            await _repository.UpdateAsync(accessCctv);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.SoftDeleteAsync(id);
        }
    }
}
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
    public class MstDepartmentService : IMstDepartmentService
    {
        private readonly MstDepartmentRepository _repository;
        private readonly IMapper _mapper;

        public MstDepartmentService(MstDepartmentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<MstDepartmentDto> GetByIdAsync(Guid id)
        {
            var department = await _repository.GetByIdAsync(id);
            return department == null ? null : _mapper.Map<MstDepartmentDto>(department);
        }

        public async Task<IEnumerable<MstDepartmentDto>> GetAllAsync()
        {
            var departments = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstDepartmentDto>>(departments);
        }

        public async Task<MstDepartmentDto> CreateAsync(MstDepartmentCreateDto createDto)
        {
            var department = _mapper.Map<MstDepartment>(createDto);
            department.Id = Guid.NewGuid();
            department.CreatedBy = "System"; // Ganti dengan user dari autentikasi
            department.Status = 1;

            var createdDepartment = await _repository.AddAsync(department);
            return _mapper.Map<MstDepartmentDto>(createdDepartment);
        }

        public async Task UpdateAsync(Guid id, MstDepartmentUpdateDto updateDto)
        {
            var department = await _repository.GetByIdAsync(id);
            if (department == null)
                throw new KeyNotFoundException("Department not found");

            _mapper.Map(updateDto, department);
            department.UpdatedBy = "System"; 

            await _repository.UpdateAsync(department);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
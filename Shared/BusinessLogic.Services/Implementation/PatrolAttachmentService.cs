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
using System.ComponentModel.DataAnnotations;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using DataView;

namespace BusinessLogic.Services.Implementation
{
    public class PatrolAttachmentService : BaseService, IPatrolAttachmentService
    {
        private readonly PatrolAttachmentRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;


        public PatrolAttachmentService(
            PatrolAttachmentRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IAuditEmitter audit
            ) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _audit = audit;
        }

        public async Task<PatrolAttachmentDto?> GetByIdAsync(Guid id)
        {
            var attachment = await _repository.GetByIdAsync(id);
            if (attachment == null)
                throw new NotFoundException($"Attachment with id {id} not found");
            return _mapper.Map<PatrolAttachmentDto>(attachment);
        }

        public async Task<IEnumerable<PatrolAttachmentDto>> GetAllAsync()
        {
            var patrolAreas = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<PatrolAttachmentDto>>(patrolAreas);
        }

        public async Task<PatrolAttachmentDto> CreateAsync(PatrolAttachmentCreateDto createDto)
        {
            if (!await _repository.CaseExistsAsync(createDto.PatrolCaseId!.Value))
                throw new NotFoundException($"Case with id {createDto.PatrolCaseId} not found");

            var attachment = _mapper.Map<PatrolCaseAttachment>(createDto);
            SetCreateAudit(attachment);
            await _repository.AddAsync(attachment);
            await _audit.Created(
                "Patrol Area",
                attachment.Id,
                "Created attachment",
                new { attachment.FileType }
            );
            return _mapper.Map<PatrolAttachmentDto>(attachment);
        }

        public async Task<PatrolAttachmentDto> UpdateAsync(Guid id, PatrolAreaUpdateDto updateDto)
        {
            var attachment = await _repository.GetByIdAsync(id);
            if (attachment == null)
                throw new NotFoundException($"Attachment with id {id} not found");

            SetUpdateAudit(attachment);
            _mapper.Map(updateDto, attachment);
            await _repository.UpdateAsync(attachment);
            await _audit.Updated(
                "Patrol Area",
                attachment.Id,
                "Updated attachment",
                new { attachment.FileType }
            );
            return _mapper.Map<PatrolAttachmentDto>(attachment);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "UpdatedAt", "Name", "Status" };

            var filterService = new GenericDataTableService<PatrolCaseAttachment, PatrolAttachmentDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
    }
}
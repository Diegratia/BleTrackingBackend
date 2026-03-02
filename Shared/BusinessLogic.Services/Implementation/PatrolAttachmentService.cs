using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    public class PatrolAttachmentService : BaseService, IPatrolAttachmentService
    {
        private readonly PatrolAttachmentRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAuditEmitter _audit;

        public PatrolAttachmentService(
            PatrolAttachmentRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IAuditEmitter audit) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _audit = audit;
        }

        public async Task<PatrolAttachmentRead> GetByIdAsync(Guid id)
        {
            var attachment = await _repository.GetByIdAsync(id);
            if (attachment == null)
                throw new NotFoundException($"Attachment with id {id} not found");
            return attachment;
        }

        public async Task<IEnumerable<PatrolAttachmentRead>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<PatrolAttachmentRead> CreateAsync(PatrolAttachmentCreateDto createDto)
        {
            if (!await _repository.CaseExistsAsync(createDto.PatrolCaseId!.Value))
                throw new NotFoundException($"Case with id {createDto.PatrolCaseId} not found");

            var attachment = _mapper.Map<PatrolCaseAttachment>(createDto);
            attachment.ApplicationId = AppId;
            SetCreateAudit(attachment);
            attachment.UploadedAt = DateTime.UtcNow;

            await _repository.AddAsync(attachment);
            _audit.Created("PatrolAttachment", attachment.Id, $"Created attachment for case {attachment.PatrolCaseId}");

            return await _repository.GetByIdAsync(attachment.Id);
        }

        public async Task<PatrolAttachmentRead> UpdateAsync(Guid id, PatrolAttachmentUpdateDto updateDto)
        {
            var attachment = await _repository.GetByIdEntityAsync(id);
            if (attachment == null)
                throw new NotFoundException($"Attachment with id {id} not found");

            SetUpdateAudit(attachment);
            _mapper.Map(updateDto, attachment);
            await _repository.UpdateAsync(attachment);
            _audit.Updated("PatrolAttachment", attachment.Id, $"Updated attachment for case {attachment.PatrolCaseId}");

            return await _repository.GetByIdAsync(attachment.Id);
        }

        public async Task DeleteAsync(Guid id)
        {
            var attachment = await _repository.GetByIdEntityAsync(id);
            if (attachment == null)
                throw new NotFoundException($"Attachment with id {id} not found");

            SetDeleteAudit(attachment);
            await _repository.DeleteAsync(attachment);
            _audit.Deleted("PatrolAttachment", id, $"Deleted attachment for case {attachment.PatrolCaseId}");
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, PatrolAttachmentFilter filter)
        {
            // Map Standard DataTables params
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            // Call repository FilterAsync (using ProjectToRead)
            var (data, total, filtered) = await _repository.FilterAsync(filter);

            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data
            };
        }
    }
}

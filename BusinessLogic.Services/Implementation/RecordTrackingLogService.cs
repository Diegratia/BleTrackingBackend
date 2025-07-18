// using AutoMapper;
// using BusinessLogic.Services.Interface;
// using System;
// using System.Collections.Generic;
// using System.Security.Claims;
// using System.Threading.Tasks;
// using Data.ViewModels;
// using Entities.Models;
// using Microsoft.AspNetCore.Http;
// using Repositories.Repository;

// namespace BusinessLogic.Services.Implementation
// {
//     public class RecordTrackingLogService : IRecordTrackingLogService
//     {
//         private readonly RecordTrackingLogRepository _repository;
//         private readonly IMapper _mapper;

//         public RecordTrackingLogService(RecordTrackingLogRepository repository, IMapper mapper)
//         {
//             _repository = repository;
//             _mapper = mapper;
//         }

//         public async Task<IEnumerable<RecordTrackingLogDto>> GetRecordTrackingLogsAsync()
//         {
//             var logs = await _repository.GetRecordTrackingLogsAsync();
//             return _mapper.Map<IEnumerable<RecordTrackingLogDto>>(logs);
//         }

//         public async Task<RecordTrackingLogDto> GetRecordTrackingLogByIdAsync(Guid id)
//         {
//             var log = await _repository.GetRecordTrackingLogByIdAsync(id);
//             return log == null ? null : _mapper.Map<RecordTrackingLogDto>(log);
//         }

//         public async Task DeleteRecordTrackingLogAsync(Guid id)
//         {
//             await _repository.DeleteRecordTrackingLogAsync(id);
//         }
//     }
// }
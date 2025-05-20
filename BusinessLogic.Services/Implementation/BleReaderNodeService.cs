// using AutoMapper;
// using BusinessLogic.Services.Interface;
// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Data.ViewModels;
// using Entities.Models;
// using Repositories.Repository;

// namespace BusinessLogic.Services.Implementation
// {
//     public class BleReaderNodeService : IBleReaderNodeService
//     {
//           private readonly BleReaderNodeRepository _repository;
//          private readonly IMapper _mapper;

//             public BleReaderNodeService(BleReaderNodeRepository repository, IMapper mapper)
//         {
//             _repository = repository;
//             _mapper = mapper;
//         }

//         public async Task<BleReaderNodeDto> GetByIdAsync(Guid id)
//         {
//             var bleReaderNode = await _context.BleReaderNodes.FirstOrDefaultAsync(n => n.Id == id);
//             if (bleReaderNode == null) return null;

//             var dto = _mapper.Map<BleReaderNodeDto>(bleReaderNode);
//             dto.Reader = await GetReaderAsync(bleReaderNode.ReaderId);
//             return dto;
//         }

//         public async Task<IEnumerable<BleReaderNodeDto>> GetAllAsync()
//         {
//             var bleReaderNodes = await _context.BleReaderNodes.ToListAsync();
//             var dtos = _mapper.Map<List<BleReaderNodeDto>>(bleReaderNodes);
//             foreach (var dto in dtos)
//             {
//                 dto.Reader = await GetReaderAsync(dto.ReaderId);
//             }
//             return dtos;
//         }

//         public async Task<BleReaderNodeDto> CreateAsync(BleReaderNodeCreateDto createDto)
//         {
//             var readerClient = _httpClientFactory.CreateClient("MstBleReaderService");
//             var readerResponse = await readerClient.GetAsync($"/{createDto.ReaderId}");
//             if (!readerResponse.IsSuccessStatusCode)
//                 throw new ArgumentException($"Reader with ID {createDto.ReaderId} not found.");

//             var appClient = _httpClientFactory.CreateClient("MstApplicationService");
//             var appResponse = await appClient.GetAsync($"/{createDto.ApplicationId}");
//             if (!appResponse.IsSuccessStatusCode)
//                 throw new ArgumentException($"Application with ID {createDto.ApplicationId} not found.");

//             var bleReaderNode = _mapper.Map<BleReaderNode>(createDto);
//             var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "";
//             bleReaderNode.Id = Guid.NewGuid();
//             bleReaderNode.CreatedBy = username;
//             bleReaderNode.CreatedAt = DateTime.UtcNow;
//             bleReaderNode.UpdatedBy = username;
//             bleReaderNode.UpdatedAt = DateTime.UtcNow;

//             _context.BleReaderNodes.Add(bleReaderNode);
//             await _context.SaveChangesAsync();

//             var dto = _mapper.Map<BleReaderNodeDto>(bleReaderNode);
//             dto.Reader = await GetReaderAsync(bleReaderNode.ReaderId);
//             return dto;
//         }

//         public async Task UpdateAsync(Guid id, BleReaderNodeUpdateDto updateDto)
//         {
//             var bleReaderNode = await _context.BleReaderNodes.FindAsync(id);
//             if (bleReaderNode == null)
//                 throw new KeyNotFoundException("BleReaderNode not found");

//             var readerClient = _httpClientFactory.CreateClient("MstBleReaderService");
//             var readerResponse = await readerClient.GetAsync($"/{updateDto.ReaderId}");
//             if (!readerResponse.IsSuccessStatusCode)
//                 throw new ArgumentException($"Reader with ID {updateDto.ReaderId} not found.");

//             var appClient = _httpClientFactory.CreateClient("MstApplicationService");
//             var appResponse = await appClient.GetAsync($"/{updateDto.ApplicationId}");
//             if (!appResponse.IsSuccessStatusCode)
//                 throw new ArgumentException($"Application with ID {updateDto.ApplicationId} not found.");

//             _mapper.Map(updateDto, bleReaderNode);
//             bleReaderNode.UpdatedBy = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "";
//             bleReaderNode.UpdatedAt = DateTime.UtcNow;

//             await _context.SaveChangesAsync();
//         }

//         public async Task DeleteAsync(Guid id)
//         {
//             var bleReaderNode = await _context.BleReaderNodes.FindAsync(id);
//             if (bleReaderNode == null)
//                 throw new KeyNotFoundException("BleReaderNode not found");

//             _context.BleReaderNodes.Remove(bleReaderNode);
//             await _context.SaveChangesAsync();
//         }

//         private async Task<MstBleReaderDto> GetReaderAsync(Guid readerId)
//         {
//             var client = _httpClientFactory.CreateClient("MstBleReaderService");
//             var response = await client.GetAsync($"/{readerId}");
//             if (!response.IsSuccessStatusCode) return null;

//             var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<MstBleReaderDto>>();
//             return apiResponse?.Collection?.Data;
//         }
//     }

//     public class HttpClientAuthorizationDelegatingHandler : DelegatingHandler
//     {
//         private readonly IHttpContextAccessor _httpContextAccessor;

//         public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
//         {
//             _httpContextAccessor = httpContextAccessor;
//         }

//         protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//         {
//             var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
//             if (!string.IsNullOrEmpty(token))
//             {
//                 request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
//                 // Console.WriteLine($"Forwarding token to request: {token}");
//             }
//             else
//             {
//                 Console.WriteLine("No Authorization token found in HttpContext.");
//             }

//             return await base.SendAsync(request, cancellationToken);
//         }
//     }
// }
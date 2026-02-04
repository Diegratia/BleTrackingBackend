using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using ClosedXML.Excel;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;



namespace BusinessLogic.Services.Implementation
{
    public class MstBleReaderService : BaseService, IMstBleReaderService
    {
        private readonly MstBleReaderRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;

        // private readonly IHttpClientFactory _httpClientFactory;

        public MstBleReaderService(MstBleReaderRepository repository,
        IMapper mapper, IHttpContextAccessor httpContextAccessor, IAuditEmitter audit)
            : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _audit = audit;
            // _httpClientFactory = httpClientFactory;
        }

        public async Task<MstBleReaderRead> GetByIdAsync(Guid id)
        {
            var bleReader = await _repository.GetByIdAsync(id);
            if (bleReader == null)
                throw new NotFoundException($"BLE Reader with id {id} not found");
            return bleReader;
        }

        public async Task<IEnumerable<MstBleReaderRead>> GetAllAsync()
        {
            var bleReaders = await _repository.GetAllAsync();
            return bleReaders;
        }
        public async Task<IEnumerable<MstBleReaderRead>> GetAllUnassignedAsync()
        {
            var bleReaders = await _repository.GetAllUnassignedAsync();
            return bleReaders;
        }

        public async Task<IEnumerable<OpenMstBleReaderDto>> OpenGetAllAsync()
        {
            var bleReaders = await _repository.GetAllExportAsync();
            return _mapper.Map<IEnumerable<OpenMstBleReaderDto>>(bleReaders);
        }

        public async Task<MstBleReaderRead> CreateAsync(MstBleReaderCreateDto createDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            if (createDto.BrandId != Guid.Empty)
            {
                var brand = await _repository.GetBrandByIdAsync(createDto.BrandId);
                if (brand == null)
                    throw new NotFoundException($"Brand with ID {createDto.BrandId} not found.");

                var invalidBrandId =
                    await _repository.CheckInvalidBrandOwnershipAsync(createDto.BrandId, AppId);
                if (invalidBrandId.Any())
                {
                    throw new UnauthorizedException(
                        $"BrandId does not belong to this Application: {string.Join(", ", invalidBrandId)}"
                    );
                }
            }

            var bleReader = _mapper.Map<MstBleReader>(createDto);
            bleReader.Id = Guid.NewGuid();
            bleReader.CreatedBy = username;
            bleReader.UpdatedBy = username;
            bleReader.CreatedAt = DateTime.UtcNow;
            bleReader.UpdatedAt = DateTime.UtcNow;
            bleReader.Status = 1;

            var createdBleReader = await _repository.AddAsync(bleReader);
            await _audit.Created(
                "BLE Reader",
                createdBleReader.Id,
                "Created BLE Reader",
                new { createdBleReader.Name }
            );
            var result = await _repository.GetByIdAsync(createdBleReader.Id);
            return result!;
        }

        public async Task<List<MstBleReaderDto>> CreateBatchAsync(List<MstBleReaderCreateDto> createDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var result = new List<MstBleReaderDto>();
            foreach (var dto in createDto)
            {
                if (dto.BrandId != Guid.Empty)
                {
                    var brand = await _repository.GetBrandByIdAsync(dto.BrandId);
                    if (brand == null)
                        throw new NotFoundException($"Brand with ID {dto.BrandId} not found.");

                    var invalidBrandId =
                        await _repository.CheckInvalidBrandOwnershipAsync(dto.BrandId, AppId);
                    if (invalidBrandId.Any())
                    {
                        throw new UnauthorizedException(
                            $"BrandId does not belong to this Application: {string.Join(", ", invalidBrandId)}"
                        );
                    }
                }

                var bleReader = _mapper.Map<MstBleReader>(dto);
                bleReader.Id = Guid.NewGuid();
                bleReader.CreatedBy = username;
                bleReader.UpdatedBy = username;
                bleReader.CreatedAt = DateTime.UtcNow;
                bleReader.UpdatedAt = DateTime.UtcNow;
                bleReader.Status = 1;

                await _repository.AddAsync(bleReader);
                await _audit.Created(
                    "BLE Reader",
                    bleReader.Id,
                    "Created BLE Reader in batch",
                    new { bleReader.Name }
                );
                result.Add(_mapper.Map<MstBleReaderDto>(bleReader));
            }
            return result;
        }

        public async Task UpdateAsync(Guid id, MstBleReaderUpdateDto updateDto)
        {
            var bleReader = await _repository.GetByIdEntityAsync(id);
            if (bleReader == null)
                throw new NotFoundException($"BLE Reader with id {id} not found");

            if (updateDto.BrandId != Guid.Empty && updateDto.BrandId != bleReader.BrandId)
            {
                var brand = await _repository.GetBrandByIdAsync(updateDto.BrandId);
                if (brand == null)
                    throw new NotFoundException($"Brand with ID {updateDto.BrandId} not found.");

                var invalidBrandId =
                    await _repository.CheckInvalidBrandOwnershipAsync(updateDto.BrandId, AppId);
                if (invalidBrandId.Any())
                {
                    throw new UnauthorizedException(
                        $"BrandId does not belong to this Application: {string.Join(", ", invalidBrandId)}"
                    );
                }
            }

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            _mapper.Map(updateDto, bleReader);
            bleReader.UpdatedBy = username ?? "";
            bleReader.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(bleReader);
            await _audit.Updated(
                "BLE Reader",
                bleReader.Id,
                "Updated BLE Reader",
                new { bleReader.Name }
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var bleReader = await _repository.GetByIdEntityAsync(id);
            if (bleReader == null)
                throw new NotFoundException($"BLE Reader with id {id} not found");
            if (bleReader.IsAssigned == true)
                throw new BusinessException("BLE Reader is still in use by floorplan device");
            
            bleReader.UpdatedBy = username ?? "";
            bleReader.UpdatedAt = DateTime.UtcNow;
            bleReader.Status = 0;

            await _repository.DeleteAsync(id);
            await _audit.Deleted(
                "BLE Reader",
                bleReader.Id,
                "Deleted BLE Reader",
                new { bleReader.Name }
            );
        }

        public async Task<IEnumerable<MstBleReaderRead>> ImportAsync(IFormFile file)
        {
            var bleReaders = new List<MstBleReader>();
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1);

            int rowNumber = 2;
            foreach (var row in rows)
            {

                var brandStr = row.Cell(1).GetValue<string>();
                if (!Guid.TryParse(brandStr, out var brandId))
                    throw new ArgumentException($"Invalid brandId format at row {rowNumber}");

                var brand = await _repository.GetBrandByIdAsync(brandId);
                if (brand == null)
                    throw new ArgumentException($"BrandId {brandId} not found at row {rowNumber}");

                // Buat entitas MstFloor
                var bleReader = new MstBleReader
                {
                    Id = Guid.NewGuid(),
                    BrandId = brandId,
                    Name = row.Cell(2).GetValue<string>(),
                    Ip = row.Cell(3).GetValue<string>(),
                    Gmac = row.Cell(4).GetValue<string>(),
                    IsAssigned = false,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                };

                bleReaders.Add(bleReader);
                rowNumber++;
            }

            // Simpan ke database
            foreach (var bleReader in bleReaders)
            {
                await _repository.AddAsync(bleReader);
            }

            var ids = bleReaders.Select(x => x.Id).ToList();
            var query = _repository.GetAllQueryable()
                .Where(x => ids.Contains(x.Id));
            return await _repository.ProjectToRead(query).ToListAsync();
        }

        public async Task<object> FilterAsync(
            DataTablesProjectedRequest request,
            MstBleReaderFilter filter
        )
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            if (request.DateFilters != null)
            {
                if (request.DateFilters.TryGetValue("UpdatedAt", out var dateFilter))
                {
                    filter.DateFrom = dateFilter.DateFrom;
                    filter.DateTo = dateFilter.DateTo;
                }
            }

            var (data, total, filtered) = await _repository.FilterAsync(filter);

            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data
            };
        }

        public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            var bleReaders = await _repository.GetAllExportAsync();

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master BLE Reader Report")
                        .SemiBold().FontSize(16).FontColor(Colors.Black).AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("IP").SemiBold();
                            header.Cell().Element(CellStyle).Text("GMAC").SemiBold();
                            // header.Cell().Element(CellStyle).Text("Engine Reader ID").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        int index = 1;
                        foreach (var bleReader in bleReaders)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(bleReader.Name);
                            table.Cell().Element(CellStyle).Text(bleReader.Ip);
                            table.Cell().Element(CellStyle).Text(bleReader.Gmac);
                            table.Cell().Element(CellStyle).Text(bleReader.Status.ToString());
                        }

                        static IContainer CellStyle(IContainer container) =>
                            container
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(4)
                                .PaddingHorizontal(6);
                    });

                    page.Footer()
                        .AlignRight()
                        .Text(txt =>
                        {
                            txt.Span("Generated at: ").SemiBold();
                            txt.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
                        });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> ExportExcelAsync()
        {
            var bleReaders = await _repository.GetAllExportAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("BLE Readers");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "IP";
            worksheet.Cell(1, 4).Value = "GMAC";
            // worksheet.Cell(1, 5).Value = "Engine Reader ID";
            worksheet.Cell(1, 5).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var bleReader in bleReaders)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = bleReader.Name;
                worksheet.Cell(row, 3).Value = bleReader.Ip;
                worksheet.Cell(row, 4).Value = bleReader.Gmac;
                // worksheet.Cell(row, 5).Value = bleReader.EngineReaderId;
                worksheet.Cell(row, 5).Value = bleReader.Status;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

    }

    // public class HttpClientAuthorizationDelegatingHandler : DelegatingHandler
    // {
    //     private readonly IHttpContextAccessor _httpContextAccessor;

    //     public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    //     {
    //         _httpContextAccessor = httpContextAccessor;
    //     }

    //     protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    //     {
    //         var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
    //         if (!string.IsNullOrEmpty(token))
    //         {
    //             request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
    //             Console.WriteLine($"Forwarding token to request: {token}");
    //         }
    //         else
    //         {
    //             Console.WriteLine("No Authorization token found in HttpContext.");
    //         }

    //         return await base.SendAsync(request, cancellationToken);
    //     }
    // }
}

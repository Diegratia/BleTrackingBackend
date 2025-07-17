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
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

namespace BusinessLogic.Services.Implementation
{
    public class MstBleReaderService : IMstBleReaderService
    {
        private readonly MstBleReaderRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        // private readonly IHttpClientFactory _httpClientFactory;

        public MstBleReaderService(MstBleReaderRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            // _httpClientFactory = httpClientFactory;
        }

        public async Task<MstBleReaderDto> GetByIdAsync(Guid id)
        {
            var bleReader = await _repository.GetByIdAsync(id);
            return bleReader == null ? null : _mapper.Map<MstBleReaderDto>(bleReader);
        }

        public async Task<IEnumerable<MstBleReaderDto>> GetAllAsync()
        {
            var bleReaders = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstBleReaderDto>>(bleReaders);
        }

        public async Task<MstBleReaderDto> CreateAsync(MstBleReaderCreateDto createDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            var bleReader = _mapper.Map<MstBleReader>(createDto);
            bleReader.Id = Guid.NewGuid();
            bleReader.CreatedBy = username;
            bleReader.UpdatedBy = username;
            bleReader.CreatedAt = DateTime.UtcNow;
            bleReader.UpdatedAt = DateTime.UtcNow;
            bleReader.Status = 1;

            var createdBleReader = await _repository.AddAsync(bleReader);
            return _mapper.Map<MstBleReaderDto>(createdBleReader);
        }



        public async Task UpdateAsync(Guid id, MstBleReaderUpdateDto updateDto)
        {
            var bleReader = await _repository.GetByIdAsync(id);
            if (bleReader == null)
                throw new KeyNotFoundException("BLE Reader not found");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            _mapper.Map(updateDto, bleReader);
            bleReader.UpdatedBy = username ?? "";
            bleReader.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(bleReader);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name", "Brand.Name" };
            var validSortColumns = new[] { "Name", "CreatedAt", "UpdatedAt", "Status" };

            var filterService = new GenericDataTableService<MstBleReader, MstBleReaderDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }

        public async Task<byte[]> ExportPdfAsync()
        {
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
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("IP").SemiBold();
                            header.Cell().Element(CellStyle).Text("GMAC").SemiBold();
                            header.Cell().Element(CellStyle).Text("Engine Reader ID").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        int index = 1;
                        foreach (var bleReader in bleReaders)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(bleReader.Name);
                            table.Cell().Element(CellStyle).Text(bleReader.Ip);
                            table.Cell().Element(CellStyle).Text(bleReader.Gmac);
                            table.Cell().Element(CellStyle).Text(bleReader.EngineReaderId);
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
            worksheet.Cell(1, 5).Value = "Engine Reader ID";
            worksheet.Cell(1, 6).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var bleReader in bleReaders)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = bleReader.Name;
                worksheet.Cell(row, 3).Value = bleReader.Ip;
                worksheet.Cell(row, 4).Value = bleReader.Gmac;
                worksheet.Cell(row, 5).Value = bleReader.EngineReaderId;
                worksheet.Cell(row, 6).Value = bleReader.Status;
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
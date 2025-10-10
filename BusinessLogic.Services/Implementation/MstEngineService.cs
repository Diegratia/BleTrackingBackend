using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using MQTTnet.Client;
using Helpers.Consumer.Mqtt;
using System.Text.Json;

namespace BusinessLogic.Services.Implementation
{
    public class MstEngineService : IMstEngineService
    {
        private readonly MstEngineRepository _engineRepository;
        private readonly IMapper _mapper;
        private readonly IMqttClientService _mqttClientService;


        public MstEngineService(MstEngineRepository engineRepository, IMapper mapper, IMqttClientService mqttClientService)
        {
            _engineRepository = engineRepository;
            _mapper = mapper;
            _mqttClientService = mqttClientService;
        }

        public async Task<IEnumerable<MstEngineDto>> GetAllEnginesAsync()
        {
            var engines = await _engineRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstEngineDto>>(engines);
        }
        public async Task<IEnumerable<MstEngineDto>> GetAllOnlineAsync()
        {
            var engines = await _engineRepository.GetAllOnlineAsync();
            // pastikan kalau null, balikin list kosong
            return _mapper.Map<IEnumerable<MstEngineDto>>(engines ?? new List<MstEngine>());
        }


        public async Task<MstEngineDto> GetEngineByIdAsync(Guid id)
        {
            var engine = await _engineRepository.GetByIdAsync(id);
            return engine == null ? null : _mapper.Map<MstEngineDto>(engine);
        }

            public async Task<MstEngineDto> GetEngineIdAsync(string engineId)
        {
            var engine = await _engineRepository.GetByEngineIdAsync(engineId);
            return engine == null ? null : _mapper.Map<MstEngineDto>(engine);
        }

        public async Task<MstEngineDto> CreateEngineAsync(MstEngineCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var engine = _mapper.Map<MstEngine>(dto);
            engine.Status = 1;

            var createdEngine = await _engineRepository.AddAsync(engine);
            return _mapper.Map<MstEngineDto>(createdEngine);
        }

        public async Task UpdateEngineAsync(Guid id, MstEngineUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var engine = await _engineRepository.GetByIdAsync(id);
            if (engine == null)
                throw new KeyNotFoundException($"Engine with ID {id} not found");

            _mapper.Map(dto, engine);
            await _engineRepository.UpdateAsync(engine);
        }

            public async Task UpdateEngineByIdAsync(string engineId, MstEngineUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var engine = await _engineRepository.GetByEngineIdAsync(engineId);
            if (engine == null)
                throw new KeyNotFoundException($"Engine with ID {engineId} not found");

            _mapper.Map(dto, engine);
            await _engineRepository.UpdateByEngineStringAsync(engine);
        }

        public async Task DeleteEngineAsync(Guid id)
        {
            var engine = await _engineRepository.GetByIdAsync(id);
            if (engine == null)
                throw new KeyNotFoundException($"Engine with ID {id} not found");
            engine.Status = 0;
            engine.IsLive = 0;

            await _engineRepository.DeleteAsync(id);
        }

        public async Task StopEngineAsync(string engineId)
        {
            var topic = $"engine/stop/{engineId}";
            var payload = JsonSerializer.Serialize(new
            {
                engineId,
                status = "stop",
                timestamp = DateTime.UtcNow
            });

            await _mqttClientService.PublishAsync(topic, payload);
            Console.WriteLine($"Sent stop command to {topic}");
        }

            public async Task StartEngineAsync(string engineId)
        {
            var topic = $"engine/start/{engineId}";
            var payload = JsonSerializer.Serialize(new
            {
                engineId,
                status = "start",
                timestamp = DateTime.UtcNow
            });

            await _mqttClientService.PublishAsync(topic, payload);
            Console.WriteLine($"Sent start command to {topic}");
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _engineRepository.GetAllQueryable();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "Name", "EngineId", "Status", "Port", "IsLive", "LastLive" };

            var filterService = new GenericDataTableService<MstEngine, MstEngineDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
         public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var engines = await _engineRepository.GetAllExportAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Engine Report")
                        .SemiBold().FontSize(16).FontColor(Colors.Black).AlignCenter();

                    page.Content().Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2); 
                            columns.RelativeColumn(2); 
                            columns.RelativeColumn(2); 
                        });

                        // Table heade
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("EngineId").SemiBold();
                            header.Cell().Element(CellStyle).Text("Port").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                            header.Cell().Element(CellStyle).Text("Is Live").SemiBold();
                            header.Cell().Element(CellStyle).Text("Last Live").SemiBold();
                            header.Cell().Element(CellStyle).Text("Services Status").SemiBold(); 
                        });

                        // Table body
                        int index = 1;
                        foreach (var engine in engines)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(engine.Name);
                            table.Cell().Element(CellStyle).Text(engine.EngineId);
                            table.Cell().Element(CellStyle).Text(engine.Port);
                            table.Cell().Element(CellStyle).Text(engine.Status);
                            table.Cell().Element(CellStyle).Text(engine.IsLive);
                            table.Cell().Element(CellStyle).Text(engine.LastLive);
                            table.Cell().Element(CellStyle).Text(engine.ServiceStatus);
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
            var engines = await _engineRepository.GetAllExportAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Engines");

            // Header
            worksheet.Cell(1, 1).Value = "No";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "EngineId";
            worksheet.Cell(1, 4).Value = "Port";
            worksheet.Cell(1, 5).Value = "Status";
            worksheet.Cell(1, 6).Value = "IsLive";
            worksheet.Cell(1, 7).Value = "LastLive";
            worksheet.Cell(1, 8).Value = "ServiceStatus";

            int row = 2;
            int no = 1;

            foreach (var engine in engines)
            {
                await _engineRepository.GetAllExportAsync(); 

                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = engine.Name;
                worksheet.Cell(row, 3).Value = engine.EngineId;
                worksheet.Cell(row, 4).Value = engine.Port;
                worksheet.Cell(row, 5).Value = engine.Status;
                worksheet.Cell(row, 6).Value = engine.IsLive;
                worksheet.Cell(row, 7).Value = engine.LastLive;
                worksheet.Cell(row, 8).Value = engine.ServiceStatus.ToString() == "1" ? "Start" : "Stop";

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
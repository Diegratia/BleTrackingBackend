using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Helpers.Consumer;
using Helpers.Consumer.Mqtt;
using Microsoft.AspNetCore.Http;
using MQTTnet.Client;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;
using System.Text.Json;

namespace BusinessLogic.Services.Implementation
{
    public class MstEngineService : BaseService, IMstEngineService
    {
        private readonly MstEngineRepository _engineRepository;
        private readonly IMqttClientService _mqttClientService;
        private readonly IAuditEmitter _audit;

        public MstEngineService(
            MstEngineRepository engineRepository,
            IMqttClientService mqttClientService,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _engineRepository = engineRepository;
            _mqttClientService = mqttClientService;
            _audit = audit;
        }

        public async Task<List<MstEngineRead>> GetAllEnginesAsync()
        {
            var engines = await _engineRepository.GetAllAsync();
            return engines; // Direct return from repository
        }

        public async Task<List<MstEngineRead>> GetAllOnlineAsync()
        {
            return await _engineRepository.GetAllOnlineAsync();
        }

        public async Task<MstEngineRead?> GetEngineByIdAsync(Guid id)
        {
            return await _engineRepository.GetByIdAsync(id);
        }

        public async Task<MstEngine?> GetEngineIdAsync(string engineTrackingId)
        {
            return await _engineRepository.GetByEngineIdAsync(engineTrackingId);
        }

        public async Task<MstEngineRead> CreateEngineAsync(MstEngineCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var engine = new MstEngine
            {
                Name = dto.Name,
                EngineTrackingId = dto.EngineTrackingId,
                Port = dto.Port,
                IsLive = dto.IsLive,
                LastLive = dto.LastLive,
                ServiceStatus = dto.ServiceStatus,
                ApplicationId = AppId,
                Status = 1
            };

            SetCreateAudit(engine);

            await _engineRepository.AddAsync(engine);

             _audit.Created(
                "MstEngine",
                engine.Id,
                $"Engine {engine.Name} created",
                engine
            );

            // Get the created entity from repository with projection
            var createdEngine = await _engineRepository.GetByIdAsync(engine.Id)
                ?? throw new InvalidOperationException($"Failed to retrieve created engine with ID {engine.Id}");

            return createdEngine;
        }

        public async Task UpdateEngineAsync(Guid id, MstEngineUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var engine = await _engineRepository.GetByIdEntityAsync(id);
            if (engine == null)
                throw new KeyNotFoundException($"Engine with ID {id} not found");

            // Manual mapping (no AutoMapper)
            if (dto.Name != null) engine.Name = dto.Name;
            if (dto.EngineTrackingId != null) engine.EngineTrackingId = dto.EngineTrackingId;
            if (dto.Port.HasValue) engine.Port = dto.Port.Value;
            if (dto.IsLive.HasValue) engine.IsLive = dto.IsLive.Value;
            if (dto.LastLive.HasValue) engine.LastLive = dto.LastLive.Value;
            if (dto.ServiceStatus.HasValue) engine.ServiceStatus = dto.ServiceStatus.Value;

            SetUpdateAudit(engine);

            await _engineRepository.UpdateAsync(engine);

             _audit.Updated(
                "MstEngine",
                engine.Id,
                $"Engine {engine.Name} updated",
                engine
            );
        }

        public async Task UpdateEngineByIdAsync(string engineTrackingId, MstEngineUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var engine = await _engineRepository.GetByEngineIdAsync(engineTrackingId);
            if (engine == null)
                throw new KeyNotFoundException($"Engine with ID {engineTrackingId} not found");

            // Manual mapping (no AutoMapper)
            if (dto.Name != null) engine.Name = dto.Name;
            if (dto.EngineTrackingId != null) engine.EngineTrackingId = dto.EngineTrackingId;
            if (dto.Port.HasValue) engine.Port = dto.Port.Value;
            if (dto.IsLive.HasValue) engine.IsLive = dto.IsLive.Value;
            if (dto.LastLive.HasValue) engine.LastLive = dto.LastLive.Value;
            if (dto.ServiceStatus.HasValue) engine.ServiceStatus = dto.ServiceStatus.Value;

            SetUpdateAudit(engine);

            await _engineRepository.UpdateByEngineStringAsync(engine);

             _audit.Updated(
                "MstEngine",
                engine.Id,
                $"Engine {engine.Name} updated by tracking ID",
                engine
            );
        }

        public async Task DeleteEngineAsync(Guid id)
        {
            var engine = await _engineRepository.GetByIdEntityAsync(id);
            if (engine == null)
                throw new KeyNotFoundException($"Engine with ID {id} not found");

            SetDeleteAudit(engine);
            engine.IsLive = 0;

            await _engineRepository.UpdateAsync(engine);

             _audit.Deleted(
                "MstEngine",
                engine.Id,
                $"Engine {engine.Name} deleted",
                engine
            );
        }

        public async Task StopEngineAsync(string engineTrackingId)
        {
            var topic = $"engine/stop/{engineTrackingId}";
            var payload = JsonSerializer.Serialize(new
            {
                EngineTrackingId = engineTrackingId,
                status = "stop",
                timestamp = DateTime.UtcNow
            });

            await _mqttClientService.PublishAsync(topic, payload);
            Console.WriteLine($"Sent stop command to {topic}");

             _audit.Action(
                AuditEmitter.AuditAction.ACTION,
                "MstEngine",
                $"Stop command sent to engine {engineTrackingId}"
            );
        }

        public async Task StartEngineAsync(string engineTrackingId)
        {
            var topic = $"engine/start/{engineTrackingId}";
            var payload = JsonSerializer.Serialize(new
            {
                EngineTrackingId = engineTrackingId,
                status = "start",
                timestamp = DateTime.UtcNow
            });

            await _mqttClientService.PublishAsync(topic, payload);
            Console.WriteLine($"Sent start command to {topic}");

             _audit.Action(
                AuditEmitter.AuditAction.ACTION,
                "MstEngine",
                $"Start command sent to engine {engineTrackingId}"
            );
        }

        public async Task<(List<MstEngineRead> Data, int Total, int Filtered)> FilterAsync(DataTablesProjectedRequest request, MstEngineFilter filter)
        {
            // Map DataTablesProjectedRequest to MstEngineFilter
            filter.Page = request.Start / request.Length + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn;
            filter.SortDir = request.SortDir;

            // Deserialize search if provided
            if (request.Filters.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                var requestFilter = JsonSerializer.Deserialize<MstEngineFilter>(
                    request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (requestFilter != null)
                {
                    filter.Search = requestFilter.Search ?? filter.Search;
                    filter.Status = requestFilter.Status ?? filter.Status;
                    filter.IsLive = requestFilter.IsLive ?? filter.IsLive;
                    filter.DateFrom = requestFilter.DateFrom ?? filter.DateFrom;
                    filter.DateTo = requestFilter.DateTo ?? filter.DateTo;
                }
            }

            return await _engineRepository.FilterAsync(filter);
        }

        // public async Task<byte[]> ExportPdfAsync()
        // {
        //     var engines = await _engineRepository.GetAllExportAsync();

        //     var document = Document.Create(container =>
        //     {
        //         container.Page(page =>
        //         {
        //             page.Margin(30);
        //             // page.Size(PageSize.A4.Landscape());
        //             page.PageColor(Colors.White);
        //             page.DefaultTextStyle(x => x.FontSize(10));

        //             page.Header()
        //                 .Text("Master Engine Report")
        //                 .SemiBold().FontSize(16).FontColor(Colors.Black).AlignCenter();

        //             page.Content().Table(table =>
        //             {
        //                 table.ColumnsDefinition(columns =>
        //                 {
        //                     columns.ConstantColumn(35);
        //                     columns.RelativeColumn(2);
        //                     columns.RelativeColumn(2);
        //                     columns.RelativeColumn(2);
        //                     columns.RelativeColumn(2);
        //                     columns.RelativeColumn(2);
        //                     columns.RelativeColumn(2);
        //                     columns.RelativeColumn(2);
        //                 });

        //                 table.Header(header =>
        //                 {
        //                     header.Cell().Element(CellStyle).Text("#").SemiBold();
        //                     header.Cell().Element(CellStyle).Text("Name").SemiBold();
        //                     header.Cell().Element(CellStyle).Text("EngineTrackingId").SemiBold();
        //                     header.Cell().Element(CellStyle).Text("Port").SemiBold();
        //                     header.Cell().Element(CellStyle).Text("Status").SemiBold();
        //                     header.Cell().Element(CellStyle).Text("Is Live").SemiBold();
        //                     header.Cell().Element(CellStyle).Text("Last Live").SemiBold();
        //                     header.Cell().Element(CellStyle).Text("Services Status").SemiBold();
        //                 });

        //                 int index = 1;
        //                 foreach (var engine in engines)
        //                 {
        //                     table.Cell().Element(CellStyle).Text(index++.ToString());
        //                     table.Cell().Element(CellStyle).Text(engine.Name ?? "");
        //                     table.Cell().Element(CellStyle).Text(engine.EngineTrackingId ?? "");
        //                     table.Cell().Element(CellStyle).Text(engine.Port?.ToString() ?? "");
        //                     table.Cell().Element(CellStyle).Text(engine.Status.ToString());
        //                     table.Cell().Element(CellStyle).Text(engine.IsLive?.ToString() ?? "");
        //                     table.Cell().Element(CellStyle).Text(engine.LastLive?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
        //                     table.Cell().Element(CellStyle).Text(engine.ServiceStatus ?? "");
        //                 }

        //                 static IContainer CellStyle(IContainer container) =>
        //                     container
        //                         .BorderBottom(1)
        //                         .BorderColor(Colors.Grey.Lighten2)
        //                         .PaddingVertical(4)
        //                         .PaddingHorizontal(6);
        //             });

        //             page.Footer()
        //                 .AlignRight()
        //                 .Text(txt =>
        //                 {
        //                     txt.Span("Generated at: ").SemiBold();
        //                     txt.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
        //                 });
        //         });
        //     });

        //     return document.GeneratePdf();
        // }

        // public async Task<byte[]> ExportExcelAsync()
        // {
        //     var engines = await _engineRepository.GetAllExportAsync();

        //     using var workbook = new XLWorkbook();
        //     var worksheet = workbook.Worksheets.Add("Engines");

        //     // Header
        //     worksheet.Cell(1, 1).Value = "No";
        //     worksheet.Cell(1, 2).Value = "Name";
        //     worksheet.Cell(1, 3).Value = "EngineTrackingId";
        //     worksheet.Cell(1, 4).Value = "Port";
        //     worksheet.Cell(1, 5).Value = "Status";
        //     worksheet.Cell(1, 6).Value = "IsLive";
        //     worksheet.Cell(1, 7).Value = "LastLive";
        //     worksheet.Cell(1, 8).Value = "ServiceStatus";

        //     int row = 2;
        //     int no = 1;

        //     foreach (var engine in engines)
        //     {
        //         worksheet.Cell(row, 1).Value = no++;
        //         worksheet.Cell(row, 2).Value = engine.Name ?? "";
        //         worksheet.Cell(row, 3).Value = engine.EngineTrackingId ?? "";
        //         worksheet.Cell(row, 4).Value = engine.Port?.ToString() ?? "";
        //         worksheet.Cell(row, 5).Value = engine.Status.ToString();
        //         worksheet.Cell(row, 6).Value = engine.IsLive?.ToString() ?? "";
        //         worksheet.Cell(row, 7).Value = engine.LastLive?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
        //         worksheet.Cell(row, 8).Value = engine.ServiceStatus == "1" ? "Start" : "Stop";

        //         row++;
        //     }

        //     worksheet.Columns().AdjustToContents();

        //     using var stream = new MemoryStream();
        //     workbook.SaveAs(stream);
        //     return stream.ToArray();
        // }
    }
}

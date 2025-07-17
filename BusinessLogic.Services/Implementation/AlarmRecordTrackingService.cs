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

namespace BusinessLogic.Services.Implementation
{
    public class AlarmRecordTrackingService : IAlarmRecordTrackingService
    {
        private readonly AlarmRecordTrackingRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AlarmRecordTrackingService(AlarmRecordTrackingRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AlarmRecordTrackingDto> GetByIdAsync(Guid id)
        {
            var alarm = await _repository.GetByIdAsync(id);
            return alarm == null ? null : _mapper.Map<AlarmRecordTrackingDto>(alarm);
        }

        public async Task<IEnumerable<AlarmRecordTrackingDto>> GetAllAsync()
        {
            var alarms = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<AlarmRecordTrackingDto>>(alarms);
        }

        public async Task<AlarmRecordTrackingDto> CreateAsync(AlarmRecordTrackingCreateDto createDto)
        {
            // Validasi relasi
            var visitor = await _repository.GetVisitorByIdAsync(createDto.VisitorId);
            if (visitor == null)
                throw new ArgumentException($"Visitor with ID {createDto.VisitorId} not found.");

            var reader = await _repository.GetReaderByIdAsync(createDto.ReaderId);
            if (reader == null)
                throw new ArgumentException($"Reader with ID {createDto.ReaderId} not found.");

            var maskedArea = await _repository.GetFloorplanMaskedAreaByIdAsync(createDto.FloorplanMaskedAreaId);
            if (maskedArea == null)
                throw new ArgumentException($"Area with ID {createDto.FloorplanMaskedAreaId} not found.");

            var app = await _repository.GetApplicationByIdAsync(createDto.ApplicationId);
            if (app == null)
                throw new ArgumentException($"Application with ID {createDto.ApplicationId} not found.");

            var alarm = _mapper.Map<AlarmRecordTracking>(createDto);

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            // Set nilai default untuk properti yang tidak ada di DTO
            alarm.Id = Guid.NewGuid();
            alarm.Timestamp = DateTime.UtcNow;
            alarm.IdleTimestamp = DateTime.UtcNow;
            alarm.DoneTimestamp = DateTime.MaxValue;
            alarm.CancelTimestamp = DateTime.MaxValue;
            alarm.WaitingTimestamp = DateTime.MaxValue;
            alarm.InvestigatedTimestamp = DateTime.MaxValue;
            alarm.IdleBy = username;
            alarm.DoneBy = username;
            alarm.CancelBy = username;
            alarm.WaitingBy = username;
            alarm.InvestigatedBy = username;
            alarm.InvestigatedDoneAt = DateTime.MaxValue;

            await _repository.AddAsync(alarm);
            return _mapper.Map<AlarmRecordTrackingDto>(alarm);
        }

        public async Task UpdateAsync(Guid id, AlarmRecordTrackingUpdateDto updateDto)
        {
            var alarm = await _repository.GetByIdAsync(id);
            if (alarm == null)
                throw new KeyNotFoundException("Alarm record not found");

            // Validasi relasi jika berubah
            if (alarm.VisitorId != updateDto.VisitorId)
            {
                var visitor = await _repository.GetVisitorByIdAsync(updateDto.VisitorId);
                if (visitor == null)
                    throw new ArgumentException($"Visitor with ID {updateDto.VisitorId} not found.");
            }

            if (alarm.ReaderId != updateDto.ReaderId)
            {
                var reader = await _repository.GetReaderByIdAsync(updateDto.ReaderId);
                if (reader == null)
                    throw new ArgumentException($"Reader with ID {updateDto.ReaderId} not found.");
            }

            if (alarm.FloorplanMaskedAreaId != updateDto.FloorplanMaskedAreaId)
            {
                var maskedArea = await _repository.GetFloorplanMaskedAreaByIdAsync(updateDto.FloorplanMaskedAreaId);
                if (maskedArea == null)
                    throw new ArgumentException($"Masked Area with ID {updateDto.FloorplanMaskedAreaId} not found.");
            }

            if (alarm.ApplicationId != updateDto.ApplicationId)
            {
                var app = await _repository.GetApplicationByIdAsync(updateDto.ApplicationId);
                if (app == null)
                    throw new ArgumentException($"Application with ID {updateDto.ApplicationId} not found.");
            }

            _mapper.Map(updateDto, alarm);

            await _repository.UpdateAsync(alarm);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.SoftDeleteAsync(id);
        }

         public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name", "Reader.Name", "Visitor.Name", "FloorplanMaskedArea.Name" }; 
            var validSortColumns = new[] { "Name" , "Reader.Name", "Visitor.Name", "FloorplanMaskedArea.Name", "AlarmRecordStatus", "Action" };

            var filterService = new GenericDataTableService<AlarmRecordTracking, AlarmRecordTrackingDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }

         public async Task<byte[]> ExportPdfAsync()
        {
            var records = await _repository.GetAllExportAsync();

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Alarm Records Report")
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
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Timestamp").SemiBold();
                            header.Cell().Element(CellStyle).Text("Visitor").SemiBold();
                            header.Cell().Element(CellStyle).Text("Reader").SemiBold();
                            header.Cell().Element(CellStyle).Text("Masked Area").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                            header.Cell().Element(CellStyle).Text("Action").SemiBold();
                        });

                        int index = 1;
                        foreach (var record in records)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(record.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(record.Visitor?.Name);
                            table.Cell().Element(CellStyle).Text(record.Reader?.Name);
                            table.Cell().Element(CellStyle).Text(record.FloorplanMaskedArea?.Name);
                            table.Cell().Element(CellStyle).Text(record.Alarm.ToString());
                            table.Cell().Element(CellStyle).Text(record.Action.ToString());
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
            var records = await _repository.GetAllExportAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Alarm Records");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Timestamp";
            worksheet.Cell(1, 3).Value = "Visitor";
            worksheet.Cell(1, 4).Value = "Reader";
            worksheet.Cell(1, 5).Value = "Masked Area";
            worksheet.Cell(1, 6).Value = "Status";
            worksheet.Cell(1, 7).Value = "Action";

            int row = 2;
            int no = 1;

            foreach (var record in records)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = record.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell(row, 3).Value = record.Visitor?.Name;
                worksheet.Cell(row, 4).Value = record.Reader?.Name;
                worksheet.Cell(row, 5).Value = record.FloorplanMaskedArea?.Name;
                worksheet.Cell(row, 6).Value = record.Alarm.ToString();
                worksheet.Cell(row, 7).Value = record.Action.ToString();
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
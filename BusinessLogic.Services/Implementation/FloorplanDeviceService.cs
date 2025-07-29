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
using Helpers.Consumer;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

namespace BusinessLogic.Services.Implementation
{
    public class FloorplanDeviceService : IFloorplanDeviceService
    {
        private readonly FloorplanDeviceRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FloorplanDeviceService(FloorplanDeviceRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<FloorplanDeviceDto> GetByIdAsync(Guid id)
        {
            var device = await _repository.GetByIdAsync(id);
            return device == null ? null : _mapper.Map<FloorplanDeviceDto>(device);
        }

        public async Task<IEnumerable<FloorplanDeviceDto>> GetAllAsync()
        {
            var devices = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<FloorplanDeviceDto>>(devices);
        }

        public async Task<FloorplanDeviceDto> CreateAsync(FloorplanDeviceCreateDto dto)
        {
            // Validasi semua foreign key
            var floorplan = await _repository.GetFloorplanByIdAsync(dto.FloorplanId);
            if (floorplan == null)
                throw new ArgumentException($"Floorplan with ID {dto.FloorplanId} not found.");

            var accessCctv = await _repository.GetAccessCctvByIdAsync(dto.AccessCctvId);
            if (accessCctv == null)
                throw new ArgumentException($"AccessCctv with ID {dto.AccessCctvId} not found.");

            var reader = await _repository.GetReaderByIdAsync(dto.ReaderId);
            if (reader == null)
                throw new ArgumentException($"Reader with ID {dto.ReaderId} not found.");

            var accessControl = await _repository.GetAccessControlByIdAsync(dto.AccessControlId);
            if (accessControl == null)
                throw new ArgumentException($"AccessControl with ID {dto.AccessControlId} not found.");

            var floorplanMaskedArea = await _repository.GetFloorplanMaskedAreaByIdAsync(dto.FloorplanMaskedAreaId);
            if (floorplanMaskedArea == null)
                throw new ArgumentException($"FloorplanMaskedArea with ID {dto.FloorplanMaskedAreaId} not found.");

            // var application = await _repository.GetApplicationByIdAsync(dto.ApplicationId);
            // if (application == null)
            //     throw new ArgumentException($"Application with ID {dto.ApplicationId} not found.");

            var device = _mapper.Map<FloorplanDevice>(dto);
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            device.CreatedBy = username;
            device.CreatedAt = DateTime.UtcNow;
            device.UpdatedBy = username;
            device.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(device);
            return _mapper.Map<FloorplanDeviceDto>(device);
        }

        public async Task UpdateAsync(Guid id, FloorplanDeviceUpdateDto dto)
        {
            var device = await _repository.GetByIdAsync(id);
            if (device == null)
                throw new KeyNotFoundException("FloorplanDevice not found");

            // Validasi foreign key jika berubah
            if (device.FloorplanId != dto.FloorplanId)
            {
                var floorplan = await _repository.GetFloorplanByIdAsync(dto.FloorplanId);
                if (floorplan == null)
                    throw new ArgumentException($"Floorplan with ID {dto.FloorplanId} not found.");
                device.FloorplanId = dto.FloorplanId;
            }

            if (device.AccessCctvId != dto.AccessCctvId)
            {
                var accessCctv = await _repository.GetAccessCctvByIdAsync(dto.AccessCctvId);
                if (accessCctv == null)
                    throw new ArgumentException($"AccessCctv with ID {dto.AccessCctvId} not found.");
                device.AccessCctvId = dto.AccessCctvId;
            }

            if (device.ReaderId != dto.ReaderId)
            {
                var reader = await _repository.GetReaderByIdAsync(dto.ReaderId);
                if (reader == null)
                    throw new ArgumentException($"Reader with ID {dto.ReaderId} not found.");
                device.ReaderId = dto.ReaderId;
            }

            if (device.AccessControlId != dto.AccessControlId)
            {
                var accessControl = await _repository.GetAccessControlByIdAsync(dto.AccessControlId);
                if (accessControl == null)
                    throw new ArgumentException($"AccessControl with ID {dto.AccessControlId} not found.");
                device.AccessControlId = dto.AccessControlId;
            }

            if (device.FloorplanMaskedAreaId != dto.FloorplanMaskedAreaId)
            {
                var floorplanMaskedArea = await _repository.GetFloorplanMaskedAreaByIdAsync(dto.FloorplanMaskedAreaId);
                if (floorplanMaskedArea == null)
                    throw new ArgumentException($"FloorplanMaskedArea with ID {dto.FloorplanMaskedAreaId} not found.");
                device.FloorplanMaskedAreaId = dto.FloorplanMaskedAreaId;
            }

            if (device.ApplicationId != dto.ApplicationId)
            {
                var application = await _repository.GetApplicationByIdAsync(dto.ApplicationId);
                if (application == null)
                    throw new ArgumentException($"Application with ID {dto.ApplicationId} not found.");
                device.ApplicationId = dto.ApplicationId;
            }

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            device.UpdatedBy = username;
            device.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(dto, device);

            await _repository.UpdateAsync(device);
        }

        public async Task DeleteAsync(Guid id)
        {
            var device = await _repository.GetByIdAsync(id);
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            device.UpdatedBy = username;
            device.UpdatedAt = DateTime.UtcNow;

            await _repository.SoftDeleteAsync(id);
        }

          public async Task<IEnumerable<FloorplanDeviceDto>> ImportAsync(IFormFile file)
        {
            var devices = new List<FloorplanDevice>();
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var userApplicationId = _httpContextAccessor.HttpContext?.User.FindFirst("ApplicationId")?.Value;

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1);

            int rowNumber = 2; 
            foreach (var row in rows)
            {

                var floorplanStr = row.Cell(1).GetValue<string>();
                if (!Guid.TryParse(floorplanStr, out var floorplanId))
                    throw new ArgumentException($"Invalid floorplanId format at row {rowNumber}");

                var floorplan = await _repository.GetFloorplanByIdAsync(floorplanId);
                if (floorplan == null)
                    throw new ArgumentException($"floorplanId {floorplanId} not found at row {rowNumber}");

                var accessCctvStr = row.Cell(2).GetValue<string>();
                if (!Guid.TryParse(accessCctvStr, out var accessCctvId))
                    throw new ArgumentException($"Invalid AccessCctvId format at row {rowNumber}");

                var accessCctv = await _repository.GetAccessCctvByIdAsync(accessCctvId);
                if (accessCctv == null)
                    throw new ArgumentException($"AccessCctvId {accessCctvId} not found at row {rowNumber}");

                var readerStr = row.Cell(3).GetValue<string>();
                if (!Guid.TryParse(readerStr, out var readerId))
                    throw new ArgumentException($"Invalid ReaderId format at row {rowNumber}");

                var reader = await _repository.GetReaderByIdAsync(readerId);
                if (reader == null)
                    throw new ArgumentException($"ReaderId {readerId} not found at row {rowNumber}");

                var accessControlStr = row.Cell(4).GetValue<string>();
                if (!Guid.TryParse(accessControlStr, out var accessControlId))
                    throw new ArgumentException($"Invalid AccessControlId format at row {rowNumber}");

                var accessControl = await _repository.GetAccessControlByIdAsync(accessControlId);
                if (accessControl == null)
                    throw new ArgumentException($"AccessControlId {accessControlId} not found at row {rowNumber}");

                var floorplanMaskedAreaStr = row.Cell(5).GetValue<string>();
                if (!Guid.TryParse(floorplanMaskedAreaStr, out var floorplanMaskedAreaId))
                    throw new ArgumentException($"Invalid FloorplanMaskedAreaId format at row {rowNumber}");

                var floorplanMaskedArea = await _repository.GetFloorplanMaskedAreaByIdAsync(floorplanMaskedAreaId);
                if (floorplanMaskedArea == null)
                    throw new ArgumentException($"FloorplanMaskedAreaId {floorplanMaskedAreaId} not found at row {rowNumber}");

                var device = new FloorplanDevice
                {
                    Id = Guid.NewGuid(),
                    FloorplanId = floorplanId,
                    AccessCctvId = accessCctvId,
                    ReaderId = readerId,
                    AccessControlId = accessControlId,
                    FloorplanMaskedAreaId = floorplanMaskedAreaId,
                    ApplicationId = Guid.Parse(userApplicationId),
                    Name = row.Cell(6).GetValue<string>(),
                    Type = (DeviceType)Enum.Parse(typeof(DeviceType), row.Cell(7).GetValue<string>()),
                    PosX = row.Cell(8).GetValue<float>(),
                    PosY = row.Cell(9).GetValue<float>(),
                    PosPxX = row.Cell(10).GetValue<float>(),
                    PosPxY = row.Cell(11).GetValue<float>(),
                    DeviceStatus = (DeviceStatus)Enum.Parse(typeof(DeviceStatus), row.Cell(12).GetValue<string>()),
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                };

                devices.Add(device);
                rowNumber++;
            }

            // Simpan ke database
            foreach (var device in devices)
            {
                await _repository.AddAsync(device);
            }

            return _mapper.Map<IEnumerable<FloorplanDeviceDto>>(devices);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name", "Floorplan.Name", "AccessCctv.Name", "Reader.Name", "AccessControl.Name", "FloorplanMaskedArea.Name" };
            var validSortColumns = new[] { "Name", "Floorplan.Name", "AccessCctv.Name", "Reader.Name", "AccessControl.Name", "FloorplanMaskedArea.Name", "CreatedAt", "UpdatedAt", "RestrictedStatus", "Status" };

            var filterService = new GenericDataTableService<FloorplanDevice, FloorplanDeviceDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
        public async Task<byte[]> ExportPdfAsync()
        {
            var devices = await _repository.GetAllExportAsync();

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Floorplan Device Report")
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
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Type").SemiBold();
                            header.Cell().Element(CellStyle).Text("Floorplan").SemiBold();
                            header.Cell().Element(CellStyle).Text("CCTV").SemiBold();
                            header.Cell().Element(CellStyle).Text("Reader").SemiBold();
                            header.Cell().Element(CellStyle).Text("Access Control").SemiBold();
                            header.Cell().Element(CellStyle).Text("Device Status").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        int index = 1;
                        foreach (var device in devices)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(device.Name);
                            table.Cell().Element(CellStyle).Text(device.Type.ToString());
                            table.Cell().Element(CellStyle).Text(device.Floorplan?.Name);
                            table.Cell().Element(CellStyle).Text(device.AccessCctv?.Name);
                            table.Cell().Element(CellStyle).Text(device.Reader?.Name);
                            table.Cell().Element(CellStyle).Text(device.AccessControl?.Name);
                            table.Cell().Element(CellStyle).Text(device.DeviceStatus.ToString());
                            table.Cell().Element(CellStyle).Text(device.Status.ToString());
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
            var devices = await _repository.GetAllExportAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Devices");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Type";
            worksheet.Cell(1, 4).Value = "Floorplan";
            worksheet.Cell(1, 5).Value = "CCTV";
            worksheet.Cell(1, 6).Value = "Reader";
            worksheet.Cell(1, 7).Value = "Access Control";
            worksheet.Cell(1, 8).Value = "Device Status";
            worksheet.Cell(1, 9).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var device in devices)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = device.Name;
                worksheet.Cell(row, 3).Value = device.Type.ToString();
                worksheet.Cell(row, 4).Value = device.Floorplan?.Name;
                worksheet.Cell(row, 5).Value = device.AccessCctv?.Name;
                worksheet.Cell(row, 6).Value = device.Reader?.Name;
                worksheet.Cell(row, 7).Value = device.AccessControl?.Name;
                worksheet.Cell(row, 8).Value = device.DeviceStatus.ToString();
                worksheet.Cell(row, 9).Value = device.Status;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
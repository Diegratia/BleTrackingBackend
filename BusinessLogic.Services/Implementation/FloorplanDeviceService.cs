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

            var application = await _repository.GetApplicationByIdAsync(dto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {dto.ApplicationId} not found.");

            var device = _mapper.Map<FloorplanDevice>(dto);
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            device.CreatedBy = username;
            device.UpdatedBy = username;

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

            _mapper.Map(dto, device);

            await _repository.UpdateAsync(device);
        }

        public async Task DeleteAsync(Guid id)
        {
            var device = await _repository.GetByIdAsync(id);
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            device.UpdatedBy = username;

            await _repository.SoftDeleteAsync(id);
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
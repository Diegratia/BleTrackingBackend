using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using ClosedXML.Excel;
using Data.ViewModels;
using Data.ViewModels.Shared.ExceptionHelper;
using DataView;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    public class MstDepartmentService : IMstDepartmentService
    {
        private readonly MstDepartmentRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;
        private readonly IAuditEmitter _audit;

        public MstDepartmentService(MstDepartmentRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IMemoryCache cache, IAuditEmitter audit)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _audit = audit;
        }

        public async Task<MstDepartmentRead> GetByIdAsync(Guid id)
        {
            var department = await _repository.GetByIdAsync(id);
            if (department == null) throw new NotFoundException($"Department with ID {id} not found");
            return _mapper.Map<MstDepartmentRead>(department);
        }

        public async Task<IEnumerable<MstDepartmentRead>> GetAllAsync()
        {
            const string cacheKey = "MstDepartmentService_GetAll";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<MstDepartmentRead> cachedData))
                return cachedData;
            var departments = await _repository.GetAllAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(cacheKey, departments, cacheOptions);
            return departments;
        }

        public async Task<IEnumerable<MstDepartmentRead>> OpenGetAllAsync()
        {
            var departments = await _repository.GetAllAsync();
            return departments;
        }

        public async Task<MstDepartmentRead> CreateAsync(MstDepartmentCreateDto createDto)
        {
            if (createDto == null) throw new BusinessException("Department data cannot be null");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var department = _mapper.Map<MstDepartment>(createDto);
            department.Id = Guid.NewGuid();
            department.CreatedBy = username;
            department.CreatedAt = DateTime.UtcNow;
            department.UpdatedBy = username;
            department.UpdatedAt = DateTime.UtcNow;
            department.Status = 1;

            var createdDepartment = await _repository.AddAsync(department);
            _cache.Remove("MstDepartmentService_GetAll");
            return _mapper.Map<MstDepartmentRead>(createdDepartment);
        }

        public async Task UpdateAsync(Guid id, MstDepartmentUpdateDto updateDto)
        {
            if (updateDto == null) throw new BusinessException("Update data cannot be null");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var department = await _repository.GetByIdAsync(id);
            if (department == null)
                throw new NotFoundException($"Department with ID {id} not found");

            _mapper.Map(updateDto, department);
            department.UpdatedBy = username;
            department.UpdatedAt = DateTime.UtcNow;
            _cache.Remove("MstDepartmentService_GetAll");
            await _repository.UpdateAsync(department);
        }

        public async Task DeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var department = await _repository.GetByIdAsync(id);
            if (department == null)
                throw new NotFoundException($"Department with ID {id} not found");

            department.UpdatedBy = username;
            department.UpdatedAt = DateTime.UtcNow;

            _cache.Remove("MstDepartmentService_GetAll");
            await _repository.DeleteAsync(id);
            await _audit.Deleted(
                "Department",
                department.Id,
                "Deleted department",
                new { department.Name }
            );
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, MstDepartmentFilter filter)
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;
#if DEBUG
            Console.WriteLine($"Filter parameters synchronized. Page: {filter.Page}, PageSize: {filter.PageSize}");
#endif
            var (data, total, filtered) = await _repository.FilterAsync(filter);
            return new { draw = request.Draw, recordsTotal = total, recordsFiltered = filtered, data };
        }

        public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            var districts = await _repository.GetAllExportAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Master Department Report")
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
                            columns.RelativeColumn(2);
                        });

                        // Table header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Code").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Department Host").SemiBold();
                            header.Cell().Element(CellStyle).Text("CreatedBy").SemiBold();
                            header.Cell().Element(CellStyle).Text("CreatedAt").SemiBold();
                            header.Cell().Element(CellStyle).Text("UpdatedBy").SemiBold();
                            header.Cell().Element(CellStyle).Text("UpdatedAt").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        // Table body
                        int index = 1;
                        foreach (var district in districts)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(district.Code);
                            table.Cell().Element(CellStyle).Text(district.Name);
                            table.Cell().Element(CellStyle).Text(district.DepartmentHost);
                            table.Cell().Element(CellStyle).Text(district.CreatedBy);
                            table.Cell().Element(CellStyle).Text(district.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(district.UpdatedBy);
                            table.Cell().Element(CellStyle).Text(district.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellStyle).Text(district.Status.ToString());
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
            var districts = await _repository.GetAllExportAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Departments");

            // Header
            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Code";
            worksheet.Cell(1, 3).Value = "Name";
            worksheet.Cell(1, 4).Value = "Department Host";
            worksheet.Cell(1, 5).Value = "Created By";
            worksheet.Cell(1, 6).Value = "Created At";
            worksheet.Cell(1, 7).Value = "Updated By";
            worksheet.Cell(1, 8).Value = "Updated At";
            worksheet.Cell(1, 9).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var district in districts)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = district.Code;
                worksheet.Cell(row, 3).Value = district.Name;
                worksheet.Cell(row, 4).Value = district.DepartmentHost;
                worksheet.Cell(row, 5).Value = district.CreatedBy;
                worksheet.Cell(row, 6).Value = district.CreatedAt;
                worksheet.Cell(row, 7).Value = district.UpdatedBy;
                worksheet.Cell(row, 8).Value = district.UpdatedAt;
                worksheet.Cell(row, 9).Value = district.Status.ToString();
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<IEnumerable<MstDepartmentRead>> ImportAsync(IFormFile file)
        {
            var departments = new List<MstDepartment>();
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // skip header

            int rowNumber = 2; // start dari baris ke 2
            foreach (var row in rows)
            {

                var department = new MstDepartment
                {
                    Id = Guid.NewGuid(),
                    Code = row.Cell(1).GetValue<string>(),
                    Name = row.Cell(2).GetValue<string>(),
                    DepartmentHost = row.Cell(3).GetValue<string>() ?? "",
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                };

                departments.Add(department);
                rowNumber++;
            }

            foreach (var department in departments)
            {
                await _repository.AddAsync(department);
            }

            return _mapper.Map<IEnumerable<MstDepartmentRead>>(departments);
        }
    }
}

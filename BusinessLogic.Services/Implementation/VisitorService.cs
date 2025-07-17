using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

namespace BusinessLogic.Services.Implementation
{
    public class VisitorService : IVisitorService
    {
        private readonly VisitorRepository _repository;
        private readonly IMapper _mapper;

        private readonly string[] _allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png" }; //tipe gambar
        private const long MaxFileSize = 5 * 1024 * 1024; // max 5mb

        public VisitorService(VisitorRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<VisitorDto> CreateVisitorAsync(VisitorCreateDto createDto)
        {
            var visitor = _mapper.Map<Visitor>(createDto);
            if (createDto == null) throw new ArgumentNullException(nameof(createDto));

            if (!await _repository.ApplicationExists(createDto.ApplicationId))
                throw new ArgumentException($"Application with ID {createDto.ApplicationId} not found.");

            if (createDto.FaceImage != null && createDto.FaceImage.Length > 0)
            {
                try
                {
                    // extensi file yang diterima
                    if (!_allowedImageTypes.Contains(createDto.FaceImage.ContentType))
                        throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

                    // Validasi ukuran file
                    if (createDto.FaceImage.Length > MaxFileSize)
                        throw new ArgumentException("File size exceeds 5 MB limit.");

                    // folder penyimpanan di lokal server
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
                    Directory.CreateDirectory(uploadDir); // akan membuat directory jika belum ada

                    // buat nama file unik
                    var fileName = $"{Guid.NewGuid()}_{createDto.FaceImage.FileName}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await createDto.FaceImage.CopyToAsync(stream);
                    }

                    visitor.FaceImage = $"/Uploads/visitorFaceImages/{fileName}";
                    visitor.UploadFr = 1; // Sukses
                    visitor.UploadFrError = "Upload successful";
                }
                catch (Exception ex)
                {
                    visitor.UploadFr = 2;
                    visitor.UploadFrError = ex.Message;
                    visitor.FaceImage = "";
                }
            }
            else
            {
                visitor.UploadFr = 0;
                visitor.UploadFrError = "No file uploaded";
                visitor.FaceImage = "";
            }

            visitor.Id = Guid.NewGuid();
            visitor.Status = 1;

            await _repository.AddAsync(visitor);
            return _mapper.Map<VisitorDto>(visitor);
        }

        public async Task<VisitorDto> GetVisitorByIdAsync(Guid id)
        {
            var visitor = await _repository.GetByIdAsync(id);
            return _mapper.Map<VisitorDto>(visitor);
        }

        public async Task<IEnumerable<VisitorDto>> GetAllVisitorsAsync()
        {
            var visitors = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<VisitorDto>>(visitors);
        }

        public async Task<VisitorDto> UpdateVisitorAsync(Guid id, VisitorUpdateDto updateDto)
        {
            if (updateDto == null) throw new ArgumentNullException(nameof(updateDto));

            var visitor = await _repository.GetByIdAsync(id);
            if (visitor == null)
            {
                throw new KeyNotFoundException($"Visitor with ID {id} not found.");
            }

            if (!await _repository.ApplicationExists(updateDto.ApplicationId))
                throw new ArgumentException($"Application with ID {updateDto.ApplicationId} not found.");

            if (updateDto.FaceImage != null && updateDto.FaceImage.Length > 0)
            {
                try
                {
                    if (!_allowedImageTypes.Contains(updateDto.FaceImage.ContentType))
                        throw new ArgumentException("Only image files (jpg, png, jpeg) are allowed.");

                    // Validasi ukuran file
                    if (updateDto.FaceImage.Length > MaxFileSize)
                        throw new ArgumentException("File size exceeds 5 MB limit.");

                    // folder penyimpanan di lokal server
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "visitorFaceImages");
                    Directory.CreateDirectory(uploadDir); // akan membuat directory jika belum ada

                    // buat nama file unik
                    var fileName = $"{Guid.NewGuid()}_{updateDto.FaceImage.FileName}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await updateDto.FaceImage.CopyToAsync(stream);
                    }

                    visitor.FaceImage = $"/Uploads/visitorFaceImages/{fileName}";
                    visitor.UploadFr = 1; // Sukses
                    visitor.UploadFrError = "Upload successful";
                }
                catch (Exception ex)
                {
                    visitor.UploadFr = 2;
                    visitor.UploadFrError = ex.Message;
                    visitor.FaceImage = "";
                }
            }
            else
            {
                visitor.UploadFr = 0;
                visitor.UploadFrError = "No file uploaded";
                visitor.FaceImage = "";
            }

            _mapper.Map(updateDto, visitor);
            await _repository.UpdateAsync();
            return _mapper.Map<VisitorDto>(visitor);
        }

        public async Task DeleteVisitorAsync(Guid id)
        {
            var visitor = await _repository.GetByIdAsync(id);
            if (visitor == null)
            {
                throw new KeyNotFoundException($"Visitor with ID {id} not found.");
            }
            visitor.Status = 0;

            await _repository.DeleteAsync(visitor);
        }

          public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name" }; 
            var validSortColumns = new[] { "Name" ,  "Gender", "CardNumber", "Status" };

            var filterService = new GenericDataTableService<Visitor, VisitorDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }

         public async Task<byte[]> ExportPdfAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var visitorBlacklistAreas = await _repository.GetAllAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Visitor Report")
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
                            header.Cell().Element(CellStyle).Text("Person ID").SemiBold();
                            header.Cell().Element(CellStyle).Text("Identity ID").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Card Number").SemiBold();
                            header.Cell().Element(CellStyle).Text("BLE Card Number").SemiBold();
                            header.Cell().Element(CellStyle).Text("Phone").SemiBold();
                            header.Cell().Element(CellStyle).Text("Email").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        int index = 1;
                        foreach (var visitor in visitorBlacklistAreas)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(visitor.PersonId ?? "-");
                            table.Cell().Element(CellStyle).Text(visitor.IdentityId ?? "-");
                            table.Cell().Element(CellStyle).Text(visitor.Name ?? "-");
                            table.Cell().Element(CellStyle).Text(visitor.CardNumber ?? "-");
                            table.Cell().Element(CellStyle).Text(visitor.BleCardNumber ?? "-");
                            table.Cell().Element(CellStyle).Text(visitor.Phone ?? "-");
                            table.Cell().Element(CellStyle).Text(visitor.Email ?? "-");
                            table.Cell().Element(CellStyle).Text(visitor.Status.ToString() ?? "-");
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
            var visitors = await _repository.GetAllAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Visitors");

            worksheet.Cell(1, 1).Value = "#";
            worksheet.Cell(1, 2).Value = "Person ID";
            worksheet.Cell(1, 3).Value = "Identity ID";
            worksheet.Cell(1, 4).Value = "Name";
            worksheet.Cell(1, 5).Value = "Card Number";
            worksheet.Cell(1, 6).Value = "BLE Card Number";
            worksheet.Cell(1, 7).Value = "Phone";
            worksheet.Cell(1, 8).Value = "Email";
            worksheet.Cell(1, 9).Value = "Gender";
            worksheet.Cell(1, 10).Value = "Address";

            int row = 2;
            int no = 1;

            foreach (var visitor in visitors)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = visitor.PersonId ?? "-";
                worksheet.Cell(row, 3).Value = visitor.IdentityId ?? "-";
                worksheet.Cell(row, 4).Value = visitor.Name ?? "-";
                worksheet.Cell(row, 5).Value = visitor.CardNumber ?? "-";
                worksheet.Cell(row, 6).Value = visitor.BleCardNumber ?? "-";
                worksheet.Cell(row, 7).Value = visitor.Phone ?? "-";
                worksheet.Cell(row, 8).Value = visitor.Email ?? "-";
                worksheet.Cell(row, 9).Value = visitor.Gender.ToString() ?? "-";
                worksheet.Cell(row, 10).Value = visitor.Address ?? "-";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        }
    }
    

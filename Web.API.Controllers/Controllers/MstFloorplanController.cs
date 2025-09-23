using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;


namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class MstFloorplanController : ControllerBase
    {
        private readonly IMstFloorplanService _service;

        public MstFloorplanController(IMstFloorplanService service)
        {
            _service = service;
        }

        // GET: api/MstFloorplan
        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var floorplans = await _service.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Floorplans retrieved successfully",
                    collection = new { data = floorplans },
                    code = 200
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        // GET: api/MstFloorplan/{id}
        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var floorplan = await _service.GetByIdAsync(id);
                if (floorplan == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "Floorplan not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "Floorplan retrieved successfully",
                    collection = new { data = floorplan },
                    code = 200
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        // POST: api/MstFloorplan
        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] MstFloorplanCreateDto dto)
        {
            if (!ModelState.IsValid || (dto.FloorplanImage != null && dto.FloorplanImage.Length == 0))
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                return BadRequest(new
                {
                    success = false,
                    msg = "Validation failed: " + string.Join(", ", errors),
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                var createdFloorplan = await _service.CreateAsync(dto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Floorplan created successfully",
                    collection = new { data = createdFloorplan },
                    code = 201
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        // PUT: api/MstFloorplan/{id}
        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] MstFloorplanUpdateDto dto)
        {
           if (!ModelState.IsValid || (dto.FloorplanImage != null && dto.FloorplanImage.Length == 0))
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                return BadRequest(new
                {
                    success = false,
                    msg = "Validation failed: " + string.Join(", ", errors),
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                await _service.UpdateAsync(id, dto);
                return Ok(new
                {
                    success = true,
                    msg = "Floorplan updated successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Floorplan not found",
                    collection = new { data = (object)null },
                    code = 404
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        // DELETE: api/MstFloorplan/{id}
        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "Floorplan deleted successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Floorplan not found",
                    collection = new { data = (object)null },
                    code = 404
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole")]
        [HttpPost("{filter}")]
        public async Task<IActionResult> Filter([FromBody] DataTablesRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                return BadRequest(new
                {
                    success = false,
                    msg = "Validation failed: " + string.Join(", ", errors),
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                var result = await _service.FilterAsync(request);
                return Ok(new
                {
                    success = true,
                    msg = "Floors filtered successfully",
                    collection = result,
                    code = 200
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    msg = ex.Message,
                    collection = new { data = (object)null },
                    code = 400
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        [HttpPost("import")]
        public async Task<IActionResult> Import([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    msg = "No file uploaded or file is empty",
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    success = false,
                    msg = "Only .xlsx files are allowed",
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                var floorplans = await _service.ImportAsync(file);
                return Ok(new
                {
                    success = true,
                    msg = "Floorplans imported successfully",
                    collection = new { data = floorplans },
                    code = 200
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    msg = ex.Message,
                    collection = new { data = (object)null },
                    code = 400
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            try
            {
                var pdfBytes = await _service.ExportPdfAsync();
                return File(pdfBytes, "application/pdf", "MstFloorplan_Report.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Failed to generate PDF: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            try
            {
                var excelBytes = await _service.ExportExcelAsync();
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "MstFloorplan_Report.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Failed to generate Excel: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        //OPEN

          // GET: api/MstFloorplan
        [AllowAnonymous]
        [HttpGet("open")]
        public async Task<IActionResult> OpenGetAll()
        {
            try
            {
                var floorplans = await _service.OpenGetAllAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Floorplans retrieved successfully",
                    collection = new { data = floorplans },
                    code = 200
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }
        
            [AllowAnonymous]
        [HttpGet("open/{id}")]
        public async Task<IActionResult> OpenGetById(Guid id)
        {
            try
            {
                var floorplan = await _service.GetByIdAsync(id);
                if (floorplan == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "Floorplan not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "Floorplan retrieved successfully",
                    collection = new { data = floorplan },
                    code = 200
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        // POST: api/MstFloorplan
        [AllowAnonymous]
        [HttpPost("open")]
        public async Task<IActionResult> OpenCreate([FromForm] MstFloorplanCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                return BadRequest(new
                {
                    success = false,
                    msg = "Validation failed: " + string.Join(", ", errors),
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                var createdFloorplan = await _service.CreateAsync(dto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Floorplan created successfully",
                    collection = new { data = createdFloorplan },
                    code = 201
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        // PUT: api/MstFloorplan/{id}
        [AllowAnonymous]
        [HttpPut("open/{id}")]
        public async Task<IActionResult> OpenUpdate(Guid id, [FromForm] MstFloorplanUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                return BadRequest(new
                {
                    success = false,
                    msg = "Validation failed: " + string.Join(", ", errors),
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                await _service.UpdateAsync(id, dto);
                return Ok(new
                {
                    success = true,
                    msg = "Floorplan updated successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Floorplan not found",
                    collection = new { data = (object)null },
                    code = 404
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        // DELETE: api/MstFloorplan/{id}
        [AllowAnonymous]
        [HttpDelete("open/{id}")]
        public async Task<IActionResult> OpenDelete(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "Floorplan deleted successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Floorplan not found",
                    collection = new { data = (object)null },
                    code = 404
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("open/{filter}")]
        public async Task<IActionResult> OpenFilter([FromBody] DataTablesRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                return BadRequest(new
                {
                    success = false,
                    msg = "Validation failed: " + string.Join(", ", errors),
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                var result = await _service.FilterAsync(request);
                return Ok(new
                {
                    success = true,
                    msg = "Floors filtered successfully",
                    collection = result,
                    code = 200
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    msg = ex.Message,
                    collection = new { data = (object)null },
                    code = 400
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }
        
    }
}
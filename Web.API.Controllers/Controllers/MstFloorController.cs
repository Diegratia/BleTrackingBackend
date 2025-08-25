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

    public class MstFloorController : ControllerBase
    {
        private readonly IMstFloorService _mstFloorService;

        public MstFloorController(IMstFloorService mstFloorService)
        {
            _mstFloorService = mstFloorService;
        }

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var floors = await _mstFloorService.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Floors retrieved successfully",
                    collection = new { data = floors },
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

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var floor = await _mstFloorService.GetByIdAsync(id);
                if (floor == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "Floor not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "Floor retrieved successfully",
                    collection = new { data = floor },
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

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] MstFloorCreateDto mstFloorDto)
        {
            if (!ModelState.IsValid || (mstFloorDto.FloorImage != null && mstFloorDto.FloorImage.Length == 0))
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
                var createdFloor = await _mstFloorService.CreateAsync(mstFloorDto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Floor created successfully",
                    collection = new { data = createdFloor },
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

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] MstFloorUpdateDto mstFloorDto)
        {
            if (!ModelState.IsValid || (mstFloorDto.FloorImage != null && mstFloorDto.FloorImage.Length == 0))
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

                await _mstFloorService.UpdateAsync(id, mstFloorDto);
                return Ok(new
                {
                    success = true,
                    msg = "Floor updated successfully",
                    collection = new { data = (object)null },
                    code = 200
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Floor not found",
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

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _mstFloorService.DeleteAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "Floor deleted successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Floor not found",
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
                var floors = await _mstFloorService.ImportAsync(file);
                return Ok(new
                {
                    success = true,
                    msg = "Floors imported successfully",
                    collection = new { data = floors },
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

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole")]
        [HttpPost("filter")]
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
                var result = await _mstFloorService.FilterAsync(request);
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

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            try
            {
                var pdfBytes = await _mstFloorService.ExportPdfAsync();
                return File(pdfBytes, "application/pdf", "MstFloor_Report.pdf");
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
                var excelBytes = await _mstFloorService.ExportExcelAsync();
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "MstFloor_Report.xlsx");
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

        [AllowAnonymous]
        [HttpGet("open/{id}")]
        public async Task<IActionResult> OpenGetById(Guid id)
        {
            try
            {
                var floor = await _mstFloorService.GetByIdAsync(id);
                if (floor == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "Floor not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "Floor retrieved successfully",
                    collection = new { data = floor },
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
        [HttpPost("open")]
        public async Task<IActionResult> OpenCreate([FromForm] MstFloorCreateDto mstFloorDto)
        {
            if (!ModelState.IsValid || (mstFloorDto.FloorImage != null && mstFloorDto.FloorImage.Length == 0))
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
                var createdFloor = await _mstFloorService.CreateAsync(mstFloorDto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Floor created successfully",
                    collection = new { data = createdFloor },
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

        [AllowAnonymous]
        [HttpPut("open/{id}")]
        public async Task<IActionResult> OpenUpdate(Guid id, [FromForm] MstFloorUpdateDto mstFloorDto)
        {
            if (!ModelState.IsValid || (mstFloorDto.FloorImage != null && mstFloorDto.FloorImage.Length == 0))
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

                await _mstFloorService.UpdateAsync(id, mstFloorDto);
                return Ok(new
                {
                    success = true,
                    msg = "Floor updated successfully",
                    collection = new { data = (object)null },
                    code = 200
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Floor not found",
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
        [HttpDelete("open/{id}")]
        public async Task<IActionResult> OpenDelete(Guid id)
        {
            try
            {
                await _mstFloorService.DeleteAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "Floor deleted successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Floor not found",
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
        [HttpPost("open/filter")]
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
                var result = await _mstFloorService.FilterAsync(request);
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




        
        


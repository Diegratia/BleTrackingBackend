using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Data.ViewModels.ResponseHelper;
using DataView;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class MstSecurityController : ControllerBase
    {
        private readonly IMstSecurityService _MstSecurityService;

        public MstSecurityController(IMstSecurityService MstSecurityService)
        {
            _MstSecurityService = MstSecurityService;
        }

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole")]
        // GET: api/MstSecurity
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var securities = await _MstSecurityService.GetAllSecuritiesAsync();
            return Ok(ApiResponse.Success("Securities retrieved successfully", securities));
        }
        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole")]
        // GET: api/MstSecurity
        [HttpGet("lookup")]
        public async Task<IActionResult> GetAllLookUpAsync()
        {
            var securities = await _MstSecurityService.GetAllLookUpAsync();
            return Ok(ApiResponse.Success("Securities retrieved successfully", securities));
        }

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole")]
        // GET: api/MstSecurity/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var security = await _MstSecurityService.GetSecurityByIdAsync(id);
            return Ok(ApiResponse.Success("Security retrieved successfully", security));
        }


        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        // POST: api/MstSecurity
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] MstSecurityCreateDto MstSecurityDto)
        {
                if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }
                var createdSecurity = await _MstSecurityService.CreateSecurityAsync(MstSecurityDto);
                return StatusCode(201, ApiResponse.Created("Security created successfully", createdSecurity));
            
        }

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        [HttpDelete("{id}")]
        // DELETE: api/MstSecurity/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
                await _MstSecurityService.DeleteSecurityAsync(id);
                return Ok(ApiResponse.Success("Security deleted successfully"));
        }


        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole")]
        [HttpPost("{filter}")]
        public async Task<IActionResult> Filter([FromBody] DataTablesRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.BadRequest("Invalid filter parameters"));

            var result = await _MstSecurityService.FilterAsync(request);
            return Ok(ApiResponse.Paginated("Securities filtered successfully", result));
        }

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        // PUT: api/MstSecurity/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] MstSecurityUpdateDto updateDto)
        {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
                }

                var security = await _MstSecurityService.UpdateSecurityAsync(id, updateDto);
                return Ok(ApiResponse.Success("Security updated successfully", security));
        }


        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _MstSecurityService.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "MstSecurity_Report.pdf");
        }


        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
                var excelBytes = await _MstSecurityService.ExportExcelAsync();
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "MstSecurity_Report.xlsx");
        }

       [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
            [HttpPost("import")]
            public async Task<IActionResult> Import([FromForm] IFormFile file)
            {
                // ✅ Manual validation sebelum service call
                if (file == null || file.Length == 0)
                    throw new BusinessException("No file uploaded or file is empty");

                if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    throw new BusinessException("Only .xlsx files are allowed");

                var floors = await _MstSecurityService.ImportAsync(file);
                return Ok(ApiResponse.Success("Securities imported successfully", floors));
            }

        //OPEN

        // GET: api/MstSecurity
        [HttpGet("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetAll()
        {
            try
            {
                var Securities = await _MstSecurityService.OpenGetAllSecuritiesAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Securities retrieved successfully",
                    collection = new { data = Securities },
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
    }
}
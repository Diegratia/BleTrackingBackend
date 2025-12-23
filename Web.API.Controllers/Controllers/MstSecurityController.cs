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
                return StatusCode(204, ApiResponse.NoContent("Security deleted successfully"));
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
                return Ok(ApiResponse.Success("Security updated successfully", updateDto));
        }





        // [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        // // POST: api/MstSecurity
        // [HttpPost("{id}/blacklist")]
        // public async Task<IActionResult> BlacklistSecurity(Guid id, [FromBody] BlacklistReasonDto dto)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
        //         return BadRequest(new
        //         {
        //             success = false,
        //             msg = "Validation failed: " + string.Join(", ", errors),
        //             collection = new { data = (object)null },
        //             code = 400
        //         });
        //     }
        //      try
        //     {
        //         await _MstSecurityService.SecurityBlacklistAsync(id, dto);
        //         return Ok(new
        //         {
        //             success = true,
        //             msg = "Security Blacklisted successfully",
        //             collection = new { data = (object)null },
        //             code = 204
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new
        //         {
        //             success = false,
        //             msg = $"Internal server error: {ex.Message}",
        //             collection = new { data = (object)null },
        //             code = 500
        //         });
        //     }
        // }

        // [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        // // POST: api/MstSecurity
        // [HttpPost("{id}/unblacklist")]
        // public async Task<IActionResult> UnBlacklistSecurity(Guid id)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
        //         return BadRequest(new
        //         {
        //             success = false,
        //             msg = "Validation failed: " + string.Join(", ", errors),
        //             collection = new { data = (object)null },
        //             code = 400
        //         });
        //     }
        //      try
        //     {
        //         await _MstSecurityService.UnBlacklistSecurityAsync(id);
        //         return Ok(new
        //         {
        //             success = true,
        //             msg = "Security Unblacklist successfully",
        //             collection = new { data = (object)null },
        //             code = 204
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new
        //         {
        //             success = false,
        //             msg = $"Internal server error: {ex.Message}",
        //             collection = new { data = (object)null },
        //             code = 500
        //         });
        //     }
        // }



        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            try
            {
                var pdfBytes = await _MstSecurityService.ExportPdfAsync();
                return File(pdfBytes, "application/pdf", "MstSecurity_Report.pdf");
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
                var excelBytes = await _MstSecurityService.ExportExcelAsync();
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "MstSecurity_Report.xlsx");
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
                var floors = await _MstSecurityService.ImportAsync(file);
                return Ok(new
                {
                    success = true,
                    msg = "Securities imported successfully",
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
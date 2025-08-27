using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize("RequireAllAndUserCreated")]
    [Authorize("RequireAll")]
    public class TrxVisitorController : ControllerBase
    {
        private readonly ITrxVisitorService _trxVisitorService;

        public TrxVisitorController(ITrxVisitorService trxVisitorService)
        {
            _trxVisitorService = trxVisitorService;
        }

        // POST: api/TrxVisitor
        [Authorize("RequireAll")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] TrxVisitorCreateDto createDto)
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
                var createdTrxVisitor = await _trxVisitorService.CreateTrxVisitorAsync(createDto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "TrxVisitor created successfully",
                    collection = new { data = createdTrxVisitor },
                    code = 201
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

        // GET: api/TrxVisitor/{id}
        [Authorize("RequireAll")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var trxVisitor = await _trxVisitorService.GetTrxVisitorByIdAsync(id);
                if (trxVisitor == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "TrxVisitor blacklist area not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "TrxVisitor retrieved successfully",
                    collection = new { data = trxVisitor },
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

        [HttpGet("public/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByPublicId(Guid id)
        {
            try
            {
                var trxVisitor = await _trxVisitorService.GetTrxVisitorByPublicIdAsync(id);
                if (trxVisitor == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "TrxVisitor blacklist area not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "TrxVisitor retrieved successfully",
                    collection = new { data = trxVisitor },
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

        // GET: api/TrxVisitor
        [HttpGet]
        [Authorize("RequireAllAndUserCreated")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var visitors = await _trxVisitorService.GetAllTrxVisitorsAsync();
                return Ok(new
                {
                    success = true,
                    msg = "TrxVisitor retrieved successfully",
                    collection = new { data = visitors },
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

        [HttpGet("minimal")]
        [Authorize("RequireAllAndUserCreated")]
        public async Task<IActionResult> GetAllMinimal()
        {
            try
            {
                var visitors = await _trxVisitorService.GetAllTrxVisitorsAsyncMinimal();
                return Ok(new
                {
                    success = true,
                    msg = "TrxVisitor retrieved successfully",
                    collection = new { data = visitors },
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

        // PUT: api/VisitorBlacklistArea/{id}
        [HttpPut("{id}")]
        [Authorize("RequireAll")]
        public async Task<IActionResult> Update(Guid id, [FromForm] TrxVisitorUpdateDto trxVisitorDto)
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
                await _trxVisitorService.UpdateTrxVisitorAsync(id, trxVisitorDto);
                return Ok(new
                {
                    success = true,
                    msg = "TrxVisitor updated successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    msg = ex.Message,
                    collection = new { data = (object)null },
                    code = 404
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

        // DELETE: api/VisitorBlacklistArea/{id}
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> Delete(Guid id)
        // {
        //     try
        //     {
        //         await _trxVisitorService.DeleteTrxVisitorAsync(id);
        //         return Ok(new
        //         {
        //             success = true,
        //             msg = "TrxVisitor deleted successfully",
        //             collection = new { data = (object)null },
        //             code = 204
        //         });
        //     }
        //     catch (KeyNotFoundException ex)
        //     {
        //         return NotFound(new
        //         {
        //             success = false,
        //             msg = ex.Message,
        //             collection = new { data = (object)null },
        //             code = 404
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
        [Authorize("RequireAllAndUserCreated")]
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
                var result = await _trxVisitorService.FilterAsync(request);
                return Ok(new
                {
                    success = true,
                    msg = "TrxVisitors filtered successfully",
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
                var pdfBytes = await _trxVisitorService.ExportPdfAsync();
                return File(pdfBytes, "application/pdf", "Trx_Visitor_Report.pdf");
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
                var excelBytes = await _trxVisitorService.ExportExcelAsync();
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Trx_Visitor_Report.xlsx");
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

        [Authorize("RequireAll")]
        [HttpPost("checkin")]
        public async Task<IActionResult> Checkin([FromBody] TrxVisitorCheckinDto request)
        {
            try
            {
                await _trxVisitorService.CheckinVisitorAsync(request);
                return Ok(new
                {
                    success = true,
                    msg = "Visitor checked in successfully",
                    collection = new { data = (object)null },
                    code = 200
                });
            }
            catch (InvalidOperationException ex)
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


        // POST: api/Visitor/{id}/checkout
        [Authorize("RequireAll")]
        [HttpPost("{id}/checkout")]
        public async Task<IActionResult> Checkout(Guid id)
        {
            try
            {
                await _trxVisitorService.CheckoutVisitorAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "Visitor checked out successfully",
                    collection = new { data = (object)null },
                    code = 200
                });
            }
            catch (InvalidOperationException ex)
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

        [Authorize("RequireAll")]
        [HttpPost("{id}/denied")]
        public async Task<IActionResult> Denied(Guid id, DenyReasonDto denyReasonDto)
        {
            try
            {
                await _trxVisitorService.DeniedVisitorAsync(id, denyReasonDto);
                return Ok(new
                {
                    success = true,
                    msg = "Visitor denied successfully",
                    collection = new { data = (object)null },
                    code = 200
                });
            }
            catch (InvalidOperationException ex)
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

        [Authorize("RequireAll")]
        [HttpPost("{id}/blocked")]
        public async Task<IActionResult> Blocked(Guid id, BlockReasonDto blockReason)
        {
            try
            {
                await _trxVisitorService.BlockVisitorAsync(id, blockReason);
                return Ok(new
                {
                    success = true,
                    msg = "Visitor blocked successfully",
                    collection = new { data = (object)null },
                    code = 200
                });
            }
            catch (InvalidOperationException ex)
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

        [Authorize("RequireAll")]
        [HttpPost("{id}/unblocked")]
        public async Task<IActionResult> Unblocked(Guid id)
        {
            try
            {
                await _trxVisitorService.UnblockVisitorAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "Visitor Unblocked successfully",
                    collection = new { data = (object)null },
                    code = 200
                });
            }
            catch (InvalidOperationException ex)
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

          // GET: api/TrxVisitor
        [HttpGet("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetAll()
        {
            try
            {
                var visitors = await _trxVisitorService.OpenGetAllTrxVisitorsAsync();
                return Ok(new
                {
                    success = true,
                    msg = "TrxVisitor retrieved successfully",
                    collection = new { data = visitors },
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
                var result = await _trxVisitorService.FilterRawAsync(request);
                return Ok(new
                {
                    success = true,
                    msg = "TrxVisitors filtered successfully",
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

             // POST: api/Visitor/{id}/checkout
        [AllowAnonymous]
        [HttpPost("open/{id}/checkout")]
        public async Task<IActionResult> OpenCheckout(Guid id)
        {
            try
            {
                await _trxVisitorService.CheckoutVisitorAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "Visitor checked out successfully",
                    collection = new { data = (object)null },
                    code = 200
                });
            }
            catch (InvalidOperationException ex)
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

        [AllowAnonymous]
        [HttpPost("open/{id}/denied")]
        public async Task<IActionResult> OpenDenied(Guid id, DenyReasonDto denyReasonDto)
        {
            try
            {
                await _trxVisitorService.DeniedVisitorAsync(id, denyReasonDto);
                return Ok(new
                {
                    success = true,
                    msg = "Visitor denied successfully",
                    collection = new { data = (object)null },
                    code = 200
                });
            }
            catch (InvalidOperationException ex)
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

        [AllowAnonymous]
        [HttpPost("open/{id}/blocked")]
        public async Task<IActionResult> OpenBlocked(Guid id, BlockReasonDto blockReason)
        {
            try
            {
                await _trxVisitorService.BlockVisitorAsync(id, blockReason);
                return Ok(new
                {
                    success = true,
                    msg = "Visitor blocked successfully",
                    collection = new { data = (object)null },
                    code = 200
                });
            }
            catch (InvalidOperationException ex)
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
            
            [AllowAnonymous]
            [HttpPost("open/{id}/unblocked")]
            public async Task<IActionResult> OpenUnblocked(Guid id)
            {
                try
                {
                    await _trxVisitorService.UnblockVisitorAsync(id);
                    return Ok(new
                    {
                        success = true,
                        msg = "Visitor Unblocked successfully",
                        collection = new { data = (object)null },
                        code = 200
                    });
                }
                catch (InvalidOperationException ex)
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
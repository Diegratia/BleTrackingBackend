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
    [Authorize("RequireAll")]
    public class VisitorController : ControllerBase
    {
        private readonly IVisitorService _visitorService;

        public VisitorController(IVisitorService visitorService)
        {
            _visitorService = visitorService;
        }

        // POST: api/Visitor
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] VisitorCreateDto visitorDto)
        {
            if (!ModelState.IsValid || (visitorDto.FaceImage != null && visitorDto.FaceImage.Length == 0))
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
                var createdVisitor = await _visitorService.CreateVisitorAsync(visitorDto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Visitor created successfully",
                    collection = new { data = createdVisitor },
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

        // GET: api/Visitor/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var visitor = await _visitorService.GetVisitorByIdAsync(id);
                if (visitor == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "Visitor blacklist area not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "Visitor retrieved successfully",
                    collection = new { data = visitor },
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
        [HttpGet("public/{id}")]
        public async Task<IActionResult> GetByIdWhitelist(Guid id)
        {
            try
            {
                var visitor = await _visitorService.GetVisitorByIdPublicAsync(id);
                if (visitor == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "Visitor not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "Visitor retrieved successfully",
                    collection = new { data = visitor },
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
        [HttpPost("public/{id}/decline-invitation")]
        public async Task<IActionResult> DeclineInvitationAsync(Guid id)
        {
            try
            {
                await _visitorService.DeclineInvitationAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "Visitor declined in successfully",
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


        // GET: api/Visitor
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var visitors = await _visitorService.GetAllVisitorsAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Visitor retrieved successfully",
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
        public async Task<IActionResult> Update(Guid id, [FromForm] VisitorUpdateDto visitorDto)
        {
            if (!ModelState.IsValid || (visitorDto.FaceImage != null && visitorDto.FaceImage.Length == 0))
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
                await _visitorService.UpdateVisitorAsync(id, visitorDto);
                return Ok(new
                {
                    success = true,
                    msg = "Visitor updated successfully",
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _visitorService.DeleteVisitorAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "Visitor deleted successfully",
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
                var result = await _visitorService.FilterAsync(request);
                return Ok(new
                {
                    success = true,
                    msg = "Visitors filtered successfully",
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
                var pdfBytes = await _visitorService.ExportPdfAsync();
                return File(pdfBytes, "application/pdf", "Visitor_Report.pdf");
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
                var excelBytes = await _visitorService.ExportExcelAsync();
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Visitor_Report.xlsx");
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

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmVisitorEmail([FromBody] ConfirmEmailDto confirmDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, msg = "Invalid input", collection = new { data = (object)null }, code = 400 });
            }

            try
            {

                await _visitorService.ConfirmVisitorEmailAsync(confirmDto);
                return Ok(new { success = true, msg = "Email confirmed successfully", code = 200 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, msg = $"Internal server error: {ex.Message}", collection = new { data = (object)null }, code = 500 });
            }
        }

        [HttpPost("{id}/send-invitation")]
        public async Task<IActionResult> SendInvitationVisitorAsync(Guid id, [FromBody] CreateInvitationDto CreateInvitationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, msg = "Invalid input", collection = new { data = (object)null }, code = 400 });
            }

            try
            {

                await _visitorService.SendInvitationVisitorAsync(id, CreateInvitationDto);
                return Ok(new { success = true, msg = "Invitation Send successfully", code = 200 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, msg = $"Internal server error: {ex.Message}", collection = new { data = (object)null }, code = 500 });
            }
        }

        [HttpPost("send-invitation")]
        public async Task<IActionResult> SendInvitationByEmailAsync([FromBody] SendEmailInvitationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, msg = "Invalid input", collection = new { data = (object)null }, code = 400 });
            }

            try
            {

                await _visitorService.SendInvitationByEmailAsync(dto);
                return Ok(new { success = true, msg = "Invitation Send successfully", code = 200 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, msg = $"Internal server error: {ex.Message}", collection = new { data = (object)null }, code = 500 });
            }
        }

         [HttpPost("batch/send-invitation")]
        public async Task<IActionResult> SendBatchInvitationByEmailAsync([FromBody]List<SendEmailInvitationDto> dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, msg = "Invalid input", collection = new { data = (object)null }, code = 400 });
            }

            try
            {

                await _visitorService.SendBatchInvitationByEmailAsync(dto);
                return Ok(new { success = true, msg = "Invitation Send successfully", code = 200 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, msg = $"Internal server error: {ex.Message}", collection = new { data = (object)null }, code = 500 });
            }
        }


        [HttpPost("fill-invitation-form")]
        [AllowAnonymous]
        public async Task<IActionResult> FillInvitationForm([FromQuery] string code, [FromQuery] Guid applicationId, [FromForm] VisitorInvitationDto dto)
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
                dto.InvitationCode = code;
                dto.ApplicationId = applicationId;
                var result = await _visitorService.FillInvitationFormAsync(dto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Invitation form filled successfully",
                    collection = new { data = result },
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
        
        [HttpPost("accept-invitation")]
        [AllowAnonymous]
        public async Task<IActionResult> AcceptInvitationFormAsync([FromQuery] string code, [FromQuery] Guid applicationId, [FromForm] MemberInvitationDto dto)
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
                dto.InvitationCode = code;
                dto.ApplicationId = applicationId;
                var result = await _visitorService.AcceptInvitationFormAsync(dto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Invitation Accepted successfully",
                    collection = new { data = result },
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
    }
}


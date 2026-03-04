using System;
using System.Threading.Tasks;
using Data.ViewModels.ResponseHelper;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Shared.Contracts;
using BusinessLogic.Services.Extension.RootExtension;
using Data.ViewModels;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MinLevel(LevelPriority.PrimaryAdmin)]

    public class MstMemberController : ControllerBase
    {
        private readonly IMstMemberService _mstMemberService;

        public MstMemberController(IMstMemberService mstMemberService)
        {
            _mstMemberService = mstMemberService;
        }

        [HttpGet]
        [MinLevel(LevelPriority.PrimaryAdmin)]
        public async Task<IActionResult> GetAll()
        {
            var members = await _mstMemberService.GetAllMembersAsync();
            return Ok(ApiResponse.Success("Members retrieved successfully", members));
        }
        [MinLevel(LevelPriority.PrimaryAdmin)]
        [HttpGet("lookup")]
        public async Task<IActionResult> GetAllLookUpAsync()
        {
            var members = await _mstMemberService.GetAllLookUpAsync();
            return Ok(ApiResponse.Success("Members retrieved successfully", members));
        }
        [MinLevel(LevelPriority.PrimaryAdmin)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var member = await _mstMemberService.GetMemberByIdAsync(id);
            return Ok(ApiResponse.Success("Member retrieved successfully", member));
        }

        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] MstMemberCreateDto dto)
        {
            var result = await _mstMemberService.CreateMemberAsync(dto);
            return Ok(ApiResponse.Created("Member created successfully", result));
        }

        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPost("{id}/blacklist")]
        public async Task<IActionResult> BlacklistMember(Guid id, [FromBody] BlacklistReasonDto dto)
        {
            await _mstMemberService.MemberBlacklistAsync(id, dto);
            return Ok(ApiResponse.NoContent("Member blacklisted successfully"));
        }
        [MinLevel(LevelPriority.PrimaryAdmin)]
        [HttpPost("{id}/unblacklist")]
        public async Task<IActionResult> UnBlacklistMember(Guid id)
        {
            await _mstMemberService.UnBlacklistMemberAsync(id);
            return Ok(ApiResponse.NoContent("Member unblacklisted successfully"));
        }

        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] MstMemberUpdateDto dto)
        {
            await _mstMemberService.UpdateMemberAsync(id, dto);
            return Ok(ApiResponse.Success("Member updated successfully"));
        }

        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mstMemberService.DeleteMemberAsync(id);
            return Ok(ApiResponse.Success("Member deleted successfully"));
        }

        [MinLevel(LevelPriority.PrimaryAdmin)]
        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            var result = await _mstMemberService.FilterAsync(request);
            return Ok(ApiResponse.Paginated("Members retrieved successfully", result));
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _mstMemberService.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "MstMember_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _mstMemberService.ExportExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MstMember_Report.xlsx");
        }

        [MinLevel(LevelPriority.PrimaryAdmin)]
        [HttpPost("import-csv")]
        public async Task<IActionResult> ImportCsv([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.BadRequest("No file uploaded or file is empty"));

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest(ApiResponse.BadRequest("Only .csv files are allowed"));

            await _mstMemberService.ImportCsvAsync(file);
            return Ok(ApiResponse.Success("Members imported successfully"));
        }

        [MinLevel(LevelPriority.PrimaryAdmin)]
        [HttpPost("import-xlsx")]
        public async Task<IActionResult> ImportExcel([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.BadRequest("No file uploaded or file is empty"));

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(ApiResponse.BadRequest("Only .xlsx files are allowed"));

            await _mstMemberService.ImportExcelAsync(file);
            return Ok(ApiResponse.Success("Members imported successfully"));
        }

        [HttpGet("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetAll()
        {
            var members = await _mstMemberService.OpenGetAllMembersAsync();
            return Ok(ApiResponse.Success("Members retrieved successfully", members));
        }
    }
}

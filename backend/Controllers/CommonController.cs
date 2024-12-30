using MediHub.Web.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediHub.Web.Controllers
{
    [ApiController]
    [Route("v1/common")]
    public class CommonController : Controller
    {
        private readonly ICommonService _commonService;

        public CommonController(ICommonService commonService)
        {
            _commonService = commonService;
        }

        /// <summary>
        /// Create new departments
        /// </summary>
        /// <param name="departments"></param>
        /// <returns></returns>
        [HttpPost("upload-docs")]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files uploaded.");
            }

            var uploadedFiles = await _commonService.UploadDocs(files);

            return StatusCode((int)uploadedFiles.StatusCode, uploadedFiles);
        }

        /// <summary>
        /// Create new departments
        /// </summary>
        /// <param name="departments"></param>
        /// <returns></returns>
        [HttpPost("get-docs")]
        public async Task<IActionResult> GetDocs(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files uploaded.");
            }

            var uploadedFiles = await _commonService.UploadDocs(files);
            return StatusCode((int)uploadedFiles.StatusCode, uploadedFiles);
        }
    }
}
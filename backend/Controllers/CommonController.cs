using MediHub.Web.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
        public async Task<IActionResult> Uploads([FromForm] List<IFormFile> files)
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
        [HttpPost("upload-doc")]
        public async Task<IActionResult> Upload(string key, IFormFile file)
        {
            var uploadedFiles = await _commonService.UploadDoc(key, file);
            return StatusCode((int)uploadedFiles.StatusCode, uploadedFiles);
        }

        /// <summary>
        /// Create new departments
        /// </summary>
        /// <param name="departments"></param>
        /// <returns></returns>
        [HttpPost("get-docs")]
        public async Task<IActionResult> GetDocs(List<string> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files uploaded.");
            }

            var uploadedFiles = await _commonService.GetDocs(files);
            return StatusCode((int)uploadedFiles.StatusCode, uploadedFiles);
        }

        /// <summary>
        /// Create new departments
        /// </summary>
        /// <param name="departments"></param>
        /// <returns></returns>
        [HttpPost("get-doc")]
        public async Task<IActionResult> GetDoc(string file)
        {
            var uploadedFiles = await _commonService.GetDoc(file);
            return StatusCode((int)uploadedFiles.StatusCode, JsonConvert.SerializeObject(uploadedFiles));
        }
    }
}
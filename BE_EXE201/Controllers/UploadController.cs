using BE_EXE201.Common;
using BE_EXE201.Common.Payloads.Requests;
using BE_EXE201.Common.Payloads.Responses;
using BE_EXE201.Services;
using Microsoft.AspNetCore.Mvc;

namespace BE_EXE201.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly CloudService _cloudService;
        private readonly ImageService _imageService;

        public UploadController(CloudService cloudService, ImageService imageService)
        {
            _cloudService = cloudService;
            _imageService = imageService;
        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest req)
        {
            if (req.File == null || req.File.Length == 0)
            {
                return BadRequest(ApiResult<UploadRespose>.Error(new UploadRespose
                {
                    Message = "No file was uploaded."
                }));
            }

            try
            {
                // Upload image to cloud storage
                var uploadFile = await _cloudService.UploadImageAsync(req.File);

                if (uploadFile.Error == null)
                {
                    // Save the image URL in the database
                    var saveResult = await _imageService.SaveImageAsync(uploadFile.SecureUrl.ToString());
                    if (saveResult)
                    {
                        return Ok(ApiResult<UploadRespose>.Succeed(new UploadRespose
                        {
                            Url = uploadFile.SecureUrl.ToString(),
                            Message = "Upload file success"
                        }));
                    }
                    else
                    {
                        return StatusCode(500, "Error saving image URL to the database.");
                    }
                }
                else
                {
                    return BadRequest(ApiResult<UploadRespose>.Error(new UploadRespose
                    {
                        Message = "Upload file error: " + uploadFile.Error.Message
                    }));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
    }

}

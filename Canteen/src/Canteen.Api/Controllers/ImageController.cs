using Canteen.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Canteen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly ILogger<ImageController> _logger;

        public ImageController(IImageService imageService, ILogger<ImageController> logger)
        {
            _imageService = imageService;
            _logger = logger;
        }

        [HttpPost("upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            _logger.LogInformation("UploadImage | Execution started");

            if (image == null || image.Length == 0)
            {
                _logger.LogError("No valid file provided");
                return BadRequest("No valid file provided");
            }

            try
            {
                var imageUrl = await _imageService.UploadImageAsync(image);
                _logger.LogInformation("UploadImage | Execution finished");
                return Ok(new { ImageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading image: {ex.Message}");
                return StatusCode(500, "Error uploading image");
            }
        }
    }
}

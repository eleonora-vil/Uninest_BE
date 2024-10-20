using BE_EXE201.Dtos;
using BE_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BE_EXE201.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly HomeService _homeService;
       // private readonly IHttpContextAccessor _httpContextAccessor;
        public HomeController(
            HomeService homeService
            //IHttpContextAccessor httpContextAccessor
            )
        {
            _homeService = homeService;
            //_httpContextAccessor = httpContextAccessor;
        }


        [HttpPost("CreateHome")]
        public async Task<IActionResult> CreateHome([FromForm] HomeModel homeModel, [FromForm] List<IFormFile> images)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); 
                if (userIdClaim == null)
                {
                    return Unauthorized("User not authenticated."); 
                }

                if (!int.TryParse(userIdClaim.Value, out var userId))
                {
                    return BadRequest("Invalid user ID."); 
                }

                var createdHome = await _homeService.CreateNewHome(homeModel, images, userId);

                if (createdHome != null)
                {
                    return Ok(createdHome);
                }
                else
                {
                    return BadRequest("Failed to create the home."); 
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); 
            }
        }


      
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetHomes()
        {
            try
            {
                var homes = await _homeService.GetAllHomes();
                return Ok(homes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetHomeByID")]
        public async Task<IActionResult> GetHomeById(int id)
        {
            try
            {
                var homes = await _homeService.GetHomeById(id);
                return Ok(homes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

   
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHome(int id)
        {
            try
            {
                var result = await _homeService.DeleteHome(id);
                if (result)
                {
                    return NoContent(); 
                }
                else
                {
                    return NotFound("Home not found."); 
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetHomesByUserId")]
        [Authorize] 
        public async Task<IActionResult> GetHomesByUserId(int userId)
        {
            try
            {
                var homes = await _homeService.GetHomesByUserId(userId);
                return Ok(homes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

}

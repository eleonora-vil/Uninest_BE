﻿using BE_EXE201.Dtos;
using BE_EXE201.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BE_EXE201.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly HomeService _homeService;

        public HomeController(HomeService homeService)
        {
            _homeService = homeService;
        }

        // Endpoint to create a new home
        [HttpPost("CreateHome")]
        public async Task<IActionResult> CreateHome([FromForm] HomeModel homeModel, [FromForm] List<IFormFile> images)
        {
            try
            {
                // Assuming you have user authentication and can get the user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); // or ClaimTypes.Email or whatever you use
                if (userIdClaim == null)
                {
                    return Unauthorized("User not authenticated."); // Return 401 Unauthorized if user not found
                }

                // Parse the user ID (assumed to be an integer here)
                if (!int.TryParse(userIdClaim.Value, out var userId))
                {
                    return BadRequest("Invalid user ID."); // Return 400 Bad Request if user ID is not valid
                }

                // Call the service to create a new home with images
                var createdHome = await _homeService.CreateNewHome(homeModel, images, userId);

                if (createdHome != null)
                {
                    return Ok(createdHome); // Return 200 OK with created home data
                }
                else
                {
                    return BadRequest("Failed to create the home."); // Return 400 Bad Request
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // Return 500 Internal Server Error
            }
        }


        // You might also want to implement additional endpoints for CRUD operations.
        // For example:

        // Endpoint to get all homes
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

        // Endpoint to delete a home
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHome(int id)
        {
            try
            {
                var result = await _homeService.DeleteHome(id);
                if (result)
                {
                    return NoContent(); // Return 204 No Content on successful deletion
                }
                else
                {
                    return NotFound("Home not found."); // Return 404 Not Found
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

}

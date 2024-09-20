using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BE_EXE201.Common;
using BE_EXE201.Common.Payloads.Requests;
using BE_EXE201.Common.Payloads.Responses;
using BE_EXE201.Dtos;
using BE_EXE201.Entities;
using BE_EXE201.Exceptions;
using BE_EXE201.Helpers;
using BE_EXE201.Services;
using BE_EXE201.Validation;

namespace BE_EXE201.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private UserService _userService;
        private readonly UserRoleService _userRoleService;

        public UserController(UserService userService, UserRoleService userRoleService)
        {
            _userService = userService;
            _userRoleService = userRoleService;
        }
        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userService.GetAllUsers();
            return Ok(ApiResult<GetUsersResponse>.Succeed(new GetUsersResponse
            {
                Users = result,
            }));
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNewUser([FromBody] CreateNewUserRequest req)
        {

            var user = req.ToUserModel();
            UserValidator validation = new UserValidator();
            var valid = await validation.ValidateAsync(user);
            if (!valid.IsValid)
            {
                throw new RequestValidationException(valid.ToProblemDetails());
            }
            var userRole = await _userRoleService.GetByName(req.RoleName);
            user.RoleID = userRole.RoleId;
            var result = _userService.CreateNewUser(user);
            user.UserRole = userRole;
            if (user is not null)
            {
                return Ok(ApiResult<CreateUsersRespone>.Succeed(new CreateUsersRespone
                {
                    User = user
                }));
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut("Update/{userId}")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserRequest req)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest("Invalid user ID");
                }

                var existingUser = await _userService.GetUserById(userId);

                if (existingUser == null)
                {
                    return NotFound("User not found");
                }


                UserValidator validation = new UserValidator();
                var valid = await validation.ValidateAsync(existingUser);

                if (!valid.IsValid)
                {
                    throw new RequestValidationException(valid.ToProblemDetails());
                }

                var updatedUser = await _userService.UpdateUser(existingUser, req);

                if (updatedUser != null)
                {
                    return Ok(ApiResult<UpdateUserRespone>.Succeed(new UpdateUserRespone
                    {
                        User = updatedUser
                    }));
                }
                else
                {
                    return BadRequest("Failed to update user");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


    }
}

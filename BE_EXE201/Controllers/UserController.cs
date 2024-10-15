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
using BE_EXE201.Extensions.NewFolder;
using System.Net.Mail;
using System.Security.Claims;
using BE_EXE201.Dtos.User;

namespace BE_EXE201.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private UserService _userService;
        private readonly UserRoleService _userRoleService;
        private readonly IdentityService _identityService;
        private readonly EmailService _emailService;
        private readonly IVnPayService _vnPayService;

        public UserController(UserService userService, UserRoleService userRoleService, IdentityService identityService, EmailService emailService, IVnPayService vnPayService)
        {
            _userService = userService;
            _userRoleService = userRoleService;
            _identityService = identityService;
            _emailService = emailService;
            _vnPayService = vnPayService;
        }
        [HttpGet("GetAll")]
        // [Authorize(Roles = "Admin")]
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
        // [Authorize(Roles = "Super Admin,Admin")]
        public async Task<IActionResult> CreateNewUser([FromBody] CreateNewUserRequest req)
        {
            var user = req.ToUserModel();
            UserValidator validations = new UserValidator();
            var valid = await validations.ValidateAsync(user);

            if (!valid.IsValid)
            {
                throw new RequestValidationException(valid.ToProblemDetails());
            }

            var userRole = await _userRoleService.GetByName(req.RoleName);
            user.RoleID = userRole.RoleId;

            var result = await _userService.CreateNewUser(user);

            if (result is not null)
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
        //[Authorize(Roles ="Admin")]
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
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest req)
        {
            var email = req.Email;
            var user = await _userService.GetUserByEmail(email);

            // Check if the user exists
            if (user == null)
            {
                return BadRequest(ApiResult<ForgotPasswordResponse>.Error(new ForgotPasswordResponse
                {
                    Message = "User with this email does not exist"
                }));
            }

            // Generate new password
            var newPassword = SecurityUtil.GenerateRandomPassword();

            // Send email with new password using the service
            var emailSent = await _userService.SendPasswordResetEmail(email, user.FullName, newPassword);
            if (!emailSent)
            {
                throw new BadRequestException("Failed to send email");
            }

            // Update the user's password
            await _userService.UpdateUserPassword(user, newPassword);

            return Ok(ApiResult<ForgotPasswordResponse>.Succeed(new ForgotPasswordResponse
            {
                Message = "A new password has been sent to your email"
            }));
        }


        /*
                [HttpPost("SubmitOTP")]
                public async Task<IActionResult> SubmitOTP(SubmitOTPResquest req)
                {
                    var result = await _userService.SubmitOTP(req);
                    if (!result)
                    {
                        throw new BadRequestException("OTP Code is not Correct");
                    }
                    return Ok(ApiResult<FirstStepResgisterInfoResponse>.Succeed(new FirstStepResgisterInfoResponse
                    {
                        message = $"Create new Account Success for email: {req.Email}"
                    }));
                }*/

        [HttpPost("SubmitOTP")]
        public async Task<IActionResult> SubmitOTP(SubmitOTPResquest req)
        {
            // Kiểm tra xem OTP có hợp lệ không
            var isValidOTP = await _userService.SubmitOTP(req);
            if (!isValidOTP)
            {
                return BadRequest(ApiResult<FirstStepResgisterInfoResponse>.Error(new FirstStepResgisterInfoResponse
                {
                    message = "OTP code is not correct or has expired."
                }));
            }

            // Nếu OTP hợp lệ, trả về thông báo thành công
            return Ok(ApiResult<FirstStepResgisterInfoResponse>.Succeed(new FirstStepResgisterInfoResponse
            {
                message = $"OTP has been verified successfully for email: {req.Email}"
            }));
        }


        [HttpGet("by-email")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            // Validate email
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email is required.");
            }

            // Validate email format
            try
            {
                var addr = new MailAddress(email);
            }
            catch
            {
                return BadRequest("Invalid email format.");
            }

            try
            {
                var user = await _userService.GetUserByEmail(email);
                if (user == null)
                {
                    return NotFound($"User with email {email} not found.");
                }

                return Ok(new
                {
                    Success = true,
                    Message = "User retrieved successfully.",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while processing your request.",
                    Error = ex.Message
                });
            }
        }

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest req)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized("User not authenticated");
                }

                var existingUser = await _userService.GetUserByEmail(userEmail);
                if (existingUser == null)
                {
                    return NotFound("User not found");
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

        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized("User not authenticated");
                }

                var existingUser = await _userService.GetUserByEmail(userEmail);
                if (existingUser == null)
                {
                    return NotFound("User not found");
                }

                var result = await _userService.ChangePassword(existingUser, req.CurrentPassword, req.NewPassword);

                if (result)
                {
                    return Ok(ApiResult<string>.Succeed("Password changed successfully"));
                }
                else
                {
                    return BadRequest("Failed to change password");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized("User not authenticated");
                }

                var user = await _userService.GetUserByEmail(userEmail);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                return Ok(ApiResult<UserModel>.Succeed(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

    }
}

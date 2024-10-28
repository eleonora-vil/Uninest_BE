using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BE_EXE201.Common.Payloads.Requests;
using BE_EXE201.Common.Payloads.Responses;
using BE_EXE201.Services;

using BE_EXE201.Common;
using BE_EXE201.Exceptions;
using BE_EXE201.Helpers;
using BE_EXE201.Dtos.Auth;
[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IdentityService _identityService;
    private readonly UserService _userService;
    private readonly EmailService _emailService;

    public AuthController(IdentityService identityService, UserService userService, EmailService emailService)
    {
        _identityService = identityService;
        _userService = userService;
        _emailService = emailService;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Signup([FromBody] SignupRequest req)
    {
        var res = await _identityService.Signup(req);
        if (!res)
        {
            var resultFail = new SignupResponse
            {
                Messages = "Sign up fail"
            };
            return BadRequest(ApiResult<SignupResponse>.Succeed(resultFail));
        }
        var result = new SignupResponse
        {
            Messages = "Sign up success",
            Status = "Inactive" // Add this line
        };

        return Ok(ApiResult<SignupResponse>.Succeed(result));
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> ResendOTP([FromBody] ResendOTPRequest req)
    {
        try
        {
            var result = await _identityService.ResendOTP(req.Email);
            if (!result)
            {
                var failResponse = new ResendOTPResponse
                {
                    Message = "Failed to resend OTP"
                };
                return BadRequest(ApiResult<ResendOTPResponse>.Error(failResponse));
            }

            var successResponse = new ResendOTPResponse
            {
                Message = "OTP has been resent successfully"
            };
            return Ok(ApiResult<ResendOTPResponse>.Succeed(successResponse));
        }
        catch (BadRequestException ex)
        {
            var failResponse = new ResendOTPResponse
            {
                Message = ex.Message
            };
            return BadRequest(ApiResult<ResendOTPResponse>.Error(failResponse));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResult<object>.Fail(ex));
        }
    }



    [AllowAnonymous]
    [HttpPost]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        var loginResult = _identityService.Login(req.Email, req.Password);

        if (!loginResult.Authenticated)
        {
            return Unauthorized(new { Message = loginResult.Message });
        }

        var handler = new JwtSecurityTokenHandler();
        var res = new LoginResponse
        {
            AccessToken = handler.WriteToken(loginResult.Token),
            Wallet = loginResult.Wallet,
            UserId = loginResult.UserId,  // Use the UserId from loginResult
            IsMember = loginResult.IsMember
        };

        return Ok(ApiResult<LoginResponse>.Succeed(res));
    }




    // [Authorize]
    [HttpGet]
    public async Task<IActionResult> CheckToken()
    {
        Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Split()[1];
        // Here goes your token validation logic
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new BadRequestException("Authorization header is missing or invalid.");
        }
        // Decode the JWT token
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Check if the token is expired
        if (jwtToken.ValidTo < DateTime.UtcNow)
        {
            throw new BadRequestException("Token has expired.");
        }

        string email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

        var user = await _userService.GetUserByEmail(email);
        if (user == null)
        {
            return BadRequest("email is in valid");
        }

        // If token is valid, return success response
        return Ok(ApiResult<CheckTokenResponse>.Succeed(new CheckTokenResponse
        {
            User = user
        }));
    }

}
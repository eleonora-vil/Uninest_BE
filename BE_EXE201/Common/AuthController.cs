using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BE_EXE201.Common.Payloads.Requests;
using BE_EXE201.Common.Payloads.Responses;
using BE_EXE201.Services;

using BE_EXE201.Common;
[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IdentityService _identityService;

    public AuthController(IdentityService identityService)
    {
        _identityService = identityService;
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
            Messages = "Sign up success"
        };

        return Ok(ApiResult<SignupResponse>.Succeed(result));
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        var loginResult = _identityService.Login(req.Email, req.Password);
        if (!loginResult.Authenticated)
        {
            return Unauthorized();
        }

        var handler = new JwtSecurityTokenHandler();
        var res = new LoginResponse
        {
            AccessToken = handler.WriteToken(loginResult.Token),
        };
        return Ok(ApiResult<LoginResponse>.Succeed(res));
    }
}
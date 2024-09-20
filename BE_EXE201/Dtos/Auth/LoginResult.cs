using Microsoft.IdentityModel.Tokens;

namespace BE_EXE201.Dtos.Auth;

public class LoginResult
{
    public bool Authenticated { get; set; }
    public SecurityToken? Token { get; set; }
}
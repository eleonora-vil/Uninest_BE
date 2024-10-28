using Microsoft.IdentityModel.Tokens;

namespace BE_EXE201.Dtos.Auth;

public class LoginResult
{
    public bool Authenticated { get; set; }
    public SecurityToken? Token { get; set; }
    public string? Message { get; set; }
    public int UserId { get; set; }
    public decimal Wallet { get; set; }
    public bool IsMember { get; set; }
}
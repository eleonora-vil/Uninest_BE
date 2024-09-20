namespace BE_EXE201.Common.Payloads.Requests;

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
namespace BE_EXE201.Common.Payloads.Responses;

public class LoginResponse
{
    public string AccessToken { get; set; } = null!;
    public int UserId { get; set; }
}
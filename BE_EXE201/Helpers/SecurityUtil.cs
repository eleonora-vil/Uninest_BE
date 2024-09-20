using System.Security.Cryptography;
using System.Text;

namespace BE_EXE201.Helpers;

public static class SecurityUtil
{
    public static string Hash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        var stringBuilder = new StringBuilder();
            
        foreach (var b in hash)
        {
            stringBuilder.Append(b.ToString("x2"));
        }
            
        return stringBuilder.ToString();
    }
 
}
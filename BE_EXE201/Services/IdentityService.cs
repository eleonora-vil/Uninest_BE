using BE_EXE201.Common.Payloads.Requests;
using BE_EXE201.Dtos.Auth;
using BE_EXE201.Entities;
using BE_EXE201.Exceptions;
using BE_EXE201.Helpers;
using BE_EXE201.Repositories;
using BE_EXE201.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BE_EXE201.Services;

public class IdentityService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IRepository<User, int> _userRepository;
    private readonly IRepository<UserRole, int> _userRoleRepository;
    private readonly EmailService _emailService;

    public IdentityService(IOptions<JwtSettings> jwtSettingsOptions, IRepository<User, int> userRepository, IRepository<UserRole, int> userRoleRepository, EmailService emailService)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtSettingsOptions.Value;
        _userRoleRepository = userRoleRepository;
        _emailService = emailService;
    }

    public async Task<bool> Signup(SignupRequest req)
    {
        var user = _userRepository.FindByCondition(u => u.Email == req.Email).FirstOrDefault();
        if (user is not null)
        {
            throw new BadRequestException("username or email already exists");
        }

        var userAdd = await _userRepository.AddAsync(new User
        {
            Email = req.Email,
            Password = SecurityUtil.Hash(req.Password),
            FullName = req.fullname,
            UserName = req.username,
            Gender = req.gender,
            Address = req.address,
            PhoneNumber = req.phone,
            Status = "Inactive",
            RoleID = 2,
        });
        var res = await _userRepository.Commit();

        return res > 0;
    }

    public LoginResult Login(string email, string password)
    {
        var user = _userRepository.FindByCondition(u => u.Email == email).FirstOrDefault();

        // Ki?m tra xem email có t?n t?i không
        if (user is null)
        {
            return new LoginResult
            {
                Authenticated = false,
                Token = null,
                Message = "Invalid email."
            };
        }

        // Ki?m tra tr?ng thái ng??i dùng
        if (user.Status != "Active")
        {
            // G?i mã OTP m?i
            var otp = new Random().Next(100000, 999999);
            user.OTPCode = otp.ToString();

            // C?p nh?t mã OTP cho ng??i dùng
            _userRepository.Update(user);
            var updateResult = _userRepository.Commit().Result;

            if (updateResult <= 0)
            {
                throw new Exception("Failed to update user with new OTP");
            }

            // G?i mã OTP qua email
            var mailData = new MailData()
            {
                EmailToId = user.Email,
                EmailToName = user.FullName,
                EmailBody = $"Your OTP code is: {otp}",
                EmailSubject = "OTP Verification"
            };

            var result = _emailService.SendEmailAsync(mailData).Result;
            if (!result)
            {
                throw new BadRequestException("Failed to send OTP email");
            }

            return new LoginResult
            {
                Authenticated = false,
                Token = null,
                Message = "Please verify your email. An OTP has been sent to your email."
            };
        }

        // Ki?m tra m?t kh?u
        var hashedPassword = SecurityUtil.Hash(password);
        if (user.Password != hashedPassword)
        {
            return new LoginResult
            {
                Authenticated = false,
                Token = null,
                Message = "Invalid password."
            };
        }

        // N?u m?i th? ??u ?n, t?o token cho ng??i dùng
        return new LoginResult
        {
            Authenticated = true,
            Token = CreateJwtToken(user), // Hàm t?o JWT token
            Message = "Login successful."
        };
    }




    private SecurityToken CreateJwtToken(User user)
    {
        var utcNow = DateTime.UtcNow;
        var userRole = _userRoleRepository.FindByCondition(u => u.RoleId == user.RoleID).FirstOrDefault();
        var authClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.UserId.ToString()),
/*            new(JwtRegisteredClaimNames.Sub, user.UserName),*/
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, userRole.RoleName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(authClaims),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            Expires = utcNow.Add(TimeSpan.FromHours(1)),
        };

        var handler = new JwtSecurityTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);

        return token;
    }
}
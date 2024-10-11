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
        var otp = new Random().Next(100000, 999999);
        var userAdd = await _userRepository.AddAsync(new User
        {
            Email = req.Email,
            Password = SecurityUtil.Hash(req.Password),
            FullName = req.fullname,
            UserName = req.username,
            Gender = req.gender,
            Address = req.address,
            PhoneNumber = req.phone,
            CreateDate = DateTime.Now,
            Status = "Inactive",
            RoleID = 2,
            OTPCode = otp.ToString()

        });

        var res = await _userRepository.Commit();
     
        var mailData = new MailData()
        {
            EmailToId = userAdd.Email,
            EmailToName = userAdd.FullName,
            EmailBody = $"Your OTP code is: {otp}",
            EmailSubject = "OTP Verification"
        };

        var result = _emailService.SendEmailAsync(mailData).Result;
        if (!result)
        {
            throw new BadRequestException("Failed to send OTP email");
        }


        return res > 0;
    }
    public async Task<bool> ResendOTP(string email)
    {
        // Check if the user exists
        var user = _userRepository.FindByCondition(u => u.Email == email).FirstOrDefault();
        if (user is null)
        {
            throw new BadRequestException("User with this email does not exist.");
        }

        if (user.Status == "Active")
        {
            throw new BadRequestException("User is already verified.");
        }
        var otp = new Random().Next(100000, 999999);
        user.OTPCode = otp.ToString();

        _userRepository.Update(user);
        var updateResult = _userRepository.Commit().Result;

        if (updateResult <= 0)
        {
            throw new Exception("Failed to update user with new OTP");
        }

        var mailData = new MailData()
        {
            EmailToId = user.Email,
            EmailToName = user.FullName,
            EmailBody = $"Your new OTP code is: {otp}",
            EmailSubject = "Resend OTP Verification"
        };

        var result = _emailService.SendEmailAsync(mailData).Result;
        if (!result)
        {
            throw new BadRequestException("Failed to send OTP email");
        }
        return true;
    }




    public LoginResult Login(string email, string password)
    {
        var user = _userRepository.FindByCondition(u => u.Email == email).FirstOrDefault();


        if (user is null)
        {
            return new LoginResult
            {
                Authenticated = false,
                Token = null,
                Message = "Invalid email."
            };
        }

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
        if (user.Status != "Active")
        {
         
            return new LoginResult
            {
                Authenticated = false,
                Token = null,
                Message = "Please verify your email. An OTP has been sent to your email."
            };
        }

     
        return new LoginResult
        {
            Authenticated = true,
            Token = CreateJwtToken(user), 
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
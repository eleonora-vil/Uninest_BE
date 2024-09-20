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

    public IdentityService(IOptions<JwtSettings> jwtSettingsOptions, IRepository<User, int> userRepository, IRepository<UserRole, int> userRoleRepository)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtSettingsOptions.Value;
        _userRoleRepository = userRoleRepository;
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
            Status = "Active",
            RoleID = 2,
        });
        var res = await _userRepository.Commit();

        return res > 0;
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
            };
        }
        var userRole = _userRoleRepository.FindByCondition(ur => ur.RoleId == user.RoleID).FirstOrDefault();

        if (userRole is null)
        {
            return new LoginResult
            {
                Authenticated = false,
                Token = null,
            };
        }



        var hash = SecurityUtil.Hash(password);
        if (!user.Password.Equals(hash))
        {
            return new LoginResult
            {
                Authenticated = false,
                Token = null,
            };
        }

        return new LoginResult
        {
            Authenticated = true,
            Token = CreateJwtToken(user),
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
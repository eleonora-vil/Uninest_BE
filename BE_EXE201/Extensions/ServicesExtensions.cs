using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BE_EXE201.Entities;
using BE_EXE201.Repositories;
using BE_EXE201.Services;
using BE_EXE201.Settings;
using AutoMapper;
using BE_EXE201.Helpers;
using BE_EXE201.Middlewares;
using MailKit;
using BE_EXE201.Mapper;
using BE_EXE201.Helpers.Photos;
using BE_EXE201.Services;
using BE_EXE201.Extensions.NewFolder;

namespace BE_EXE201.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ExceptionMiddleware>();
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        //Add Mapper
        var mapperConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new ApplicationMapper());
        });

        IMapper mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);

        //Set time
        //AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
        services.Configure<JwtSettings>(val =>
        {
            val.Key = jwtSettings.Key;
        });

        services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));
        services.Configure<CloundSettings>(configuration.GetSection(nameof(CloundSettings)));

        services.AddAuthorization();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });

        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseSqlServer(configuration.GetConnectionString("SqlDbConnection"));
        });

        services.AddScoped(typeof(IRepository<,>), typeof(GenericRepository<,>));
       // services.AddScoped<DatabaseInitialiser>();
        services.AddScoped<IdentityService>();
        services.AddScoped<UserService>();
        services.AddScoped<UserRoleService>();
        services.AddScoped<EmailService>();
        services.AddScoped<CloudService>();
        services.AddScoped<ImageService>();
        services.AddScoped<HomeService>();
        services.AddScoped<IVnPayService, VnPayService>();


        return services;
    }
}
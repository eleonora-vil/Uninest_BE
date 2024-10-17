
using BE_EXE201.Data;
using BE_EXE201.Entities;
using BE_EXE201.Extensions;
using BE_EXE201.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Net.payOS;

public class Program
{
    public static async Task Main(string[] args)
    {

        IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        PayOS payOS = new PayOS(configuration["Environment:PAYOS_CLIENT_ID"] ?? throw new Exception("Cannot find environment"),
                            configuration["Environment:PAYOS_API_KEY"] ?? throw new Exception("Cannot find environment"),
                            configuration["Environment:PAYOS_CHECKSUM_KEY"] ?? throw new Exception("Cannot find environment"));
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton(payOS);


        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "BE API", Version = "v1" });
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
        });


        builder.Services.AddCors(option =>
            option.AddPolicy("CORS", builder =>
                builder.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed((host) => true)));

        var app = builder.Build();

        // Hook into application lifetime events and trigger only application fully started 
        app.Lifetime.ApplicationStarted.Register(async () =>
        {
            // Database Initialiser 
            await app.InitialiseDatabaseAsync();
        });
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            await using (var scope = app.Services.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.Database.MigrateAsync();
            }

            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("CORS");

        app.UseHttpsRedirection();

        app.UseMiddleware<ExceptionMiddleware>();


        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}

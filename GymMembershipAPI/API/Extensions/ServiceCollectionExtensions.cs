using System.Text;
using System.Threading.RateLimiting;
using GymMembershipAPI.API.Services;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GymMembershipAPI.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connString = configuration.GetConnectionString("DefaultConn");

        // Base de datos
        services.AddDbContext<GymDbContext>(options =>
            options.UseNpgsql(connString));

        // Servicios de dominio
        services.AddScoped<IMembershipTypeService, MembershipTypeService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<IGroupClassService, GroupClassService>();
        services.AddScoped<IRegisterAccessService, RegisterAccessService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();

        // Documentación y Health Checks
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHealthChecks().AddNpgSql(connString!);

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        //JWT config
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
                    ),
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }

    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        //Authorization policys config
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
            .AddPolicy("MemberAccess", policy => policy.RequireClaim("membership_active", "true"));

        return services;
    }

    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("LoginLimit", limit =>
            {
                limit.PermitLimit = 5;
                limit.Window = TimeSpan.FromMinutes(1);
                limit.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limit.QueueLimit = 0; //Rechaza inmediatamente si se excede
            });

            options.AddFixedWindowLimiter("LightLimit", limit =>
            {
                limit.PermitLimit = 100;
                limit.Window = TimeSpan.FromMinutes(1);
                limit.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limit.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("MediumLimit", limit =>
            {
                limit.PermitLimit = 40;
                limit.Window = TimeSpan.FromMinutes(1);
                limit.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limit.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("HeavyLimit", limit =>
            {
                limit.PermitLimit = 10;
                limit.Window = TimeSpan.FromMinutes(1);
                limit.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limit.QueueLimit = 0;
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var problemDetails = new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.10",
                    title = "Too Many Requests",
                    status = "429",
                    detail = "Has excedido el limite de intentos de inicio de sesión, por favor espera un poco",
                    instance = context.HttpContext.Request.Path.Value
                };

                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            };
        });

        return services;
    }
}
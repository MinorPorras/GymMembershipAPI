using System.Text;
using System.Threading.RateLimiting;
using GymMembershipAPI.API.Middlewares;
using GymMembershipAPI.API.Services;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var connString = builder.Configuration.GetConnectionString("DefaultConn");

builder.Services.AddDbContext<GymDbContext>(options =>
    options.UseNpgsql(connString));

builder.Services.AddScoped<IMembershipTypeService, MembershipTypeService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<IGroupClassService, GroupClassService>();
builder.Services.AddScoped<IRegisterAccessService, RegisterAccessService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddNpgSql(connString!);

//JWT config
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
            ClockSkew = TimeSpan.Zero
        };
    });

//Authorization policys config
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("MemberAccess", policy => policy.RequireClaim("membership_active", "true"));

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("LoginLimit", limit =>
    {
        limit.PermitLimit = 5;
        limit.Window = TimeSpan.FromMinutes(1);
        limit.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limit.QueueLimit = 0; //Rechaza inmediatamente si se excede
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingAndTimingMiddleware>();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
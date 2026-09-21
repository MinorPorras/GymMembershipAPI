using System.Text;
using GymMembershipAPI.API.Middlewares;
using GymMembershipAPI.API.Services;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<GymDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConn")));

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler("/Error");
app.UseHttpsRedirection();
app.UseMiddleware<RequestLoggingAndTimingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
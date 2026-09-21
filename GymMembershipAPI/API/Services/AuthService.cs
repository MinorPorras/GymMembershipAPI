using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymMembershipAPI.API.DTOs.Auth;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace GymMembershipAPI.API.Services;

public class AuthService : IAuthService
{
    private readonly GymDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(GymDbContext context, ILogger<AuthService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto dto)
    {
        var user = await _context.Users
            .Include(u => u.Member)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Result<LoginResponseDto>.Failure(AuthErrors.InvalidCredentials);

        // Generar Token JWT
        var token = GenerateJwtToken(user);
        var expirationMinutes = Convert.ToDouble(_configuration["Jwt:ExpirationInMinutes"]);
        var expiresIn = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var response = new LoginResponseDto(
            Token: token,
            ExpiresIn: expiresIn,
            Role: user.Role.ToString(),
            MemberPublicId: user.Member?.PublicId
        );

        return Result<LoginResponseDto>.Success(response);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.PublicId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("role", user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (user.MemberId.HasValue && user.Member != null)
        {
            claims.Add(new Claim("member_public_id", user.Member.PublicId.ToString()));
            claims.Add(new Claim("membership_active", user.Member.IsActive ? "true" : "false"));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationInMinutes"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
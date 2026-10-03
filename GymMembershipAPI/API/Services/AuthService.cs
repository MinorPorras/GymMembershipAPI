using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GymMembershipAPI.API.DTOs.Auth;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly.Registry;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace GymMembershipAPI.API.Services;

public class AuthService : IAuthService
{
    private readonly GymDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;

    public AuthService(GymDbContext context, IConfiguration configuration,
        ResiliencePipelineProvider<string> pipelineProvider, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _pipelineProvider = pipelineProvider;
        _logger = logger;
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto dto, CancellationToken ct)
    {
        var pipeline = _pipelineProvider.GetPipeline("db-pipeline");
        try
        {
            var user = await pipeline.ExecuteAsync<User?>(async innerCt =>
            {
                return (await _context.Users
                    .Include(u => u.Member)
                    .FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken: innerCt))!;
            }, ct);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Result<LoginResponseDto>.Failure(AuthErrors.InvalidCredentials);

            // Generar Token JWT
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            var expirationMinutes = Convert.ToDouble(_configuration["Jwt:ExpirationInMinutes"]);
            var accessTokenExpiresIn = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = refreshTokenExpiresAt,
                CreatedAt = DateTime.UtcNow,
            };

            await _context.RefreshTokens.AddAsync(refreshTokenEntity, ct);
            await _context.SaveChangesAsync(ct);

            var response = new LoginResponseDto(
                Token: token,
                ExpiresIn: accessTokenExpiresIn,
                Role: user.Role.ToString(),
                MemberPublicId: user.Member?.PublicId,
                RefreshToken: refreshToken,
                RefreshTokenExpiration: refreshTokenExpiresAt
            );

            return Result<LoginResponseDto>.Success(response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fallo crítico tras agotar reintentos en Login");

            return Result<LoginResponseDto>.Failure(Error.Unknown);
        }
    }

    public async Task<Result<LoginResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        try
        {
            var storedToken = await _context.RefreshTokens
                .Include(t => t.User)
                .ThenInclude(user => user.Member)
                .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);
            if (storedToken is not { IsActive: true })
                return Result<LoginResponseDto>.Failure(AuthErrors.InvalidRefreshToken);

            if (storedToken.ReplacedByRefreshTokenId.HasValue)
            {
                _logger.LogWarning(
                    "ANOMALÍA DETECTADA: Reuso del refresh token para el usuario {UserId}. Revocando toda la sesión",
                    storedToken.UserId);
                var allUserTokens = await _context.RefreshTokens
                    .Where(t => t.UserId == storedToken.UserId && t.RevokedAt == null)
                    .ToListAsync(ct);

                foreach (var token in allUserTokens)
                    token.RevokedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(ct);
                return Result<LoginResponseDto>.Failure(AuthErrors.InvalidRefreshToken);
            }

            var newRefreshTokenString = GenerateRefreshToken();
            var newRefreshTokenExpiredAt = DateTime.UtcNow.AddDays(7);

            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = storedToken.UserId,
                Token = newRefreshTokenString,
                ExpiresAt = newRefreshTokenExpiredAt,
                CreatedAt = DateTime.UtcNow,
            };

            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.ReplacedByRefreshTokenId = newRefreshTokenEntity.Id;

            var newAccessToken = GenerateJwtToken(storedToken.User);
            var accessTokenExpiresIn =
                DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationInMinutes"]));

            await _context.RefreshTokens.AddAsync(newRefreshTokenEntity, ct);
            await _context.SaveChangesAsync(ct);

            var response = new LoginResponseDto(
                Token: newAccessToken,
                ExpiresIn: accessTokenExpiresIn,
                Role: storedToken.User.Role.ToString(),
                MemberPublicId: storedToken.User.Member?.PublicId,
                RefreshToken: newRefreshTokenString,
                RefreshTokenExpiration: newRefreshTokenExpiredAt
            );

            return Result<LoginResponseDto>.Success(response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fallo crítico en RefreshTokenAsync");
            return Result<LoginResponseDto>.Failure(Error.Unknown);
        }
    }

    public async Task<Result> LogoutAsync(string refreshToken, CancellationToken ct)
    {
        try
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);

            if (storedToken != null && storedToken.IsActive)
            {
                storedToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
                _logger.LogInformation("Usuario {UserId} cerró sesión exitosamente.",  storedToken.UserId);
            }

            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fallo crítico en LogoutAsync");
            return Result.Failure(Error.Unknown);
        }
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.PublicId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("role", user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
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
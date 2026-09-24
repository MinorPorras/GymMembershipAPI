using GymMembershipAPI.API.DTOs.Auth;
using GymMembershipAPI.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthService _authService;

    public AuthController(ILogger<AuthController> logger, IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    [HttpPost("Login")]
    [EnableRateLimiting("LoginLimit")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(dto, ct);
        if (result.IsFailure)
        {
            _logger.LogWarning("Intento de login fallido para el email: {Email}", dto.Email);
            return Unauthorized(new { message = result.Error.Message });
        }

        _logger.LogInformation("Login exitoso para el usuario {Email} con rol: {Role}", dto.Email, result.Value.Role);
        return Ok(result.Value);
    }
}
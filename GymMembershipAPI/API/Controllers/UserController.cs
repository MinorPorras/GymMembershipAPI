using GymMembershipAPI.API.DTOs.User;
using GymMembershipAPI.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // Hereda [Authorize] automáticamente
    [HttpGet("{userPublicId:guid}")]
    [Authorize]
    [EnableRateLimiting("LightLimit")]
    public async Task<IActionResult> GetByPublicIdAsync(Guid userPublicId, CancellationToken ct)
    {
        var result = await _userService.GetByPublicIdAsync(userPublicId, ct);
        if (result.IsFailure) return NotFound(new { message = result.Error.Message });
        return Ok(new
        {
            publicId = result.Value.PublicId,
            email = result.Value.Email,
            role = result.Value.Role.ToString(),
            memberPublicId = result.Value.Member?.PublicId
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("HeavyLimit")]
    public async Task<IActionResult> CreateAsync([FromBody] UserCreateRequestDto dto, CancellationToken ct)
    {
        var result = await _userService.CreateAsync(dto, ct);
        if (result.IsFailure) return BadRequest(new { message = result.Error.Message });
        return CreatedAtAction(
            "GetByPublicId",
            new { userPublicId = result.Value.PublicId },
            new
            {
                publicId = result.Value.PublicId,
                email = result.Value.Email,
                role = result.Value.Role.ToString()
            }
        );
    }
}
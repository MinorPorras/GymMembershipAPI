using GymMembershipAPI.API.DTOs.User;
using GymMembershipAPI.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Todo el controller protegido
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // Hereda [Authorize] automáticamente
    [HttpGet("{userPublicId:guid}")]
    public async Task<IActionResult> GetyPublicIdAsync(Guid userPublicId, CancellationToken ct)
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
    [AllowAnonymous] // Excepción: Público
    public async Task<IActionResult> CreateAsync([FromBody] UserCreateRequestDto dto, CancellationToken ct)
    {
        var result = await _userService.CreateAsync(dto, ct);
        if (result.IsFailure) return BadRequest(new { message = result.Error.Message });
        return Ok(new
        {
            publicId = result.Value.PublicId,
            email = result.Value.Email,
            role = result.Value.Role.ToString()
        });
    }
}
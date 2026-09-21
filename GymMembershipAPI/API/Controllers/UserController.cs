using GymMembershipAPI.API.DTOs.User;
using GymMembershipAPI.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{userPublicId:guid}")]
    public async Task<IActionResult> GetyPublicIdAsync(Guid userPublicId)
    {
        var result = await _userService.GetByPublicIdAsync(userPublicId);
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
    public async Task<IActionResult> CreateAsync([FromBody] UserCreateRequestDto dto)
    {
        var result = await _userService.CreateAsync(dto);
        if (result.IsFailure) return BadRequest(new { message = result.Error.Message });
        return Ok(new
        {
            publicId = result.Value.PublicId,
            email = result.Value.Email,
            role = result.Value.Role.ToString()
        });
    }
}
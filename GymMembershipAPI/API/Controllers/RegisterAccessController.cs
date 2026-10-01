using GymMembershipAPI.API.DTOs.RegisterAccess;
using GymMembershipAPI.API.DTOs.Shared;
using GymMembershipAPI.API.Extensions;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RegisterAccessController : ControllerBase
{
    private readonly IRegisterAccessService _service;

    public RegisterAccessController(IRegisterAccessService service)
    {
        _service = service;
    }


    [HttpPost]
    [Authorize(Roles = "Admin, Staff")]
    [EnableRateLimiting("MediumLimit")]
    public async Task<IActionResult> Register([FromBody] RegisterAccessRequestDto dto, CancellationToken ct)
    {
        var result = await _service.RegisterAsync(dto, ct);
        if (result.IsFailure) return BadRequest(new { message = result.Error.Message });
        var response = RegisterAccessMapper.ToResponseDto(result.Value);
        return Ok(response);
    }

    [HttpGet]
    [Authorize(Roles = "Admin, Staff")]
    [EnableRateLimiting("MediumLimit")]
    public async Task<IActionResult> GetAll(CancellationToken ct, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(page, pageSize, ct);
        var response = result.Value.MapTo(RegisterAccessMapper.ToResponseDtos);
        return Ok(response);
    }

    [HttpGet("{publicId:guid}")]
    [Authorize(Roles = "Admin, Staff")]
    [EnableRateLimiting("LightLimit")]
    public async Task<IActionResult> GetByPublicId([FromRoute] Guid publicId, CancellationToken ct)
    {
        var result = await _service.GetByPublicIdAsync(publicId, ct);
        if (result.IsFailure) return NotFound(new { message = result.Error.Message });
        var response = RegisterAccessMapper.ToResponseDto(result.Value);
        return Ok(response);
    }

    [HttpGet("member/{memberPublicId:guid}")]
    [Authorize(Roles = "Admin, Staff")]
    [EnableRateLimiting("MediumLimit")]
    public async Task<IActionResult> GetByMemberPublicId([FromRoute] Guid memberPublicId, CancellationToken ct,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetByMemberPublicIdAsync(memberPublicId, page, pageSize, ct);
        var response = result.Value.MapTo(RegisterAccessMapper.ToResponseDtos);
        return Ok(response);
    }

    [HttpGet("date/{date:datetime}")]
    [Authorize(Roles = "Admin, Staff")]
    [EnableRateLimiting("MediumLimit")]
    public async Task<IActionResult> GetByDate([FromRoute] DateTime date, CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetByDateAsync(date, page, pageSize, ct);
        var response = result.Value.MapTo(RegisterAccessMapper.ToResponseDtos);
        return Ok(response);
    }

    [HttpGet("me/registeredAccesses")]
    [EnableRateLimiting("MediumLimit")]
    public async Task<IActionResult> GetMyAccesses(CancellationToken ct, int page = 1, int pageSize = 10)
    {
        var memberPublicId = User.GetMemberPublicIdFromToken();
        if (memberPublicId == null) return Forbid("Solo un miembro puede ver sus propios registros de acceso.");

        var result = await _service.GetByMemberPublicIdAsync(memberPublicId.Value, page, pageSize, ct);
        var response = result.Value.MapTo(RegisterAccessMapper.ToResponseDtos);
        return Ok(response);
    }
}
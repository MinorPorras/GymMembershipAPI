using GymMembershipAPI.API.DTOs.RegisterAccess;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin, Staff")]
public class RegisterAccessController : ControllerBase
{
    private readonly IRegisterAccessService _service;

    public RegisterAccessController(IRegisterAccessService service)
    {
        _service = service;
    }


    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterAccessRequestDto dto, CancellationToken ct)
    {
        var result = await _service.RegisterAsync(dto, ct);
        if (result.IsFailure) return BadRequest(new { message = result.Error.Message });
        var response = RegisterAccessMapper.ToResponseDto(result.Value);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(RegisterAccessMapper.ToResponseDtos(result.Value));
    }

    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> GetByPublicId([FromRoute] Guid publicId, CancellationToken ct)
    {
        var result = await _service.GetByPublicIdAsync(publicId, ct);
        if (result.IsFailure) return NotFound(new { message = result.Error.Message });
        var response = RegisterAccessMapper.ToResponseDto(result.Value);
        return Ok(response);
    }

    [HttpGet("member/{memberPublicId:guid}")]
    public async Task<IActionResult> GetByMemberPublicId([FromRoute] Guid memberPublicId, CancellationToken ct)
    {
        var result = await _service.GetByMemberPublicIdAsync(memberPublicId, ct);
        var response = RegisterAccessMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    [HttpGet("date/{date:datetime}")]
    public async Task<IActionResult> GetByDate([FromRoute] DateTime date, CancellationToken ct)
    {
        var result = await _service.GetByDateAsync(date, ct);
        var response = RegisterAccessMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    [HttpGet("me/registeredAccesses")]
    [Authorize]
    public async Task<IActionResult> GetMyAccesses(CancellationToken ct)
    {
        var memberPublicId = GetMemberPublicIdFromToken();
        if (memberPublicId == null) return Forbid("Solo un miembro puede ver sus propios registros de acceso.");

        var result = await _service.GetByMemberPublicIdAsync(memberPublicId.Value, ct);
        var response = RegisterAccessMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    // Helpers
    private Guid? GetMemberPublicIdFromToken()
    {
        var claim = User.FindFirst("member_public_id")?.Value;
        return string.IsNullOrEmpty(claim) ? null : Guid.Parse(claim);
    }
}
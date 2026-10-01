using GymMembershipAPI.API.DTOs.Membership;
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
public class MembershipController : ControllerBase
{
    private readonly IMembershipService _service;

    public MembershipController(IMembershipService service)
    {
        _service = service;
    }


    [HttpGet("{publicId:guid}")]
    [Authorize(Roles = "Admin, Staff")]
    [EnableRateLimiting("LightLimit")]
    public async Task<IActionResult> GetByPublicId([FromRoute] Guid publicId, CancellationToken ct)
    {
        var result = await _service.GetByPublicIdAsync(publicId, ct);
        if (result.IsFailure)
            return NotFound(new { message = result.Error.Message });
        var response = MembershipMapper.ToResponseDto(result.Value);
        return Ok(response);
    }

    [HttpGet("member/{memberPublicId:guid}/active")]
    [Authorize(Roles = "Admin, Staff")]
    [EnableRateLimiting("MediumLimit")]
    public async Task<IActionResult> GetActiveByMember([FromRoute] Guid memberPublicId, CancellationToken ct)
    {
        var result = await _service.GetActiveByMemberPublicId(memberPublicId, ct);
        if (result.IsFailure)
            return NotFound(new { message = result.Error.Message });
        var response = MembershipMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    [HttpGet("member/{memberPublicId:guid}/history")]
    [Authorize(Roles = "Admin, Staff")]
    [EnableRateLimiting("MediumLimit")]
    public async Task<IActionResult> GetHistoryByMember([FromRoute] Guid memberPublicId, CancellationToken ct)
    {
        var result = await _service.GetHistoryByMemberPublicIdAsync(memberPublicId, ct);
        if (result.IsFailure)
            return NotFound(new { message = result.Error.Message });
        var response = MembershipMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    [HttpGet("me/active")]
    [EnableRateLimiting("LightLimit")]
    public async Task<IActionResult> GetMyActiveMembership(CancellationToken ct)
    {
        var memberPublicId = User.GetMemberPublicIdFromToken();
        if (memberPublicId == null)
            return Forbid("Solo miembros puede consultar su propia membresía aquí.");

        var result = await _service.GetActiveByMemberPublicId(memberPublicId.Value, ct);
        if (result.IsFailure) return NotFound(new { message = result.Error.Message });

        var response = MembershipMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    [HttpGet("me/history")]
    [EnableRateLimiting("MediumLimit")]
    public async Task<IActionResult> GetMyMembershipHistory(CancellationToken ct)
    {
        var memberPublicId = User.GetMemberPublicIdFromToken();
        if (memberPublicId == null)
            return Forbid("Solo miembros puede consultar su propio historial de membresías aquí");

        var result = await _service.GetHistoryByMemberPublicIdAsync(memberPublicId.Value, ct);
        if (result.IsFailure) return NotFound(new { message = result.Error.Message });

        var response = MembershipMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    //POST
    [HttpPost]
    [Authorize(Roles = "Admin, Staff")]
    [EnableRateLimiting("HeavyLimit")]
    public async Task<IActionResult> Create([FromBody] MembershipRequestDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (result.IsFailure) return BadRequest(new { message = result.Error.Message });
        var response = MembershipMapper.ToResponseDto(result.Value);
        return CreatedAtAction(
            nameof(GetByPublicId),
            new { publicId = response.PublicId },
            response
        );
    }

    //DELETE
    [HttpDelete("{membershipPublicId:guid}/cancel")]
    [Authorize(Roles = "Admin, Staff")]
    [EnableRateLimiting("HeavyLimit")]
    public async Task<IActionResult> Cancel([FromRoute] Guid membershipPublicId, CancellationToken ct)
    {
        var result = await _service.CancelAsync(membershipPublicId, ct);
        if (result.IsFailure) return NotFound(new { message = result.Error.Message });
        return NoContent();
    }
}
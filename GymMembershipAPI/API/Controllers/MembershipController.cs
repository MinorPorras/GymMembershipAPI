using GymMembershipAPI.API.DTOs.Members;
using GymMembershipAPI.API.DTOs.Membership;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin, Staff")]
public class MembershipController : ControllerBase
{
    private readonly IMembershipService _service;

    public MembershipController(IMembershipService service)
    {
        _service = service;
    }


    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> GetByPublicId([FromRoute] Guid publicId)
    {
        var result = await _service.GetByPublicIdAsync(publicId);
        if (result.IsFailure)
            return NotFound(result.Error.Message);
        var response = MembershipMapper.ToResponseDto(result.Value);
        return Ok(response);
    }

    [HttpGet("member/{memberPubliId:guid}/active")]
    public async Task<IActionResult> GetActiveByMember([FromRoute] Guid memberPubliId)
    {
        var result = await _service.GetActiveByMemberPublicId(memberPubliId);
        if (result.IsFailure)
            return NotFound(result.Error.Message);
        var response = MembershipMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    [HttpGet("member/{memberPublicId:guid}/history")]
    public async Task<IActionResult> GetHistoryByMember([FromRoute] Guid memberPublicId)
    {
        var result = await _service.GetHistoryByMemberPublicIdAsync(memberPublicId);
        if (result.IsFailure)
            return NotFound(new { message = result.Error.Message });
        var response = MembershipMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    [HttpGet("me/active")]
    [Authorize]
    public async Task<IActionResult> GetMyActiveMembership()
    {
        var memberPublicId = GetMemberPublicIdFromToken();
        if (memberPublicId == null)
            return Forbid("Solo miembros puede consultar su propia membresía aquí.");

        var result = await _service.GetActiveByMemberPublicId(memberPublicId.Value);
        if (result.IsFailure) return NotFound(new { message = result.Error.Message });

        var response = MembershipMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    [HttpGet("me/history")]
    [Authorize]
    public async Task<IActionResult> GetMyMembershipHistory()
    {
        var memberPublicId = GetMemberPublicIdFromToken();
        if (memberPublicId == null)
            return Forbid("Solo miembros puede consultar su propio historial de membresías aquí");

        var result = await _service.GetHistoryByMemberPublicIdAsync(memberPublicId.Value);
        if (result.IsFailure) return NotFound(new { message = result.Error.Message });

        var response = MembershipMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    //POST
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MembershipRequestDto dto)
    {
        var result = await _service.CreateAsync(dto);
        if (result.IsFailure) return BadRequest(result.Error.Message);
        var response = MembershipMapper.ToResponseDto(result.Value);
        return CreatedAtAction(
            nameof(GetByPublicId),
            new { publicId = response.PublicId },
            response
        );
    }

    //DELETE
    [HttpDelete("{membershipPublicId}/cancel")]
    public async Task<IActionResult> Cancel([FromRoute] Guid membershipPublicId)
    {
        var result = await _service.CancelAsync(membershipPublicId);
        if (result.IsFailure) return NotFound(new { message = result.Error.Message });
        return NoContent();
    }

    // Helpers
    private Guid? GetMemberPublicIdFromToken()
    {
        var claim = User.FindFirst("member_public_id")?.Value;
        return string.IsNullOrEmpty(claim) ? null : Guid.Parse(claim);
    }
}
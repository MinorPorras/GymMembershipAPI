using GymMembershipAPI.API.DTOs.Members;
using GymMembershipAPI.API.DTOs.Membership;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    
    [HttpGet("member/{memberPubliId:guid}/history")]
    public async Task<IActionResult> GetHistoryByMember([FromRoute] Guid memberPubliId)
    {
        var result = await _service.GetHistoryByMemberPublicIdAsync(memberPubliId);
        if (result.IsFailure)
            return NotFound(result.Error.Message);
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
        if (result.IsFailure) return NotFound(result.Error.Message);
        return NoContent();
    }    
}
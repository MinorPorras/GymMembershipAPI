using System.Diagnostics;
using GymMembershipAPI.API.DTOs;
using GymMembershipAPI.API.DTOs.MembershipType;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/membershiptypes")]
[Authorize(Roles = "Admin, Staff")]
public class MembershipTypeController : ControllerBase
{
    private readonly IMembershipTypeService _service;

    public MembershipTypeController(IMembershipTypeService service)
    {
        _service = service;
    }
    // GET

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        if (result.IsFailure) return BadRequest(new { message = result.Error.Message });
        var dto = MembershipTypeMapper.ToResponseDtos(result.Value);
        return Ok(dto);
    }

    [HttpGet("{publicId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByPublicId([FromRoute] Guid publicId, CancellationToken ct)
    {
        var result = await _service.GetByPublicIdAsync(publicId, ct);
        if (result.IsFailure) return NotFound(new { message = result.Error.Message });
        var dto = MembershipTypeMapper.ToResponseDto(result.Value);
        return Ok(dto);
    }

    // POST
    [HttpPost]
    public async Task<IActionResult> CreateMembershipType([FromBody] MembershipTypeRequestDto newTypeRequestDto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(newTypeRequestDto, ct);
        if (result.IsFailure) return BadRequest(new { message = result.Error.Message });
        var response = MembershipTypeMapper.ToResponseDto(result.Value);
        return CreatedAtAction(nameof(GetByPublicId), new { publicId = response.PublicId }, response);
    }

    // PATCH
    [HttpPut("{publicId:guid}")]
    public async Task<IActionResult> UpdateMembershipType([FromRoute] Guid publicId,
        [FromBody] MembershipTypeRequestDto updatedTypeDto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(publicId, updatedTypeDto, ct);
        if (result.IsFailure)
            return result.Error.Code == "MembershipType.NotFound"
                ? NotFound(new { message = result.Error.Message })
                : BadRequest(new { message = result.Error.Message });
        var response = MembershipTypeMapper.ToResponseDto(result.Value);
        return Ok(response);
    }

    //DELETE
    [HttpDelete("{publicId:guid}")]
    public async Task<IActionResult> DeleteMembershipType([FromRoute] Guid publicId, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(publicId, ct);
        if (result.IsFailure) return NotFound(new { message = result.Error.Message });
        return NoContent();
    }
}
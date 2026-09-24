using GymMembershipAPI.API.DTOs.Members;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.API.Services;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MemberController : ControllerBase
{
    private readonly IMemberService _service;

    public MemberController(IMemberService service)
    {
        _service = service;
    }

    // GET
    [HttpGet]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        if (result.IsFailure) return BadRequest(result.Error.Message);
        var dtoList = MemberMapper.ToResponseDto(result.Value);
        return Ok(dtoList);
    }

    [HttpGet("{publicId}")]
    public async Task<IActionResult> GetByPublicId(Guid publicId, CancellationToken ct)
    {
        var result = await _service.GetByPublicIdAsync(publicId, ct);
        if (result.IsFailure) return BadRequest(result.Error.Message);
        var dto = MemberMapper.ToResponseDto(result.Value);
        return Ok(dto);
    }

    //POST
    [HttpPost]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> CreateAsync([FromBody] MemberCreateDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (result.IsFailure)
            return BadRequest(result.Error.Message);
        var response = MemberMapper.ToResponseDto(result.Value);
        return CreatedAtAction(nameof(GetByPublicId),
            new { publicId = response.PublicId },
            response);
    }

    // PUT
    [HttpPut("{publicId}")]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid publicId, [FromBody] MemberUpdateDto dto,
        CancellationToken ct)
    {
        var result = await _service.UpdateAsync(publicId, dto, ct);
        if (result.IsFailure)
            return result.Error.Code == MemberErrors.NotFound.Code
                ? NotFound(result.Error.Message)
                : BadRequest(result.Error.Message);

        var response = MemberMapper.ToResponseDto(result.Value);
        return Ok(response);
    }

    //DELETE
    [HttpDelete(("{publicId}"))]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid publicId, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(publicId, ct);
        return result.IsFailure
            ? BadRequest(result.Error.Message)
            : NoContent();
    }
}
using GymMembershipAPI.API.DTOs.Members;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.API.Services;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.results;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MemberController: ControllerBase
{
    private readonly MemberService _service;

    public MemberController(MemberService service)
    {
        _service = service;
    }

    // GET
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        if (result.IsFailure) return BadRequest(result.Error.Message);
        var dtoList = MemberMapper.ToResponseDto(result.Value);
        return Ok(dtoList);
    }

    [HttpGet("{publicId}")]
    public async Task<IActionResult> GetByPublicId(Guid publicId)
    {
        var result = await _service.GetByPublicIdAsync(publicId);
        if (result.IsFailure) return BadRequest(result.Error.Message);
        var dto = MemberMapper.ToResponseDto(result.Value);
        return Ok(dto);
    }

    //POST
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] MemberCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        if (result.IsFailure) 
            return BadRequest(result.Error.Message);
        var response = MemberMapper.ToResponseDto(result.Value);
        return CreatedAtAction(nameof(GetByPublicId),
            new { publicId = response.PublicId },
            response);
    }

    // PUT
    [HttpPut("{publicId}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid publicId, [FromBody] MemberUpdateDto dto)
    {
        var result = await _service.UpdateAsync(publicId, dto);
        if (result.IsFailure)
            return result.Error.Code == "Member.NotFound"
                ? NotFound(result.Error.Message)
                : BadRequest(result.Error.Message);

        var response = MemberMapper.ToResponseDto(result.Value);
        return Ok(response);
    }

    //DELETE
    [HttpDelete(("{publicId}"))]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid publicId)
    {
        var result = await _service.DeleteAsync(publicId);
        return result.IsFailure
            ? BadRequest(result.Error.Message)
            : NoContent();
    }
}
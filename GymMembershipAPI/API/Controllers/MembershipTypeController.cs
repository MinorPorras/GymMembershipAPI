using System.Diagnostics;
using GymMembershipAPI.API.DTOs;
using GymMembershipAPI.API.DTOs.MembershipType;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.results;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/membershiptypes")]
public class MembershipTypeController: ControllerBase
{
    private readonly IMembershipTypeService _service;

    public MembershipTypeController(IMembershipTypeService service)
    {
        _service = service;
    }
    // GET

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        if (result.IsFailure) return Conflict(result.Error.Message);
        var dto = MembershipTypeMapper.ToResponseDto(result.Value);
        return Ok(dto);
    }

    [HttpGet("{publicId:guid}")]
    public IActionResult GetMembershipTypesForPublicId([FromRoute] Guid publicId)
    {
        var result = _service.GetByPublicIdAsync(publicId).Result;
        if (result.IsFailure) return Conflict(result.Error.Message);
        var dto = MembershipTypeMapper.ToResponseDto(result.Value);
        return Ok(dto);
    }

    // POST
    [HttpPost]
    public async Task<IActionResult> CreateMembershipType([FromBody]MembershipTypeRequestDto newTypeRequestDto)
    {
        var result = await _service.CreateAsync(newTypeRequestDto);
        if (result.IsFailure) return Conflict(result.Error.Message);
        var response = MembershipTypeMapper.ToResponseDto(result.Value);
        return CreatedAtAction(nameof(CreateMembershipType), 
            new { publicId = result.Value.PublicId },
            response);
    }

    // PATCH
    [HttpPut("{publicId:guid}")]
    public async Task<IActionResult> UpdateMembershipType([FromRoute] Guid publicId, [FromBody] MembershipTypeRequestDto updatedTypeDto)
    {
        var result = await _service.UpdateAsync(publicId, updatedTypeDto);
        if (result.IsFailure) 
            return result.Error.Code == "MembershipType.NotFound" 
                ? NotFound(result.Error.Message) 
                : BadRequest(result.Error.Message);
        var response = MembershipTypeMapper.ToResponseDto(result.Value);
        return Ok(response);
    }
    
    //DELETE
    [HttpDelete("{publicId:guid}")]
    public async Task<IActionResult> DeleteMembershipType([FromRoute] Guid publicId)
    {
        var result = await _service.DeleteAsync(publicId);
        if (result.IsFailure) return Conflict(result.Error);
        return NoContent();
    }
    
}
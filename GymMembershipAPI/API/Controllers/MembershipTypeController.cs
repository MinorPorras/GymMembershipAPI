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
public class MembershipTypeController(IMembershipTypeService service) : Controller
{
    private readonly IMembershipTypeService _service = service;

    // GET
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetMembershipTypes()
    {
        var result = await _service.GetAllAsync();
        if (result.IsFailure) return Conflict(result.Error.message);
        var dto = MembershipTypeMapper.ToResponseDto(result.Value);
        return Ok(dto);
    }

    [HttpGet("{publicId}")]
    public IActionResult GetMembershipTypesForPublicId([FromRoute] Guid publicId)
    {
        var result = _service.GetByPublicIdAsync(publicId).Result;
        if (result.IsFailure) return Conflict(result.Error.message);
        var dto = MembershipTypeMapper.ToResponseDto(result.Value);
        return Ok(dto);
    }

    // POST
    [HttpPost]
    public async Task<IActionResult> CreateMembershipType([FromBody]CreateMembershipTypeDto newTypeDto)
    {
        var result = await _service.CreateAsync(newTypeDto);
        if (result.IsFailure) return Conflict(result.Error.message);
        var response = MembershipTypeMapper.ToResponseDto(result.Value);
        return CreatedAtAction(nameof(GetMembershipTypesForPublicId), 
            new { publicId = result.Value.PublicId },
            response);
    }

    // PATCH
    [HttpPut("{publicId}")]
    public async Task<IActionResult> UpdateMembershipType([FromRoute] Guid publicId, [FromBody] UpdateMembershipTypeDto updatedTypeDto)
    {
        var result = await _service.UpdateAsync(publicId, updatedTypeDto);
        if (result.IsFailure) 
            return result.Error.code == "MembershipType.NotFound" 
                ? NotFound(result.Error.message) 
                : BadRequest(result.Error.message);
        var response = MembershipTypeMapper.ToResponseDto(result.Value);
        return Ok(response);
    }
    
    //DELETE
    [HttpDelete("{publicId}")]
    public async Task<IActionResult> DeleteMembershipType([FromRoute] string publicId)
    {
        if (string.IsNullOrEmpty(publicId)) return BadRequest(Error.NullPublicId);
        if (!Guid.TryParse(publicId, out var publicIdGuid)) return BadRequest(Error.InvalidGuid);
        var result = await _service.DeleteAsync(publicIdGuid);
        if (result.IsFailure) return Conflict(result.Error);
        return NoContent();
    }
    
}
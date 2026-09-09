using GymMembershipAPI.API.DTOs.GroupClass;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.results;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupClassController : ControllerBase
{
    private readonly IGroupClassService _service;

    public GroupClassController(IGroupClassService service)
    {
        _service = service;
    }

    // GET
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        if (result.IsFailure) return NotFound(result.Error.Message);
        var response = GroupClassMapper.ToDtos(result.Value);
        return Ok(response);
    }

    [HttpGet("{PublicId:guid}")]
    public async Task<IActionResult> GetByPublicId([FromRoute] Guid publicId)
    {
        var result = await _service.GetByPublicIdAsync(publicId);
        if (result.IsFailure) return StatusCode(500, result.Error.Message);
        var response = GroupClassMapper.ToDto(result.Value);
        return Ok(response);
    }

    [HttpGet("date/{date:datetime}")]
    public async Task<IActionResult> GetByDate([FromRoute] DateTime date)
    {
        var result = await _service.GetByDateAsync(date);
        if (result.IsFailure) return NotFound(result.Error.Message);
        var response = GroupClassMapper.ToDtos(result.Value);
        return Ok(response);
    }
    
    //POST
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GroupClassRequestDto dto)
    {
        var result = await _service.CreateAsync(dto);
        if (result.IsFailure)
        {
            if (result.Error == GroupClassErrors.NameAlreadyExists) return Conflict(result.Error.Message);
            return BadRequest(result.Error.Message);
        }
        var response = GroupClassMapper.ToDto(result.Value);
        return CreatedAtAction(nameof(GetByPublicId), new { publicId = response.PublicId }, response);
    }
    
    //PUT
    [HttpPut("{publicId:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid publicId, [FromBody] GroupClassRequestDto dto)
    {
        var result = await _service.UpdateAsync(publicId, dto);
        if (result.IsFailure)
        {
            if (result.Error == GroupClassErrors.NameAlreadyExists) return Conflict(result.Error.Message);
            return BadRequest(result.Error.Message);
        }
        var response = GroupClassMapper.ToDto(result.Value);
        return Ok(response);
    }
    
    //DELETE
    [HttpDelete("{publicId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid publicId)
    {
        var result = await _service.DeleteAsync(publicId);
        if (result.IsFailure) return NotFound(result.Error.Message);
        return NoContent();
    }
}
using GymMembershipAPI.API.DTOs.RegisterAccess;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegisterAccessController : ControllerBase
{
    private readonly IRegisterAccessService _service;

    public RegisterAccessController(IRegisterAccessService service)
    {
        _service = service;
    }


    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterAccessRequestDto dto)
    {
        var result = await _service.RegisterAsync(dto);
        if (result.IsFailure) return BadRequest(result.Error);
        var response = RegisterAccessMapper.ToResponseDto(result.Value);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        if (result.IsFailure) return NotFound(result.Error);
        var response = RegisterAccessMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> GetByPublicId([FromRoute] Guid publicId)
    {
        var result = await _service.GetByPublicIdAsync(publicId);
        if (result.IsFailure) return NotFound(result.Error);
        var response = RegisterAccessMapper.ToResponseDto(result.Value);
        return Ok(response);
    }

    [HttpGet("member/{memberPublicId:guid}")]
    public async Task<IActionResult> GetByMemberPublicId([FromRoute] Guid memberPublicId)
    {
        var result = await _service.GetByMemberPublicIdAsync(memberPublicId);
        if (result.IsFailure) return NotFound(result.Error);
        var response = RegisterAccessMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    [HttpGet("date/{date:datetime}")]
    public async Task<IActionResult> GetByDate([FromRoute] DateTime date)
    {
        var result = await _service.GetByDateAsync(date);
        if (result.IsFailure) return NotFound(result.Error);
        var response = RegisterAccessMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }
}
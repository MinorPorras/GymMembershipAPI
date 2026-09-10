using GymMembershipAPI.API.DTOs.Booking;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.results;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _bookingService.GetAllBookings();
        if (result.IsFailure) return StatusCode(500, result.Error.Message);
        var response = BookingMapper.ToDtos(result.Value);
        return Ok(response);
    }

    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid publicId)
    {
        var result = await _bookingService.GetByPublicIdAsync(publicId);
        if (result.IsFailure) return NotFound(result.Error.Message);
        var response = BookingMapper.ToDto(result.Value);
        return Ok(response);
    }

    [HttpGet("member/{memberId:guid}")]
    public async Task<IActionResult> GetByMemberPublicId([FromRoute] Guid memberId)
    {
        var result = await _bookingService.GetBookingByMemberPublicId(memberId);
        if (result.IsFailure) return NotFound(result.Error.Message);
        var response = BookingMapper.ToDtos(result.Value);
        return Ok(response);
    }

    [HttpGet("class/{groupClassId:guid}")]
    public async Task<IActionResult> GetByGroupClassId([FromRoute] Guid groupClassId)
    {
        var result = await _bookingService.GetByClassPublicIdAsync(groupClassId);
        if (result.IsFailure) return NotFound(result.Error.Message);
        var response = BookingMapper.ToDtos(result.Value);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BookingRequestDto dto)
    {
        var result = await _bookingService.CreateAsync(dto);
        if (result.IsFailure)
        {
            if (result.Error.Code == BookingErrors.ClassIsFull.Code)
                return Conflict(result.Error.Message);

            return BadRequest(result.Error.Message);
        }

        var response = BookingMapper.ToDto(result.Value);
        return CreatedAtAction(nameof(GetById),
            new { publicId = response.PublicId },
            response);
    }

    [HttpDelete("{publicId:guid}")]
    public async Task<IActionResult> Cancel([FromRoute] Guid publicId)
    {
        var result = await _bookingService.CancelAsync(publicId);
        if (result.IsSuccess) return NoContent();
        return result.Error.Code == BookingErrors.NotFound.Code
            ? NotFound(result.Error.Message)
            : BadRequest(result.Error.Message);
    }
}
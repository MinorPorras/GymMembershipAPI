using GymMembershipAPI.API.DTOs.Booking;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin, Staff")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _bookingService.GetAllAsync(ct);
        var response = BookingMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> GetByPublicId([FromRoute] Guid publicId, CancellationToken ct)
    {
        var result = await _bookingService.GetByPublicIdAsync(publicId, ct);
        if (result.IsFailure) return NotFound(new { message = result.Error.Message });
        var response = BookingMapper.ToResponseDto(result.Value);
        return Ok(response);
    }

    [HttpGet("member/{memberId:guid}")]
    public async Task<IActionResult> GetByMemberPublicId([FromRoute] Guid memberId, CancellationToken ct)
    {
        var result = await _bookingService.GetByMemberPublicIdAsync(memberId, ct);
        var response = BookingMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    [HttpGet("class/{groupClassId:guid}")]
    public async Task<IActionResult> GetByGroupClassId([FromRoute] Guid groupClassId, CancellationToken ct)
    {
        var result = await _bookingService.GetByClassPublicIdAsync(groupClassId, ct);
        var response = BookingMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    [HttpGet("me/bookings")]
    [Authorize]
    public async Task<IActionResult> GetMyBookings(CancellationToken ct)
    {
        var memberPublicId = GetMemberPublicIdFromToken();
        if (memberPublicId == null) return Forbid("Solo miembros puede ver sus propias reservaciones aquí.");

        var result = await _bookingService.GetByMemberPublicIdAsync(memberPublicId.Value, ct);
        var response = BookingMapper.ToResponseDtos(result.Value);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Policy = "MemberAccess")]
    public async Task<IActionResult> Create([FromBody] BookingRequestDto dto, CancellationToken ct)
    {
        var result = await _bookingService.CreateAsync(dto, ct);
        if (result.IsFailure)
        {
            if (result.Error.Code == BookingErrors.ClassIsFull.Code)
                return Conflict(new { message = result.Error.Message });

            return BadRequest(new { message = result.Error.Message });
        }

        var response = BookingMapper.ToResponseDto(result.Value);
        return CreatedAtAction(nameof(GetByPublicId),
            new { publicId = response.PublicId },
            response);
    }

    [HttpDelete("{publicId:guid}")]
    public async Task<IActionResult> Cancel([FromRoute] Guid publicId, CancellationToken ct)
    {
        var result = await _bookingService.CancelAsync(publicId, ct);
        if (result.IsSuccess) return NoContent();
        return result.Error.Code == BookingErrors.NotFound.Code
            ? NotFound(new { message = result.Error.Message })
            : BadRequest(new { message = result.Error.Message });
    }

    // Helpers
    private Guid? GetMemberPublicIdFromToken()
    {
        var claim = User.FindFirst("member_public_id")?.Value;
        return string.IsNullOrEmpty(claim) ? null : Guid.Parse(claim);
    }
}
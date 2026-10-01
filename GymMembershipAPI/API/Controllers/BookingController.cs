using GymMembershipAPI.API.DTOs.Booking;
using GymMembershipAPI.API.Extensions;
using GymMembershipAPI.API.Mappers;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GymMembershipAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    [EnableRateLimiting("HeavyLimit")]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> GetAll(CancellationToken ct, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _bookingService.GetAllAsync(page, pageSize, ct);
        var response = result.Value?.MapTo(BookingMapper.ToResponseDtos);
        return Ok(response);
    }

    [HttpGet("{publicId:guid}")]
    [EnableRateLimiting("LightLimit")]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> GetByPublicId([FromRoute] Guid publicId, CancellationToken ct)
    {
        var result = await _bookingService.GetByPublicIdAsync(publicId, ct);
        if (result.IsFailure) return NotFound(new { message = result.Error.Message });
        var response = BookingMapper.ToResponseDto(result.Value);
        return Ok(response);
    }

    [HttpGet("member/{memberId:guid}")]
    [EnableRateLimiting("MediumLimit")]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> GetByMemberPublicId([FromRoute] Guid memberId, CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _bookingService.GetByMemberPublicIdAsync(memberId, page, pageSize, ct);
        var response = result.Value?.MapTo(BookingMapper.ToResponseDtos);
        return Ok(response);
    }

    [HttpGet("class/{groupClassId:guid}")]
    [EnableRateLimiting("MediumLimit")]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> GetByGroupClassId([FromRoute] Guid groupClassId, CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _bookingService.GetByClassPublicIdAsync(groupClassId, page, pageSize, ct);
        var response = result.Value?.MapTo(BookingMapper.ToResponseDtos);
        return Ok(response);
    }

    [HttpGet("me/bookings")]
    [EnableRateLimiting("MediumLimit")]
    public async Task<IActionResult> GetMyBookings(CancellationToken ct, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var memberPublicId = User.GetMemberPublicIdFromToken();
        if (memberPublicId == null) return Forbid("Solo miembros puede ver sus propias reservaciones aquí.");

        var result = await _bookingService.GetByMemberPublicIdAsync(memberPublicId.Value, page, pageSize, ct);
        var response = result.Value?.MapTo(BookingMapper.ToResponseDtos);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Policy = "MemberAccess")]
    [EnableRateLimiting("HeavyLimit")]
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
    [EnableRateLimiting("MediumLimit")]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> Cancel([FromRoute] Guid publicId, CancellationToken ct)
    {
        var result = await _bookingService.CancelAsync(publicId, ct);
        if (result.IsSuccess) return NoContent();
        return result.Error.Code == BookingErrors.NotFound.Code
            ? NotFound(new { message = result.Error.Message })
            : BadRequest(new { message = result.Error.Message });
    }
}
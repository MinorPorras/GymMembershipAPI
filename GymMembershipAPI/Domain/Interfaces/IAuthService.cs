using GymMembershipAPI.API.DTOs.Auth;
using GymMembershipAPI.Domain.Results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto dto);
}
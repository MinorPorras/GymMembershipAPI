using GymMembershipAPI.API.DTOs.User;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IUserService : IServiceBase<User>
{
    Task<Result<User>> CreateAsync(UserCreateRequestDto dto);
}
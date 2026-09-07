using GymMembershipAPI.Domain.results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IServiceBase<T> where T : class
{
    public Task<Result<T>> GetByPublicIdAsync(Guid publicId);
    public Task<Result<List<T>>> GetAllAsync();
}
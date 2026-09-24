using GymMembershipAPI.Domain.Results;

namespace GymMembershipAPI.Domain.Interfaces;

public interface IServiceBase<T> where T : class
{
    public Task<Result<T>> GetByPublicIdAsync(Guid publicId, CancellationToken ct);
}
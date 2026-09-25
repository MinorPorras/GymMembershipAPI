using GymMembershipAPI.API.DTOs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GymMembershipAPI.API.Extensions;

public static class PaginatedResultExtensions
{
    public static PaginatedResult<TDto> MapTo<T, TDto>(
        this PaginatedResult<T> source,
        Func<IEnumerable<T>, IEnumerable<TDto>> mapper
    )
    {
        return new PaginatedResult<TDto>(
            mapper(source.Data),
            source.CurrentPage,
            source.PageSize,
            source.TotalRecords
        );
    }

    public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
        this IQueryable<T> source,
        int page,
        int pageSize,
        CancellationToken ct
    )
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var totalRecords = await source.CountAsync(ct);

        var skipAmount = (page - 1) * pageSize;

        var data = await source.Skip(skipAmount).Take(pageSize).ToListAsync(ct);
        return new PaginatedResult<T>(data, page, pageSize, totalRecords);
    }
}
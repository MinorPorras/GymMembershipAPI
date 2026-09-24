namespace GymMembershipAPI.API.DTOs.Shared;

public class PaginatedResult<T>
{
    public PaginatedResult(IEnumerable<T> data, int currentPage, int pageSize, int totalRecords)
    {
        Data = data;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalRecords = totalRecords;
    }

    public IEnumerable<T> Data { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
}
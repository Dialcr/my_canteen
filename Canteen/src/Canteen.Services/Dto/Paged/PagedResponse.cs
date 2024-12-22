using Canteen.Services.Dto.Responses;

namespace Canteen.Services.Dto;

/// <summary>
/// Represents a response object that contains paged data.
/// </summary>
/// <typeparam name="T"></typeparam>
public class PagedResponse<T> : Response<IEnumerable<T>>
{
    public virtual int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }

    public PagedResponse(IEnumerable<T> data, int pageNumber, int pageSize, int totalPages, int totalRecords)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = totalPages;
        TotalRecords = totalRecords;
        Data = data;
        Message = null;
    }
}

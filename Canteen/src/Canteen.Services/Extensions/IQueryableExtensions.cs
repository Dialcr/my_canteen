using Canteen.Services.Dto;
using Microsoft.EntityFrameworkCore;

namespace AvangTur.Application.Extensions;
public static class IQueryableExtensions
{
    public static async Task<PagedResponse<TResult>> ToPagedResultAsync<T, TResult>(this IQueryable<T> source, int pageNumber, int pageSize, Func<T, TResult> projection)
    {
        var (items, totalCount, totalPages) = await source.CorePageResultAsync(pageNumber, pageSize);
        return new PagedResponse<TResult>(items.Select(f => projection(f)).ToList(), pageNumber, pageSize, totalPages, totalCount);
    }

    public static async Task<PagedResponse<T>> ToPagedResultAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize)
    {
        var (items, totalCount, totalPages) = await source.CorePageResultAsync(pageNumber, pageSize);
        return new PagedResponse<T>(items.ToList(), pageNumber, pageSize, totalPages, totalCount);
    }

    public static async Task<PagedResponse<T>> ToPagedResultAsync<T>(this IQueryable<T> source, int? pageNumber, int? pageSize)
    {
        if (pageNumber is null || pageSize is null)
            throw new ArgumentNullException(pageNumber is null ? "PageNumber" : "PageSize", "PageNumber or PageSize can not be null");

        var (items, totalCount, totalPages) = await source.CorePageResultAsync(pageNumber, pageSize);
        return new PagedResponse<T>(items.ToList(), (int)pageNumber, (int)pageSize, totalPages, totalCount);
    }

    private static async Task<(IQueryable<T> source, int totalCount, int totalPages)> CorePageResultAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "El número de página debe ser mayor o igual a 1.");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "El tamaño de página debe ser mayor o igual a 1.");

        var totalCount = await source.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return (source.Skip((pageNumber - 1) * pageSize).Take(pageSize), totalCount, totalPages);
    }

    // public static IQueryable<T> AsAsyncQueryable<T>(this IEnumerable<T> source)
    // {
    //     var queryableData = new EnumerableQuery<T>(source);
    //     return new AsyncQueryableWrapper<T>(queryableData);
    // }

    public static PagedResponse<TResult> ToPagedResult<T, TResult>(this IEnumerable<T> source, int pageNumber, int pageSize, Func<T, TResult> projection)
    {
        var (items, totalCount, totalPages) = source.CorePageResult(pageNumber, pageSize);
        return new PagedResponse<TResult>(items.Select(f => projection(f)).ToList(), pageNumber, pageSize, totalPages, totalCount);
    }

    public static PagedResponse<T> ToPagedResult<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var (items, totalCount, totalPages) = source.CorePageResult(pageNumber, pageSize);
        return new PagedResponse<T>(items.ToList(), pageNumber, pageSize, totalPages, totalCount);
    }

    public static PagedResponse<T> ToPagedResult<T>(this IEnumerable<T> source, int? pageNumber, int? pageSize)
    {
        if (pageNumber is null || pageSize is null)
            throw new ArgumentNullException(pageNumber is null ? "PageNumber" : "PageSize", "PageNumber or PageSize can not be null");

        var (items, totalCount, totalPages) = source.CorePageResult(pageNumber, pageSize);
        return new PagedResponse<T>(items.ToList(), (int)pageNumber, (int)pageSize, totalPages, totalCount);
    }

    private static (IEnumerable<T> source, int totalCount, int totalPages) CorePageResult<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "El número de página debe ser mayor o igual a 1.");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "El tamaño de página debe ser mayor o igual a 1.");

        var totalCount = source.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return (source.Skip((pageNumber - 1) * pageSize).Take(pageSize), totalCount, totalPages);
    }

    private static (IEnumerable<T> source, int totalCount, int totalPages) CorePageResult<T>(this IEnumerable<T> source, int? pageNumber, int? pageSize)
    {
        if (pageNumber is null || pageSize is null)
            throw new ArgumentNullException(pageNumber is null ? "PageNumber" : "PageSize", "PageNumber or PageSize can not be null");

        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "El número de página debe ser mayor o igual a 1.");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "El tamaño de página debe ser mayor o igual a 1.");

        var totalCount = source.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / (int)pageSize);

        return (source.Skip(((int)pageNumber - 1) * (int)pageSize).Take((int)pageSize), totalCount, totalPages);
    }

    private static async Task<(IQueryable<T> source, int totalCount, int totalPages)> CorePageResultAsync<T>(this IQueryable<T> source, int? pageNumber, int? pageSize)
    {
        if (pageNumber is null || pageSize is null)
            throw new ArgumentNullException(pageNumber is null ? "PageNumber" : "PageSize", "PageNumber or PageSize can not be null");

        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "El número de página debe ser mayor o igual a 1.");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "El tamaño de página debe ser mayor o igual a 1.");

        var totalCount = await source.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / (int)pageSize);

        return (source.Skip(((int)pageNumber - 1) * (int)pageSize).Take((int)pageSize), totalCount, totalPages);
    }
}

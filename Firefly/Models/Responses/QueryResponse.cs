using System.Collections.Generic;

namespace Firefly.Models.Responses;

public class QueryResponse<T>
{
    public static QueryResponse<T> Empty => new([], 0, 0, 0);

    public int CurrentPage { get; }

    public bool HasRecord => Records.Count > 0;

    public IReadOnlyList<T> Records { get; }

    public int TotalPages { get; }

    public int TotalRecords { get; }

    public QueryResponse(IEnumerable<T> records, int currentPage, int totalPages, int totalRecords)
    {
        Records = [.. records];
        CurrentPage = currentPage;
        TotalPages = totalPages;
        TotalRecords = totalRecords;
    }

    public override string ToString()
    {
        return $"第 {CurrentPage:N0}/{TotalPages:N0} 页, 共 {TotalRecords:N0} 条记录 (已获取 {Records.Count:N0} 条记录)";
    }
}

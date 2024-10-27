namespace I72_Backend.Entities;

// Result pagination
public class PageableResult
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPage { get; set; }
    // Rows specififed in the response
    public List<Dictionary<string, object?>> Rows { get; set; }
}
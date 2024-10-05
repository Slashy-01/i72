namespace I72_Backend.Entities;

public class PageableResult
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPage { get; set; }
    public List<Dictionary<string, object?>> Rows { get; set; }
}
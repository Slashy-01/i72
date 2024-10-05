using I72_Backend.Entities;

namespace I72_Backend.Interfaces;

public interface IManagementRepository
{
    public int ExecuteCreateScript(String script);
    public List<Dictionary<string, object?>> ExecuteQuery(String query);

    public List<Dictionary<string, object?>> GetRowsByTable(String table, Dictionary<string, string> filters,
        PaginationParams pageable);

    public int GetTotalPages(String table, Dictionary<string, string> filters, PaginationParams pageable);
}
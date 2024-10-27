using I72_Backend.Entities;

namespace I72_Backend.Interfaces;

/*
 * Abstract layer for the management repository
 * The repository is responsible for handling native query on the dynamic tables
 * @author Dung Tran
 */
public interface IManagementRepository
{
    // Execute the create SQL script, transaction for execution was used
    public int ExecuteCreateScript(String script);
    // Execute the READ relevant query
    public List<Dictionary<string, object?>> ExecuteQuery(String query);
    // Read all the rows in a specific table
    public List<Dictionary<string, object?>> GetRowsByTable(String table, Dictionary<string, string> filters,
        PaginationParams pageable);

    // Calculate the total number of pages
    public int GetTotalPages(String table, Dictionary<string, string> filters, PaginationParams pageable);
}
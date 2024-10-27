using I72_Backend.Entities;
using I72_Backend.Entities.Enums;
using I72_Backend.Interfaces;
using Mysqlx.Datatypes;

namespace I72_Backend.Services;

/* The management service is responsible for handling relevant dynamic inventory operation
 * Please note that the SQL native query is needed to use
 * Note that a Dictionary was utilized to enable dynamic query
 */
public class ManagementService : IManagementService
{
    private readonly IManagementRepository _repository;
    private readonly ILogger<ManagementService> _logger;

    public ManagementService(IManagementRepository repository, ILogger<ManagementService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /* Function to create a new table if not exists in the database */
    public void CreateTables(CreateListTablesDto dto)
    {
        var sqlScript = @"";
        foreach (var table in dto.Menu)
        {
            // Aggregate column types of the schema
            var columnTypes = string.Join(", ", table.ColumnDefinitions.Select(column =>
                $"{column.Name} {column.Type}{(column.Key ? " PRIMARY KEY" : "")}"
            ));
            
            // Add constraints for the query
            var constraints = "";
            if (table.Constraints != null && table.Constraints.Count != 0)
            {
                constraints = "," + string.Join(", ", table.Constraints);
            }
            // Finalize the create script
            var createScript = $"CREATE TABLE IF NOT EXISTS `{table.TableName}`(updated_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,{columnTypes}{constraints} );";
            sqlScript += createScript;
        }

        _logger.LogInformation($"Create query: {sqlScript}");
        _repository.ExecuteCreateScript(sqlScript);
    }

    /* Get bar chart aggregate data */
    public List<Dictionary<string, object>> GetBarChartData(String table, String columnX, String columnY,
        AggregationType aggregationType)
    {
        // Create the query for execution
        String query = $@"SELECT {columnX}, {aggregationType}({columnY}) FROM {table} GROUP BY {columnX} HAVING {columnX} IS NOT NULL;";
        _logger.LogInformation($"Get bar chart query: {query}");
        return _repository.ExecuteQuery(query);
    }

    /* Perform insert for a specific table inside the database */
    public String PerformInsert(String table, Dictionary<String, String> values)
    {
        // Generate the columns part of the query by joining the keys of the dictionary
        String columns = string.Join(", ", values.Keys);
        // Generate the values part of the query by joining the values of the dictionary, and ensuring they are properly quoted
        String valuesString = string.Join(", ", values.Values.Select(v => $"'{v}'"));

        // Build the full SQL query
        String query = $@"INSERT INTO `{table}` ({columns}) VALUES ({valuesString})";
        _logger.LogInformation($"Insert query: {query}");
        int insertedRows = _repository.ExecuteCreateScript(query);

        return $"{insertedRows} rows have been inserted";
    }

    /* Perform batch update if match records */
    public String PerformBatchUpdate(String table, Dictionary<String, String?> whereCondition,
        Dictionary<String, String?> updateFields)
    {
        // Where condition for the query
        String whereConditionString =
            string.Join("AND", whereCondition.Keys.Select(k => $"`{k}` = '{whereCondition[k]}'"));
        // Update the corresponding fields
        String setStatementString = string.Join(",", updateFields.Keys.Select(k => $"`{k}` = '{updateFields[k]}'"));
        String query = $@"UPDATE `{table}` SET {setStatementString} WHERE {whereConditionString};";
        _logger.LogInformation($"Batch update query: {query}");
        int res = _repository.ExecuteCreateScript(query);
        return $"{res} rows have been updated";
    }

    public String PerformDeleteById(String table, String column, String id)
    {
        String query = $@"DELETE FROM {table} WHERE `{column}` = '{id}'";
        _logger.LogInformation($"Delete query: {query}");
        int res = _repository.ExecuteCreateScript(query);
        return $"{res} row has been deleted";
    }

    /* Search method to read from the database */
    public PageableResult PerformRead(String table, PaginationParams pageParams,
        Dictionary<String, String?> conditions)
    {
        List<Dictionary<String, object?>> res = _repository.GetRowsByTable(table, conditions, pageParams);
        _logger.LogInformation($"Read query get: {res.Count} records");
        var pageableRes = new PageableResult();
        pageableRes.Page = pageParams.Page;
        pageableRes.PageSize = pageParams.PageSize;
        pageableRes.Rows = res;
        pageableRes.TotalPage = _repository.GetTotalPages(table, conditions, pageParams);
        return pageableRes;
    }

    /* Delete a collection of records in the coresponding table */
    public String PerformBatchDelete(String table, Dictionary<String, String?> whereConditions)
    {
        // Prepare where condition
        String whereConditionString = string.Join(" AND ",
            whereConditions.Keys.Select(k =>  $"`{k}` = '{whereConditions[k]}'"));
        String query = 
            $@"DELETE FROM `{table}` WHERE {whereConditionString}";
        _logger.LogInformation($"Delete query: {query}");
        int res = _repository.ExecuteCreateScript(query);
        return $"{res} row has been deleted";
    }
}
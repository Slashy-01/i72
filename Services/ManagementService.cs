using I72_Backend.Entities;
using I72_Backend.Entities.Enums;
using I72_Backend.Interfaces;
using Mysqlx.Datatypes;

namespace I72_Backend.Services;

public class ManagementService : IManagementService
{
    private readonly IManagementRepository _repository;
    private readonly ILogger<ManagementService> _logger;

    public ManagementService(IManagementRepository repository, ILogger<ManagementService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public void CreateTables(CreateListTablesDto dto)
    {
        var sqlScript = @"";
        foreach (var table in dto.Menu)
        {
            var columnTypes = string.Join(", ", table.ColumnDefinitions.Select(column =>
                $"{column.Name} {column.Type}{(column.Key ? " PRIMARY KEY" : "")}"
            ));
            var createScript = $"CREATE TABLE IF NOT EXISTS `{table.TableName}`({columnTypes});";
            sqlScript += createScript;
        }

        _logger.LogInformation($"Create query: {sqlScript}");
        _repository.ExecuteCreateScript(sqlScript);
    }

    public List<Dictionary<string, object>> GetBarChartData(String table, String columnX, String columnY,
        AggregationType aggregationType)
    {
        String query = $@"SELECT {columnX}, {aggregationType}({columnY}) FROM {table} GROUP BY {columnX}";
        _logger.LogInformation($"Get bar chart query: {query}");
        return _repository.ExecuteQuery(query);
    }

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

    public String PerformBatchUpdate(String table, Dictionary<String, String?> whereCondition,
        Dictionary<String, String?> updateFields)
    {
        String whereConditionString =
            string.Join("AND", whereCondition.Keys.Select(k => $"`{k}` = '{whereCondition[k]}'"));
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

    public List<Dictionary<String, object>> PerformRead(String table, PaginationParams pageParams,
        Dictionary<String, String?> conditions)
    {
        String whereConditionString = string.Join(" AND ",
            conditions.Keys.Select(k => conditions[k] != null ? $"`{k}` LIKE '%{conditions[k]}%'" : $"`{k}` IS NULL"));
        if(whereConditionString.Length == 0 )
        {
            whereConditionString = "TRUE";
        }

        String query =
            @$"SELECT * FROM `{table}` WHERE {whereConditionString} LIMIT {pageParams.PageSize * pageParams.Page}, {pageParams.PageSize}";
        _logger.LogInformation($"Read query: {query}");
        List<Dictionary<String, object>> res = _repository.ExecuteQuery(query);
        return res;
    }

    public String PerformBatchDelete(String table, Dictionary<String, String?> whereConditions)
    {
        String whereConditionString = string.Join(" AND ",
            whereConditions.Keys.Select(k =>  $"`{k}` LIKE '{whereConditions[k]}'"));
        String query = 
            $@"DELETE FROM `{table}` WHERE {whereConditionString}";
        _logger.LogInformation($"Delete query: {query}");
        int res = _repository.ExecuteCreateScript(query);
        return $"{res} row has been deleted";
    }
}
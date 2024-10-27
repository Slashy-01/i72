using I72_Backend.Data;
using I72_Backend.Entities;
using I72_Backend.Exceptions;
using I72_Backend.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace I72_Backend.Repository;

public class ManagementRepository : IManagementRepository
{
    private readonly DB_Context _dbContext;
    private readonly ILogger<ManagementRepository> _logger;

    public ManagementRepository(DB_Context dbContext, ILogger<ManagementRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /* Function implemented to calculate the total pages of the query */
    public int GetTotalPages(String table, Dictionary<string, string> filters, PaginationParams pageable)
    {
        var result = 0;
        // Build the WHERE clause dynamically based on conditions
        string whereClause = string.Empty;
        if (filters != null && filters.Count > 0)
        {
            var conditionsList = new List<string>();
            foreach (var condition in filters)
            {
                conditionsList.Add($"{condition.Key} LIKE @{condition.Key}"); // Creates "ColumnName = @ColumnName"
            }
            whereClause = "WHERE " + string.Join(" AND ", conditionsList);
        }
        // Open the database connection and create a command
        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = $"SELECT CEIL(COUNT(*) / @PageSize) FROM (SELECT * FROM {table} {whereClause} ORDER BY updated_time DESC) AS query_table;";
            // Add parameters for pagination
            command.Parameters.Add(new MySqlParameter("@PageSize", pageable.PageSize));
            _logger.LogInformation("Executing query: " + command.CommandText);
            // Add condition parameters
            if (filters != null && filters.Count > 0)
            {
                foreach (var condition in filters)
                {
                    command.Parameters.Add(new MySqlParameter($"@{condition.Key}", $"%{condition.Value}%"));
                }
            }
            _dbContext.Database.OpenConnection();
            result = Convert.ToInt32(command.ExecuteScalar());
            _dbContext.Database.CloseConnection();
        }

        return result;
    }
    
    // Execute the read query
    public List<Dictionary<string, object?>> GetRowsByTable(String table, Dictionary<string, string> filters, PaginationParams pageable)
    {
        var result = new List<Dictionary<string, object?>>();
        // Build the WHERE clause dynamically based on conditions
        string whereClause = string.Empty;
        if (filters != null && filters.Count > 0)
        {
            var conditionsList = new List<string>();
            foreach (var condition in filters)
            {
                conditionsList.Add($"{condition.Key} LIKE @{condition.Key}"); // Creates "ColumnName = @ColumnName"
            }
            whereClause = "WHERE " + string.Join(" AND ", conditionsList);
        }
        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = $"SELECT * FROM `{table}` {whereClause} ORDER BY updated_time DESC LIMIT @Offset, @PageSize";
            // Add parameters for pagination
            command.Parameters.Add(new MySqlParameter("@Offset", pageable.Page * pageable.PageSize));
            command.Parameters.Add(new MySqlParameter("@PageSize", pageable.PageSize));
            _logger.LogInformation("Executing query: " + command.CommandText);
            // Add condition parameters
            if (filters != null && filters.Count > 0)
            {
                foreach (var condition in filters)
                {
                    // Add all the parameter into the query
                    command.Parameters.Add(new MySqlParameter($"@{condition.Key}", $"%{condition.Value}%"));
                }
            }
            _dbContext.Database.OpenConnection();
            
            // Retrieving the data and convert it into a corresponding object
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var row = new Dictionary<string, object?>();
                    // Interate through the query result
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.GetValue(i);
                        row[reader.GetName(i)] = value == DBNull.Value ? "NULL" : value;
                    }

                    result.Add(row);
                }
            }

            _dbContext.Database.CloseConnection();
        }

        return result;

    }

    // Execute the read query
    public List<Dictionary<string, object?>> ExecuteQuery(String query)
    {
        var result = new List<Dictionary<string, object?>>();

        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = query;
            _dbContext.Database.OpenConnection();
            
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var row = new Dictionary<string, object?>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.GetValue(i);
                        row[reader.GetName(i)] = value == DBNull.Value ? null : value;
                        _logger.LogInformation(reader.GetName(i), value);
                    }

                    result.Add(row);
                }
            }

            _dbContext.Database.CloseConnection();
        }

        return result;

    }
    public int ExecuteCreateScript(String script)
    {
        // Start a transaction
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
                // Raw SQL query to create a table
                int res = _dbContext.Database.ExecuteSqlRaw(script);
                transaction.Commit();
                return res;
            }
            catch (MySqlException ex)
            {
                // Rollback the transaction in case of an error
                transaction.Rollback();

                // Log the error or throw an exception, as needed
                _logger.Log(LogLevel.Error,$"Error: {ex.Message}");
                throw new AppSqlException(ex.Message, ex.Data);
            }
        }
    }
}
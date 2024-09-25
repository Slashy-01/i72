using I72_Backend.Data;
using I72_Backend.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public List<Dictionary<string, object>> QuerySomething()
    {
        var result = new List<Dictionary<string, object>>();

        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = @"SELECT * FROM CUSTOMER_ORDER";
            _dbContext.Database.OpenConnection();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var row = new Dictionary<string, object>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }

                    result.Add(row);
                }
            }

            _dbContext.Database.CloseConnection();
        }

        return result;

    }
    public void ExecuteCreateScript(String script)
    {
        // Start a transaction
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            try
            {
                // Raw SQL query to create a table
                _dbContext.Database.ExecuteSqlRaw(script);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of an error
                transaction.Rollback();

                // Log the error or throw an exception, as needed
                _logger.Log(LogLevel.Error,$"Error: {ex.Message}");
                throw;
            }
        }

    }
}
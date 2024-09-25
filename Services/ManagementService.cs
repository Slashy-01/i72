using I72_Backend.Entities;
using I72_Backend.Interfaces;

namespace I72_Backend.Services;

public class ManagementService : IManagementService
{
    private readonly IManagementRepository _repository;
    public ManagementService(IManagementRepository repository)
    {
        _repository = repository;
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
        _repository.ExecuteCreateScript(sqlScript);
    }
}
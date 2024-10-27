using I72_Backend.Entities;
using I72_Backend.Entities.Enums;

namespace I72_Backend.Interfaces;
/*
 * The interface layer of the management service used for handling business logic
 */
public interface IManagementService
{
    public void CreateTables(CreateListTablesDto dto);

    public List<Dictionary<string, object>> GetBarChartData(String table, String columnX, String columnY,
        AggregationType aggregationType);

    public String PerformInsert(String table, Dictionary<String, String> values);

    public String PerformBatchUpdate(String table, Dictionary<String, String?> whereCondition,
        Dictionary<String, String?> updateFields);

    public String PerformDeleteById(String table, String column, String id);

    public PageableResult PerformRead(String table, PaginationParams pageParams,
        Dictionary<String, String?> conditions);

    public String PerformBatchDelete(String table, Dictionary<String, String?> whereConditions);
}
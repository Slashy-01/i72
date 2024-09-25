namespace I72_Backend.Interfaces;

public interface IManagementRepository
{
    public int ExecuteCreateScript(String script);
    public List<Dictionary<string, object>> ExecuteQuery(String query);
}
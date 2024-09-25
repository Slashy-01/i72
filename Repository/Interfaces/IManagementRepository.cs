namespace I72_Backend.Interfaces;

public interface IManagementRepository
{
    public void ExecuteCreateScript(String script);
    public List<Dictionary<string, object>> QuerySomething();
}
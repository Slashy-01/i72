using I72_Backend.Entities;

namespace I72_Backend.Interfaces;

public interface IManagementService
{
    public void CreateTables(CreateListTablesDto dto);
}
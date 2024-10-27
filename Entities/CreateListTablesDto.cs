namespace I72_Backend.Entities;

// The Dto to create a collection of tables inside the database
public record CreateListTablesDto(List<CreateTableDto> Menu);
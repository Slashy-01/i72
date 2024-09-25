namespace I72_Backend.Entities;

public record CreateTableDto(
    String TableName,
    List<ColumnDefinition> ColumnDefinitions
);
namespace I72_Backend.Entities;

// Create table dto specified by the request
public record CreateTableDto(
    String TableName,
    List<ColumnDefinition> ColumnDefinitions,
    List<String>? Constraints);
namespace I72_Backend.Entities;

public record ColumnDefinition(
    String Name,
    String Type,
    Boolean Key
);
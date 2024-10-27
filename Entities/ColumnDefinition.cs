namespace I72_Backend.Entities;

// The definition of the column to generate the table inside the database
public record ColumnDefinition(
    String Name,
    String Type,
    Boolean Key
);
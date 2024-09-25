namespace I72_Backend.Entities;

public record UpdateRequestDto(
    Dictionary<String, String?> Where,
    Dictionary<String, String?> UpdatedField
    );
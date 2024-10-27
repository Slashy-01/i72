namespace I72_Backend.Entities;


/*
 * Search params request for pagination
 */
public record PaginationParams(
    int Page,
    int PageSize
    );
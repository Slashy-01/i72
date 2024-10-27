using System.Text.Json.Serialization;

namespace I72_Backend.Entities;

/*
 * The standard response object of the application
 */
public class ResponseRestDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Object Data { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public String Message { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Page { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PageSize { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TotalPage { get; set; }
}
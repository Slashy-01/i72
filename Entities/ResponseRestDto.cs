using System.Text.Json.Serialization;

namespace I72_Backend.Entities;

public class ResponseRestDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Object Data { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public String Message { get; set; }
}
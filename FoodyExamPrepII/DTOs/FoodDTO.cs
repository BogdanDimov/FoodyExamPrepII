using System.Text.Json.Serialization;

namespace FoodyExamPrepII.DTOs;

public class FoodDTO
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
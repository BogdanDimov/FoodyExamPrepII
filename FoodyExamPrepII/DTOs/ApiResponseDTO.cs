using System.Text.Json.Serialization;

namespace FoodyExamPrepII.DTOs;

public class ApiResponseDTO
{
    [JsonPropertyName("msg")]
    public string Msg { get; set; } = string.Empty;
    [JsonPropertyName("foodId")]
    public string? FoodId { get; set; }
}
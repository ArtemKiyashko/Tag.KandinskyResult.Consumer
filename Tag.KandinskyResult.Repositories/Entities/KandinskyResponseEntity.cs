using System.Text.Json.Serialization;

namespace Tag.KandinskyResult.Repositories.Entities;

public class KandinskyResponseEntity
{
    [JsonPropertyName("uuid")]
    public required  string uuid { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("result")]
    public KandinskyGenerationResultEntity? Result { get; set; }

    [JsonPropertyName("censored")]
    public bool Censored { get; set; }

    [JsonPropertyName("generationTime")]
    public int? GenerationTime { get; set; }
}

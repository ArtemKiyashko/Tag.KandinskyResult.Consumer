using System;
using System.Text.Json.Serialization;

namespace Tag.KandinskyResult.Repositories.Entities;

public class KandinskyGenerationResultEntity
{
    [JsonPropertyName("files")]
    public List<string>? Files { get; set; }
}

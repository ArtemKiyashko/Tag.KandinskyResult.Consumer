namespace Tag.KandinskyResult.Managers;

public class KandinskyOptions
{
    public Uri BaseAddress { get; set; } = new Uri("https://api-key.fusionbrain.ai");
    public string? XKey { get; set; }
    public string? XSecret { get; set; }
}

namespace Tag.KandinskyResult.Managers;

public class GenerationActivityOptions
{
    public Uri? TablesServiceUri { get; set; }
    public string? TablesConnectionString { get; set; }
    public string GenerationActivityTable { get; set; } = "taggenerationactivities";
}

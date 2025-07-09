using Tag.KandinskyResult.Repositories.Enums;

namespace Tag.KandinskyResult.Repositories.Entities;

public class GenerationActivityEntity : BaseEntity
{
    public required long ChatTgId { get; set; }
    public required GenerationStatuses GenerationStatus { get; set; }
    public required DateTimeOffset StartedDateTime { get; set; }
    public DateTimeOffset? FinishedDateTime { get; set; }
    public required DateTimeOffset GenerationRequestedDateTime { get; set; }
    public string? ResultContainer { get; set; }
    public string? ResultPath { get; set; }
    public required string Prompt { get; set; }
    public required string Uuid { get; set; }
    public int ReadRetryCount { get; set; } = 0;
}

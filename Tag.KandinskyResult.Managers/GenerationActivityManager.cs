using Tag.KandinskyResult.Managers.Dtos;
using Tag.KandinskyResult.Repositories;
using Tag.KandinskyResult.Repositories.Enums;

namespace Tag.KandinskyResult.Managers;

internal class GenerationActivityManager(IGenerationActivityRepository activityRepository) : IGenerationActivityManager
{
    private readonly IGenerationActivityRepository _activityRepository = activityRepository;

    public async Task CompleteActivity(GenerationActivityDto activityDto)
    {
        var entity = await _activityRepository.GetActivityForDate(activityDto.GenerationRequestedDateTime, activityDto.Id.ToString());
        entity.GenerationStatus = GenerationStatuses.Done;
        entity.FinishedDateTime = DateTimeOffset.UtcNow;
        await _activityRepository.UpdateActivity(entity);
    }

    public async Task<IEnumerable<GenerationActivityDto>> GetActivitiesForToday()
    {
        var entities = await _activityRepository.GetActivitiesForDate(DateTimeOffset.UtcNow);
        return BuildResultList(entities);
    }

    public async Task<IEnumerable<GenerationActivityDto>> GetRecentActivities()
    {
        var entities = await _activityRepository.GetActivitiesForDateRange(DateTimeOffset.UtcNow, TimeSpan.FromDays(1));
        return BuildResultList(entities);
    }

    private static List<GenerationActivityDto> BuildResultList(IEnumerable<Repositories.Entities.GenerationActivityEntity> entities)
    {
        var result = new List<GenerationActivityDto>(entities.Count());
        foreach (var entity in entities)
        {
            var dto = new GenerationActivityDto
            {
                Id = Guid.Parse(entity.RowKey),
                ChatTgId = entity.ChatTgId,
                StartedDateTime = entity.StartedDateTime,
                FinishedDateTime = entity.FinishedDateTime,
                ResultContainer = entity.ResultContainer,
                ResultPath = entity.ResultPath,
                GenerationRequestedDateTime = entity.GenerationRequestedDateTime,
                Prompt = entity.Prompt,
                Uuid = entity.Uuid
            };

            result.Add(dto);
        }

        return result;
    }
}

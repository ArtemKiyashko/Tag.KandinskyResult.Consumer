using Tag.KandinskyResult.Managers.Dtos;

namespace Tag.KandinskyResult.Managers;

public interface IGenerationActivityManager
{
    Task<IEnumerable<GenerationActivityDto>> GetActivitiesForToday();
    Task CompleteActivity(GenerationActivityDto activityDto); 
}

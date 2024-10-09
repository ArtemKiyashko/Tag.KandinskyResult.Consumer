using System.Runtime.CompilerServices;
using Tag.KandinskyResult.Repositories.Entities;

[assembly: InternalsVisibleTo("Tag.KandinskyResult.Managers")]

namespace Tag.KandinskyResult.Repositories;

internal interface IGenerationActivityRepository
{
    Task<IEnumerable<GenerationActivityEntity>> GetActivitiesForDate(DateTimeOffset date);
    Task UpdateActivity(GenerationActivityEntity entity);
    Task<GenerationActivityEntity> GetActivityForDate(DateTimeOffset date, string uuid);
}

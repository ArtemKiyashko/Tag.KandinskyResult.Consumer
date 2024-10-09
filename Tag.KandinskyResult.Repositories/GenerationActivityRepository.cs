using Azure.Data.Tables;
using Tag.KandinskyResult.Repositories.Entities;
using Tag.KandinskyResult.Repositories.Enums;

namespace Tag.KandinskyResult.Repositories;

internal class GenerationActivityRepository(TableClient tableClient) : IGenerationActivityRepository
{
    private readonly TableClient _tableClient = tableClient;

    public async Task<IEnumerable<GenerationActivityEntity>> GetActivitiesForDate(DateTimeOffset date)
    {
        var result = new List<GenerationActivityEntity>();
        var partitionKey = date.ToString("dd-MM-yyyy");
        var entitiesQuery = _tableClient.QueryAsync<GenerationActivityEntity>(
            e => e.PartitionKey == partitionKey && 
            e.GenerationStatus.Equals(GenerationStatuses.InProgress.ToString()));

        await foreach (var entity in entitiesQuery)
        {
            if (entity is not null)
                result.Add(entity);
        }
        return result;
    }

    public async Task<GenerationActivityEntity> GetActivityForDate(DateTimeOffset date, string id)
        => (await _tableClient.GetEntityAsync<GenerationActivityEntity>(date.ToPartitionKey(), id)).Value;

    public Task UpdateActivity(GenerationActivityEntity entity) => _tableClient.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Merge);
}

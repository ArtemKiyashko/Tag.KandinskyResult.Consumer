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
            
        return await BuildResultList(entitiesQuery);
    }

    private static async Task<IEnumerable<GenerationActivityEntity>> BuildResultList(Azure.AsyncPageable<GenerationActivityEntity> entitiesQuery)
    {
        var result = new List<GenerationActivityEntity>();
        await foreach (var entity in entitiesQuery)
        {
            if (entity is not null)
                result.Add(entity);
        }
        return result.OrderBy(r => r.ChatTgId).ThenBy(r => r.GenerationRequestedDateTime);
    }

    public async Task<IEnumerable<GenerationActivityEntity>> GetActivitiesForDateRange(DateTimeOffset date, TimeSpan range)
    {
        var result = new List<GenerationActivityEntity>();
        var partitionKeyEnd = date.ToString("dd-MM-yyyy");
        var partitionKeyStart = date.Subtract(range).ToString("dd-MM-yyyy");
        var entitiesQuery = _tableClient.QueryAsync<GenerationActivityEntity>(
            e => (e.PartitionKey == partitionKeyStart || e.PartitionKey == partitionKeyEnd) &&
            e.GenerationStatus.Equals(GenerationStatuses.InProgress.ToString()));

        return await BuildResultList(entitiesQuery);
    }

    public async Task<GenerationActivityEntity> GetActivityForDate(DateTimeOffset date, string id)
        => (await _tableClient.GetEntityAsync<GenerationActivityEntity>(date.ToPartitionKey(), id)).Value;

    public Task UpdateActivity(GenerationActivityEntity entity) => _tableClient.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Merge);
}

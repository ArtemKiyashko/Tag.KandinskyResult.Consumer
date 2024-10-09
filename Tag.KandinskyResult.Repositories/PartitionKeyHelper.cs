namespace Tag.KandinskyResult.Repositories;

public static class PartitionKeyHelper
{
    public static string ToPartitionKey(this DateTimeOffset date) => date.UtcDateTime.ToString("dd-MM-yyyy");
}

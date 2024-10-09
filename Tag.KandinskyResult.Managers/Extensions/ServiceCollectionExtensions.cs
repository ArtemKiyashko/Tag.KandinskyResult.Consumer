using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Tag.KandinskyResult.Repositories;

namespace Tag.KandinskyResult.Managers.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGenerationActivityManager(this IServiceCollection services, GenerationActivityOptions options)
    {
        services.AddAzureClients(clientBuilder => {
            clientBuilder.UseCredential(new ManagedIdentityCredential());
            if (options.TablesServiceUri is not null)
                clientBuilder.AddTableServiceClient(options.TablesServiceUri);
            else
            {
                if (string.IsNullOrEmpty(options.TablesConnectionString))
                    throw new ArgumentException($"{nameof(options.TablesServiceUri)} or {nameof(options.TablesConnectionString)} required");
                clientBuilder.AddTableServiceClient(options.TablesConnectionString);
            }
        });

        services.AddSingleton<IGenerationActivityManager, GenerationActivityManager>();
        services.AddSingleton<IGenerationActivityRepository, GenerationActivityRepository>(builder => {
            var tableServiceClient = builder.GetRequiredService<TableServiceClient>();
            var tableClient = tableServiceClient.GetTableClient(options.GenerationActivityTable);
            tableClient.CreateIfNotExists();
            return new GenerationActivityRepository(tableClient);
        });

        return services;
    }

    public static IServiceCollection AddKandinskyManager(this IServiceCollection services, KandinskyOptions options)
    {
        services.AddSingleton<IKandinskyManager, KandinskyManager>();
        services.AddHttpClient<IKandinskyRepository, KandinskyRepository>(client =>
        {
            client.BaseAddress = options.BaseAddress;
            client.DefaultRequestHeaders.Add("X-Key", options.XKey);
            client.DefaultRequestHeaders.Add("X-Secret", options.XSecret);
        });
        return services;
    }
}

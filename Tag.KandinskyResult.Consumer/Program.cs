using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Tag.KandinskyResult.Managers;
using Tag.KandinskyResult.Managers.Extensions;
using Telegram.Bot;
using Microsoft.Extensions.Logging;

IConfiguration _functionConfig;
GenerationActivityOptions _generationActivityOptions = new();
KandinskyOptions _kandinskyOptions = new();

_functionConfig = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        _functionConfig.GetSection(nameof(GenerationActivityOptions)).Bind(_generationActivityOptions);
        _functionConfig.GetSection(nameof(KandinskyOptions)).Bind(_kandinskyOptions);

        services.AddGenerationActivityManager(_generationActivityOptions);
        services.AddKandinskyManager(_kandinskyOptions);

        services.AddSingleton<ITelegramBotClient>(factory => {
            var botToken = _functionConfig.GetValue<string>("TELEGRAM_BOT_TOKEN") ?? throw new ArgumentException("Bot token required", "TELEGRAM_BOT_TOKEN");
            return new TelegramBotClient(botToken);
        });

        //ref: https://github.com/devops-circle/Azure-Functions-Logging-Tests/blob/master/Func.Isolated.Net7.With.AI/Program.cs#L46
        services.Configure<LoggerFilterOptions>(options =>
        {
            var toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

            if (toRemove is not null)
            {
                options.Rules.Remove(toRemove);
            }
        });
    })
    .Build();

host.Run();

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Tag.KandinskyResult.Consumer
{
    public class GenerationChecker
    {
        private readonly ILogger _logger;

        public GenerationChecker(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GenerationChecker>();
        }

        [Function("GenerationChecker")]
        public void Run([TimerTrigger("*/30 * * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}

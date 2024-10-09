using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Tag.KandinskyResult.Managers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Tag.KandinskyResult.Consumer
{
    public class GenerationChecker(
        ILoggerFactory loggerFactory,
        IKandinskyManager kandinskyManager,
        IGenerationActivityManager generationActivityManager,
        ITelegramBotClient telegramBotClient)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<GenerationChecker>();
        private readonly IKandinskyManager _kandinskyManager = kandinskyManager;
        private readonly IGenerationActivityManager _generationActivityManager = generationActivityManager;
        private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;

        [Function("GenerationChecker")]
        public async Task Run([TimerTrigger("*/30 * * * * *")] TimerInfo myTimer)
        {
            var activities = await _generationActivityManager.GetActivitiesForToday();
            foreach (var activity in activities)
            {
                try
                {
                    var imageBase64 = await _kandinskyManager.GetImageBase64(activity.Uuid);
                    if (imageBase64 is null)
                        return;

                    using var photoStream = new MemoryStream(Convert.FromBase64String(imageBase64));
                    await _telegramBotClient.SetChatPhotoAsync(chatId: activity.ChatTgId, InputFileStream.FromStream(photoStream));
                    await _generationActivityManager.CompleteActivity(activity);
                } 
                catch (InvalidOperationException ex) when (ex.Message == "The picture has been censored")
                {
                    _logger.LogInformation(
                        ex, "Picture censored. Prompt: [{prompt}]. ChatId: [{chatId}]. RequestId: [{requestId}]", activity.Prompt, activity.ChatTgId, activity.Id);
                    await _telegramBotClient.SendTextMessageAsync(
                        chatId: activity.ChatTgId, 
                        text: $"Это название [{activity.Prompt}] было отцензурено. Попробуйте использовать другое.");
                    await _generationActivityManager.CompleteActivity(activity);
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    await _generationActivityManager.CompleteActivity(activity);
                }
                catch (Exception ex){
                    _logger.LogError(ex, "Unhandled exception appeared. Suppressing to let others activities go. Check InnerException for details");
                }
            }
        }
    }
}

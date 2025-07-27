using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Tag.KandinskyResult.Managers;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
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
            var activities = await _generationActivityManager.GetRecentActivities();
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
                catch (InvalidOperationException ex) when (ex.Message == "The limit has been reached")
                {
                    _logger.LogInformation(
                        ex, "Limit reached for activity: {activityId}. ChatId: {chatId}", activity.Id, activity.ChatTgId);
                    await _telegramBotClient.SendTextMessageAsync(
                        chatId: activity.ChatTgId,
                        text: $"Достигнут лимит генерации изображений. Попробуйте позже.");
                    await _generationActivityManager.CompleteActivity(activity);
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    if (activity.ReadRetryCount >= 3)
                    {
                        _logger.LogInformation(
                            ex, "Image not found after 3 retries. Activity: {activityId}. ChatId: {chatId}", activity.Id, activity.ChatTgId);
                        await _telegramBotClient.SendTextMessageAsync(
                            chatId: activity.ChatTgId,
                            text: $"Изображение не удалось сгенерировать. Попробуйте повторить");
                        await _generationActivityManager.CompleteActivity(activity);
                    }
                    else
                    {
                        _logger.LogInformation(ex, "Image not found. Retrying for activity: {activityId}. ChatId: {chatId}", activity.Id, activity.ChatTgId);
                        await _generationActivityManager.SetReadRetryCountTo(activity, activity.ReadRetryCount++);
                    }
                }
                catch (ApiRequestException ex) when (ex.ErrorCode == 400 && ex.Message == "Bad Request: not enough rights to change chat photo")
                {
                    _logger.LogInformation(
                        ex, "Bot not granted required permissions to set chat (id: {chatId}) photo", activity.ChatTgId);
                    await _telegramBotClient.SendTextMessageAsync(
                        chatId: activity.ChatTgId,
                        text: $"Нет прав для установки аватарки чата. Дайте мне необходимые права и повторите /generate");
                    await _generationActivityManager.CompleteActivity(activity);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception appeared. Suppressing to let others activities go. Check InnerException for details");
                }
            }
        }
    }
}

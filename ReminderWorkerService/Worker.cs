using Microsoft.EntityFrameworkCore;
using ReminderWorkerService.Data;
using EmailSenderLibrary;
using System.Net.Mail;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Serilog;

namespace ReminderWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _config;

        public Worker(ILogger<Worker> logger, IServiceProvider provider, IConfiguration config)
        {
            _logger = logger;
            _provider = provider;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var telegramBotId = _config["telegramBotId"];
            ITelegramBotClient botClient = new TelegramBotClient(telegramBotId);
            ReceiverOptions receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                        UpdateType.Message
                    },
                ThrowPendingUpdates = true,
            };

            botClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, CancellationToken.None);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Сервис запущен");
                }

                var now = DateTime.Now;
                var checkDateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);

                using (IServiceScope scope = _provider.CreateScope())
                {
                    var scopedProvider = scope.ServiceProvider;

                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var toDoThings = await context.ToDoThings.Where(t => t.RemindTime == 0 && t.Date == checkDateTime || t.RemindTime != 0 && t.Date == checkDateTime.AddMinutes(t.RemindTime)).Include(t => t.AspNetUser).ToListAsync();
                    _logger.LogInformation("Получено {count} событий",toDoThings.Count);
                    if (toDoThings.Any())
                    {
                        var emailLogin = _config["emailSenderEmail"];
                        var password = _config["emailSenderPassword"];
                        var emailSender = new EmailSender();
                        foreach (var toDoThing in toDoThings)
                        {
                            emailSender.SendEmail(emailLogin, password, emailLogin, [toDoThing.AspNetUser.Email], toDoThing.Title, toDoThing.Description);
                            _logger.LogInformation("Отправлено email сообщение на почту {email} по событию с Id {id}", toDoThing.AspNetUser.Email, toDoThing.Id);
                            if (toDoThing.AspNetUser.TelegramChatId != null)
                            {
                                await botClient.SendTextMessageAsync(
                                toDoThing.AspNetUser.TelegramChatId,
                                toDoThing.Title + " - " + toDoThing.Description);
                                _logger.LogInformation("Отправлено сообщение в Telegram по ChatId {chatId} по событию с Id {id}", toDoThing.AspNetUser.TelegramChatId, toDoThing.Id);
                            }
                        }
                    }
                }


                await Task.Delay(60000, stoppingToken);
            }
        }

        private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message)
                {
                    switch (update.Message.Text)
                    {
                        case "/start":
                            {
                                await botClient.SendTextMessageAsync(
                                    update.Message.Chat.Id,
                                    "Здравствуйте! Пожалуйста, отправьте мне сообщение с Id вашего профиля в веб-приложении (его можно найти в профиле вашего аккаунта в веб-приложении ReminderApp), этот Id мне нужен, чтобы привязать ваш Telegram аккаунт к аккаунту в веб-приложении (пожалуйста, вводите ТОЛЬКО Id профиля)",
                                    replyToMessageId: update.Message.MessageId);

                                _logger.LogInformation("Запущено общение с ботом пользователем с ChatId {chatId}", update.Message.Chat.Id);

                                break;
                            }
                        default:
                            {
                                using (IServiceScope scope = _provider.CreateScope())
                                {
                                    var scopedProvider = scope.ServiceProvider;

                                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                    var user = await context.AspNetUsers.FirstOrDefaultAsync(u => u.Id == update.Message.Text);

                                    if (user != null)
                                    {
                                        if(user.TelegramChatId == null)
                                        {
                                            user.TelegramChatId = update.Message.Chat.Id;
                                            await context.SaveChangesAsync();

                                            await botClient.SendTextMessageAsync(
                                                update.Message.Chat.Id,
                                                "Спасибо! Теперь я буду присылать вам уведомления о ваших событиях! Больше мне писать ничего не нужно, я буду делать все сам :)",
                                                replyToMessageId: update.Message.MessageId);
                                        }
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(
                                            update.Message.Chat.Id,
                                            "Извините, такого Id профиля в веб-приложении не существует, проверьте правильность ввода Id (Внимание, пожалуйста, пишите в сообщении ТОЛЬКО Id и ничего больше)",
                                            replyToMessageId: update.Message.MessageId);
                                    }
                                }
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            _logger.LogError(error.Message);

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}

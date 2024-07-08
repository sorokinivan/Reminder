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
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                var now = DateTime.Now;
                var checkDateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);

                using (IServiceScope scope = _provider.CreateScope())
                {
                    var scopedProvider = scope.ServiceProvider;

                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var toDoThings = await context.ToDoThings.Where(t => t.RemindTime == 0 && t.Date == checkDateTime || t.RemindTime != 0 && t.Date == checkDateTime.AddMinutes(t.RemindTime)).Include(t => t.AspNetUser).ToListAsync();
                    _logger.LogInformation("�������� ���� {time}, ���������� {count} ���", DateTimeOffset.Now, toDoThings.Count);
                    if (toDoThings.Any())
                    {
                        var emailLogin = _config["emailSenderEmail"];
                        var password = _config["emailSenderPassword"];
                        var emailSender = new EmailSender();
                        foreach (var toDoThing in toDoThings)
                        {
                            emailSender.SendEmail(emailLogin, password, emailLogin, [toDoThing.AspNetUser.Email], toDoThing.Title, toDoThing.Description);
                            if (toDoThing.AspNetUser.TelegramChatId != null)
                            {
                                await botClient.SendTextMessageAsync(
                                toDoThing.AspNetUser.TelegramChatId,
                                toDoThing.Title + " - " + toDoThing.Description);
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
                                    "������ ����! ����������, �������� ���� Id ������������, ��������� � ������� � ���-����������, ����� � ��� ���������� ��� ����������� � ��������, ��������� � ���-���������� (��������, ��������� ������ ��������� ������ ID �������)",
                                    replyToMessageId: update.Message.MessageId);

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
                                        user.TelegramChatId = update.Message.Chat.Id;
                                        await context.SaveChangesAsync();

                                        await botClient.SendTextMessageAsync(
                                            update.Message.Chat.Id,
                                            "�������! � ������� ���������� � ID �������",
                                            replyToMessageId: update.Message.MessageId);
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(
                                            update.Message.Chat.Id,
                                            "��������, ID �� ������, ��������, �� �������� � ��� ���������, ���������� ��� ���",
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

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}

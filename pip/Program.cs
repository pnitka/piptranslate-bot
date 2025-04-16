using System;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Handlers;

class Program
{
    private static ITelegramBotClient? botClient;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Токен");
        var token = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine("я не полнял что это");
            return; 
        }

        botClient = new TelegramBotClient(token);

        try
        {
            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Информейшон о боте:");
            Console.WriteLine($"Название: {me.FirstName}");
            Console.WriteLine($"Username бОтА: @{me.Username}");

            Console.WriteLine($"ЗАПУСТИТЬ БОТА ({me.FirstName})? \n Да - 1 ");
            var userInput = Console.ReadLine();

            if (userInput == "1")
            {
                Console.WriteLine("я готов к прожарке");
            }
            else
            {
                Console.WriteLine("пишите нормально");
                return; 
            }

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            botClient.StartReceiving(
                updateHandler: BotMethods.HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: CancellationToken.None
            );

            await Task.Delay(-1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiException => $"Ошибка Telegram API:\n[{apiException.ErrorCode}]\n{apiException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}
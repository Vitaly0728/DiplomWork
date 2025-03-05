using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using DiplomWork.Model;
namespace DiplomWork
{
    internal class Program
    {
        private static TelegramBotClient _botClient;
        private static CancellationTokenSource _cts = new CancellationTokenSource();        
        static async Task Main(string[] args)
        {           
            var builder = WebApplication.CreateBuilder(args);
            
            if (builder.Environment.IsProduction())
            {
                builder.Configuration.AddUserSecrets<Program>();
            }

            TelegramBotConfig.Token = builder.Configuration["TelegramBot:Token"];       
            


            if (string.IsNullOrEmpty(TelegramBotConfig.Token))
            {
                Console.WriteLine("Ошибка: токен бота не установлен.");
                return;
            }
            _botClient = new TelegramBotClient(TelegramBotConfig.Token);

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
                DropPendingUpdates = true
            };

            var handler = new UpdateHandler();
            handler.OnHandleUpdateStarted += (message) =>
                Console.WriteLine($"Началась обработка сообщения '{message}'");
            handler.OnHandleUpdateCompleted += (message) =>
                Console.WriteLine($"Закончилась обработка сообщения '{message}'");           

            _botClient.StartReceiving(
             handler.HandleUpdateAsync,
             handler.HandleErrorAsync,
             receiverOptions,
             _cts.Token
         );

            var me = await _botClient.GetMe();
            Console.WriteLine($"{me.FirstName} запущен!");

            Console.WriteLine("Нажмите клавишу A для выхода");
            while (true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.A)
                {
                    _cts.Cancel();
                    break;
                }
                else
                {
                    var botInfo = await _botClient.GetMe();
                    Console.WriteLine($"Информация о боте: {botInfo.FirstName} ({botInfo.Username})");
                }
            }
        }
    }
}


using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using DiplomWork.Model;

namespace DiplomWork
{
    class UpdateHandler : IUpdateHandler
    {
        public event Action<string> OnHandleUpdateStarted;
        public event Action<string> OnHandleUpdateCompleted;
        private static readonly HttpClient client = new HttpClient();
        private SQLMachine sqlMachine;
        private UserSQL userSQL;
        private string currentCommand;
        private Location userLocation;
        private string photoFilePath;        
        private readonly ImgurUploader imgurUploader;        
        public UpdateHandler()
        {
            imgurUploader = new ImgurUploader();
            sqlMachine = new SQLMachine();            
            userSQL = new UserSQL();
        }
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message.Text != null)
            {
                var firstName = update.Message.From.FirstName;
                long telegramId = update.Message.From.Id;
                OnHandleUpdateStarted?.Invoke(update.Message.Text);
                bool isAdmin = await userSQL.IsUserAdmin(telegramId);

                switch (update.Message.Text)
                {
                    case "/start":
                        await userSQL.RegisterUser(botClient, update);
                        await ShowMenu(botClient, update);
                        break;
                    case "Добавить автомат":                        
                            currentCommand = "/addAutomat";
                            await RequestLocation(botClient, update);                        
                        break;

                    case "Найти кофе автомат":
                        currentCommand = "/searchAutomat";
                        await RequestLocation(botClient, update);
                        break;
                    case "Модерация":
                        if (isAdmin)
                        {
                            await HandleModeration(botClient, update);
                            await ShowMenu(botClient, update);
                        }
                        break;
                    case "Рейтинг":
                        var rating = await userSQL.GetTopUsersRating();
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, rating);
                        break;
                    case "Меню":
                        await ShowMenu(botClient, update);
                        break;
                    default:
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Неизвестная команда.");
                        break;
                }

                OnHandleUpdateCompleted?.Invoke(update.Message.Text);
            }
            else if (update.Type == UpdateType.Message && update.Message.Location != null)
            {
                if (currentCommand == "/addAutomat")
                {
                    userLocation = new Location(update.Message.Location.Latitude, update.Message.Location.Longitude);
                    await RequestPhoto(botClient, update);

                }
                else if (currentCommand == "/searchAutomat")
                {
                    userLocation = new Location(update.Message.Location.Latitude, update.Message.Location.Longitude);
                    await SearchAvtomat(botClient, update);
                    currentCommand = null;
                    await ShowMenu(botClient, update);
                }
            }
            else if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Photo)
            {
                if (currentCommand == "/addAutomat" && userLocation != null)
                {
                    await AddAvtomatWithPhoto(botClient, update);
                    currentCommand = null;
                    userLocation = null;
                }
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {                
                await HandleCallbackQueryAsync(botClient, update.CallbackQuery);                
            }
            else
            {
                Console.WriteLine($"Неизвестный тип обновления: {update.Type}");
            }
        }

        private async Task RequestLocation(ITelegramBotClient botClient, Update update)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
            {
                KeyboardButton.WithRequestLocation("Отправить свою геолокацию"),
                new KeyboardButton("Меню")
            })

            {
                ResizeKeyboard = true
            };

            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Пожалуйста, отправьте свою геолокацию.", replyMarkup: replyKeyboardMarkup);
        }
        private async Task RequestPhoto(ITelegramBotClient botClient, Update update)
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Пожалуйста, отправьте фотографию кофе автомата.");
        }

        [Obsolete]
        private async Task SearchAvtomat(ITelegramBotClient botClient, Update update)
        {
            if (userLocation != null)
            {
                var nearestMachine = await sqlMachine.FindNearestCofeMachine(userLocation);

                if (nearestMachine != null)
                {
                    string latitudeString = nearestMachine.Location.Latitude.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
                    string longitudeString = nearestMachine.Location.Longitude.ToString("G", System.Globalization.CultureInfo.InvariantCulture);

                    string yandexMapsLink = $"https://yandex.ru/maps/?ll={longitudeString},{latitudeString}&mode=whatshere&whatshere[point]={longitudeString},{latitudeString}&whatshere[zoom]=25";
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                        $"Ближайший кофе автомат: {nearestMachine.Name} по координатам ({longitudeString}, {latitudeString})\n" +
                        $"Ссылка на Яндекс.Карты: {yandexMapsLink}");
                    await botClient.SendPhotoAsync(update.Message.Chat.Id, nearestMachine.PhotoURL);
                }
                else
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, "К сожалению, не найдено ни одного кофе автомата.");
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Ошибка: геолокация не была получена.");
            }
        }

        [Obsolete]
        private async Task AddAvtomatWithPhoto(ITelegramBotClient botClient, Update update)
        {            
            var fileId = update.Message.Photo.Last().FileId;
            var file = await botClient.GetFileAsync(fileId);

            using (var httpClient = new HttpClient())
            {
                var token = TelegramBotConfig.Token;
                var fileUrl = $"https://api.telegram.org/file/bot{token}/{file.FilePath}";

                var response = await httpClient.GetAsync(fileUrl);
                response.EnsureSuccessStatusCode();

                using (var stream = new MemoryStream())
                {
                    await response.Content.CopyToAsync(stream);
                    byte[] photoData = stream.ToArray();

                    try
                    {
                        string imgurUrl = await imgurUploader.UploadImageAsync(photoData);
                        if (await sqlMachine.CheckIfMachineExists(userLocation.Latitude, userLocation.Longitude))
                        {
                            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Кофе автомат с такими координатами уже существует!");
                            return;
                        }
                        await sqlMachine.AddCoffeMachine(userLocation, "Кофе автомат", imgurUrl);
                        var userId = update.Message.From.Id;
                        await userSQL.UpdateUserRating(userId, 1);
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id,
            "Кофе автомат успешно добавлен! +1 баллов к вашему рейтингу!");
                        await ShowMenu(botClient, update);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при добавлении кофе автомата {ex.Message}");
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Произошла ошибка при добавлении кофе автомата.");
                    }
                }
            }
        }
        [Obsolete]
        private async Task ShowMenu(ITelegramBotClient botClient, Update update)
        {
            long telegramId = update.Message.From.Id;
            bool isAdmin = await userSQL.IsUserAdmin(telegramId);

            var keyboardRows = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton>
                {
                    new KeyboardButton("Найти кофе автомат"),
                    new KeyboardButton("Добавить автомат")
                },
                new List<KeyboardButton>
                {
                    new KeyboardButton("Рейтинг"),
                    new KeyboardButton("Меню")
                }
    };            
            
            if (isAdmin)
            {
                var adminRow = new List<KeyboardButton>
                {            
                 new KeyboardButton("Модерация")
                };

                keyboardRows.Add(adminRow);
            }

            var keyboard = new ReplyKeyboardMarkup(keyboardRows)
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false,  
                Selective = true
            };

            await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Выберите действие:",
                replyMarkup: keyboard);
        }

        [Obsolete]
        private async Task HandleModeration(ITelegramBotClient botClient, Update update)
        {
            var pendingMachines = await sqlMachine.GetPendingMachines();

            if (!pendingMachines.Any())
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Нет ожидающих кофе автоматов.");
                return;
            }

            foreach (var machine in pendingMachines)
            {
                var markup = new InlineKeyboardMarkup(new[]
                {
            InlineKeyboardButton.WithCallbackData("Одобрить", $"approve_{machine.Id}"),
            InlineKeyboardButton.WithCallbackData("Отказать", $"reject_{machine.Id}")
        });

                var sentMessage = await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    $"Кофе автомат: {machine.Name}\n" +
                    $"Адрес: {machine.Adres}\n" +
                    $"Фото: {machine.PhotoURL}",
                    replyMarkup: markup);

                machine.MessageId = sentMessage.MessageId;
                machine.ChatId = sentMessage.Chat.Id;
            }
        }

        [Obsolete]
        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var data = callbackQuery.Data.Split('_');

            if (data[0] == "approve")
            {
                int id = int.Parse(data[1]);
                await sqlMachine.ApproveMachine(id);

                var sentMessage = await botClient.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    "Кофе автомат одобрен.");
                
                await botClient.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);

                await Task.Delay(1000);
                await botClient.DeleteMessageAsync(sentMessage.Chat.Id, sentMessage.MessageId);
            }
            else if (data[0] == "reject")
            {
                int id = int.Parse(data[1]);
                await sqlMachine.RejectMachine(id);
                
                var sentMessage = await botClient.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    "Кофе автомат отклонен.");
               
                await botClient.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                
                await Task.Delay(1000);
                await botClient.DeleteMessageAsync(sentMessage.Chat.Id, sentMessage.MessageId);
            }

           
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
    }

}





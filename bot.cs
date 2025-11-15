using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text.Json;

string token = "7623714923:AAEuDlJmDKsDe71mU-JtlwsNxYeJGWE4HSc";
var bot = new TelegramBotClient(token);

Console.WriteLine("Бот запускается...");

bot.StartReceiving(
    updateHandler: UpdateHandler,
    pollingErrorHandler: HandlePollingErrorAsync
);

Console.WriteLine("Бот запущен! Нажми Enter чтобы выйти.");
Console.ReadLine();

// ФУНКЦИЯ ОБРАБОТКИ СООБЩЕНИЙ
async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    try
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                {
                    var message = update.Message;
                    var user = message.From;

                    Console.WriteLine($"{user.FirstName} ({user.Id}) написал сообщение: {message.Text}");

                    var chat = message.Chat;

                    // Клавиатура с кнопкой курса валют
                    var mainKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new[] {
                            new KeyboardButton("🇷🇺 RUB"),
                            new KeyboardButton("🇺🇦 UAH"),
                        },

                        new[] {
                            new KeyboardButton("🌍 Прочие"),
                           new KeyboardButton("💲 Crypto"),
                        },

                        new[] { new KeyboardButton("Калькутор") }
                    })
                    {
                        ResizeKeyboard = true
                    };

                    // ОБРАБОТКА КОМАНДЫ /start - ПОКАЗЫВАЕМ КЛАВИАТУРУ
                    if (message.Text?.ToLower() == "/start")
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chat.Id,
                            text: $"Привет, {user.FirstName}! Я бот курсов валют.",
                            replyMarkup: mainKeyboard
                        );
                        return;
                    }

                    // ОБРАБОТКА НАЖАТИЯ КНОПОК
                    string response;
                    IReplyMarkup? keyboard = mainKeyboard;

                    switch (message.Text)
                    {
                        case "🇷🇺 RUB":
                            response = await GetCurrencyRates();
                            break;
                        default:
                            response = "❌Команда не найдена❌";
                            break;
                    }

                    // ОТПРАВЛЯЕМ ОТВЕТ С КЛАВИАТУРОЙ
                    await botClient.SendTextMessageAsync(
                        chatId: chat.Id,
                        text: response,
                        replyMarkup: keyboard,
                        replyToMessageId: message.MessageId
                    );

                    return;
                }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}

// ФУНКЦИЯ ПОЛУЧЕНИЯ КУРСА ВАЛЮТ
async Task<string> GetCurrencyRates()
{
    try
    {
        using var httpClient = new HttpClient();

        // Получаем курс USDT/RUB
        var usdResponse = await httpClient.GetStringAsync("https://api.exchangerate-api.com/v4/latest/USDT");
        var usdData = JsonSerializer.Deserialize<CurrencyData>(usdResponse);
        var usdRate = usdData?.rates?.GetValueOrDefault("RUB") ?? 0;

        // Получаем курс EUR/RUB
        var eurResponse = await httpClient.GetStringAsync("https://api.exchangerate-api.com/v4/latest/EUR");
        var eurData = JsonSerializer.Deserialize<CurrencyData>(eurResponse);
        var eurRate = eurData?.rates?.GetValueOrDefault("RUB") ?? 0;

        // Получаем курс CNY/RUB
        var cnyResponse = await httpClient.GetStringAsync("https://api.exchangerate-api.com/v4/latest/CNY");
        var cnyData = JsonSerializer.Deserialize<CurrencyData>(cnyResponse);
        var cnyRate = cnyData?.rates?.GetValueOrDefault("RUB") ?? 0;

        var currentTime = DateTime.Now.ToString("D");

        return $" 🇷🇺 𝐑𝐔𝐁\n\n" +
               $"🇺🇸 𝗨𝗦𝗗𝗧 - {usdRate:F1}₽\n" +
               $"🇪🇺 𝗘𝗨𝗥 - {eurRate:F1}₽\n" +
               $"🇨🇳 𝗖𝗡𝗬 - {cnyRate:F1}₽\n\n" +
               $"Обновление курса произошло {currentTime}\n";
    }
    catch (Exception ex)
    {
        return $"❌ Не удалось получить курсы валют\nОшибка: {ex.Message}";
    }
}

// ФУНКЦИЯ ОБРАБОТКИ ОШИБОК - ДОЛЖНА БЫТЬ ЗДЕСЬ
Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    Console.WriteLine($"Ошибка: {exception.Message}");
    return Task.CompletedTask;
}

// Класс для десериализации JSON
public class CurrencyData
{
    public Dictionary<string, decimal> rates { get; set; }
}
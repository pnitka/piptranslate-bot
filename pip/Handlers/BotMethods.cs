using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Handlers
{
    public static class BotMethods
    {
        private static readonly Dictionary<string, (string English, string Spanish)> Dictionary = new(StringComparer.OrdinalIgnoreCase)
        {
        { "книга", ("book", "libro") },
        { "окно", ("window", "ventana") },
        { "стол", ("table", "mesa") },
        { "стул", ("chair", "silla") },
        { "дерево", ("tree", "árbol") },
        { "цвет", ("color", "color") },
        { "вода", ("water", "agua") },
        { "огонь", ("fire", "fuego") },
        { "земля", ("ground", "tierra") },
        { "воздух", ("air", "aire") },
        { "день", ("day", "día") },
        { "ночь", ("night", "noche") },
        { "солнце", ("sun", "sol") },
        { "луна", ("moon", "luna") },
        { "звезда", ("star", "estrella") },
        { "машина", ("car", "coche") },
        { "школа", ("school", "escuela") },
        { "работа", ("work", "trabajo") },
        { "деньги", ("money", "dinero") },
        { "время", ("time", "tiempo") }
        };

        private static readonly HttpClient httpClient = new HttpClient();

        private static readonly Dictionary<long, (string Word, string CorrectAnswer)> QuizState = new();

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { Text: { } messageText } || update.Message.From is null)
                return;

            var chatId = update.Message.Chat.Id;
            var username = update.Message.From.Username ?? "тумбучка";

            try
            {
                if (messageText == "/start")
                {
                    await SendMessage(botClient, chatId, $"Привет, @{username}! Этот бот переводит текст с русского на английский и испанский, а также предлагает викторину.");
                    return;
                }

                if (messageText == "/author")
                {
                    await SendMessage(botClient, chatId, "Автор шикарного бота: 🔥@pnitka🔥 лучшая програмистка на свете от Бога");
                    return;
                }

                if (messageText == "/quiz")
                {
                    var randomWord = Dictionary.Keys.ElementAt(new Random().Next(Dictionary.Count));
                    var quizTranslation = Dictionary[randomWord];

                    var isEnglish = new Random().Next(2) == 0;
                    var correctAnswer = isEnglish ? quizTranslation.English : quizTranslation.Spanish;

                    QuizState[chatId] = (randomWord, correctAnswer);

                    await SendMessage(botClient, chatId, $"Угадай перевод задуманное шикарной дамой:\nКак переводится слово \"{randomWord}\" на {(isEnglish ? "английский" : "испанский")}?");
                    return;
                }

                if (QuizState.TryGetValue(chatId, out var quizData))
                {
                    var (word, correctAnswer) = quizData;

                    if (messageText.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase))
                    {
                        await SendMessage(botClient, chatId, "🔥ПРАВИЛЬНО!🔥 Ты просто огурец!🥒");
                    }
                    else
                    {
                        await SendMessage(botClient, chatId, $"❌ НЕПРАВИЛЬНО ❌ Правильный ответ: {correctAnswer}");
                    }
                    
                    QuizState.Remove(chatId);
                    return;
                }

                if (Dictionary.TryGetValue(messageText.Trim(), out var translation))
                {
                    await SendMessage(botClient, chatId, $"Перевод с:\n Английского: {translation.English}\n Испанского: {translation.Spanish}");
                }
                else
                {
                    var russianTranslation = await TranslateTextAsync(messageText, "auto", "ru");
                    await SendMessage(botClient, chatId, $"Перевод на русский с англ или испанского: {russianTranslation}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                await SendMessage(botClient, chatId, "ОШИБКА");
            }
        }

        static async Task SendMessage(ITelegramBotClient botClient, long chatId, string text)
        {
            await botClient.SendTextMessageAsync(chatId, text, cancellationToken: CancellationToken.None);
        }

        static async Task<string> TranslateTextAsync(string text, string sourceLang, string targetLang)
        {
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLang}&tl={targetLang}&dt=t&q={Uri.EscapeDataString(text)}";
            var response = await httpClient.GetStringAsync(url);

            var result = JsonSerializer.Deserialize<List<object>>(response);
            return result?[0]?.ToString()?.Split('"')[1] ?? "Ошибка: Нет перевода";
        }
    }
}
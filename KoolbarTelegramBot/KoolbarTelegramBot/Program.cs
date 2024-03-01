using KoolbarTelegramBot;
using Telegram.Bot;
const string token = "6987060819:AAF4G3gaSyRDWf_NuT21tCJBU3ukmQ9rxxw";
var botClient = new TelegramBotClient(token);
var metBot = new BotEngine(botClient);
await metBot.ListenForMessagesAsync();

Console.ReadKey();

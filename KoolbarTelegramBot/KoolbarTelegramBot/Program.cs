using KoolbarTelegramBot;
using Telegram.Bot;
var botClient = new TelegramBotClient("6987060819:AAF4G3gaSyRDWf_NuT21tCJBU3ukmQ9rxxw");
var metBot = new BotEngine(botClient);
await metBot.ListenForMessagesAsync();
Console.ReadKey();

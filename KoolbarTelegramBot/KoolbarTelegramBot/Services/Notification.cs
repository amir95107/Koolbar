using Telegram.Bot;

namespace KoolbarTelegramBot.Services
{
    public static class Notification
    {
        private static readonly TelegramBotClient _botClient;

        static Notification()
        {
            _botClient = new TelegramBotClient("6987060819:AAF4G3gaSyRDWf_NuT21tCJBU3ukmQ9rxxw");
        }

        public static async Task SendMessage(string message)
        {
            await _botClient.SendTextMessageAsync(-1001974756992, message,parseMode:Telegram.Bot.Types.Enums.ParseMode.Html,disableWebPagePreview:true);
        }

    }
}

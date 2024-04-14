using KoolbarTelegramBot;
using Telegram.Bot;
const string token = "7001628889:AAGkzn7E2Uw2vs9aFo3GCG2V-kuheFdz4BU";
//const string token = "6987060819:AAF4G3gaSyRDWf_NuT21tCJBU3ukmQ9rxxw";
//var _secret = "1603010200010001fc030386e24c3add";
//var proxy = new WebProxy("142.132.201.180", 443, Credentials: );
//ICredentials credentials = new NetworkCredential(cred);
//var httpclienthanlder = new HttpClientHandler
//{
//    Proxy = proxy,
//    UseCookies = true,
//    Credentials = ICredentials.
//};
//var httpclient = new HttpClient();
//httpclient.DefaultRequestHeaders.Add("Secret", _secret);
var botClient = new TelegramBotClient(token);
var metBot = new BotEngine(botClient);
await metBot.ListenForMessagesAsync();

Console.ReadKey();

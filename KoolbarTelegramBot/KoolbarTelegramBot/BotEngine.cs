using Datalayer.Enumerations;
using Datalayer.Migrations;
using Koolbar.Dtos;
using KoolbarTelegramBot.HttpClientProvider;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KoolbarTelegramBot
{
    public class BotEngine
    {
        private readonly TelegramBotClient _botClient;
        private static RequestStatus _requestStatus = RequestStatus.New;
        private static string? _requestType;
        private static string? Username;
        private static string? _description;
        private static string? _source;
        private static string? _destination;
        private static DateTime? _flightDate;

        public BotEngine(TelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task ListenForMessagesAsync()
        {
            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };
            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await _botClient.GetMeAsync();
            //await _botClient.DeleteWebhookAsync();
            Console.WriteLine("Telegram bot is start running...");
        }

        private readonly string[] CurrentCommandList = ["/start", "/create", "/update", "/all"];


        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates


            if (update.CallbackQuery is not null)
            {

                if (update.Type == UpdateType.CallbackQuery)
                {
                    var queryType = update.CallbackQuery.Data.Split('-')[0];
                    switch (queryType)
                    {
                        case "type":
                            await HandleAddTypeCallbackAsync(update.CallbackQuery.From.Id, update.CallbackQuery);
                            break;

                        case "description":

                            break;
                    }

                }
                else if (update.Id is 0)
                {

                }
            }


            if (update.Message is not { } message)
            {
                return;
            }

            Username = update.Message.Chat.Username;

            var text = message.Text;

            if (text == null)
            {
                return;
            }

            switch (_requestStatus)
            {
                case RequestStatus.TypeDeclared:
                    if (message.Text.ToLower().StartsWith("s:"))
                    {
                        await HandleAddSourceCallbackAsync(update.Message.Chat.Id, text);
                    }
                    else
                    {
                        await HandleSearchSourceCallbackAsync(update.Message.Chat.Id, text);
                    }
                    break;

                case RequestStatus.SourceDeclared:
                    if (message.Text.ToLower().StartsWith("s:"))
                    {
                        await HandleAddDestionationCallbackAsync(update.Message.Chat.Id, text);
                    }
                    else
                    {
                        await HandleSearchDestionationCallbackAsync(update.Message.Chat.Id, text);
                    }
                    break;

                case RequestStatus.DestinationDeclared:
                    await HandleAddDescriptionCallbackAsync(update.Message.Chat.Id, text);
                    break;

                case RequestStatus.DescriptionDeclared:
                    await HandleAddFlightDateCallbackAsync(update.Message.Chat.Id, text);
                    break;
            }



            if (text.StartsWith("/"))
            {
                if (string.IsNullOrWhiteSpace(Username))
                {
                    await _botClient.SendTextMessageAsync(update.Message.Chat.Id, "لطفا برای ادامه فرآیند برای حساب کاربری با مراجعه به تنظیمات تلگرام یک نام کاربری انتخاب کنید.");
                    return;
                }

                await HandleCommands(text.Split(' ')[0], message.Chat.Id);
            }

            // Only process text messages
            //await _botClient.SendTextMessageAsync(message.Chat.Id, $"Thanks for your message {message.Chat.Username}");
        }


        int messageId = 0;
        private async Task HandleAddTypeCallbackAsync(long chatid, CallbackQuery callback)
        {
            try
            {
                var data = callback.Data.Split('-')[1];
                var a = await ApiCall.PostAsync("requests/type", new RequestDto
                {
                    ChatId = chatid,
                    RequestType = Enum.Parse<RequestType>(data),
                });
                var message = await _botClient.SendTextMessageAsync(chatid, "لطفا مبدا خود را جستجو کنید:");
                messageId = message.MessageId;
                _requestStatus = RequestStatus.TypeDeclared;
                _requestType = Enum.Parse<RequestType>(data) == RequestType.FreightOwner ? "بار دار" : "مسافر";

            }
            catch (Exception ex)
            {

                await _botClient.SendTextMessageAsync(chatid, ex.Message);
                return;
            }
        }

        private async Task HandleSearchSourceCallbackAsync(long id, string source)
        {
            var cities = await ApiCall.GetAsync<List<CityDto>>($"states/search/{source}");

            ReplyKeyboardMarkup x = GenerateKeyboardButtonForCities(cities,"s");

            await _botClient.SendTextMessageAsync(id, "لطفا از بین شهرهای زیر شهر مبدا خود را انتخاب کنید:", replyMarkup: x, replyToMessageId:messageId);
        }

        private async Task HandleSearchDestionationCallbackAsync(long id, string source)
        {
            var cities = await ApiCall.GetAsync<List<CityDto>>($"states/search/{source}");

            ReplyKeyboardMarkup x = GenerateKeyboardButtonForCities(cities, "d");

            await _botClient.SendTextMessageAsync(id, "لطفا از بین شهرهای زیر شهر مقصد خود را انتخاب کنید:", replyMarkup: x, replyToMessageId: messageId);
        }


        private static ReplyKeyboardMarkup GenerateKeyboardButtonForCities(List<CityDto> cities, string sord)
        {
            var buttons = new List<List<KeyboardButton>>();
            var buttonsfour = new List<KeyboardButton>();
            for (int i = 0; i < cities.Count; i++)
            {
                var city = cities[i];
                buttonsfour.Add(new KeyboardButton($"{sord}:{city.Title}"));
                if (cities.Count >= 4 && i % 4 == 3 || cities.Count < 4 && i == cities.Count - 1)
                {
                    buttons.Add(buttonsfour);
                    buttonsfour = new List<KeyboardButton>();
                }
            }

            ReplyKeyboardMarkup x = new ReplyKeyboardMarkup(buttons);
            return x;
        }

        private async Task HandleAddSourceCallbackAsync(long id, string source)
        {
            await ApiCall.PostAsync("requests/source", new RequestDto
            {
                ChatId = id,
                Source = source.Split("s:")[1],
            });

            _source = source;

            _requestStatus = RequestStatus.SourceDeclared;



            await _botClient.SendTextMessageAsync(id, "لطفا مقصد خود را وارد کنید:");
        }

        private async Task HandleAddDestionationCallbackAsync(long id, string destination)
        {
            await ApiCall.PostAsync("requests/destination", new RequestDto
            {
                ChatId = id,
                Destination = destination.Split("d:")[1],
            });

            _destination = destination;

            _requestStatus = RequestStatus.DestinationDeclared;

            await _botClient.SendTextMessageAsync(id, "لطفا توضیحات خود را وارد کنید:");
        }

        private async Task HandleAddDescriptionCallbackAsync(long id, string message)
        {
            await ApiCall.PostAsync("requests/description", new RequestDto
            {
                ChatId = id,
                Description = message,
            });

            _description = message;

            _requestStatus = RequestStatus.DescriptionDeclared;

            var text = (Username != null ? $"<a href='http://t.me/{Username}'>@{Username}</a>" : "کاربر") + "\n"
                + $"#{_requestType}" + "\n"
                + $"مبدا: {_source} " + "\n"
                + $"مقصد: {_destination}" + "\n"
                + $"<b>{_description}</b>";

            if (_requestType == "مسافر")
            {
                text = $"لطفا تاریخ پرواز را مشخص کنید." + "\n"
                    + $"فرمت تاریخ: yyyy/mm/dd"
                    + $"مثال: 2023/12/30";

                await _botClient.SendTextMessageAsync(id, text, parseMode: ParseMode.Html);
                return;
            }

            await GenerateSuggestedText(id, 20);


            InlineKeyboardButton urlButton = new InlineKeyboardButton("پیام به کاربر");
            urlButton.Url = $"https://t.me/{Username}";
            InlineKeyboardMarkup markup = new InlineKeyboardMarkup(urlButton);
            await _botClient.SendTextMessageAsync(-1001974756992, text, replyMarkup: markup, parseMode: ParseMode.Html);
        }



        private async Task HandleAddFlightDateCallbackAsync(long id, string date)
        {
            var convertedDate = DateTime.Now;
            try
            {
                convertedDate = DateTime.Parse(date);
            }
            catch (Exception ex)
            {
                await _botClient.SendTextMessageAsync(id, "فرمت تاریخ وارد شده صحیح نیست:");
            }

            await ApiCall.PostAsync("requests/flightdate", new RequestDto
            {
                ChatId = id,
                FlightDate = convertedDate,
            });

            _flightDate = convertedDate;

            _requestStatus = RequestStatus.FlightDateDeclared;

            var text = (Username != null ? $"<a href='http://t.me/{Username}'>{Username}</a>" : "کاربر") + "\n"
                + $"#{_requestType}" + "\n \n"
                + $"مبدا: {_source} " + "\n"
                + $"مقصد: {_destination}" + "\n"
                + $"تاریخ سفر: {convertedDate.ToString("yyyy/MM/dd")}" + "\n"
                + $"#{_description}";


            InlineKeyboardButton urlButton = new InlineKeyboardButton("پیام به کاربر");
            urlButton.Url = $"https://t.me/{Username}";
            InlineKeyboardMarkup markup = new InlineKeyboardMarkup(urlButton);
            await _botClient.SendTextMessageAsync(-1001974756992, text, replyMarkup: markup, parseMode: ParseMode.Html);
            await GenerateSuggestedText(id, 10);
        }

        private async Task<string> GenerateSuggestedText(long id, int requestType)
        {
            var list = await ApiCall.GetAsync<List<RequestDto>>($"requests/suggest/{id}");

            var text = list.Count == 0 ? "موردی جهت نمایش وجود ندارد!" : string.Empty;
            int num = 0;
            foreach (var item in list)
            {
                var t = $"مبدا: {item.Source} \n مقصد: {item.Destination} \n توضیحات: {item.Description} \n تاریخ پرواز: {item.FlightDate} \n  <a href='https://t.me/{item.Username}'><b>@{item.Username}</b>></a>\n";
                t += num == list.Count - 1 ? "------------------------------------ \n\n" : "";
                num++;
            }

            await _botClient.SendTextMessageAsync(id, text, parseMode: ParseMode.Html);
            return "";
        }


        private async Task HandleCommands(string command, long chatid)
        {
            if (!CurrentCommandList.Contains(command))
                await _botClient.SendTextMessageAsync(chatid, $"The command is invalid!");

            await HandleCommands(command, chatid, "");
        }

        private async Task HandleCommands(string command, long id, string _type = "")
        {
            switch (command)
            {
                case "/create":
                    await HandleCreateCommands(id, "");
                    break;

                case "/update":
                    await HandleUpdateCommands(id, "");
                    break;

                case "/start":
                    await HandleStartCommands(id, "");
                    break;
            }
        }

        private async Task HandleStartCommands(long id, string v)
        {
            await _botClient.SendTextMessageAsync(id, $"It is pleasure to use our bot ...");
        }

        private async Task HandleUpdateCommands(long id, string _type = "")
        {
            await _botClient.SendTextMessageAsync(id, "The update command is being executed");
        }

        private async Task HandleCreateCommands(long id, string _type = "")
        {
            var a = await _botClient.GetChatMemberAsync(-1001974756992, id);

            if (a.Status == ChatMemberStatus.Left)
            {

            }

            try
            {
                await ApiCall.PostAsync("requests", new RequestDto
                {
                    ChatId = id
                });
            }
            catch (Exception ex)
            {

                await _botClient.SendTextMessageAsync(id, ex.Message);
                return;
            }

            InlineKeyboardButton urlButton = new InlineKeyboardButton("مسافر هستم");
            InlineKeyboardButton urlButton2 = new InlineKeyboardButton("بار دارم");

            urlButton.CallbackData = "type-" + RequestType.Passenger.ToString();
            urlButton2.CallbackData = "type-" + RequestType.FreightOwner.ToString();

            var buttons = new InlineKeyboardButton[] { urlButton, urlButton2 };

            // Keyboard markup
            InlineKeyboardMarkup inline = new InlineKeyboardMarkup(buttons);

            // Send message!
            try
            {
                await _botClient.SendTextMessageAsync(id, "نوع درخواست خود را مشخص کنید.", replyMarkup: inline);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

    }
}

using Datalayer.Enumerations;
using Koolbar.Dtos;
using KoolbarTelegramBot.HttpClientProvider;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace KoolbarTelegramBot
{
    public class BotEngine
    {
        private readonly TelegramBotClient _botClient;
        private static Dictionary<string, RequestDto> Requests = new Dictionary<string, RequestDto>();

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
            if (update.ChannelPost != null || update.MyChatMember != null)
                return;
            // Only process Message updates
            #region Check if user has username
            var userChannelsStatus = await _botClient.GetChatMemberAsync(-1001974756992, update.Message != null ? update.Message.Chat.Id : update.CallbackQuery.From.Id);

            if (string.IsNullOrWhiteSpace(userChannelsStatus.User.Username))
            {
                var txt = "لطفا برای استفاده از خدمات بات، با مراجعه به تنظیمات تلگرام، Username خود را وارد کنید." + "\n\n";
                await _botClient.SendTextMessageAsync(update.Message.Chat.Id, txt);

                return;
            }
            var Username = update.Message != null ? update.Message.Chat.Username : update.CallbackQuery.From.Username;
            #endregion

            if (update.CallbackQuery is not null)
            {

                if (update.Type == UpdateType.CallbackQuery)
                {
                    var queryType = update.CallbackQuery.Data.Split('-')[0];
                    switch (queryType)
                    {
                        case "type":
                            await HandleAddTypeCallbackAsync(update.CallbackQuery.From.Id, update.CallbackQuery, Username);
                            break;

                        case "description":

                            break;
                    }
                    if (queryType.StartsWith("datepicker_month_"))
                    {
                        var month = queryType.Split("_")[2];
                        Requests[Username].FlightMonth = month;
                        var days = GetDayPickerInlineKeyboard();
                        await _botClient.EditMessageReplyMarkupAsync(new ChatId(update.CallbackQuery.Message.Chat.Id), Requests[Username].MessageId.Value, replyMarkup: days);
                    }
                    if (queryType.StartsWith("datepicker_day_"))
                    {
                        var day = queryType.Split("_")[2];
                        Requests[Username].FlightDay = int.Parse(day);

                        await HandleAddFlightDateCallbackAsync(update.CallbackQuery.Message.Chat.Id, Requests[Username].FlightMonth, int.Parse(day), Username);
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



            var text = message.Text;

            if (text == null)
            {
                return;
            }

            if (Requests.Count == 0 || !Requests.Keys.Any(x => x == Username))
            {
                var request = await ApiCall.GetAsync<RequestDto>($"requests/{update.Message.Chat.Id}");
                if (request == null || (request.RequestType == RequestType.Passenger && request.RequestStatus < RequestStatus.FlightDateDeclared) || 
                    request.RequestType == RequestType.FreightOwner && request.RequestStatus < RequestStatus.DescriptionDeclared)
                {
                    Requests[Username] = request;
                }
            }

            if (Requests[Username] != null)
            {
                switch (Requests[Username].RequestStatus)
                {
                    case RequestStatus.TypeDeclared:
                        if (message.Text.ToLower().StartsWith("s:"))
                        {
                            await HandleAddSourceCallbackAsync(update.Message.Chat.Id, text, Username);
                        }
                        else
                        {
                            await HandleSearchSourceCallbackAsync(update.Message.Chat.Id, text);
                        }
                        return;

                    case RequestStatus.SourceDeclared:
                        if (message.Text.ToLower().StartsWith("d:"))
                        {
                            await HandleAddDestionationCallbackAsync(update.Message.Chat.Id, text, Username);
                        }
                        else
                        {
                            await HandleSearchDestionationCallbackAsync(update.Message.Chat.Id, text, Username);
                        }
                        return;

                    case RequestStatus.DestinationDeclared:
                        await HandleAddDescriptionCallbackAsync(update.Message.Chat.Id, text, Username);
                        return;

                    case RequestStatus.DescriptionDeclared:
                        await HandleContinueAddDateAsync(update.Message.Chat.Id, Requests[Username].RequestType.Value, Username);
                        return;
                }
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
            #region Check if exists in channel(s)
            var userChannelsStatus = await _botClient.GetChatMemberAsync(-1001974756992, id);
            if (userChannelsStatus.Status is not ChatMemberStatus.Member and not ChatMemberStatus.Creator and not ChatMemberStatus.Administrator)
            {
                var text = "لطفا برای استفاده از خدمات بات، کانال(ها‌ی) زیر را دنبال کنید." + "\n\n" +
                            "<a href='https://t.me/koolbar_international'>@koolbar_international</a>";
                await _botClient.SendTextMessageAsync(id, text, parseMode: ParseMode.Html, disableWebPagePreview: true);

                return;
            }
            #endregion

            #region Check if user has username
            if (string.IsNullOrWhiteSpace(userChannelsStatus.User.Username))
            {
                var text = "لطفا برای استفاده از خدمات بات، با مراجعه به تنظیمات تلگرام، Username خود را وارد کنید." + "\n\n";
                await _botClient.SendTextMessageAsync(id, text);

                return;
            }
            #endregion

            try
            {
                var request = await ApiCall.PostAsync("requests", new RequestDto
                {
                    ChatId = id,
                    Username = userChannelsStatus.User.Username
                });

                Requests[userChannelsStatus.User.Username] = request;

                switch (request.RequestStatus)
                {
                    case RequestStatus.TypeDeclared:
                        await _botClient.SendTextMessageAsync(id, "لطفا مبدا خود را جستجو کنید:");
                        return;

                    case RequestStatus.SourceDeclared:
                        await _botClient.SendTextMessageAsync(id, "لطفا مقصد خود را جستجو کنید:");
                        return;

                    case RequestStatus.DestinationDeclared:
                        await _botClient.SendTextMessageAsync(id, "لطفا توضیحات خود را وارد کنید:");
                        return;

                    case RequestStatus.DescriptionDeclared:
                        await HandleContinueAddDateAsync(id, request.RequestType.Value, userChannelsStatus.User.Username);
                        return;
                }

            }
            catch (Exception ex)
            {
                await _botClient.SendTextMessageAsync(id, "خطا در ثبت اولیه درخواست.");
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
            await _botClient.SendTextMessageAsync(id, "نوع درخواست خود را مشخص کنید.", replyMarkup: inline);
        }



        private async Task HandleAddTypeCallbackAsync(long chatid, CallbackQuery callback, string username)
        {
            try
            {
                var data = callback.Data.Split('-')[1];
                await ApiCall.PostAsync("requests/type", new RequestDto
                {
                    ChatId = chatid,
                    RequestType = Enum.Parse<RequestType>(data),
                });
                var message = await _botClient.SendTextMessageAsync(chatid, "لطفا مبدا خود را جستجو کنید:");

                if (Requests.Count == 0 || !Requests.Keys.Any(x => x == username))
                {
                    var request = await ApiCall.GetAsync<RequestDto>($"requests/{chatid}");
                    Requests[username] = request;
                }

                Requests[username].RequestStatus = RequestStatus.TypeDeclared;
                Requests[username].RequestType = Enum.Parse<RequestType>(data);

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

            ReplyKeyboardMarkup x = GenerateKeyboardButtonForCities(cities, "s");

            var text = cities.Count > 0 ? "لطفا از بین شهرهای زیر شهر مبدا خود را انتخاب کنید:" : "موردی یافت نشد. لطفا مجددا تلاش کنید.";

            await _botClient.SendTextMessageAsync(id, text, replyMarkup: x);
        }

        private async Task HandleSearchDestionationCallbackAsync(long id, string source, string username)
        {
            var cities = await ApiCall.GetAsync<List<CityDto>>($"states/search/{source}");

            ReplyKeyboardMarkup x = GenerateKeyboardButtonForCities(cities, "d");

            var text = cities.Count > 0 ? "لطفا از بین شهرهای زیر شهر مقصد خود را انتخاب کنید:" : "موردی یافت نشد. لطفا مجددا تلاش کنید.";

            await _botClient.SendTextMessageAsync(id, text, replyMarkup: x);
        }

        private static ReplyKeyboardMarkup GenerateKeyboardButtonForCities(List<CityDto> cities, string sord)
        {
            int culomn = 3;
            var buttons = new List<List<KeyboardButton>>();
            var buttonsfour = new List<KeyboardButton>();
            for (int i = 0; i < cities.Count; i++)
            {
                var city = cities[i];
                buttonsfour.Add(new KeyboardButton($"{sord}:{city.Title}"));
                if (cities.Count >= culomn && i % culomn == culomn - 1 || cities.Count < culomn && i == cities.Count - 1)
                {
                    buttons.Add(buttonsfour);
                    buttonsfour = new List<KeyboardButton>();
                }
            }

            ReplyKeyboardMarkup x = new ReplyKeyboardMarkup(buttons);
            return x;
        }

        private async Task HandleAddSourceCallbackAsync(long id, string source, string username)
        {
            await ApiCall.PostAsync("requests/source", new RequestDto
            {
                ChatId = id,
                Source = source.Split("s:")[1],
            });

            Requests[username].Source = source.Split("s:")[1];
            Requests[username].RequestStatus = RequestStatus.SourceDeclared;

            await _botClient.SendTextMessageAsync(id, "لطفا مقصد خود را جستجو کنید:");
        }

        private async Task HandleAddDestionationCallbackAsync(long id, string destination, string username)
        {
            await ApiCall.PostAsync("requests/destination", new RequestDto
            {
                ChatId = id,
                Destination = destination.Split("d:")[1],
            });

            Requests[username].Destination = destination.Split("d:")[1];
            Requests[username].RequestStatus = RequestStatus.DestinationDeclared;

            await _botClient.SendTextMessageAsync(id, "لطفا توضیحات خود را وارد کنید:");
        }

        private async Task HandleContinueAddDateAsync(long id, RequestType requestType, string username)
        {
            var text = string.Empty;
            if (requestType == RequestType.Passenger)
            {
                text = $"لطفا تاریخ پرواز را مشخص کنید.";

                var months = GetMonthPickerInlineKeyboard();

                var msg = await _botClient.SendTextMessageAsync(id, text, replyMarkup: months, disableWebPagePreview: true);
                Requests[username].MessageId = msg.MessageId;
            }
        }

        private async Task HandleAddDescriptionCallbackAsync(long id, string message, string username)
        {
            await ApiCall.PostAsync("requests/description", new RequestDto
            {
                ChatId = id,
                Description = message,
            });

            Requests[username].Description = message;
            Requests[username].RequestStatus = RequestStatus.DescriptionDeclared;

            var reqTypeText = Requests[username].RequestType == RequestType.Passenger ? "مسافر" : "دارای بار";

            var text = $"<a href='http://t.me/{username}'>@{username}</a> \n"
                + $"#{reqTypeText}" + "\n"
                + $"مبدا: {Requests[username].Source} " + "\n"
                + $"مقصد: {Requests[username].Destination}" + "\n"
                + $"<b>{Requests[username].Description}</b>";

            if (reqTypeText == "مسافر")
            {
                text = $"لطفا تاریخ پرواز را مشخص کنید.";

                var months = GetMonthPickerInlineKeyboard();

                var msg = await _botClient.SendTextMessageAsync(id, text, replyMarkup: months, disableWebPagePreview: true);
                Requests[username].MessageId = msg.MessageId;
                return;
            }
            Requests.Remove(username);
            await GenerateSuggestedText(id, 20);


            InlineKeyboardButton urlButton = new InlineKeyboardButton("پیام به کاربر");
            urlButton.Url = $"https://t.me/{username}";
            InlineKeyboardMarkup markup = new InlineKeyboardMarkup(urlButton);
            await _botClient.SendTextMessageAsync(-1001974756992, text, replyMarkup: markup, parseMode: ParseMode.Html, disableWebPagePreview: true);
        }

        private async Task HandleAddFlightDateCallbackAsync(long id, string month, int day, string username)
        {
            var months = (new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" }).ToArray();

            var date = new DateTime(DateTime.UtcNow.Year, Array.IndexOf(months, month), day);

            await ApiCall.PostAsync("requests/flightdate", new RequestDto
            {
                ChatId = id,
                FlightDate = date,
            });

            Requests[username].FlightDate = date;
            Requests[username].RequestStatus = RequestStatus.DestinationDeclared;

            var text = $"<a href='http://t.me/{username}'>@{username}</a> \n"
                + $"#{Requests[username].RequestType}" + "\n \n"
                + $"مبدا: {Requests[username].Source} " + "\n"
                + $"مقصد: {Requests[username].Destination}" + "\n"
                + $"تاریخ سفر: {date.ToString("yyyy/MM/dd")}" + "\n"
                + $"{Requests[username].Description}";


            InlineKeyboardButton urlButton = new InlineKeyboardButton("پیام به کاربر");
            urlButton.Url = $"https://t.me/{username}";
            InlineKeyboardMarkup markup = new InlineKeyboardMarkup(urlButton);
            await _botClient.SendTextMessageAsync(-1001974756992, text, replyMarkup: markup, parseMode: ParseMode.Html, disableWebPagePreview: true);
            await GenerateSuggestedText(id, 10);
            Requests.Remove(username);
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

            await _botClient.SendTextMessageAsync(id, text, parseMode: ParseMode.Html, disableWebPagePreview: true);
            return "";
        }

        private static InlineKeyboardMarkup GetMonthPickerInlineKeyboard()
        {
            var months = new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            var buttons = new List<List<InlineKeyboardButton>>();
            foreach (var month in months)
            {
                var row = new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(month, $"datepicker_month_{month}")
            };
                buttons.Add(row);
            }

            return new InlineKeyboardMarkup(buttons);
        }

        private static InlineKeyboardMarkup GetDayPickerInlineKeyboard()
        {
            var days = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" };

            var buttonsList = new List<List<InlineKeyboardButton>>();
            var row = new List<InlineKeyboardButton>();
            for (int i = 0; i < days.Length; i++)
            {
                var day = days[i];
                
                row.Add(InlineKeyboardButton.WithCallbackData(day, $"datepicker_day_{day}"));

                if(i % 5 == 4)
                {
                    buttonsList.Add(row);
                    row = new List<InlineKeyboardButton>();
                }
            }
            buttonsList.Add(new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData("31", $"datepicker_day_{31}")
            });

            return new InlineKeyboardMarkup(buttonsList);
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

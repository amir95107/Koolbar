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
        private const long ChannelId = -1001974756992;


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

        private readonly string[] CurrentCommandList = ["/start", "/create"];


        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long chatId = 0;
            try
            {
                if (update.ChannelPost != null || update.MyChatMember != null)
                    return;


                if (update.CallbackQuery != null)
                {
                    chatId = update.CallbackQuery.Message.Chat.Id;
                }
                else if (update.Message != null)
                {
                    chatId = update.Message.Chat.Id;
                }
                else if (update.MyChatMember != null)
                {
                    chatId = update.MyChatMember.From.Id;
                }
                else if (update.ChannelPost != null)
                {
                    chatId = update.ChannelPost.From.Id;
                }
                else
                {
                    chatId = 0;
                }

                #region Check if exists in channel(s)
                var userChannelsStatus = await _botClient.GetChatMemberAsync(-1001974756992, chatId);
                if (userChannelsStatus.Status is not ChatMemberStatus.Member and not ChatMemberStatus.Creator and not ChatMemberStatus.Administrator)
                {
                    var txt = "لطفا برای استفاده از خدمات بات، کانال(ها‌ی) زیر را دنبال کنید." + "\n\n" +
                                "<a href='https://t.me/koolbar_international'>@koolbar_international</a>";
                    await _botClient.SendTextMessageAsync(chatId, txt, parseMode: ParseMode.Html, disableWebPagePreview: true);

                    return;
                }
                #endregion

                #region Check if user has username
                if (string.IsNullOrWhiteSpace(userChannelsStatus.User.Username))
                {
                    var txt = "لطفا برای استفاده از خدمات بات، با مراجعه به تنظیمات تلگرام، Username خود را وارد کنید." + "\n\n";
                    await _botClient.SendTextMessageAsync(chatId, txt);

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
                                await HandleAddTypeCallbackAsync(chatId, update.CallbackQuery, Username);
                                break;
                        }

                        if (queryType.StartsWith("datepicker_month_"))
                        {
                            var month = queryType.Split("_")[2];
                            Requests[Username].FlightMonth = month;
                            var days = GetDayPickerInlineKeyboard();
                            await _botClient.EditMessageReplyMarkupAsync(new ChatId(chatId), Requests[Username].MessageId.Value, replyMarkup: days);
                        }
                        if (queryType.StartsWith("datepicker_day_"))
                        {
                            var day = queryType.Split("_")[2];
                            Requests[Username].FlightDay = int.Parse(day);

                            await HandleAddFlightDateCallbackAsync(chatId, Requests[Username].FlightMonth, int.Parse(day), Username);
                        }
                        if(update.CallbackQuery.Data == "final-confirm")
                        {
                            await HandleFinalConfirmRequestAsync(chatId, Username);
                        }
                        if (update.CallbackQuery.Data == "final-retry")
                        {
                            await HandleCreateCommands(chatId, "");
                        }
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

                if (Requests.Keys.Contains(Username))
                {
                    switch (Requests[Username].RequestStatus)
                    {
                        case RequestStatus.TypeDeclared:
                            if (message.Text.ToLower().StartsWith("s:"))
                            {
                                await HandleAddSourceCallbackAsync(chatId, text, Username);
                            }
                            else
                            {
                                await HandleSearchSourceCallbackAsync(chatId, text);
                            }
                            return;

                        case RequestStatus.SourceDeclared:
                            if (message.Text.ToLower().StartsWith("d:"))
                            {
                                await HandleAddDestionationCallbackAsync(chatId, text, Username);
                            }
                            else
                            {
                                await HandleSearchDestionationCallbackAsync(chatId, text, Username);
                            }
                            return;

                        case RequestStatus.DestinationDeclared:
                            await HandleAddDescriptionCallbackAsync(chatId, text, Username);
                            return;

                        case RequestStatus.DescriptionDeclared:
                            await HandleContinueAddDateAsync(chatId, Requests[Username].RequestType.Value, Username);
                            return;
                    }
                }

                //HGandle slash commands
                if (text.StartsWith("/"))
                {
                    await HandleCommands(text.Split(' ')[0], message.Chat.Id);
                }
            }
            catch (Exception ex)
            {
                await _botClient.SendTextMessageAsync(266809220, ex.Message);
                if (chatId > 0)
                    await _botClient.SendTextMessageAsync(chatId, "An error occured; Please wait... We try to fix the error as soon as possible.");
            }
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

        private bool CheckRequest(string username)
        {
            return Requests[username] != null;
        }

        private async Task HandleCreateCommands(long id, string _type = "")
        {
            var userChannelsStatus = await _botClient.GetChatMemberAsync(-1001974756992, id);

            try
            {
                var username = userChannelsStatus!.User!.Username;

                var request = new RequestDto
                {
                    ChatId = id,
                    Username = username
                };

                Requests[username!] = request;
            }
            catch (Exception ex)
            {
                await _botClient.SendTextMessageAsync(id, "خطا در ثبت اولیه درخواست.");
                return;
            }

            InlineKeyboardButton urlButton = new InlineKeyboardButton("مسافر هستم ✈️");
            InlineKeyboardButton urlButton2 = new InlineKeyboardButton("بار دارم 📦");

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
                if (!CheckRequest(username))
                    await HandleCreateCommands(chatid);

                var data = callback.Data.Split('-')[1];
                
                var message = await _botClient.SendTextMessageAsync(chatid, "لطفا مبدا خود را جستجو کنید:");

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

        private async Task HandleAddSourceCallbackAsync(long id, string source, string username)
        {
            Requests[username].Source = source.Split("s:")[1];
            Requests[username].RequestStatus = RequestStatus.SourceDeclared;

            await _botClient.SendTextMessageAsync(id, "لطفا مقصد خود را جستجو کنید:");
        }

        private async Task HandleSearchDestionationCallbackAsync(long id, string source, string username)
        {
            var cities = await ApiCall.GetAsync<List<CityDto>>($"states/search/{source}");

            ReplyKeyboardMarkup x = GenerateKeyboardButtonForCities(cities, "d");

            var text = cities.Count > 0 ? "لطفا از بین شهرهای زیر شهر مقصد خود را انتخاب کنید:" : "موردی یافت نشد. لطفا مجددا تلاش کنید.";

            await _botClient.SendTextMessageAsync(id, text, replyMarkup: x);
        }

        private async Task HandleAddDestionationCallbackAsync(long id, string destination, string username)
        {
            Requests[username].Destination = destination.Split("d:")[1];
            Requests[username].RequestStatus = RequestStatus.DestinationDeclared;

            await _botClient.SendTextMessageAsync(id, "لطفا توضیحات خود را وارد کنید:");
        }

        private static ReplyKeyboardMarkup GenerateKeyboardButtonForCities(List<CityDto> cities, string sord)
        {
            cities = cities.DistinctBy(x => x.Title).ToList();
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
            Requests[username].Description = message;
            Requests[username].RequestStatus = RequestStatus.DescriptionDeclared;

            var reqTypeText = Requests[username].RequestType == RequestType.Passenger ? "مسافر" : "دارای بار";
            
            var text = Repeat(Emojies.Airplane, 6) +
                $"<a href='http://t.me/{username}'>@{username}</a> \n"
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
            //Requests.Remove(username);
            //await GenerateSuggestedText(id, 20);


            //InlineKeyboardButton urlButton = new InlineKeyboardButton("پیام به کاربر");
            //urlButton.Url = $"https://t.me/{username}";

            InlineKeyboardButton urlButton = new InlineKeyboardButton($"تایید {Emojies.Check}");
            InlineKeyboardButton urlButton2 = new InlineKeyboardButton($"شروع مجدد {Emojies.Retry}");

            urlButton.CallbackData = "final-confirm";
            urlButton2.CallbackData = "final-retry";

            var buttons = new InlineKeyboardButton[] { urlButton2, urlButton };

            // Keyboard markup
            InlineKeyboardMarkup inline = new InlineKeyboardMarkup(buttons);
            await _botClient.SendTextMessageAsync(ChannelId, text, replyMarkup: inline);
        }

        private async Task HandleAddFlightDateCallbackAsync(long id, string month, int day, string username)
        {
            var months = (new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" }).ToArray();

            var date = new DateTime(DateTime.UtcNow.Year, Array.IndexOf(months, month)+1, day);

            Requests[username].FlightDate = date;
            Requests[username].RequestStatus = RequestStatus.DestinationDeclared;

            var text = $"<a href='http://t.me/{username}'>@{username}</a> \n"
                + $"#{Requests[username].RequestType}" + "\n \n"
                + $"مبدا: {Requests[username].Source} " + "\n"
                + $"مقصد: {Requests[username].Destination}" + "\n"
                + $"تاریخ سفر: {date.ToString("yyyy/MM/dd")}" + "\n\n"
                + $"توضیحات: {Requests[username].Description}";


            //InlineKeyboardButton urlButton = new InlineKeyboardButton("پیام به کاربر");
            //urlButton.Url = $"https://t.me/{username}";
            //InlineKeyboardMarkup markup = new InlineKeyboardMarkup(urlButton);
            InlineKeyboardButton urlButton = new InlineKeyboardButton($"تایید {Emojies.Check}");
            InlineKeyboardButton urlButton2 = new InlineKeyboardButton($"شروع مجدد {Emojies.Retry}");

            urlButton.CallbackData = "final-confirm";
            urlButton2.CallbackData = "final-retry";

            var buttons = new InlineKeyboardButton[] { urlButton2, urlButton };

            // Keyboard markup
            InlineKeyboardMarkup inline = new InlineKeyboardMarkup(buttons);
            await _botClient.SendTextMessageAsync(id, text, replyMarkup: inline, parseMode: ParseMode.Html, disableWebPagePreview: true);
            //await GenerateSuggestedText(id, 10);
            //Requests.Remove(username);
        }

        private async Task<string> GenerateSuggestedText(long id, int requestType)
        {
            var list = await ApiCall.GetAsync<List<RequestDto>>($"requests/suggest/{id}");

            var text = list.Count == 0 ? "موردی جهت نمایش وجود ندارد!" : string.Empty;
            int num = 0;
            var t = string.Empty;
            foreach (var item in list)
            {
                t = $"مبدا: {item.Source} \n مقصد: {item.Destination} \n توضیحات: {item.Description} \n تاریخ پرواز: {item.FlightDate} \n  <a href='https://t.me/{item.Username}'><b>@{item.Username}</b>></a>\n";
                t += num == list.Count - 1 ? "------------------------------------ \n\n" : "";
                num++;
            }
            if (!string.IsNullOrEmpty(t))
                text = t;

            await _botClient.SendTextMessageAsync(id, text, parseMode: ParseMode.Html, disableWebPagePreview: true);
            return "";
        }

        private async Task HandleFinalConfirmRequestAsync(long chatId, string username)
        {
            var request = Requests[username];
            try
            {
                await ApiCall.PostAsync("requests/all", request);
            }
            catch (Exception ex)
            {
                await _botClient.SendTextMessageAsync(266809220, ex.Message);
                await _botClient.SendTextMessageAsync(chatId, "An error occured; Please wait... We try to fix the error as soon as possible.");
                return;
            }

            var emoji = request.RequestType == RequestType.FreightOwner ? Emojies.Package : Emojies.Airplane;
            var typeStr = request.RequestType == RequestType.FreightOwner ? "باردار" : "مسافر";
            var flightDateTxt = "تاریخ پرواز: " + (request.RequestType == RequestType.FreightOwner ? "" : request.FlightDate!.Value.ToString("yyyy/MM/dd")) + "\n\n";
            var text = Repeat(emoji,6) + "\n\n" +
                $"#{typeStr} \n\n" +
                $"مبدا: {request.Source} \n\n" +
                $"مقصد: {request.Source} \n\n" +
                $"{flightDateTxt}" +
                $"توضیحات: {request.Description}";

            InlineKeyboardButton urlButton = new InlineKeyboardButton("پیام به کاربر");
            urlButton.Url = $"https://t.me/{username}";

            InlineKeyboardMarkup markup = new InlineKeyboardMarkup(urlButton);

            await _botClient.SendTextMessageAsync(ChannelId, text, replyMarkup: markup);
            Requests.Remove(username);
            await GenerateSuggestedText(chatId, (int)request.RequestType!);
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

                if (i % 5 == 4)
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

        private static string Repeat(string str, int num)
        {
            var res = str;
            for(int i = 0; i< num; i++)
            {
                res += str;
            }
            return str;
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

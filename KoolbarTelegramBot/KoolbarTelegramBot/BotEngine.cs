using Datalayer.Enumerations;
using Koolbar.Dtos;
using KoolbarTelegramBot.HttpClientProvider;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        //
#if DEBUG
        private const long ChannelId = -1002095988136;
#else 
    private const long ChannelId = -1001974756992;
#endif
        private static List<string> Admins = ["Amirhosseinjnmi", "abolfazl_rezaiee"];
        private static DateTime? SendNotifHistory;

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

        private readonly string[] CurrentCommandList = ["/start"];


        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long chatId = 0;
            try
            {
                if (update.ChannelPost != null || update.MyChatMember != null)
                    return;

                #region ChatId
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
                #endregion

                #region Check if exists in channel(s)
                var userChannelsStatus = await _botClient.GetChatMemberAsync(-1001974756992, chatId);
                if (userChannelsStatus.Status is not ChatMemberStatus.Member and not ChatMemberStatus.Creator and not ChatMemberStatus.Administrator)
                {
                    var txt = "لطفا برای استفاده از خدمات بات، کانال(ها‌ی) زیر را دنبال نمایید." + "\n\n" +
                                "<a href='https://t.me/koolbar_international'>@koolbar_international</a>";

                    InlineKeyboardButton urlButton = new InlineKeyboardButton("عضو شدم");
                    urlButton.CallbackData = "joined";
                    var buttons = new InlineKeyboardButton[] { urlButton };
                    InlineKeyboardMarkup inline = new InlineKeyboardMarkup(buttons);

                    await _botClient.SendTextMessageAsync(chatId, txt, parseMode: ParseMode.Html, disableWebPagePreview: true, replyMarkup: inline);

                    return;
                }
                #endregion

                #region Check if user has username
                if (string.IsNullOrWhiteSpace(userChannelsStatus.User.Username))
                {
                    var txt = "لطفا برای استفاده از خدمات بات، با مراجعه به تنظیمات تلگرام، Username خود را وارد نمایید." + "\n\n";
                    InlineKeyboardButton urlButton = new InlineKeyboardButton("تنظیم کردم");

                    urlButton.CallbackData = "usernameset";

                    var buttons = new InlineKeyboardButton[] { urlButton };

                    // Keyboard markup
                    InlineKeyboardMarkup inline = new InlineKeyboardMarkup(buttons);
                    await _botClient.SendTextMessageAsync(chatId, txt, replyMarkup: inline);

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
                        if (update.CallbackQuery.Data == "final-confirm")
                        {
                            await HandleFinalConfirmRequestAsync(chatId, Username);
                        }
                        if (update.CallbackQuery.Data is "final-retry" or "usernameset" or "joined")
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

                if (text.StartsWith("/"))
                {
                    await HandleCommands(text.Split(' ')[0], message.Chat.Id);
                }

                if (text.StartsWith("#sta"))
                {
                    text = text.Split('\n')[1];
                    await SendMessageToAll(Username, text);
                    return;
                }

                if (text.StartsWith("#ncj"))
                {
                    await NotifForCompleteJourney(Username);
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
                case "/start":
                    await HandleCreateCommands(id, "");
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
            ClearifyRequests(DateTime.Now.AddDays(-3));
            var userChannelsStatus = await _botClient.GetChatMemberAsync(-1001974756992, id);

            try
            {
                var username = userChannelsStatus!.User!.Username;

                var request = new RequestDto
                {
                    ChatId = id,
                    Username = username,
                    CreatedAt = DateTime.Now
                };

                Requests[username!] = request;
            }
            catch (Exception ex)
            {
                await _botClient.SendTextMessageAsync(id, "خطا در ثبت اولیه درخواست.");
                return;
            }

            InlineKeyboardButton urlButton = new InlineKeyboardButton("مسافر هستم ✈️");
            InlineKeyboardButton urlButton2 = new InlineKeyboardButton("ارسال بار 📦");
            InlineKeyboardButton urlButton3 = new InlineKeyboardButton("درخواست خرید 🛍");
            InlineKeyboardButton urlButton4 = new InlineKeyboardButton("اطلاعات سفر 🗂");

            urlButton.CallbackData = "type-" + RequestType.Passenger.ToString();
            urlButton2.CallbackData = "type-" + RequestType.FreightOwner.ToString();
            urlButton3.Url = "https://t.me/vlansupport";
            urlButton4.Url = "https://t.me/Flightiranbot";


            var buttons = new List<List<InlineKeyboardButton>> { new List<InlineKeyboardButton> { urlButton }, new List<InlineKeyboardButton> { urlButton2 }, new List<InlineKeyboardButton> { urlButton3 }, new List<InlineKeyboardButton> { urlButton4 } };

            // Keyboard markup
            InlineKeyboardMarkup inline = new InlineKeyboardMarkup(buttons);

            // Send message!
            await _botClient.SendTextMessageAsync(id, "نوع درخواست خود را مشخص نمایید.", replyMarkup: inline);
        }

        private void ClearifyRequests(DateTime dateTime)
        {
            if (Requests != null && Requests.Any())
            {
                foreach (var item in Requests.Where(x => x.Value.CreatedAt < dateTime))
                {
                    Requests.Remove(item.Key);
                }
            }
        }

        private async Task HandleAddTypeCallbackAsync(long chatid, CallbackQuery callback, string username)
        {
            try
            {
                if (!CheckRequest(username))
                    await HandleCreateCommands(chatid);

                var data = callback.Data.Split('-')[1];

                var message = await _botClient.SendTextMessageAsync(chatid, "لطفا شهر مبدا خود را به <strong>انگلیسی</strong> جستجو نمایید:", parseMode: ParseMode.Html, disableWebPagePreview: true);

                Requests[username].RequestStatus = RequestStatus.TypeDeclared;
                Requests[username].RequestType = Enum.Parse<RequestType>(data);

            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("The given key"))
                {
                    await HandleCreateCommands(chatid, "");
                    return;
                }
                await _botClient.SendTextMessageAsync(chatid, ex.Message);
                return;
            }
        }

        private async Task HandleSearchSourceCallbackAsync(long id, string source)
        {
            var cities = await ApiCall.GetAsync<List<CityDto>>($"states/search/{source}");

            ReplyKeyboardMarkup x = GenerateKeyboardButtonForCities(cities, "s");

            var text = cities.Count > 0 ? "لطفا از بین شهرهای زیر شهر مبدا خود را انتخاب نمایید:" : "موردی یافت نشد. لطفا مجددا تلاش نمایید. دقت بفرمایید که املای شهر را به درستی و به <strong>انگلیسی</strong> وارد کرده باشید.";

            await _botClient.SendTextMessageAsync(id, text, replyMarkup: x, parseMode: ParseMode.Html, disableWebPagePreview: true);
        }

        private async Task HandleAddSourceCallbackAsync(long id, string source, string username)
        {
            source = source.Split(":")[1];
            var emojiArr = source.Split("(");
            string? emoji = emojiArr.Length > 1 ? emojiArr[1].Split(")")[0] : null;
            Requests[username].Source = new CityDto
            {
                Title = emoji != null ? source.Split("_")[1].Replace($"({emoji})", "").Trim() : source.Split("_")[1].Trim(),
                State = new Dtos.StateDto
                {
                    Title = "",
                    Country = new Dtos.CountryDto
                    {
                        Title = source.Split('_')[0],
                        Emoji = emoji
                    }
                }
            };
            Requests[username].RequestStatus = RequestStatus.SourceDeclared;

            await _botClient.SendTextMessageAsync(id, "لطفا شهر مقصد خود را به <strong>انگلیسی</strong> جستجو نمایید:", parseMode: ParseMode.Html, disableWebPagePreview: true);
        }

        private async Task HandleSearchDestionationCallbackAsync(long id, string source, string username)
        {
            var cities = await ApiCall.GetAsync<List<CityDto>>($"states/search/{source}");

            ReplyKeyboardMarkup x = GenerateKeyboardButtonForCities(cities, "d");

            var text = cities.Count > 0 ? "لطفا از بین شهرهای زیر شهر مقصد خود را انتخاب نمایید:" : "موردی یافت نشد. لطفا مجددا تلاش نمایید. دقت بفرمایید که املای شهر را به درستی و به <strong>انگلیسی</strong> وارد کرده باشید.";

            await _botClient.SendTextMessageAsync(id, text, replyMarkup: x, parseMode: ParseMode.Html, disableWebPagePreview: true);
        }

        private async Task HandleAddDestionationCallbackAsync(long id, string destination, string username)
        {
            destination = destination.Split(":")[1];
            var emojiArr = destination.Split("(");
            string? emoji = emojiArr.Length > 1 ? emojiArr[1].Split(")")[0] : null;
            Requests[username].Destination = new CityDto
            {
                Title = emoji != null ? destination.Split("_")[1].Replace($"({emoji})", "").Trim() : destination.Split("_")[1].Trim(),
                State = new Dtos.StateDto
                {
                    Title = "",
                    Country = new Dtos.CountryDto
                    {
                        Title = destination.Split('_')[0],
                        Emoji = emoji
                    }
                }
            }; ;
            Requests[username].RequestStatus = RequestStatus.DestinationDeclared;

            var text = "لطفا توضیحات درباره بارهای مورد پذیرش را وارد نمایید:";
            if (Requests[username].RequestType == RequestType.FreightOwner)
                text = "لطفا اطلاعات مربوط به بار خود را وارد نمایید:";

            await _botClient.SendTextMessageAsync(id, text);
        }

        private static ReplyKeyboardMarkup GenerateKeyboardButtonForCities(List<CityDto> cities, string sord)
        {
            int culomn = 1;
            var buttons = new List<List<KeyboardButton>>();
            var buttonsfour = new List<KeyboardButton>();
            for (int i = 0; i < cities.Count; i++)
            {
                var city = cities[i];
                var emoji = !string.IsNullOrWhiteSpace(city.State.Country.Emoji) ? $" ({city.State.Country.Emoji})" : "";
                buttonsfour.Add(new KeyboardButton($"{sord}:{city.State.Country.Title}_{city.Title}{emoji}"));
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
                text = $"لطفا تاریخ پرواز را مشخص نمایید.";

                var months = GetMonthPickerInlineKeyboard();

                var msg = await _botClient.SendTextMessageAsync(id, text, replyMarkup: months, disableWebPagePreview: true);
                Requests[username].MessageId = msg.MessageId;
            }
        }

        private async Task HandleAddDescriptionCallbackAsync(long id, string message, string username)
        {
            Requests[username].Description = message;
            Requests[username].RequestStatus = RequestStatus.DescriptionDeclared;

            var reqTypeText = Requests[username].RequestType == RequestType.Passenger ? "مسافر" : "دارای_بار";

            var text = Repeat(Emojies.Package, 6) + "\n\n" +
                $"<a href='http://t.me/{username}'>@{username}</a> \n\n"
                + $"#{reqTypeText}" + "\n\n"
                + $"مبدا: {Requests[username].Source.Title} " + "\n\n"
                + $"مقصد: {Requests[username].Destination.Title}" + "\n\n"
                + $"توضیحات: \n <b>{Requests[username].Description}</b>";

            if (reqTypeText == "مسافر")
            {
                text = $"لطفا تاریخ پرواز را مشخص نمایید.";

                var months = GetMonthPickerInlineKeyboard();

                var msg = await _botClient.SendTextMessageAsync(id, text, replyMarkup: months, parseMode: ParseMode.Html, disableWebPagePreview: true);
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
            var msg2 = await _botClient.SendTextMessageAsync(id, text, replyMarkup: inline, parseMode: ParseMode.Html, disableWebPagePreview: true);
            Requests[username].MessageId = msg2.MessageId;
        }

        private async Task HandleAddFlightDateCallbackAsync(long id, string month, int day, string username)
        {
            var months = (new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" }).ToArray();

            var date = new DateTime(DateTime.UtcNow.Year, Array.IndexOf(months, month) + 1, day);

            Requests[username].FlightDate = date;
            Requests[username].RequestStatus = RequestStatus.DestinationDeclared;

            var text = $"<a href='http://t.me/{username}'>@{username}</a> \n\n"
                + $"#{Requests[username].RequestType}" + "\n \n"
                + $"مبدا: {Requests[username].Source.Title} " + "\n\n"
                + $"مقصد: {Requests[username].Destination.Title}" + "\n\n"
                + $"تاریخ سفر: {date.ToString("yyyy/MM/dd")}" + "\n\n"
                + $"توضیحات: \n <strong>{Requests[username].Description}</strong>";


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
            var msg = await _botClient.SendTextMessageAsync(id, text, replyMarkup: inline, parseMode: ParseMode.Html, disableWebPagePreview: true);
            Requests[username].MessageId = msg.MessageId;
            //await GenerateSuggestedText(id, 10);
            //Requests.Remove(username);
        }

        private async Task<List<RequestDto>> GenerateSuggestedText(long id, int requestType, string? username, bool showInChannel = true)
        {
            var text = "درخواست شما در کانال ثبت شد. \n" +
                "<a href='https://t.me/koolbar_international'>کانال اطلاع رسانی کولبر</a>";

            if (showInChannel)
            {
                await _botClient.SendTextMessageAsync(id, text, parseMode: ParseMode.Html, disableWebPagePreview: true);
            }
            var list = await ApiCall.GetAsync<List<RequestDto>>($"requests/suggest/{id}");
            text = "<strong>پیشنهادات: </strong> \n\n";
            text += list.Count == 0 ? "موردی جهت نمایش وجود ندارد!" : string.Empty;
            int num = 0;
            var t = string.Empty;
            foreach (var item in list)
            {
                t = $"مبدا: {item.Source.Title} \n مقصد: {item.Destination.Title} \n توضیحات: {item.Description} \n تاریخ پرواز: {item.FlightDate} \n  <a href='https://t.me/{item.Username}'><b>@{item.Username}</b></a>\n";
                t += num != list.Count - 1 ? "------------------------------------ \n\n" : "";
                num++;
            }

            if (!string.IsNullOrEmpty(t))
                text += t;

            await _botClient.SendTextMessageAsync(id, text, parseMode: ParseMode.Html, disableWebPagePreview: true);

            return list;
        }

        private async Task HandleFinalConfirmRequestAsync(long chatId, string username)
        {
            var request = Requests[username];
            var key = 0;
            try
            {
                var result = await ApiCall.PostAsync("requests/all", request);
                key = result.Key;
            }
            catch (Exception ex)
            {
                await _botClient.SendTextMessageAsync(266809220, ex.Message);
                await _botClient.SendTextMessageAsync(chatId, "An error occured; Please wait... We try to fix the error as soon as possible.");
                return;
            }

            var emoji = request.RequestType == RequestType.FreightOwner ? Emojies.Package : Emojies.Airplane;
            var typeStr = request.RequestType == RequestType.FreightOwner ? "دارای_بار" : "مسافر";
            var flightDateTxt = (request.RequestType == RequestType.FreightOwner ? "" : "\n\n تاریخ پرواز: " + request.FlightDate!.Value.ToString("yyyy/MM/dd")) + "\n\n";
            var text = Repeat(emoji, 6) + "\n\n" +
                $"id: {key} \n\n" +
                $"{request.Source.State.Country.Emoji ?? request.Source.State.Country.Title} -> {request.Destination.State.Country.Emoji ?? request.Destination.State.Country.Title} \n\n" +
                $"#{typeStr} \n\n" +
                $"مبدا: {request.Source.Title} \n\n" +
                $"مقصد: {request.Destination.Title}" +
                $"{flightDateTxt}" +
                $"توضیحات: \n {request.Description} \n\n" +
                $"<a href='https://t.me/koolbar_bot'>@koolbar_bot 🤖</a>";

            InlineKeyboardButton urlButton = new InlineKeyboardButton("پیام به کاربر");
            urlButton.Url = $"https://t.me/{username}";

            InlineKeyboardMarkup markup = new InlineKeyboardMarkup(urlButton);

            await _botClient.SendTextMessageAsync(ChannelId, text, replyMarkup: markup, parseMode: ParseMode.Html, disableWebPagePreview: true);
            InlineKeyboardButton urlButton2 = new InlineKeyboardButton($"شروع مجدد {Emojies.Retry}");
            urlButton2.CallbackData = "final-retry";

            var buttons = new InlineKeyboardButton[] { urlButton2 };

            // Keyboard markup
            InlineKeyboardMarkup inline = new InlineKeyboardMarkup(buttons);

            await _botClient.EditMessageReplyMarkupAsync(new ChatId(chatId), request.MessageId.Value, replyMarkup: inline);
            var list = await GenerateSuggestedText(chatId, (int)request.RequestType!, username: username);

            await NotifyOthers(chatId, username, list);

            Requests.Remove(username);
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

        private async Task NotifyOthers(long chatId, string username, List<RequestDto> list)
        {
            var request = Requests[username];
            foreach (var item in list)
            {
                var f = string.Empty;
                if (item.RequestType == RequestType.Passenger)
                {
                    f = "\n\nتاریخ پرواز: " + item.FlightDate!.Value.ToString("yyyy/MM/dd") + "\n\n";
                }

                var text = "فردی متناسب با درخواست شما پیدا شد: \n\n" +
                    $"مبدا: {request.Source.Title} \n\n" +
                    $"مقصد: {request.Destination.Title} \n\n" +
                    f +
                    $"توضیحات: {request.Description} \n\n" +
                    $"<a href='https://t.me/{username}'>آیدی آگهی دهنده: @{username}</a>";

                await _botClient.SendTextMessageAsync(item.ChatId, text, parseMode: ParseMode.Html, disableNotification: true);
            }
        }

        private async Task SendMessageToAll(string username, string message)
        {
            if (AuthenticateUser(username))
            {
                try
                {
                    var result = await ApiCall.GetAsync<List<long>>("user/chatids");
                    if (result != null)
                    {
                        foreach (var item in result)
                        {
                            try
                            {
                                await _botClient.SendTextMessageAsync(new ChatId(item), message);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _botClient.SendTextMessageAsync(266809220, ex.Message);
                    return;
                }
            }
        }

        private async Task NotifForCompleteJourney(string username)
        {
            if (AuthenticateUser(username))
            {
                if (SendNotifHistory != null && SendNotifHistory.Value.AddDays(1) > DateTime.Now)
                {
                    await _botClient.SendTextMessageAsync(username, "در 24 ساعت گذشته 1 بار پیام ارسال شده.");
                    return;
                }
                if (Requests != null)
                {
                    foreach (var item in Requests)
                    {
                        var message = $"کاربر گرامی: {item.Value.Username} \n\n" +
                            "شما درخواست ناتمامی دارید؛ لطفا به تمکیل آن اقدام نمایید و در صورت بروز هرگونه مشکل حین ثبت درخواست، به پشتیبانی پیام دهید. \n\n"+
                            "<a href='https://t.me/vlansupport'>@vlansupport</a>";
                        try
                        {
                            await _botClient.SendTextMessageAsync(new ChatId(item.Value.ChatId), message, parseMode:ParseMode.Html, disableWebPagePreview:true);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        private bool AuthenticateUser(string username)
        {
            return Admins.Contains(username);
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
            var res = string.Empty;
            for (int i = 0; i < num; i++)
            {
                res += str;
            }
            return res;
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

        private void ValidateInput(string methodName, object input)
        {
            switch (methodName)
            {
                case "":

                    return;
            }
        }
    }
}

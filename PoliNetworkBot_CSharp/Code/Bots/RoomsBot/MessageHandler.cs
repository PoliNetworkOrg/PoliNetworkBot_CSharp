using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.RoomsBot.Utils;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Bots.RoomsBot;

public static class MessageHandler
{
    private static readonly Dictionary<long, Conversation> UserIdToConversation = new();

    public static async Task<ActionDoneObject> HandleMessage(TelegramBotAbstract botClient, Message message)
    {
        if (!UserIdToConversation.TryGetValue(message.From!.Id, out var conversation))
        {
            conversation = new Conversation(message.From.Id);
            UserIdToConversation.Add(message.From.Id, conversation);
        }

        try
        {
            var messageText = message.Text!;

            if (messageText == "back")
            {
                conversation.State = Data.Enums.ConversationState.START;
                conversation.ResetConversationFunctions();
            }

            try
            {
                var action = conversation.State switch
                {
                    Data.Enums.ConversationState.START => StartKeyboard(botClient, message, messageText),
                    Data.Enums.ConversationState.MAIN => MainMenu(botClient, message, messageText),
                    Data.Enums.ConversationState.SELECT_CAMPUS => SelectCampus(botClient, message, messageText),
                    Data.Enums.ConversationState.SELECT_DATE => SelectDate(botClient, message, messageText),
                    Data.Enums.ConversationState.SELECT_START_HOUR => SelectStartHour(botClient, message, messageText),
                    Data.Enums.ConversationState.SELECT_END_HOUR => SelectEndHour(botClient, message, messageText),
                    Data.Enums.ConversationState.SELECT_CLASSROOM => SelectClassRoom(botClient, message, messageText),
                    _ => StartKeyboard(botClient, message, messageText)
                };

                await action;
            }
            catch (TooManyRequestsException e)
            {
                Logger.WriteLine(e);
                conversation.State = Data.Enums.ConversationState.START;
                await botClient.SendTextMessageAsync(message.From.Id,
                    new L("it", "stiamo gestendo un numero elevato di richieste riprovare tra qualche minuto",
                        "en", "we are handling a large number of requests try again in a few minutes"),
                    ChatType.Private, message.From!.LanguageCode, ParseMode.Html,
                    new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                    null, message.MessageThreadId);
            }

            return new ActionDoneObject(ActionDoneEnum.NONE, null, null);
        }
        catch (Exception exception)
        {
            Logger.WriteLine(exception);
            var errorText = new L("it", "E' avvenuto un errore interno al bot, riprova più tardi",
                "en", "An internal error occurred in the bot, try again later");
            await botClient.SendTextMessageAsync(message.From.Id, errorText, ChatType.Private,
                message.From.LanguageCode,
                ParseMode.Html,
                null, null, message.MessageThreadId);
            return new ActionDoneObject(ActionDoneEnum.NONE, null, null);
        }
    }

    private static async Task<MessageSentResult?> MainMenu(TelegramBotAbstract botClient, Message message,
        string messageText, Data.Enums.Function? callbackFunction = null)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;
        ReplyMarkupObject markupObject;
        L replyLang;
        var choicesToState =
            langCode == "it" ? Data.Enums.MainMenuOptionsToFunction : Data.Enums.MainMenuOptionsToStateEn;
        var function = Data.Enums.Function.NULL_FUNCTION;
        if (callbackFunction != null)
        {
            function = conversation!.CurrentFunction;
        }
        else
        {
            if (!choicesToState.TryGetValue(messageText, out function))
            {
                markupObject = ReplyMarkupGenerator.MainKeyboard(langCode!);

                replyLang = new L("it", "Seleziona un opzione valida", "en", "Select a valid option");
                return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                    ParseMode.Html,
                    markupObject, null, message.MessageThreadId);
            }

            conversation!.CurrentFunction = function;

            if (conversation!.Campus == null && function != Data.Enums.Function.SETTINGS)
            {
                ForceSelectCampus(botClient, conversation.CurrentFunction, message, conversation);
                return null;
            }
        }

        switch (function)
        {
            case Data.Enums.Function.OCCUPANCIES:
            case Data.Enums.Function.FREE_CLASSROOMS:
            case Data.Enums.Function.ROOM_OCCUPANCY:
                conversation.State = Data.Enums.ConversationState.SELECT_DATE;
                markupObject = ReplyMarkupGenerator.DateKeyboard();
                replyLang = new L("it", "Seleziona una data", "en", "Select a date");
                return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                    ParseMode.Html,
                    markupObject, null, message.MessageThreadId);
            case Data.Enums.Function.FREE_CLASSROOMS_NOW:
                _ = SelectDate(botClient, message, null , true);
                return null;
            case Data.Enums.Function.SETTINGS:
                conversation.Campus = null;
                SendCampusKeyboard(botClient, message, conversation);
                return null;
            case Data.Enums.Function.NULL_FUNCTION:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void SendCampusKeyboard(TelegramBotAbstract botClient, Message message, Conversation conversation)
    {
        var langCode = message?.From?.LanguageCode ?? "en";
        var markupObject = ReplyMarkupGenerator.CampusKeyboard(langCode, false);

        var replyLang = new L("it", "Seleziona una sede", "en", "Select a campus");

        conversation.State = Data.Enums.ConversationState.SELECT_CAMPUS;
        var messageMessageThreadId = message?.MessageThreadId;
        _ = botClient.SendTextMessageAsync(message?.From?.Id, replyLang, ChatType.Private, langCode,
            ParseMode.Html,
            markupObject, null, messageMessageThreadId);
    }

    private static void ForceSelectCampus(TelegramBotAbstract telegramBotAbstract,
        Data.Enums.Function conversationCurrentFunction, Message message,
        Conversation conversation)
    {
        SendCampusKeyboard(telegramBotAbstract, message, conversation);
        conversation.CallbackNextFunction = conversationCurrentFunction;
        conversation.State = Data.Enums.ConversationState.SELECT_CAMPUS;
    }

    private static async Task<MessageSentResult?> SelectClassRoom(TelegramBotAbstract botClient, Message message,
        string messageText)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;

        ReplyMarkupObject? markupObject;
        L replyLang;

        var classRooms = Fetcher.GetAllClassrooms(conversation!.Campus!, conversation.Date);
        var uglyClassRoomWLineBreaks = classRooms?.Find((classRoom) => classRoom?.Contains(messageText) ?? false);
        if (uglyClassRoomWLineBreaks == null)
        {
            markupObject = null;
            replyLang = new L("it", "Seleziona un'aula valida", "en", "Select a valid classroom");
            return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                ParseMode.Html,
                markupObject, null, message.MessageThreadId);
        }


        conversation.State = Data.Enums.ConversationState.START;
        var fileString = Fetcher.GetSingleClassroom(conversation.Campus!, uglyClassRoomWLineBreaks, conversation.Date);

        markupObject = ReplyMarkupGenerator.BackButton();
        replyLang = new L("it", "", "en", "");

        if (fileString == null)
        {
            replyLang = new L("it", "Errore interno", "en", "Internal error");
            return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                ParseMode.Html,
                markupObject, null, message.MessageThreadId);
        }

        var encoding = Encoding.UTF8;
        var stream = new MemoryStream(encoding.GetBytes(fileString));
        PeerAbstract peer = new(message.From?.Id, message.Chat.Type);

        var file = new TelegramFile(stream, $"Classroom_{conversation.Date:dd-MM-yy}_{messageText}.html", replyLang,
            "text/plain",
            TextAsCaption.AS_CAPTION);
        var replyMarkup = ReplyMarkupGenerator.MainKeyboard(message.From?.LanguageCode ?? "en");
        conversation.State = Data.Enums.ConversationState.MAIN;
        conversation.ResetConversationFunctions();
        botClient.SendFileAsync(file, peer, message.From?.Username,
            message.From?.LanguageCode, null, true, message.MessageThreadId, replyMarkup);
        return null;
    }

    private static async Task<MessageSentResult?> SelectStartHour(TelegramBotAbstract botClient, Message message,
        string messageText)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;

        ReplyMarkupObject? markupObject;
        L replyLang;

        if (!int.TryParse(messageText, out var startHour) || startHour is < 8 or > 19)
        {
            markupObject = null;

            replyLang = new L("it", "Seleziona un numero valido", "en", "Select a valid number");
            return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                ParseMode.Html,
                markupObject, null, message.MessageThreadId);
        }

        conversation!.StartHour = startHour;

        conversation.State = Data.Enums.ConversationState.SELECT_END_HOUR;
        markupObject = ReplyMarkupGenerator.HourSelector(startHour, 20);
        replyLang = new L("it", "A che orario?", "en", "To what time?");
        return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
            ParseMode.Html,
            markupObject, null, message.MessageThreadId);
    }

    private static async Task<MessageSentResult?> SelectEndHour(TelegramBotAbstract botClient, Message message,
        string messageText)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;

        if (!int.TryParse(messageText, out var endHour) || endHour < conversation!.StartHour || endHour > 20)
        {
            ReplyMarkupObject? markupObject = null;

            var replyLang = new L("it", "Seleziona un numero valido", "en", "Select a valid number");
            return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                ParseMode.Html,
                markupObject, null, message.MessageThreadId);
        }

        conversation!.EndHour = endHour;
        conversation.State = Data.Enums.ConversationState.START;
        return conversation.CurrentFunction switch
        {
            Data.Enums.Function.FREE_CLASSROOMS => await SendFreeClassrooms(conversation, message, botClient),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static async Task<MessageSentResult?> SendFreeClassrooms(Conversation conversation, Message message, TelegramBotAbstract botClient)
    {
        conversation.ResetConversationFunctions();
        conversation.State = Data.Enums.ConversationState.START;
        // fetching classrooms and adding necessary quarter of an hour to start and end hours
        var freeClassrooms = Fetcher.GetFreeClassrooms(conversation.Campus!, conversation.Date!,
            conversation.StartHour, conversation.EndHour);

        L? replyLang;
        if (freeClassrooms?.Count == 0)
        {
            replyLang = new L("it", "Errore interno - Non ho trovato aule in questo campus", "en",
                "Internal error - No classrooms found in this campus");
            var markupObject = ReplyMarkupGenerator.MainKeyboard(message.From?.LanguageCode ?? "en");
            conversation.State = Data.Enums.ConversationState.MAIN;
            return await botClient.SendTextMessageAsync(message.From?.Id, replyLang, ChatType.Private, message.From?.LanguageCode,
                ParseMode.Html,
                markupObject, null, message.MessageThreadId);
        }

        var fixedFreeClassRooms = freeClassrooms?.Select(classRoom => classRoom?.Trim()
            .Replace("\n", "")
            .Replace("\t", "")
            .Replace("\r", "")
            .Trim()).ToList() ?? new List<string?>();

        var freeClassroomsString = string.Concat("- ", string.Join("\n- ", fixedFreeClassRooms));
        var textIt = "Aule libere dalle " + conversation.StartHour + ".15 alle " + conversation.EndHour + ".15:\n"
                     + freeClassroomsString;
        var textEn = "Free classrooms from " + conversation.StartHour + ".15 to " + conversation.EndHour + ".15:\n"
                     + freeClassroomsString;
        replyLang = new L("it", textIt, "en", textEn);
        conversation.State = Data.Enums.ConversationState.MAIN;
        var replyMarkup = ReplyMarkupGenerator.MainKeyboard(message.From?.LanguageCode ?? "en");
        return await botClient.SendTextMessageAsync(message.From!.Id, replyLang, ChatType.Private, message.From?.LanguageCode,
            ParseMode.Html,
            replyMarkup, null, message.MessageThreadId, splitMessage: true);
    }

    private static async Task<MessageSentResult?> SelectDate(TelegramBotAbstract botClient, Message message,
        string? messageText, bool? now = false)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;
        ReplyMarkupObject? markupObject;
        L replyLang;
        DateTime date;
        // try to parse the date and check if it less than 30 days in the future
        if (now != null && now.Value)
        {
            date = DateTime.Now;
        }
        else
        {
            if (!DateTime.TryParseExact(messageText, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out date) ||
                date > DateTime.Now.AddDays(ReplyMarkupGenerator.DaysAmount))
            {
                // Logger.WriteLine($"Selected invalid date in bot: {messageText}, {date}, {date > DateTime.Now.AddDays(ReplyMarkupGenerator.DaysAmount)}");
                markupObject = null;
                replyLang = new L("it", "Seleziona una data valida", "en", "Select a valid date");
                return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                    ParseMode.Html,
                    markupObject, null, message.MessageThreadId);
            }
        }

        conversation!.Date = date;
        switch (conversation.CurrentFunction)//todo
        {
            case Data.Enums.Function.OCCUPANCIES:
                conversation.ResetConversationFunctions();
                conversation.State = Data.Enums.ConversationState.START;
                var fileString = Fetcher.GetRawOccupancies(conversation.Campus!, date);
                markupObject = ReplyMarkupGenerator.BackButton();
                replyLang = new L("it", "", "en", "");

                if (fileString == null)
                {
                    replyLang = new L("it", "Errore interno", "en", "Internal error");
                    return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                        ParseMode.Html,
                        markupObject, null, message.MessageThreadId);
                }

                var encoding = Encoding.UTF8;
                var stream = new MemoryStream(encoding.GetBytes(fileString));
                PeerAbstract peer = new(message.From?.Id, message.Chat.Type);

                var file = new TelegramFile(stream, $"Occupancies_{conversation.Date:dd-MM-yy}.html", replyLang,
                    "text/plain",
                    TextAsCaption.AS_CAPTION);
                var replyMarkup = ReplyMarkupGenerator.MainKeyboard(message.From?.LanguageCode ?? "en");
                conversation.State = Data.Enums.ConversationState.MAIN;
                botClient.SendFileAsync(file, peer, message.From?.Username,
                    message.From?.LanguageCode, null, true, message.MessageThreadId, replyMarkup);
                return null;
            case Data.Enums.Function.FREE_CLASSROOMS:
                conversation.State = Data.Enums.ConversationState.SELECT_START_HOUR;
                markupObject = ReplyMarkupGenerator.HourSelector(8, 19);
                replyLang = new L("it", "Da che orario?", "en", "From what time?");
                break;
            case Data.Enums.Function.ROOM_OCCUPANCY:
                conversation.State = Data.Enums.ConversationState.SELECT_CLASSROOM;
                var classRooms = Fetcher.GetAllClassrooms(conversation.Campus!, date);
                if (classRooms?.Count > 0)
                {
                    classRooms = classRooms.Select(classRoom => classRoom.Trim()
                        .Replace("\n", "")
                        .Replace("\t", "")
                        .Replace("\r", "")
                        .Trim()).ToList();
                }
                else
                {
                    classRooms = new List<string> { "No classrooms available" };
                }

                markupObject =
                    ReplyMarkupGenerator.ClassroomsKeyboard(classRooms);
                replyLang = new L("it", "Seleziona un'aula", "en", "Select a classroom");

                break;
            case Data.Enums.Function.FREE_CLASSROOMS_NOW:
                conversation.StartHour = DateTime.Now.Hour;
                conversation.EndHour = 19;
                return await SendFreeClassrooms(conversation, message, botClient);
            default:
                throw new ArgumentOutOfRangeException();
        }

        return await botClient.SendTextMessageAsync(message.From?.Id, replyLang, ChatType.Private, langCode,
            ParseMode.Html,
            markupObject, null, message.MessageThreadId);
    }

    private static async Task<MessageSentResult?> SelectCampus(TelegramBotAbstract botClient, Message message,
        string messageText)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;
        ReplyMarkupObject? markupObject;
        L replyLang;

        if (!Data.Enums.Campuses.TryGetValue(messageText, out var campus))
        {
            markupObject = ReplyMarkupGenerator.CampusKeyboard(langCode!, true);

            replyLang = new L("it", "Seleziona una sede valida", "en", "Select a valid campus");
            return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                ParseMode.Html,
                markupObject, null, message.MessageThreadId);
        }

        conversation!.Campus = campus;

        switch (conversation!.CallbackNextFunction)
        {
            case Data.Enums.Function.FREE_CLASSROOMS_NOW:
            case Data.Enums.Function.OCCUPANCIES:
            case Data.Enums.Function.ROOM_OCCUPANCY:
            case Data.Enums.Function.FREE_CLASSROOMS:
                conversation.State = Data.Enums.ConversationState.MAIN;
                markupObject = null;
                conversation.CurrentFunction = conversation.CallbackNextFunction;
                break;
            case Data.Enums.Function.NULL_FUNCTION:
            case Data.Enums.Function.SETTINGS:
            default:
                conversation.State = Data.Enums.ConversationState.MAIN;
                markupObject = ReplyMarkupGenerator.MainKeyboard(langCode!);
                break;
        }

        replyLang = new L("it", "Sede selezionata", "en", "Campus selected");

        if (conversation.CallbackNextFunction is Data.Enums.Function.NULL_FUNCTION or Data.Enums.Function.SETTINGS)
            return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                ParseMode.Html, markupObject, null, message.MessageThreadId);
        await MainMenu(botClient, message, "", conversation.CallbackNextFunction);
        return null;
    }


    private static async Task<MessageSentResult?> StartKeyboard(TelegramBotAbstract botClient, Message message,
        string? choice)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;

        var replyLang = new L("it", "Seleziona un'opzione", "en", "Select an option")!;
        var markupObject = ReplyMarkupGenerator.MainKeyboard(langCode!);

        conversation!.State = Data.Enums.ConversationState.MAIN;
        return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
            ParseMode.Html,
            markupObject, null, message.MessageThreadId);
    }
}
using System;
using System.Collections.Generic;
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
            }

            try
            {
                var action = conversation.State switch
                {
                    Data.Enums.ConversationState.START => StartKeyboard(botClient, message, messageText),
                    Data.Enums.ConversationState.MAIN => MainMenuKeyboard(botClient, message, messageText),
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
                    null);
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
                null, null);
            return new ActionDoneObject(ActionDoneEnum.NONE, null, null);
        }
    }

    private static async Task<MessageSentResult?> MainMenuKeyboard(TelegramBotAbstract botClient, Message message,
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
                    markupObject, null);
            }

            conversation!.CurrentFunction = function;

            if (conversation!.Campus == null)
            {
                ForceSelectCampus(botClient, conversation.CurrentFunction, message, conversation);
                return null;
            }
        }

        switch (function)
        {
            case Data.Enums.Function.OCCUPANCIES:
            case Data.Enums.Function.FREE_CLASSROOMS:
            case Data.Enums.Function.FIND_CLASSROOM:
                conversation.State = Data.Enums.ConversationState.SELECT_DATE;
                markupObject = ReplyMarkupGenerator.DateKeyboard();
                replyLang = new L("it", "Seleziona una data", "en", "Select a date");
                return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                    ParseMode.Html,
                    markupObject, null);
            case Data.Enums.Function.SETTINGS:
                conversation.Campus = null;
                conversation.State = Data.Enums.ConversationState.START;
                return await StartKeyboard(botClient, message, messageText);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void ForceSelectCampus(TelegramBotAbstract telegramBotAbstract,
        Data.Enums.Function conversationCurrentFunction, Message message,
        Conversation conversation)
    {
        var langCode = message?.From?.LanguageCode ?? "en";
        var markupObject = ReplyMarkupGenerator.CampusKeyboard(langCode, false);

        var replyLang = new L("it", "Seleziona una sede", "en", "Select a campus");

        conversation.State = Data.Enums.ConversationState.SELECT_CAMPUS;
        _ = telegramBotAbstract.SendTextMessageAsync(message?.From!.Id, replyLang, ChatType.Private, langCode,
            ParseMode.Html,
            markupObject, null);

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
        var uglyClassRoomWLineBreaks = classRooms.Find((classRoom) => classRoom.Contains(messageText));
        if (uglyClassRoomWLineBreaks == null)
        {
            markupObject = null;
            replyLang = new L("it", "Seleziona un'aula valida", "en", "Select a valid classroom");
            return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                ParseMode.Html,
                markupObject, null);
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
                markupObject, null);
        }

        var encoding = Encoding.UTF8;
        var stream = new MemoryStream(encoding.GetBytes(fileString));
        PeerAbstract peer = new(message.From?.Id, message.Chat.Type);

        var file = new TelegramFile(stream, $"Classroom_{conversation.Date:dd-MM-yy}_{messageText}.html", replyLang, "text/plain",
            TextAsCaption.AS_CAPTION);
        var replyMarkup = ReplyMarkupGenerator.MainKeyboard(message.From?.LanguageCode ?? "en");
        conversation.State = Data.Enums.ConversationState.MAIN;
        botClient.SendFileAsync(file, peer, message.From?.Username,
            message.From?.LanguageCode, null, true, replyMarkup);
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
                markupObject, null);
        }

        conversation!.StartHour = startHour;

        conversation.State = Data.Enums.ConversationState.SELECT_END_HOUR;
        markupObject = ReplyMarkupGenerator.HourSelector(9, 20);
        replyLang = new L("it", "a che orario?", "en", "to what time?");
        return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
            ParseMode.Html,
            markupObject, null);
    }

    private static async Task<MessageSentResult?> SelectEndHour(TelegramBotAbstract botClient, Message message,
        string messageText)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;

        ReplyMarkupObject? markupObject;
        L replyLang;

        if (!int.TryParse(messageText, out var endHour) || endHour is < 9 or > 20)
        {
            markupObject = null;

            replyLang = new L("it", "Seleziona un numero valido", "en", "Select a valid number");
            return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                ParseMode.Html,
                markupObject, null);
        }

        conversation!.EndHour = endHour;
        conversation.State = Data.Enums.ConversationState.START;
        switch (conversation.CurrentFunction)
        {
            case Data.Enums.Function.FREE_CLASSROOMS:
                conversation.State = Data.Enums.ConversationState.START;
                var freeClassrooms = Fetcher.GetFreeClassrooms(conversation.Campus!, conversation.Date!,
                    conversation.StartHour!, conversation.EndHour!);

                markupObject = null;
                if (freeClassrooms.Count == 0)
                {
                    replyLang = new L("it", "Errore interno - Non ho trovato aule in questo campus", "en", "Internal error - No classrooms found in this campus");
                    markupObject = ReplyMarkupGenerator.MainKeyboard(langCode ?? "en");
                    conversation.State = Data.Enums.ConversationState.MAIN;
                    return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                        ParseMode.Html,
                        markupObject, null);
                }

                var fixedFreeClassRooms = freeClassrooms.Select(classRoom => classRoom.Trim()
                    .Replace("\n", "")
                    .Replace("\t", "")
                    .Replace("\r", "")
                    .Trim()).ToList();
                
                var freeClassroomsString = string.Join("\n- ", fixedFreeClassRooms);
                var textIt = "Aule libere dalle " + conversation.StartHour + " alle " + conversation.EndHour + ":\n"
                             + freeClassroomsString;
                var textEn = "Free classrooms from " + conversation.StartHour + " to " + conversation.EndHour + ":\n"
                             + freeClassroomsString;
                replyLang = new L("it", textIt, "en", textEn);
                conversation.State = Data.Enums.ConversationState.MAIN;
                var replyMarkup = ReplyMarkupGenerator.MainKeyboard(message.From?.LanguageCode ?? "en");
                return await botClient.SendTextMessageAsync(message.From!.Id, replyLang, ChatType.Private, langCode,
                    ParseMode.Html,
                    replyMarkup, null, splitMessage: true);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static async Task<MessageSentResult?> SelectDate(TelegramBotAbstract botClient, Message message,
        string messageText)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;
        ReplyMarkupObject? markupObject;
        L replyLang;

        // try to parse the date and check if it less than 30 days in the future

        if (!DateTime.TryParse(messageText, out var date) ||
            date > DateTime.Now.AddDays(ReplyMarkupGenerator.DaysAmount))
        {
            Logger.WriteLine($"Selected invalid date in bot: {messageText}, {date}, {date > DateTime.Now.AddDays(ReplyMarkupGenerator.DaysAmount)}");
            markupObject = null;
            replyLang = new L("it", "Seleziona una data valida", "en", "Select a valid date");
            return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                ParseMode.Html,
                markupObject, null);
        }

        conversation!.Date = date;
        switch (conversation.CurrentFunction)
        {
            case Data.Enums.Function.OCCUPANCIES:
                conversation.State = Data.Enums.ConversationState.START;
                var fileString = Fetcher.GetRawOccupancies(conversation.Campus!, date);
                markupObject = ReplyMarkupGenerator.BackButton();
                replyLang = new L("it", "", "en", "");

                if (fileString == null)
                {
                    replyLang = new L("it", "Errore interno", "en", "Internal error");
                    return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                        ParseMode.Html,
                        markupObject, null);
                }

                var encoding = Encoding.UTF8;
                var stream = new MemoryStream(encoding.GetBytes(fileString));
                PeerAbstract peer = new(message.From?.Id, message.Chat.Type);

                var file = new TelegramFile(stream, $"Occupancies_{conversation.Date:dd-MM-yy}.html", replyLang, "text/plain",
                    TextAsCaption.AS_CAPTION);
                var replyMarkup = ReplyMarkupGenerator.MainKeyboard(message.From?.LanguageCode ?? "en");
                conversation.State = Data.Enums.ConversationState.MAIN;
                botClient.SendFileAsync(file, peer, message.From?.Username,
                    message.From?.LanguageCode, null, true, replyMarkup);
                return null;
            case Data.Enums.Function.FREE_CLASSROOMS:
                conversation.State = Data.Enums.ConversationState.SELECT_START_HOUR;
                markupObject = ReplyMarkupGenerator.HourSelector(8, 19);
                replyLang = new L("it", "da che orario?", "en", "from what time?");
                break;
            case Data.Enums.Function.FIND_CLASSROOM:
                conversation.State = Data.Enums.ConversationState.SELECT_CLASSROOM;
                var classRooms = Fetcher.GetAllClassrooms(conversation.Campus!, date);
                if (classRooms.Count > 0)
                {
                    classRooms = classRooms.Select(classRoom => classRoom.Trim()
                            .Replace("\n", "")
                            .Replace("\t", "")
                            .Replace("\r", "")
                            .Trim()).ToList();
                }
                else
                {
                    classRooms.Add("No classrooms available");
                }

                markupObject =
                    ReplyMarkupGenerator.ClassroomsKeyboard(classRooms);
                replyLang = new L("it", "seleziona un'aula", "en", "select a classroom");
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return await botClient.SendTextMessageAsync(message.From?.Id, replyLang, ChatType.Private, langCode, ParseMode.Html,
            markupObject, null);
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
                markupObject, null);
        }

        conversation!.Campus = campus;

        switch (conversation!.CallbackNextFunction)
        {
            case Data.Enums.Function.OCCUPANCIES:
            case Data.Enums.Function.FIND_CLASSROOM:
            case Data.Enums.Function.FREE_CLASSROOMS:
                conversation.State = Data.Enums.ConversationState.MAIN;
                markupObject = null;
                conversation.CurrentFunction = conversation.CallbackNextFunction;
                break;
            default:
                conversation.State = Data.Enums.ConversationState.START;
                markupObject = ReplyMarkupGenerator.MainKeyboard(langCode!);
                break;
        }

        replyLang = new L("it", "Sede selezionata", "en", "Campus selected");

        if (conversation.CallbackNextFunction == Data.Enums.Function.NULL_FUNCTION)
            return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                ParseMode.Html, markupObject, null);
        await MainMenuKeyboard(botClient, message, "", conversation.CallbackNextFunction);
        return null;
    }


    private static async Task<MessageSentResult?> StartKeyboard(TelegramBotAbstract botClient, Message message,
        string? choice)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;

        ReplyMarkupObject markupObject;
        L replyLang;

        replyLang = new L("it", "Seleziona un'opzione", "en", "Select an option")!;
        markupObject = ReplyMarkupGenerator.MainKeyboard(langCode!);

        conversation!.State = Data.Enums.ConversationState.MAIN;
        return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
            ParseMode.Html,
            markupObject, null);
    }
}
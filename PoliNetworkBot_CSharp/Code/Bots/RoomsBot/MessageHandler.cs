﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.RoomsBot.Utils;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
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


        var messageText = message.Text!;
        //try catch block that handles the exceptions, when it's a TooManyRequestsException, it sedds a message to the user
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
                _ => throw new ArgumentOutOfRangeException()
            };

            await action;
        }
        catch (TooManyRequestsException e)
        {
            await botClient.SendTextMessageAsync(message.From.Id,
                new L("it", "stiamo gestendo un numero elevato di richieste riprovare tra qualche minuto",
                    "en", "we are handling a large number of requests try again in a few minutes"),
                ChatType.Private, message.From!.LanguageCode, ParseMode.Html,
                new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                null);
        }

        return new ActionDoneObject(ActionDoneEnum.NONE, null, null);
    }

    private static async Task<MessageSentResult?> MainMenuKeyboard(TelegramBotAbstract botClient, Message message,
        string messageText)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;
        var choicesToState =
            langCode == "it" ? Data.Enums.MainMenuOptionsToFunction : Data.Enums.MainMenuOptionsToStateEn;

        ReplyMarkupObject markupObject;
        L replyLang;

        if (!choicesToState.TryGetValue(messageText, out var function))
        {
            markupObject = ReplyMarkupGenerator.CampusKeyboard(langCode!);

            replyLang = new L("it", "Seleziona un opzione valida", "en", "Select a valid option");

            conversation!.State = Data.Enums.ConversationState.SELECT_CAMPUS;
            return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                ParseMode.Html,
                markupObject, null);
        }

        conversation!.CurrentFunction = function;

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
        }

        markupObject = ReplyMarkupGenerator.CampusKeyboard(langCode!);

        replyLang = new L("it", "Seleziona un opzione valida", "en", "Select a valid option");

        conversation!.State = Data.Enums.ConversationState.SELECT_CAMPUS;
        return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
            ParseMode.Html,
            markupObject, null);
    }

    private static async Task<MessageSentResult?> SelectClassRoom(TelegramBotAbstract botClient, Message message, string messageText)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;
        
        ReplyMarkupObject markupObject;
        L replyLang;

        var classRooms = Fetcher.GetAllClassrooms(conversation!.Campus!, conversation.Date!);
        if (!classRooms.Contains(messageText))
        {
            markupObject = ReplyMarkupGenerator.CampusKeyboard(langCode!);

            replyLang = new L("it", "Seleziona un'aula valida", "en", "Select a valid classroom");

            conversation!.State = Data.Enums.ConversationState.SELECT_CAMPUS;
            return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                ParseMode.Html,
                markupObject, null);
        }

        
        conversation.State = Data.Enums.ConversationState.END;
        var fileString = Fetcher.GetSingleClassroom(conversation.Campus!, messageText, conversation.Date);
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

        var file = new TelegramFile(stream, "classroom.hml", replyLang, "text/plain",
            TextAsCaption.AS_CAPTION);
        botClient.SendFileAsync(file, peer, message.From?.Username,
            message.From?.LanguageCode, null, true);

        return await botClient.SendTextMessageAsync(message.From!.Id, replyLang, ChatType.Private, langCode,
            ParseMode.Html,
            markupObject, null);
    }

    private static async Task<MessageSentResult?> SelectStartHour(TelegramBotAbstract botClient, Message message, string messageText)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;
        
        ReplyMarkupObject markupObject;
        L replyLang;
        
        if (!int.TryParse(messageText, out var startHour) || startHour is < 8 or > 19)
        {
            markupObject = ReplyMarkupGenerator.CampusKeyboard(langCode!);

            replyLang = new L("it", "Seleziona un numero valido", "en", "Select a valid number");

            conversation!.State = Data.Enums.ConversationState.SELECT_CAMPUS;
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

    private static async Task<MessageSentResult?> SelectEndHour(TelegramBotAbstract botClient, Message message, string messageText)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;
        
        ReplyMarkupObject markupObject;
        L replyLang;
        
        if (!int.TryParse(messageText, out var endHour) || endHour is < 9 or > 20)
        {
            markupObject = ReplyMarkupGenerator.CampusKeyboard(langCode!);

            replyLang = new L("it", "Seleziona un numero valido", "en", "Select a valid number");

            conversation!.State = Data.Enums.ConversationState.SELECT_CAMPUS;
            return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                ParseMode.Html,
                markupObject, null);
        }
        
        conversation!.EndHour = endHour;
        conversation.State = Data.Enums.ConversationState.END;
        switch (conversation.CurrentFunction)
        {
            case Data.Enums.Function.FREE_CLASSROOMS:
                conversation.State = Data.Enums.ConversationState.END;
                var fileString = Fetcher.GetFreeClassrooms(conversation.Campus!, conversation.Date!, conversation.StartHour!, conversation.EndHour!);
                markupObject = ReplyMarkupGenerator.BackButton();
                replyLang = new L("it", "", "en", "");

                if (fileString.Count == 0)
                {
                    replyLang = new L("it", "Errore interno", "en", "Internal error");
                    return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                        ParseMode.Html,
                        markupObject, null);
                }
                
                //TODO: send message with free classrooms
                return await botClient.SendTextMessageAsync(message.From!.Id, replyLang, ChatType.Private, langCode,
                    ParseMode.Html,
                    markupObject, null);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static async Task<MessageSentResult?> SelectDate(TelegramBotAbstract botClient, Message message,
        string messageText)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;
        ReplyMarkupObject markupObject;
        L replyLang;

        // try to parse the date and check if it less than 30 days in the future


        if (!DateTime.TryParse(messageText, out var date) ||
            date > DateTime.Now.AddDays(ReplyMarkupGenerator.DaysAmount))
        {
            markupObject = ReplyMarkupGenerator.DateKeyboard();
            var peer = new PeerAbstract(message.From.Id, ChatType.Private);
            replyLang = new L("it", "Seleziona una data valida", "en", "Select a valid date");
            return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                ParseMode.Html,
                markupObject, null);
        }

        conversation!.Date = date;
        switch (conversation.CurrentFunction)
        {
            case Data.Enums.Function.OCCUPANCIES:
                conversation.State = Data.Enums.ConversationState.END;
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

                var file = new TelegramFile(stream, "occupancies.html", replyLang, "text/plain",
                    TextAsCaption.AS_CAPTION);
                botClient.SendFileAsync(file, peer, message.From?.Username,
                    message.From?.LanguageCode, null, true);

                return await botClient.SendTextMessageAsync(message.From!.Id, replyLang, ChatType.Private, langCode,
                    ParseMode.Html,
                    markupObject, null);
            case Data.Enums.Function.FREE_CLASSROOMS:
                conversation.State = Data.Enums.ConversationState.SELECT_START_HOUR;
                markupObject = ReplyMarkupGenerator.HourSelector(8, 19);
                replyLang = new L("it", "da che orario?", "en", "from what time?");

                return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                    ParseMode.Html,
                    markupObject, null);
            case Data.Enums.Function.FIND_CLASSROOM:
                conversation.State = Data.Enums.ConversationState.SELECT_CLASSROOM;
                markupObject =
                    ReplyMarkupGenerator.ClassroomsKeyboard(Fetcher.GetAllClassrooms(conversation.Campus!, date));
                replyLang = new L("it", "seleziona un'aula", "en", "select a classroom");

                return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                    ParseMode.Html,
                    markupObject, null);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static async Task<MessageSentResult?> SelectCampus(TelegramBotAbstract botClient, Message message,
        string messageText)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;
        ReplyMarkupObject markupObject;
        L replyLang;

        if (!Data.Enums.Campuses.TryGetValue(messageText, out var campus))
        {
            markupObject = ReplyMarkupGenerator.CampusKeyboard(langCode!);

            replyLang = new L("it", "Seleziona una sede valida", "en", "Select a valid campus");

            conversation!.State = Data.Enums.ConversationState.SELECT_CAMPUS;
            return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                ParseMode.Html,
                markupObject, null);
        }

        conversation!.Campus = campus;
        conversation.State = Data.Enums.ConversationState.START;
        markupObject = ReplyMarkupGenerator.CampusKeyboard(langCode!);

        replyLang = new L("it", "Sede selezionata", "en", "Campus selected");

        conversation!.State = Data.Enums.ConversationState.SELECT_CAMPUS;
        return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
            ParseMode.Html,
            markupObject, null);
    }


    private static async Task<MessageSentResult?> StartKeyboard(TelegramBotAbstract botClient, Message message,
        string choice)
    {
        UserIdToConversation.TryGetValue(message.From!.Id, out var conversation);
        var langCode = message.From!.LanguageCode;

        ReplyMarkupObject markupObject;
        L replyLang;
        if (conversation!.Campus == null)
        {
            markupObject = ReplyMarkupGenerator.CampusKeyboard(langCode!);

            replyLang = new L("it", "Seleziona una sede", "en", "Select a campus");

            conversation.State = Data.Enums.ConversationState.SELECT_CAMPUS;
            return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
                ParseMode.Html,
                markupObject, null);
        }

        replyLang = new L("it", "Seleziona un'opzione", "en", "Select an option")!;
        markupObject = ReplyMarkupGenerator.MainKeyboard(langCode!);

        conversation.State = Data.Enums.ConversationState.MAIN;
        return await botClient.SendTextMessageAsync(message.From.Id, replyLang, ChatType.Private, langCode,
            ParseMode.Html,
            markupObject, null);
    }
}
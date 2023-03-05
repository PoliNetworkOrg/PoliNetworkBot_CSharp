using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils.Assoc;

public static class AssocSend
{
    
    public static async Task<bool> Assoc_SendAsync(TelegramBotAbstract? sender, MessageEventArgs? e, bool dry = false)
    {
        try
        {
            var replyTo = e?.Message.ReplyToMessage;

            if (replyTo == null)
            {
                await AssocGeneric.Assoc_ObjectToSendNotValid(sender, e);
                return false;
            }

            var languageList = new Language(new Dictionary<string, string?>
            {
                { "it", "Scegli l'entità per il quale stai componendo il messaggio" },
                { "en", "Choose the entity you are writing this message for" }
            });

            var messageFromIdEntity = await AssocGeneric.GetIdEntityFromPersonAsync(e?.Message.From?.Id, languageList,
                sender, e?.Message.From?.LanguageCode, e?.Message.From?.Username);

            if (messageFromIdEntity == null && !Owners.CheckIfOwner(e?.Message.From?.Id) )
            {
                await AssocGeneric.EntityNotFoundAsync(sender, e);
                return false;
            }

            var hasThisEntityAlreadyReachedItsLimit =
                AssocGeneric.CheckIfEntityReachedItsMaxLimit(messageFromIdEntity, sender, true, e?.Message.From?.Id) ?? true;

            return !hasThisEntityAlreadyReachedItsLimit
                ? await AssocSend3Async(sender, e, dry, replyTo, messageFromIdEntity)
                : await AssocSendTooManyMessages(sender, e);
        }
        catch (Exception? ex)
        {
            await AssocGeneric.HandleException(ex, sender, e);
            return false;
        }
    }

    private static async Task<bool> AssocSendTooManyMessages(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var languageList4 = new Language(new Dictionary<string, string?>
        {
            { "it", "Spiacente! In questo periodo hai inviato troppi messaggi" },
            { "en", "I'm sorry! In this period you have sent too many messages" }
        });
        if (e?.Message == null)
            return false;

        if (sender != null)
            await sender.SendTextMessageAsync(e.Message.From?.Id, languageList4, ChatType.Private, default,
                ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e.Message.From?.Username);
        return false;
    }

    private static async Task<bool> AssocSend3Async(TelegramBotAbstract? sender, MessageEventArgs? e, bool dry, Message replyTo,
        long? messageFromIdEntity)
    {
        var languageList2 = new Language(new Dictionary<string, string?>
            {
                { "it", "Vuoi mettere in coda o scegliere una data d'invio?" },
                { "en", "You want to add it to the queue or select a date to send the message?" }
            }
        );

        var opt1 = new Language(new Dictionary<string, string?>
            { { "it", "Metti in coda" }, { "en", "Place in queue" } });
        var opt2 = new Language(
            new Dictionary<string, string?> { { "it", "Scegli la data" }, { "en", "Choose the date" } });
        var options = new List<List<Language>>
        {
            new() { opt1, opt2 }
        };

        if (e?.Message != null)
            return await AssocSend2Async(sender, e, dry, languageList2, options, replyTo, messageFromIdEntity);

        var lang4 = new Language(new Dictionary<string, string?>
        {
            { "en", "The message has not been enqueued" },
            { "it", "Il messaggio non è stato accodato" }
        });
        if (sender == null)
            return false;

        if (e?.Message != null)
            await sender.SendTextMessageAsync(e.Message.From?.Id, lang4,
                ChatType.Private, e.Message.From?.LanguageCode,
                ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                e.Message.From?.Username);
        return false;
    }

    private static async Task<bool> AssocSend2Async(TelegramBotAbstract? sender, MessageEventArgs e, bool dry,
        Language languageList2, IReadOnlyList<List<Language>> options, Message replyTo, long? messageFromIdEntity)
    {
        var fromId = e.Message.From?.Id;
        var queueOrPreciseDate = await AskUser.AskBetweenRangeAsync(fromId,
            languageList2, sender, e.Message.From?.LanguageCode, options, e.Message.From?.Username);

        DateTime? sentDate = null;

        if (!Language.EqualsLang(queueOrPreciseDate, options[0][0], e.Message.From?.LanguageCode))
        {
            sentDate = DateTime.Parse(await AskUser.AskAsync(fromId,
                new L("it", "Inserisci una data in formato AAAA-MM-DD HH:mm", "en",
                    "Insert a date AAAA-MM-DD HH:mm"),
                sender, e.Message.From?.LanguageCode, e.Message.From?.Username) ?? "");

            if (AssocGeneric.CheckIfDateTimeIsValid(sentDate) == false)
            {
                var lang4 = new Language(new Dictionary<string, string?>
                {
                    { "en", "The date you choose is invalid!" },
                    { "it", "La data che hai scelto non è valida!" }
                });
                if (sender != null)
                    await sender.SendTextMessageAsync(fromId, lang4,
                        ChatType.Private, e.Message.From?.LanguageCode,
                        ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                        e.Message.From?.Username);
                return false;
            }
        }

        var idChatsSentInto = Channels.Assoc.GetChannels().ToList();
        if (fromId != null)
        {
            idChatsSentInto.Add(fromId.Value);
        }

        const ChatType chatTypeSendInto = ChatType.Group;
        if (!dry) 
            await QueueMessages(sender, e, replyTo, messageFromIdEntity, idChatsSentInto, sentDate, chatTypeSendInto);
        
        return await NotifySuccessQueue(sender, e, fromId);
    }

    private static async Task<bool> NotifySuccessQueue(TelegramBotAbstract? sender, MessageEventArgs e, long? fromId)
    {
        var lang3 = new Language(new Dictionary<string, string?>
        {
            { "en", "The message has been submitted correctly" },
            { "it", "Il messaggio è stato inviato correttamente" }
        });
        if (sender == null)
            return true;

        await sender.SendTextMessageAsync(fromId, lang3,
            ChatType.Private, e.Message.From?.LanguageCode,
            ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
            e.Message.From?.Username);


        return true;
    }

    private static async Task QueueMessages(TelegramBotAbstract? sender, MessageEventArgs e, Message replyTo,
        long? messageFromIdEntity, List<long> idChatsSentInto, DateTime? sentDate, ChatType chatTypeSendInto)
    {
        foreach (var idChat in idChatsSentInto)
        {
            await QueueMessage(sender, e, replyTo, messageFromIdEntity, sentDate, idChat, chatTypeSendInto);
        }
    }

    private static async Task QueueMessage(TelegramBotAbstract? sender, MessageEventArgs e, Message replyTo,
        long? messageFromIdEntity, DateTime? sentDate, long idChat, ChatType chatTypeSendInto)
    {
        sentDate ??= DateTime.Now.AddMinutes(-1);
        
        var successQueue = SendMessage.PlaceMessageInQueue(replyTo,
            new DateTimeSchedule(sentDate, true),
            e.Message.From?.Id,
            messageFromIdEntity, idChat, sender, chatTypeSendInto);

        switch (successQueue)
        {
            case SuccessQueue.INVALID_ID_TO_DB:
                break;

            case SuccessQueue.INVALID_OBJECT:
            {
                await AssocGeneric.Assoc_ObjectToSendNotValid(sender, e);
                return;
            }

            case SuccessQueue.SUCCESS:
                break;

            case SuccessQueue.DATE_INVALID:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        if (successQueue == SuccessQueue.SUCCESS) 
            return;

        await NotifyUtil.NotifyOwnerWithLog2(
            new Exception("Success queue is " + successQueue + " while trying to send a message!"),
            sender, EventArgsContainer.Get(e));
    }
}
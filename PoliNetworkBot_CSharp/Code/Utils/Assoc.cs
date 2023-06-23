#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;
using PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class Assoc
{
    private static async Task<long?> GetIdEntityFromPersonAsync(long? id, Language? question,
        TelegramBotAbstract? sender, string? lang, string? username)
    {
        const string? q =
            "SELECT Entities.id, Entities.name FROM (SELECT * FROM PeopleInEntities WHERE id_person = @idp) AS T1, Entities WHERE T1.id_entity = Entities.id";
        var r = Database.ExecuteSelect(q, sender?.DbConfig,
            new Dictionary<string, object?> { { "@idp", id } });
        if (r == null || r.Rows.Count == 0) return null;

        if (r.Rows.Count == 1) return Convert.ToInt64(r.Rows[0].ItemArray[0]);

        var l = new Dictionary<string, long?>();
        foreach (DataRow dr in r.Rows)
        {
            var s = dr.ItemArray[1]?.ToString();
            if (!string.IsNullOrEmpty(s)) l[s] = Convert.ToInt64(dr.ItemArray[0]);
        }

        var l3 = l.Keys.Select(
            l2 => new Language(
                new Dictionary<string, string?>
                {
                    { "en", l2 }
                })
        ).ToList();

        var options = KeyboardMarkup.ArrayToMatrixString(l3);
        var r2 = await AskUser.AskBetweenRangeAsync(id, question, sender, lang,
            options, username);

        return r2 != null ? l[r2] : null;
    }

    public static async Task<bool> Assoc_SendAsync(TelegramBotAbstract? sender, MessageEventArgs? e, bool dry = false)
    {
        try
        {
            var replyTo = e?.Message.ReplyToMessage;

            if (replyTo == null)
            {
                await Assoc_ObjectToSendNotValid(sender, e);
                return false;
            }

            var languageList = new Language(new Dictionary<string, string?>
            {
                { "it", "Scegli l'entità per il quale stai componendo il messaggio" },
                { "en", "Choose the entity you are writing this message for" }
            });

            var messageFromIdEntity = await GetIdEntityFromPersonAsync(e?.Message.From?.Id, languageList,
                sender, e?.Message.From?.LanguageCode, e?.Message.From?.Username);

            if (messageFromIdEntity == null)
            {
                await EntityNotFoundAsync(sender, e);
                return false;
            }

            var hasThisEntityAlreadyReachedItsLimit =
                CheckIfEntityReachedItsMaxLimit(messageFromIdEntity.Value, sender, true);
            if (hasThisEntityAlreadyReachedItsLimit != null && hasThisEntityAlreadyReachedItsLimit.Value)
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
            {
                var queueOrPreciseDate = await AskUser.AskBetweenRangeAsync(e.Message.From?.Id,
                    languageList2, sender, e.Message.From?.LanguageCode, options, e.Message.From?.Username);

                var sentDate = new DateTime();

                if (!Language.EqualsLang(queueOrPreciseDate, options[0][0], e.Message.From?.LanguageCode))
                {
                    string? dateTimeString = null;
                    var parseSuccess = false;
                    while (dateTimeString == null && !parseSuccess)
                        dateTimeString = await AskUser.AskAsync(e.Message.From?.Id,
                            new L("it", "Inserisci una data in formato AAAA-MM-DD HH:mm", "en",
                                "Insert a date AAAA-MM-DD HH:mm"),
                            sender, e.Message.From?.LanguageCode, e.Message.From?.Username);

                    parseSuccess = DateTime.TryParseExact(
                        dateTimeString,
                        "yyyy-MM-dd HH:mm",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out sentDate);
                    if (!parseSuccess || CheckIfDateTimeIsValid(sentDate) == false)
                    {
                        var lang4 = new Language(new Dictionary<string, string?>
                        {
                            { "en", "The date you choose is invalid!" },
                            { "it", "La data che hai scelto non è valida!" }
                        });
                        if (sender != null)
                            await sender.SendTextMessageAsync(e.Message.From?.Id, lang4,
                                ChatType.Private, e.Message.From?.LanguageCode,
                                ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                                e.Message.From?.Username);
                        return false;
                    }
                }

                var idChatsSentInto = Channels.Assoc.GetChannels();
                //const long idChatSentInto = -432645805;
                const ChatType chatTypeSendInto = ChatType.Group;
                if (!dry)
                    foreach (var idChat in idChatsSentInto)
                    {
                        if (sentDate == null) return false;
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
                                await Assoc_ObjectToSendNotValid(sender, e);
                                return false;
                            }

                            case SuccessQueue.SUCCESS:
                                break;

                            case SuccessQueue.DATE_INVALID:
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        if (successQueue == SuccessQueue.SUCCESS)
                            continue;

                        await NotifyUtil.NotifyOwnerWithLog2(
                            new Exception("Success queue is " + successQueue + " while trying to send a message!"),
                            sender, EventArgsContainer.Get(e));

                        return false;
                    }
            }

            var lang3 = new Language(new Dictionary<string, string?>
            {
                { "en", "The message has been submitted correctly" },
                { "it", "Il messaggio è stato inviato correttamente" }
            });
            if (sender == null) return true;
            if (e?.Message != null)
                await sender.SendTextMessageAsync(e.Message.From?.Id, lang3,
                    ChatType.Private, e.Message.From?.LanguageCode,
                    ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                    e.Message.From?.Username);

            return true;
        }
        catch (Exception? ex)
        {
            await NotifyUtil.NotifyOwnerWithLog2(ex, sender, EventArgsContainer.Get(e));
            return false;
        }
    }

    private static bool CheckIfDateTimeIsValid(DateTime? sdt)
    {
        if (sdt == null)
            return false;

        if (sdt.Value.Year < DateTime.Now.Year)
            return false;

        if (sdt.Value.Year == DateTime.Now.Year && sdt.Value.Month < DateTime.Now.Month)
            return false;

        return sdt.Value.Year != DateTime.Now.Year || sdt.Value.Month != DateTime.Now.Month ||
               sdt.Value.Day >= DateTime.Now.Day;
    }

    private static async Task Assoc_ObjectToSendNotValid(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var lang2 = new Language(new Dictionary<string, string?>
        {
            { "en", "You have to attach something! (A photo, for example)" },
            { "it", "Devi allegare qualcosa! (Una foto, ad esempio)" }
        });
        if (sender != null)
            if (e?.Message != null)
                await sender.SendTextMessageAsync(e.Message.From?.Id,
                    lang2,
                    ChatType.Private, e.Message.From?.LanguageCode,
                    ParseMode.Html,
                    new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e.Message.From?.Username);
    }

    internal static async Task<bool> Assoc_Publish(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        try
        {
            return await MessageDb.CheckMessagesToSend(true, sender, e);
        }
        catch (Exception? e1)
        {
            await NotifyUtil.NotifyOwnerWithLog2(e1, sender, EventArgsContainer.Get(e));
            return false;
        }
    }

    internal static async Task<bool> Assoc_Delete(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var messages = await GetMessagesInQueueAsync(sender, e, false);

        if (messages == null)
            return false;

        if (messages.Count == 1) return await DeleteMessageFromQueueAsync(messages, 0, sender, e);

        var count = 0;
        foreach (DataRow m in messages)
        {
            _ = await SendMessageAssocToUserAsync(m, sender, e, true, count);
            count++;
        }

        var dict = new Dictionary<string, string?>
        {
            { "it", "Quale vuoi rimuovere dalla coda?" },
            { "en", "Which one do you want to remove from queue?" }
        };
        var question = new Language(dict);
        var list = new List<Language>();
        for (var i = 0; i < count; i++)
            list.Add(new Language(new Dictionary<string, string?>
            {
                { "en", i.ToString() }
            }));

        var options = KeyboardMarkup.ArrayToMatrixString(list);
        var options2 = new List<Language>
        {
            new(new Dictionary<string, string?>
            {
                { "it", "Annulla" },
                { "en", "Cancel" }
            })
        };
        options?.Insert(0, options2);
        var r1 = await AskUser.AskBetweenRangeAsync(e?.Message.From?.Id, question, sender,
            e?.Message.From?.LanguageCode, options, e?.Message.From?.Username);

        long? index = null;
        try
        {
            index = Convert.ToInt64(r1);
        }
        catch
        {
            // ignored
        }

        if (index == null) return true;

        return await DeleteMessageFromQueueAsync(messages, index.Value, sender, e);
    }

    private static async Task<bool> DeleteMessageFromQueueAsync(DataRowCollection messages, long v,
        TelegramBotAbstract? telegramBotAbstract, MessageEventArgs? e)
    {
        var r = DeleteMessageFromQueueSingle(messages, v, telegramBotAbstract);
        if (r)
        {
            var text1 = new Language(new Dictionary<string, string?>
            {
                { "it", "Messaggio [" + v + "] eliminato con successo" },
                { "en", "Message [" + v + "] deleted successfully" }
            });
            if (telegramBotAbstract == null) return r;
            if (e == null) return r;
            await telegramBotAbstract.SendTextMessageAsync(e.Message.From?.Id, text1,
                e.Message.Chat.Type, e.Message.From?.LanguageCode, ParseMode.Html, null,
                e.Message.From?.Username,
                null, true);
        }
        else
        {
            var text2 = new Language(new Dictionary<string, string?>
            {
                { "it", "Messaggio [" + v + "] non eliminato, errore" },
                { "en", "Message [" + v + "] not deleted, error" }
            });
            if (telegramBotAbstract == null)
                return r;

            if (e?.Message != null)
                await telegramBotAbstract.SendTextMessageAsync(e.Message.From?.Id, text2,
                    e.Message.Chat.Type, e.Message.From?.LanguageCode, ParseMode.Html, null,
                    e.Message.From?.Username,
                    null, true);
        }

        return r;
    }

    private static bool DeleteMessageFromQueueSingle(DataRowCollection messages, long v, TelegramBotAbstract? sender)
    {
        const string? q = "DELETE FROM Messages WHERE ID = @id";
        DataRow? dr = null;

        try
        {
            dr = messages[(int)v];
        }
        catch
        {
            // ignored
        }

        if (dr == null)
            return false;

        var id = Convert.ToInt64(dr["id"]);

        var args = new Dictionary<string, object?>
        {
            { "@id", id }
        };
        Database.Execute(q, sender?.DbConfig, args);

        return true;
    }

    internal static string? GetNameOfEntityFromItsId(long value, TelegramBotAbstract? sender)
    {
        var q = "SELECT name FROM Entities WHERE id = " + value;
        var r = Database.ExecuteSelect(q, sender?.DbConfig);
        if (r == null || r.Rows.Count == 0)
            return null;

        return r.Rows[0].ItemArray[0]?.ToString();
    }

    internal static async Task<bool> Assoc_ReadAll(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        return await Assoc_Read(sender, e, true);
    }

    private static async Task EntityNotFoundAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var languageList3 = new Language(new Dictionary<string, string?>
        {
            {
                "en",
                "We can't find the entity you want to post from. Are you sure you are a member of some entity allowed to post?"
            },
            {
                "it",
                "Non riusciamo a trovare l'organizzazione per la quale vuoi postare. Sei sicuro di essere un membro di qualche organizzazione autorizzata a postare?"
            }
        });
        if (sender != null)
            if (e?.Message.From != null)
                await sender.SendTextMessageAsync(e.Message.From.Id, languageList3, ChatType.Private, default,
                    ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e.Message.From.Username);
    }

    internal static async Task<bool> Assoc_Read(TelegramBotAbstract? sender, MessageEventArgs? e, bool allAssoc)
    {
        var messages = await GetMessagesInQueueAsync(sender, e, allAssoc);

        if (messages == null)
            return false;

        var count = 0;
        foreach (DataRow m in messages)
        {
            _ = await SendMessageAssocToUserAsync(m, sender, e, true, count);
            count++;
        }

        return true;
    }

    private static async Task<DataRowCollection?> GetMessagesInQueueAsync(TelegramBotAbstract? sender,
        MessageEventArgs? e, bool allAssoc)
    {
        var conditionOnIdEntity = "";
        Dictionary<string, object?>? dict2 = null;

        if (allAssoc == false)
            if (e?.Message != null)
            {
                var messageFromIdEntity = await GetIdEntityFromPersonAsync(e.Message.From?.Id, null,
                    sender, e.Message.From?.LanguageCode, e.Message.From?.Username);

                if (messageFromIdEntity == null)
                {
                    await EntityNotFoundAsync(sender, e);
                    return null;
                }

                conditionOnIdEntity = "from_id_entity = @id AND";
                dict2 = new Dictionary<string, object?> { { "@id", messageFromIdEntity.Value } };
            }

        var q = "SELECT * FROM Messages WHERE " + conditionOnIdEntity + " has_been_sent = FALSE";
        var r = Database.ExecuteSelect(q, sender?.DbConfig, dict2);
        if (r != null && r.Rows.Count != 0) return r.Rows;
        var text = new Language(new Dictionary<string, string?>
        {
            { "it", "Non ci sono messaggi in coda!" },
            { "en", "There are no message in the queue!" }
        });
        if (e?.Message != null)
            await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id, e.Message.From?.LanguageCode,
                e.Message.From?.Username,
                text, ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));

        return null;
    }

    private static async Task<MessageSentResult?> SendMessageAssocToUserAsync(DataRow? m, TelegramBotAbstract? sender,
        MessageEventArgs? e, bool extraInfo, int count)
    {
        return m == null
            ? new MessageSentResult(false, null, null)
            : e?.Message != null
                ? await MessageDb.SendMessageFromDataRow(m, e.Message.From?.Id, ChatType.Private, extraInfo,
                    sender,
                    count)
                : null;
    }

    private static bool? CheckIfEntityReachedItsMaxLimit(long messageFromIdEntity, TelegramBotAbstract? sender,
        bool tempDisable)
    {
        const int polinetworkEntity = 2;

        return messageFromIdEntity == polinetworkEntity || tempDisable
            ? false
            : CheckIfEntityReachedItsMaxLimit2(messageFromIdEntity, sender);
    }

    private static bool? CheckIfEntityReachedItsMaxLimit2(long messageFromIdEntity, TelegramBotAbstract? sender)
    {
        var q = "SELECT COUNT(*) " +
                "FROM Messages " +
                "WHERE Messages.from_id_entity = " + messageFromIdEntity +
                " AND ((NOW() - interval 30 day) <= (Messages.sent_date)) ";

        if (sender == null) return null;
        var dt = Database.ExecuteSelect(q, sender.DbConfig);

        if (dt?.Rows == null)
            return null;

        long? count = null;

        try
        {
            count = Convert.ToInt64(dt.Rows[0].ItemArray[0]);
        }
        catch
        {
            // ignored
        }

        if (count == null)
            return null;

        return count.Value >= 2;
    }

    /// <summary>
    ///     Looks up the associations list from the polimi website
    /// </summary>
    /// <returns>a list with the name of the associations</returns>
    private static async Task<List<string>?> GetAssocList()
    {
        const string url = "https://www.polimi.it/studenti-iscritti/rappresentanti-e-associazioni/";
        var webReply = await Web.DownloadHtmlAsync(url);
        if (!webReply.IsValid()) return null;

        // parse the html document
        var doc = new HtmlDocument();
        doc.LoadHtml(webReply.GetData());

        // select the second ce-bodytext in the page, and of that div select the first child, which is the dotted ul
        // list with the associations name
        var elementsByTagAndClassName = HtmlUtil.GetElementsByTagAndClassName(doc.DocumentNode, "div", "ce-bodytext");
        var assocUl = elementsByTagAndClassName?[1]
            ?.ChildNodes[0];
        // map each li element to its inner text, from which only the name should be taken
        return assocUl?.ChildNodes.Select(li => li.InnerText.Split('[')[0].Replace("&nbsp;", " ").Trim()).ToList();
    }

    /// <summary>
    ///     Schedules a message to be allowed if nobody objects in the next {timeSpan}
    /// </summary>
    /// <param name="e"></param>
    /// <param name="sender"></param>
    /// <param name="timeSpan"></param>
    public static async Task AllowMessage(MessageEventArgs? e, TelegramBotAbstract? sender, TimeSpan timeSpan)
    {
        string? message = null;

        if (e is { Message: not null } && (e.Message.ReplyToMessage == null ||
                                           (string.IsNullOrEmpty(e.Message.ReplyToMessage.Text) &&
                                            string.IsNullOrEmpty(e.Message.ReplyToMessage.Caption))))
        {
            // the command is being called without a reply, ask for the message:
            var question = new Language(new Dictionary<string, string?>
            {
                { "en", "Type the message you want to allow" },
                { "it", "Scrivi il messaggio che vuoi approvare" }
            });
            message = await AskUser.AskAsync(e.Message.From?.Id, question, sender, e.Message.From?.LanguageCode,
                e.Message.From?.Username, true);
        }
        else
        {
            // the message which got replied to is used for the text
            if (e?.Message?.ReplyToMessage != null)
                message = e.Message.ReplyToMessage.Text ?? e.Message.ReplyToMessage?.Caption;
        }

        var groupsQuestion = new Language(new Dictionary<string, string?>
        {
            { "en", "In which groups do you want to allow it?" },
            { "it", "In quale gruppo le vuoi approvare?" }
        });
        var groups = await AskUser.AskAsync(e?.Message?.From?.Id, groupsQuestion, sender,
            e?.Message?.From?.LanguageCode,
            e?.Message?.From?.Username, true);

        var typeQuestion = new Language(new Dictionary<string, string?>
        {
            { "en", "What type of message is it? (e.g Promotional message, Invite to an event, ecc.)" },
            { "it", "Che tipo di messagio è? (ad esempio Messaggio promozionale, Invito ad un evento, ecc.)" }
        });
        var messageType = await AskUser.AskAsync(e?.Message?.From?.Id, typeQuestion, sender,
            e?.Message?.From?.LanguageCode,
            e?.Message?.From?.Username, true);

        var assocList = await GetAssocList();
        var assocQuestion = new Language(new Dictionary<string, string?>
        {
            { "en", "For what association? Select Departmental Club if it's an approved departmental club" },
            {
                "it",
                "Per che associazione? Seleziona Club Dipartimentale se si tratta di un club dipartimentale approvato"
            }
        });

        var depClub = new Language(new Dictionary<string, string?>
        {
            { "en", "Departmental Club" },
            { "it", "Club Dipartimentale" }
        });

        if (assocList == null) return;

        var assocAndClub = assocList.Select(a =>
            new Language(
                new Dictionary<string, string?>
                {
                    { "uni", a }
                })
        ).ToList();

        assocAndClub.Add(depClub);

        var options = KeyboardMarkup.ArrayToMatrixString(assocAndClub);

        if (e?.Message?.From == null) return;

        var assocOrClub = await AskUser.AskBetweenRangeAsync(e.Message.From.Id, assocQuestion,
            lang: e.Message.From.LanguageCode,
            options: options, username: e.Message.From.Username, sendMessageConfirmationChoice: true,
            sender: sender);

        if (assocOrClub is "Departmental Club" or "Club Dipartimentale")
        {
            depClub = new Language(new Dictionary<string, string?>
            {
                { "en", "What is the name of the departimental club?" },
                { "it", "Qual è il nome del club dipartimentale?" }
            });
            assocOrClub = await AskUser.AskAsync(e.Message.From.Id, depClub, sender,
                e.Message.From.LanguageCode,
                e.Message.From.Username, true);
        }

        var permittedSpamMessage =
            await NotifyUtil.NotifyAllowedMessage(sender, EventArgsContainer.Get(e), message, groups,
                messageType, assocOrClub);

        var privateConfirmationMessage = new Language(new Dictionary<string, string?>
        {
            { "uni", permittedSpamMessage }
        });

        await SendMessage.SendMessageInPrivate(sender,
            e.Message.From.Id, "uni",
            null, privateConfirmationMessage,
            ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));

        var splitMessage = false;

        if (message is { Length: > 4000 })
        {
            permittedSpamMessage = NotifyUtil.CreatePermittedSpamMessage(EventArgsContainer.Get(e),
                "#### MESSAGE IS TOO LONG! Read above this message ####", groups, messageType,
                assocOrClub);
            splitMessage = true;
        }

        await HandleVetoAnd4HoursAsync(message, e, sender, permittedSpamMessage, splitMessage, timeSpan);
    }

    private static async void NotifyMessageIsAllowed(MessageEventArgs? eventArgs, TelegramBotAbstract? sender,
        string? message)
    {
        try
        {
            if (!MessagesStore.MessageIsAllowed(message))
            {
                Logger.Logger.WriteLine("Allowed message: Message Is Allowed resulted in false");
                return;
            }

            var privateText = new Language(new Dictionary<string, string?>
            {
                { "en", "The message is allowed to be sent" },
                { "it", "Il messaggio è approvato per l'invio" }
            });

            if (sender != null)
                await sender.SendTextMessageAsync(
                    eventArgs?.Message.From?.Id,
                    privateText, ChatType.Private,
                    eventArgs?.Message.From?.LanguageCode, ParseMode.Html, null, null,
                    eventArgs?.Message.MessageId);
        }
        catch (Exception? ex)
        {
            Logger.Logger.WriteLine(ex, LogSeverityLevel.ERROR);
        }
    }

    private static async void RemoveVetoButton(CallbackAssocVetoData assocVetoData, TelegramBotAbstract? sender)
    {
        try
        {
            if (assocVetoData.MessageSent?.GetMessage() is not Message m1 ||
                assocVetoData.MessageSent.GetMessageId() == null || assocVetoData.Modified) return;
            if (sender != null)
                await sender.EditMessageTextAsync(m1.Chat.Id,
                    int.Parse(assocVetoData.MessageSent?.GetMessageId()?.ToString() ?? "0"),
                    assocVetoData.MessageWithMetadata, ParseMode.Html);
        }
        catch (Exception? ex)
        {
            Logger.Logger.WriteLine(ex, LogSeverityLevel.ERROR);
        }
    }

    private static async Task HandleVetoAnd4HoursAsync(string? message, MessageEventArgs? messageEventArgs,
        TelegramBotAbstract? sender, string? permittedSpamMessage, bool splitMessage, TimeSpan timeSpan)
    {
        MessagesStore.AddMessage(message, MessageAllowedStatusEnum.PENDING, timeSpan);

        var allowedTime = MessagesStore.GetAllowedTime(message);
        if (allowedTime.HasValue)
        {
            var allowedNotificationTimeLater = allowedTime.Value - DateTime.Now + new TimeSpan(0, 1, 0);
            _ = TimeUtils.ExecuteAtLaterTime(allowedNotificationTimeLater,
                () => NotifyMessageIsAllowed(messageEventArgs, sender, message));
            permittedSpamMessage +=
                "\nAllowed at time: " + allowedTime.Value.ToString("dd'-'MMM'-'yyyy' 'HH':'mm':'ss");
            Logger.Logger.WriteLine("Assoc, notification in: " + allowedNotificationTimeLater.Seconds + " seconds");
        }

        long? replyTo = null;

        if (splitMessage)
            if (sender != null)
            {
                var m = await sender.SendTextMessageAsync(GroupsConstants.PermittedSpamGroup, new Language(
                    new Dictionary<string, string?>
                    {
                        { "en", message }
                    }), ChatType.Group, "en", ParseMode.Html, null, null);
                replyTo = m?.GetMessageId();
            }

        var councilMessage = new Language(
            new Dictionary<string, string?>
            {
                { "uni", permittedSpamMessage }
            });

        List<CallbackOption> options = new()
        {
            new CallbackOption("❌ Veto", 0, null)
        };

        var assocVetoData = new CallbackAssocVetoData(options, VetoCallbackButton, message, messageEventArgs,
            permittedSpamMessage);

        await CallbackUtils.CallbackUtils.SendMessageWithCallbackQueryAsync(assocVetoData,
            GroupsConstants.ConsiglioDegliAdminRiservato,
            councilMessage, sender, ChatType.Group, "uni", null, true, replyTo);

        _ = TimeUtils.ExecuteAtLaterTime(new TimeSpan(48, 0, 0), () => RemoveVetoButton(assocVetoData, sender));
    }

    private static async void VetoCallbackButton(CallbackGenericData callbackGenericData)
    {
        try
        {
            try
            {
                if (callbackGenericData.CallBackQueryFromTelegram != null && !Permissions.CheckPermissions(
                        Permission.HEAD_ADMIN,
                        callbackGenericData.CallBackQueryFromTelegram.From))
                {
                    if (callbackGenericData.Bot != null)
                        await callbackGenericData.Bot.AnswerCallbackQueryAsync(
                            callbackGenericData.CallBackQueryFromTelegram.Id,
                            "Veto Denied! You need to be Head Admin!");
                    return;
                }

                if (callbackGenericData is not CallbackAssocVetoData assocVetoData)
                    throw new Exception("callbackGenericData needs to be an instance of CallbackAssocVetoData");

                if (!MessagesStore.CanBeVetoed(assocVetoData.Message))
                {
                    if (callbackGenericData.Bot == null) return;
                    if (callbackGenericData.CallBackQueryFromTelegram != null)
                        await callbackGenericData.Bot.AnswerCallbackQueryAsync(
                            callbackGenericData.CallBackQueryFromTelegram.Id,
                            "Veto Denied! The 48h time frame has expired.");
                    return;
                }

                if (assocVetoData.CallBackQueryFromTelegram is { Message: null })
                    throw new Exception("callBackQueryFromTelegram is null on callbackButton");

                var vetoInTime = MessagesStore.VetoMessage(assocVetoData.Message);

                try
                {
                    if (assocVetoData.CallBackQueryFromTelegram?.Message != null)
                        if (callbackGenericData.CallBackQueryFromTelegram != null)
                        {
                            var vetoInStWindow = vetoInTime ? "\nVeto in 1st window" : "\nVeto in 2nd window";
                            var newMessage = assocVetoData.CallBackQueryFromTelegram.Message.Text + "\n\n" +
                                             "<b>VETO</b> by " + UserbotPeer.GetHtmlStringWithUserLink(
                                                 callbackGenericData.CallBackQueryFromTelegram.From
                                             ) + vetoInStWindow;

                            if (callbackGenericData.Bot != null)
                                await callbackGenericData.Bot.EditMessageTextAsync(
                                    assocVetoData.CallBackQueryFromTelegram.Message.Chat.Id,
                                    assocVetoData.CallBackQueryFromTelegram.Message.MessageId, newMessage,
                                    ParseMode.Html);
                        }

                    assocVetoData.OnCallback();

                    var privateText = new Language(new Dictionary<string, string?>
                    {
                        { "en", "The message has received a veto" },
                        { "it", "Il messaggio ha ricevuto un veto" }
                    });

                    if (callbackGenericData.Bot != null)
                        await callbackGenericData.Bot.SendTextMessageAsync(
                            assocVetoData.MessageEventArgs?.Message.From?.Id,
                            privateText, ChatType.Private,
                            assocVetoData.MessageEventArgs?.Message.From?.LanguageCode, ParseMode.Html, null, null,
                            assocVetoData.MessageEventArgs?.Message.MessageId);
                }
                catch (Exception? exc)
                {
                    var ev = EventArgsContainer.Get(callbackGenericData);
                    await NotifyUtil.NotifyOwnersWithLog(exc, assocVetoData.Bot, null, ev);
                    await NotifyUtil.NotifyOwnersWithLog(new Exception("COUNCIL VETO ERROR ABOVE, DO NOT IGNORE!"),
                        assocVetoData.Bot, null, ev);
                }
            }
            catch (Exception? e)
            {
                await NotifyUtil.NotifyOwnersWithLog(e, callbackGenericData.Bot, null,
                    EventArgsContainer.Get(callbackGenericData));
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }
}
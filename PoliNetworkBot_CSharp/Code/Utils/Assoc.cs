﻿#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class Assoc
    {
        private static async Task<long?> GetIdEntityFromPersonAsync(long id, Language question,
            TelegramBotAbstract sender, string lang, string username)
        {
            const string q =
                "SELECT Entities.id, Entities.name FROM (SELECT * FROM PeopleInEntities WHERE id_person = @idp) AS T1, Entities WHERE T1.id_entity = Entities.id";
            var r = SqLite.ExecuteSelect(q, new Dictionary<string, object> { { "@idp", id } });
            if (r == null || r.Rows.Count == 0) return null;

            if (r.Rows.Count == 1) return Convert.ToInt64(r.Rows[0].ItemArray[0]);

            var l = new Dictionary<string, long>();
            foreach (DataRow dr in r.Rows)
            {
                var s = dr.ItemArray[1].ToString();
                if (!string.IsNullOrEmpty(s)) l[s] = Convert.ToInt64(dr.ItemArray[0]);
            }

            var l3 = l.Keys.Select(
                l2 => new Language(
                    new Dictionary<string, string>
                    {
                        { "en", l2 }
                    })
            ).ToList();

            var options = KeyboardMarkup.ArrayToMatrixString(l3);
            var r2 = await AskUser.AskBetweenRangeAsync(id, question, sender, lang,
                options, username);

            return l[r2];
        }

        public static async Task<bool> Assoc_SendAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var replyTo = e.Message.ReplyToMessage;

            if (replyTo == null)
            {
                await Assoc_ObjectToSendNotValid(sender, e);
                return false;
            }

            var languageList = new Language(new Dictionary<string, string>
            {
                { "it", "Scegli l'entità per il quale stai componendo il messaggio" },
                { "en", "Choose the entity you are writing this message for" }
            });

            var messageFromIdEntity = await GetIdEntityFromPersonAsync(e.Message.From.Id, languageList,
                sender, e.Message.From.LanguageCode, e.Message.From.Username);

            if (messageFromIdEntity == null)
            {
                await EntityNotFoundAsync(sender, e);
                return false;
            }

            var hasThisEntityAlreadyReachedItsLimit = CheckIfEntityReachedItsMaxLimit(messageFromIdEntity.Value);
            if (hasThisEntityAlreadyReachedItsLimit != null && hasThisEntityAlreadyReachedItsLimit.Value)
            {
                var languageList4 = new Language(new Dictionary<string, string>
                {
                    { "it", "Spiacente! In questo periodo hai inviato troppi messaggi" },
                    { "en", "I'm sorry! In this period you have sent too many messages" }
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, languageList4, ChatType.Private, default,
                    ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e.Message.From.Username);
                return false;
            }

            var languageList2 = new Language(new Dictionary<string, string>
                {
                    { "it", "Data di pubblicazione?" },
                    { "en", "Date of publication?" }
                }
            );

            var opt1 = new Language(new Dictionary<string, string>
                { { "it", "Metti in coda" }, { "en", "Place in queue" } });
            var opt2 = new Language(
                new Dictionary<string, string> { { "it", "Scegli la data" }, { "en", "Choose the date" } });
            var options = new List<List<Language>>
            {
                new() { opt1, opt2 }
            };

            var queueOrPreciseDate = await AskUser.AskBetweenRangeAsync(e.Message.From.Id,
                languageList2, sender, e.Message.From.LanguageCode, options, e.Message.From.Username);

            Tuple<DateTimeSchedule, Exception, string> sentDate;
            if (Language.EqualsLang(queueOrPreciseDate, options[0][0], e.Message.From.LanguageCode))
            {
                sentDate = new Tuple<DateTimeSchedule, Exception, string>(new DateTimeSchedule(null, false), null,
                    null);
            }
            else
            {
                sentDate = await AskUser.AskDateAsync(e.Message.From.Id, e.Message.Text,
                    e.Message.From.LanguageCode, sender, e.Message.From.Username);

                if (sentDate.Item2 != null)
                {
                    await NotifyUtil.NotifyOwners(new ExceptionNumbered(sentDate.Item2), sender, e, 0, sentDate.Item3);
                    return false;
                }

                var sdt = sentDate.Item1.GetDate();
                if (CheckIfDateTimeIsValid(sdt) == false)
                {
                    var lang4 = new Language(new Dictionary<string, string>
                    {
                        { "en", "The date you choose is invalid!" },
                        { "it", "La data che hai scelto non è valida!" }
                    });
                    await sender.SendTextMessageAsync(e.Message.From.Id, lang4,
                        ChatType.Private, e.Message.From.LanguageCode,
                        ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                        e.Message.From.Username);
                    return false;
                }
            }

            const long idChatSentInto = Channels.PoliAssociazioni;
            //const long idChatSentInto = -432645805;
            var chatTypeSendInto = ChatType.Group;

            var successQueue = SendMessage.PlaceMessageInQueue(replyTo, sentDate.Item1, e.Message.From.Id,
                messageFromIdEntity, idChatSentInto, sender, chatTypeSendInto);

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

            if (successQueue != SuccessQueue.SUCCESS) return false;

            var lang3 = new Language(new Dictionary<string, string>
            {
                { "en", "The message has been submitted correctly" },
                { "it", "Il messaggio è stato inviato correttamente" }
            });
            await sender.SendTextMessageAsync(e.Message.From.Id, lang3,
                ChatType.Private, e.Message.From.LanguageCode,
                ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                e.Message.From.Username);
            return true;
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

        private static async Task Assoc_ObjectToSendNotValid(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var lang2 = new Language(new Dictionary<string, string>
            {
                { "en", "You have to attach something! (A photo, for example)" },
                { "it", "Devi allegare qualcosa! (Una foto, ad esempio)" }
            });
            await sender.SendTextMessageAsync(e.Message.From.Id,
                lang2,
                ChatType.Private, e.Message.From.LanguageCode,
                ParseMode.Html,
                new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e.Message.From.Username);
        }

        internal static async Task<bool> Assoc_Publish(TelegramBotAbstract sender, MessageEventArgs e)
        {
            try
            {
                return await MessageDb.CheckMessagesToSend(true, sender, e);
            }
            catch (Exception e1)
            {
                await NotifyUtil.NotifyOwners(e1, sender, e);
                return false;
            }
        }

        internal static async Task<bool> Assoc_Delete(TelegramBotAbstract sender, MessageEventArgs e)
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

            var dict = new Dictionary<string, string>
            {
                { "it", "Quale vuoi rimuovere dalla coda?" },
                { "en", "Which one do you want to remove from queue?" }
            };
            var question = new Language(dict);
            var list = new List<Language>();
            for (var i = 0; i < count; i++)
                list.Add(new Language(new Dictionary<string, string>
                {
                    { "en", i.ToString() }
                }));

            var options = KeyboardMarkup.ArrayToMatrixString(list);
            var options2 = new List<Language>
            {
                new(new Dictionary<string, string>
                {
                    { "it", "Annulla" },
                    { "en", "Cancel" }
                })
            };
            options.Insert(0, options2);
            var r1 = await AskUser.AskBetweenRangeAsync(e.Message.From.Id, question, sender,
                e.Message.From.LanguageCode, options, e.Message.From.Username);

            long? index = null;
            try
            {
                index = Convert.ToInt64(r1);
            }
            catch
            {
                ;
            }

            if (index == null) return true;

            return await DeleteMessageFromQueueAsync(messages, index.Value, sender, e);
        }

        private static async Task<bool> DeleteMessageFromQueueAsync(DataRowCollection messages, long v,
            TelegramBotAbstract telegramBotAbstract, MessageEventArgs e)
        {
            var r = DeleteMessageFromQueueSingle(messages, v);
            if (r)
            {
                var text1 = new Language(new Dictionary<string, string>
                {
                    { "it", "Messaggio [" + v + "] eliminato con successo" },
                    { "en", "Message [" + v + "] deleted successfully" }
                });
                await telegramBotAbstract.SendTextMessageAsync(e.Message.From.Id, text1,
                    e.Message.Chat.Type, e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username,
                    null, true);
            }
            else
            {
                var text2 = new Language(new Dictionary<string, string>
                {
                    { "it", "Messaggio [" + v + "] non eliminato, errore" },
                    { "en", "Message [" + v + "] not deleted, error" }
                });
                await telegramBotAbstract.SendTextMessageAsync(e.Message.From.Id, text2,
                    e.Message.Chat.Type, e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username,
                    null, true);
            }

            return r;
        }

        private static bool DeleteMessageFromQueueSingle(DataRowCollection messages, long v)
        {
            const string q = "DELETE FROM Messages WHERE ID = @id";
            DataRow dr = null;

            try
            {
                dr = messages[(int)v];
            }
            catch
            {
                ;
            }

            if (dr == null)
                return false;

            var id = Convert.ToInt64(dr["id"]);

            var args = new Dictionary<string, object>
            {
                { "@id", id }
            };
            SqLite.Execute(q, args);

            return true;
        }

        internal static string GetNameOfEntityFromItsId(long value)
        {
            var q = "SELECT name FROM Entities WHERE id = " + value;
            var r = SqLite.ExecuteSelect(q);
            if (r == null || r.Rows.Count == 0)
                return null;

            return r.Rows[0].ItemArray[0].ToString();
        }

        internal static async Task<bool> Assoc_ReadAll(TelegramBotAbstract sender, MessageEventArgs e)
        {
            return await Assoc_Read(sender, e, true);
        }

        private static async Task EntityNotFoundAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var languageList3 = new Language(new Dictionary<string, string>
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
            await sender.SendTextMessageAsync(e.Message.From.Id, languageList3, ChatType.Private, default,
                ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e.Message.From.Username);
        }

        internal static async Task<bool> Assoc_Read(TelegramBotAbstract sender, MessageEventArgs e, bool allAssoc)
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

        private static async Task<DataRowCollection> GetMessagesInQueueAsync(TelegramBotAbstract sender,
            MessageEventArgs e, bool allAssoc)
        {
            Language languageList = null;
            long? messageFromIdEntity = null;
            var conditionOnIdEntity = "";
            Dictionary<string, object> dict2 = null;

            if (allAssoc == false)
            {
                messageFromIdEntity = await GetIdEntityFromPersonAsync(e.Message.From.Id, languageList,
                    sender, e.Message.From.LanguageCode, e.Message.From.Username);

                if (messageFromIdEntity == null)
                {
                    await EntityNotFoundAsync(sender, e);
                    return null;
                }

                conditionOnIdEntity = "from_id_entity = @id AND";
                dict2 = new Dictionary<string, object> { { "@id", messageFromIdEntity.Value } };
            }

            var q = "SELECT * FROM Messages WHERE " + conditionOnIdEntity + " has_been_sent = FALSE";
            var r = SqLite.ExecuteSelect(q, dict2);
            if (r != null && r.Rows.Count != 0) return r.Rows;
            var text = new Language(new Dictionary<string, string>
            {
                { "it", "Non ci sono messaggi in coda!" },
                { "en", "There are no message in the queue!" }
            });
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id, e.Message.From.LanguageCode,
                e.Message.From.Username,
                text, ParseMode.Html, null);

            return null;
        }

        private static async Task<MessageSentResult> SendMessageAssocToUserAsync(DataRow m, TelegramBotAbstract sender,
            MessageEventArgs e, bool extraInfo, int count)
        {
            if (m == null) return new MessageSentResult(false, null, null);

            return await MessageDb.SendMessageFromDataRow(m, e.Message.From.Id, ChatType.Private, extraInfo, sender,
                count);
        }

        private static bool? CheckIfEntityReachedItsMaxLimit(long messageFromIdEntity)
        {
            switch (messageFromIdEntity)
            {
                case 13: //terna che ci sta aiutando col test (sarà tolto)
                case 2: //polinetwork
                {
                    return false;
                }
            }

            var q = "SELECT COUNT (*) " +
                    "FROM Messages " +
                    "WHERE Messages.from_id_entity = " + messageFromIdEntity +
                    " AND(julianday('now') - 30) <= julianday(Messages.sent_date) ";

            var dt = SqLite.ExecuteSelect(q);

            if (dt?.Rows == null)
                return null;

            long? count = null;

            try
            {
                count = Convert.ToInt64(dt.Rows[0].ItemArray[0]);
            }
            catch
            {
                ;
            }

            if (count == null)
                return null;

            return count.Value >= 2;
        }

        /// <summary>
        ///     Looks up the associations list from the polimi website
        /// </summary>
        /// <returns>a list with the name of the associations</returns>
        public static async Task<List<string>> GetAssocList()
        {
            var url = "https://www.polimi.it/studenti-iscritti/rappresentanti-e-associazioni/";
            var webReply = await Web.DownloadHtmlAsync(url, RequestCacheLevel.NoCacheNoStore);
            if (webReply == null || !webReply.IsValid()) return null;

            // parse the html document
            var doc = new HtmlDocument();
            doc.LoadHtml(webReply.GetData());

            // select the second ce-bodytext in the page, and of that div select the first child, wich is the dotted ul
            // list with the associations name 
            var assocUl = HtmlUtil.GetElementsByTagAndClassName(doc.DocumentNode, "div", "ce-bodytext")[1]
                .ChildNodes[0];
            // map each li element to its inner text, from which only the name should be taken
            return assocUl.ChildNodes.Select(li => li.InnerText.Split('[')[0].Replace("&nbsp;", " ").Trim()).ToList();
        }

        /// <summary>
        ///     Schedules a message to be allowed if nobody objects in the next 4 hours
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static async Task AllowMessage(MessageEventArgs e, TelegramBotAbstract sender)
        {
            string message;

            if (e.Message.ReplyToMessage == null || string.IsNullOrEmpty(e.Message.ReplyToMessage.Text) &&
                string.IsNullOrEmpty(e.Message.ReplyToMessage.Caption))
            {
                // the command is being called without a reply, ask for the message:
                var question = new Language(new Dictionary<string, string>
                {
                    { "en", "Type the message you want to allow" },
                    { "it", "Scrivi il messaggio che vuoi approvare" }
                });
                message = await AskUser.AskAsync(e.Message.From.Id, question, sender, e.Message.From.LanguageCode,
                    e.Message.From.Username, true);
            }
            else
            {
                // the message which got replied to is used for the text
                message = e.Message.ReplyToMessage.Text ?? e.Message.ReplyToMessage.Caption;
            }


            var groupsQuestion = new Language(new Dictionary<string, string>
            {
                { "en", "In which groups do you want to allow it?" },
                { "it", "In quale gruppo le vuoi approvare?" }
            });
            var groups = await AskUser.AskAsync(e.Message.From.Id, groupsQuestion, sender, e.Message.From.LanguageCode,
                e.Message.From.Username, true);


            var typeQuestion = new Language(new Dictionary<string, string>
            {
                { "en", "What type of message is it? (e.g Promotional message, Invite to an event, ecc.)" },
                { "it", "Che tipo di messagio è? (ad esempio Messaggio promozionale, Invito ad un evento, ecc.)" }
            });
            var messageType = await AskUser.AskAsync(e.Message.From.Id, typeQuestion, sender,
                e.Message.From.LanguageCode,
                e.Message.From.Username, true);


            var assocList = await GetAssocList();
            var assocQuestion = new Language(new Dictionary<string, string>
            {
                { "en", "For what association? Select Departmental Club if it's an approved departmental club" },
                { "it", "Per che associazione? Seleziona Club Dipartimentale se si tratta di un club dipartimentale approvato" }
            });
            
            var depClub = new Language(new Dictionary<string, string>
            {
                { "en", "Departmental Club" },
                { "it", "Club Dipartimentale" },
            });

            var assocAndClub = assocList.Select(a =>
                new Language(
                    new Dictionary<string, string>
                    {
                        {"uni", a},
                    })
            ).ToList();
            
            assocAndClub.Add(depClub);
            
            var options = KeyboardMarkup.ArrayToMatrixString(assocAndClub);
            
            var assocOrClub = await AskUser.AskBetweenRangeAsync(e.Message.From.Id, assocQuestion, lang: e.Message.From.LanguageCode,
                options: options, username: e.Message.From.Username, sendMessageConfirmationChoice: true,
                sender: sender);

            if (assocOrClub is "Departmental Club" or "Club Dipartimentale")
            {
                depClub = new Language(new Dictionary<string, string>
                {
                    { "en", "What is the name of the departimental club?" },
                    { "it", "Qual è il nome del club dipartimentale?" }
                });
                assocOrClub = await AskUser.AskAsync(e.Message.From.Id, depClub, sender,
                    e.Message.From.LanguageCode,
                    e.Message.From.Username, true);
            }

            var permittedSpamMessage =
                await NotifyUtil.NotifyAllowedMessage(sender, e, message, groups, messageType, assocOrClub);
            
            await SendMessage.SendMessageInPrivate(sender,
                e.Message.From.Id, permittedSpamMessage.Item2, null, permittedSpamMessage.Item1, ParseMode.Html, null);
            
            var splitMessage = false;
            
            if (message.Length > 4000)
            {
                var newMessage = NotifyUtil.CreatePermittedSpamMessage(e, "#### MESSAGE IS TOO LONG! Read above this message ####", groups, messageType, assocOrClub);
                permittedSpamMessage = new Tuple<Language, string>(
                    new Language(new Dictionary<string, string>
                    {
                        {"en", newMessage}
                    }), permittedSpamMessage.Item2);
                splitMessage = true;
            }
            
            await HandleVetoAnd4HoursAsync(message, e, sender, permittedSpamMessage, splitMessage);
        }

        private static async void NotifyMessageIsAllowed(MessageEventArgs eventArgs, TelegramBotAbstract sender, string message)
        {
            if (MessagesStore.MessageIsAllowed(message))
            {
                var privateText = new Language(new Dictionary<string, string>
                {
                    {"en", "The message is allowed to be sent"},
                    {"it", "Il messaggio è approvato per l'invio"}
                });

                await sender.SendTextMessageAsync(
                    eventArgs?.Message?.From?.Id,
                    privateText, ChatType.Private,
                    eventArgs.Message.From.LanguageCode, ParseMode.Html, null, null,
                    eventArgs.Message.MessageId);
            }
        }

        private static async void RemoveVetoButton(CallbackAssocVetoData assocVetoData, TelegramBotAbstract sender)
        {
            if (assocVetoData.MessageSent.GetMessage() is Message m1 &&
                assocVetoData.MessageSent.GetMessageID() != null && !assocVetoData.Modified)
            {
                await sender.EditMessageTextAsync(m1.Chat.Id,
                    int.Parse(assocVetoData.MessageSent?.GetMessageID()?.ToString() ?? "0"),
                    assocVetoData.MessageWithMetadata, ParseMode.Html);
            }
        }

        private static async Task HandleVetoAnd4HoursAsync(string message, MessageEventArgs messageEventArgs,
            TelegramBotAbstract sender, Tuple<Language, string> permittedSpamMessage, bool splitMessage)
        {

            var fourHours = new TimeSpan(4, 0, 0);

            MessagesStore.AddMessage(message,  MessageAllowedStatusEnum.PENDING, fourHours);

            _ = TimeUtils.ExecuteAtLaterTime(fourHours.Add(new TimeSpan(0, 1, 0)), () => NotifyMessageIsAllowed(messageEventArgs, sender, message));
            
            long? replyTo = null;
            
            if (splitMessage)
            {
                var m = await sender.SendTextMessageAsync(Data.Constants.Groups.PermittedSpamGroup, new Language(
                    new Dictionary<string, string>
                    {
                        {"en", message}
                    }), ChatType.Group, "en", ParseMode.Html, null, null);
                replyTo = m.GetMessageID();
            }
            
            List<CallbackOption> options = new() { 
                new CallbackOption("❌ Veto")
            };
            
            var assocVetoData = new CallbackAssocVetoData(options,  VetoCallbackButton, message, messageEventArgs, permittedSpamMessage.Item1.Select(permittedSpamMessage.Item2));
            
            await Utils.CallbackUtils.CallbackUtils.SendMessageWithCallbackQueryAsync(assocVetoData, Data.Constants.Groups.PermittedSpamGroup, 
            permittedSpamMessage.Item1, sender, ChatType.Group, permittedSpamMessage.Item2, null, true, replyTo);
            
            _ = TimeUtils.ExecuteAtLaterTime(new TimeSpan(48, 0,0), () => RemoveVetoButton(assocVetoData, sender));
            
        }

        private static async void VetoCallbackButton(CallbackGenericData callbackGenericData)
        {
            try
            {
                try
                {
                    if (!Permissions.CheckPermissions(Permission.HEAD_ADMIN,
                            callbackGenericData.CallBackQueryFromTelegram.From))
                    {
                        await callbackGenericData.Bot.AnswerCallbackQueryAsync(
                            callbackGenericData.CallBackQueryFromTelegram.Id,
                            "Veto Denied! You need to be Head Admin!");
                        return;
                    }

                    if (callbackGenericData is not CallbackAssocVetoData assocVetoData)
                        throw new Exception("callbackGenericData needs to be an instance of CallbackAssocVetoData");

                    if (!MessagesStore.CanBeVetoed(assocVetoData.message))
                    {
                        await callbackGenericData.Bot.AnswerCallbackQueryAsync(
                            callbackGenericData.CallBackQueryFromTelegram.Id,
                            "Veto Denied! The 48h time frame has expired.");
                        return;
                    }

                    if (assocVetoData.CallBackQueryFromTelegram.Message == null)
                        throw new Exception("callBackQueryFromTelegram is null on callbackButton");
                
                    var vetoInTime = MessagesStore.VetoMessage(assocVetoData.message);

                    try
                    {
                        var newMessage = assocVetoData.CallBackQueryFromTelegram.Message.Text + "\n\n" +
                                         "<b>VETO</b> by @"
                                         + callbackGenericData.CallBackQueryFromTelegram.From.Username + (vetoInTime ? "\nVeto in 1st window": "\nVeto in 2nd window");

                        await callbackGenericData.Bot.EditMessageTextAsync(
                            assocVetoData.CallBackQueryFromTelegram.Message.Chat.Id,
                            assocVetoData.CallBackQueryFromTelegram.Message.MessageId, newMessage, ParseMode.Html);

                        assocVetoData.OnCallback();

                        var privateText = new Language(new Dictionary<string, string>
                        {
                            {"en", "The message has received a veto"},
                            {"it", "Il messaggio ha ricevuto un veto"}
                        });

                        await callbackGenericData.Bot.SendTextMessageAsync(
                            assocVetoData.MessageEventArgs?.Message?.From?.Id,
                            privateText, ChatType.Private,
                            assocVetoData.MessageEventArgs?.Message?.From?.LanguageCode, ParseMode.Html, null, null,
                            assocVetoData.MessageEventArgs?.Message?.MessageId);
                    }

                    catch (Exception exc)
                    {
                        await NotifyUtil.NotifyOwners(exc, assocVetoData.Bot);
                        await NotifyUtil.NotifyOwners(new Exception("COUNCIL VETO ERROR ABOVE, DO NOT IGNORE!"),
                            assocVetoData.Bot);
                    }
                    
                }
                catch (Exception e)
                {
                    await NotifyUtil.NotifyOwners(e, callbackGenericData.Bot);
                }
            }
            catch (Exception)
            {
                ;
            }
        }
    }
}
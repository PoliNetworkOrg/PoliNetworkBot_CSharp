#region

using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class Assoc
    {
        private static async Task<int?> GetIdEntityFromPersonAsync(int id, Language question,
            TelegramBotAbstract sender, string lang, string username)
        {
            const string q =
                "SELECT Entities.id, Entities.name FROM (SELECT * FROM PeopleInEntities WHERE id_person = @idp) AS T1, Entities WHERE T1.id_entity = Entities.id";
            var r = SqLite.ExecuteSelect(q, new Dictionary<string, object> { { "@idp", id } });
            if (r == null || r.Rows.Count == 0) return null;

            if (r.Rows.Count == 1) return Convert.ToInt32(r.Rows[0].ItemArray[0]);

            var l = new Dictionary<string, int>();
            foreach (DataRow dr in r.Rows)
            {
                var s = dr.ItemArray[1].ToString();
                if (!string.IsNullOrEmpty(s)) l[s] = Convert.ToInt32(dr.ItemArray[0]);
            }

            var l3 = l.Keys.Select(
                l2 => new Language(
                    new Dictionary<string, string>
                    {
                        {"en", l2}
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
                {"it", "Scegli l'entità per il quale stai componendo il messaggio"},
                {"en", "Choose the entity you are writing this message for"}
            });

            var messageFromIdEntity = await GetIdEntityFromPersonAsync(e.Message.From.Id, languageList,
                sender, e.Message.From.LanguageCode, e.Message.From.Username);

            if (messageFromIdEntity == null)
            {
                await EntityNotFoundAsync(sender, e);
                return false;
            }

            bool? hasThisEntityAlreadyReachedItsLimit = CheckIfEntityReachedItsMaxLimit(messageFromIdEntity.Value);
            if (hasThisEntityAlreadyReachedItsLimit != null && hasThisEntityAlreadyReachedItsLimit.Value)
            {
                var languageList4 = new Language(new Dictionary<string, string>
                {
                    {"it", "Spiacente! In questo periodo hai inviato troppi messaggi"},
                    {"en", "I'm sorry! In this period you have sent too many messages"}
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, languageList4, ChatType.Private, default,
                    ParseMode.Default, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e.Message.From.Username);
                return false;
            }

            var languageList2 = new Language(new Dictionary<string, string>
                {
                    {"it", "Data di pubblicazione?"},
                    {"en", "Date of publication?"}
                }
            );

            var opt1 = new Language(new Dictionary<string, string> { { "it", "Metti in coda" }, { "en", "Place in queue" } });
            var opt2 = new Language(
                new Dictionary<string, string> { { "it", "Scegli la data" }, { "en", "Choose the date" } });
            var options = new List<List<Language>>
            {
                new List<Language> {opt1, opt2}
            };

            var queueOrPreciseDate = await AskUser.AskBetweenRangeAsync(e.Message.From.Id,
                languageList2, sender, e.Message.From.LanguageCode, options, e.Message.From.Username);

            Tuple<DateTimeSchedule, Exception, string> sentDate;
            if (Language.EqualsLang(queueOrPreciseDate, options[0][0], e.Message.From.LanguageCode))
                sentDate = new Tuple<DateTimeSchedule, Exception, string>( new DateTimeSchedule(null, false), null, null);
            else
            {
                sentDate = await DateTimeClass.AskDateAsync(e.Message.From.Id, e.Message.Text,
                    e.Message.From.LanguageCode, sender, e.Message.From.Username);

                if (sentDate.Item2 != null)
                {
                    await Utils.NotifyUtil.NotifyOwners(sentDate.Item2, sender, 0, sentDate.Item3);
                    return false;
                }

                DateTime? sdt = sentDate.Item1.GetDate();
                if (CheckIfDateTimeIsValid(sdt) == false)
                {
                    var lang4 = new Language(new Dictionary<string, string>
                    {
                        {"en", "The date you choose is invalid!"},
                        {"it", "La data che hai scelto non è valida!"}
                    });
                    await sender.SendTextMessageAsync(e.Message.From.Id, lang4,
                        ChatType.Private, e.Message.From.LanguageCode,
                        ParseMode.Default, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                        e.Message.From.Username);
                    return false;
                }
            }

            const long idChatSentInto = Channels.PoliAssociazioni;
            //const long idChatSentInto = -432645805;
            ChatType chatTypeSendInto = ChatType.Group;

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

            if (successQueue != SuccessQueue.SUCCESS)
            {
                return false;
            }

            var lang3 = new Language(new Dictionary<string, string>
            {
                {"en", "The message has been submitted correctly"},
                {"it", "Il messaggio è stato inviato correttamente"}
            });
            await sender.SendTextMessageAsync(e.Message.From.Id, lang3,
                ChatType.Private, e.Message.From.LanguageCode,
                ParseMode.Default, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
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

            if (sdt.Value.Year == DateTime.Now.Year && sdt.Value.Month == DateTime.Now.Month && sdt.Value.Day < DateTime.Now.Day)
                return false;

            return true;
        }

        private static async Task Assoc_ObjectToSendNotValid(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var lang2 = new Language(new Dictionary<string, string>
                        {
                            {"en", "You have to attach something! (A photo, for example)"},
                            {"it", "Devi allegare qualcosa! (Una foto, ad esempio)"}
                        });
            await sender.SendTextMessageAsync(e.Message.From.Id,
                lang2,
                ChatType.Private, e.Message.From.LanguageCode,
                ParseMode.Default,
                new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e.Message.From.Username);
        }

        internal static async Task<bool> Assoc_Publish(TelegramBotAbstract sender, MessageEventArgs e)
        {
            try
            {
                return await MessageDb.CheckMessagesToSend(true, sender);
            }
            catch (Exception e1)
            {
                await Utils.NotifyUtil.NotifyOwners(e1, sender);
                return false;
            }
        }

        internal async static Task<bool> Assoc_Delete(TelegramBotAbstract sender, MessageEventArgs e)
        {
            DataRowCollection messages = await GetMessagesInQueueAsync(sender, e, false);

            if (messages == null)
                return false;

            if (messages.Count == 1)
            {
                return await DeleteMessageFromQueueAsync(messages, 0, sender, e);
            }

            int count = 0;
            foreach (DataRow m in messages)
            {
                _ = await SendMessageAssocToUserAsync(m, sender, e, extraInfo: true, count);
                count++;
            }

            Dictionary<string, string> dict = new Dictionary<string, string>() {
                {"it", "Quale vuoi rimuovere dalla coda?" },
                {"en", "Which one do you want to remove from queue?" }
            };
            Language question = new Language(dict);
            List<Language> list = new List<Language>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new Language(dict: new Dictionary<string, string>() {
                    {"en", i.ToString() }
                }));
            }

            List<List<Language>> options = Utils.KeyboardMarkup.ArrayToMatrixString(list);
            List<Language> options2 = new List<Language>() {
                new Language(dict: new Dictionary<string, string>(){
                    {"it", "Annulla" },
                    {"en", "Cancel" }
                })
            };
            options.Insert(0, options2);
            var r1 = await AskUser.AskBetweenRangeAsync(e.Message.From.Id, question, sender, e.Message.From.LanguageCode, options, e.Message.From.Username, true);

            int? index = null;
            try
            {
                index = Convert.ToInt32(r1);
            }
            catch
            {
                ;
            }

            if (index == null)
            {
                return true;
            }

            return await DeleteMessageFromQueueAsync(messages, index.Value, sender, e);
        }

        private static async Task<bool> DeleteMessageFromQueueAsync(DataRowCollection messages, int v, TelegramBotAbstract telegramBotAbstract, MessageEventArgs e)
        {
            bool r = DeleteMessageFromQueueSingle(messages, v);
            if (r)
            {
                Language text1 = new Language(dict: new Dictionary<string, string>() {
                    {"it", "Messaggio ["+v+"] eliminato con successo" },
                    {"en", "Message ["+v+"] deleted successfully" }
                });
                await telegramBotAbstract.SendTextMessageAsync(e.Message.From.Id, text1,
                    e.Message.Chat.Type, e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, null, true);
            }
            else
            {
                Language text2 = new Language(dict: new Dictionary<string, string>() {
                    {"it", "Messaggio ["+v+"] non eliminato, errore" },
                    {"en", "Message ["+v+"] not deleted, error" }
                });
                await telegramBotAbstract.SendTextMessageAsync(e.Message.From.Id, text2,
                 e.Message.Chat.Type, e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, null, true);
            }

            return r;
        }

        private static bool DeleteMessageFromQueueSingle(DataRowCollection messages, int v)
        {
            string q = "DELETE FROM Messages WHERE ID = @id";
            DataRow dr = null;

            try
            {
                dr = messages[v];
            }
            catch
            {
                ;
            }

            if (dr == null)
                return false;

            int id = Convert.ToInt32(dr["id"]);

            Dictionary<string, object> args = new Dictionary<string, object>() {
                {"@id", id }
            };
            Utils.SqLite.Execute(q, args);

            return true;
        }

        internal static string GetNameOfEntityFromItsID(int value)
        {
            string q = "SELECT name FROM Entities WHERE id = " + value.ToString();
            var r = Utils.SqLite.ExecuteSelect(q);
            if (r == null || r.Rows.Count == 0)
                return null;

            return r.Rows[0].ItemArray[0].ToString();
        }

        internal async static Task<bool> Assoc_ReadAll(TelegramBotAbstract sender, MessageEventArgs e)
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
                ParseMode.Default, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e.Message.From.Username);
        }

        internal static async Task<bool> Assoc_Read(TelegramBotAbstract sender, MessageEventArgs e, bool allAssoc)
        {
            DataRowCollection messages = await GetMessagesInQueueAsync(sender, e, allAssoc);

            if (messages == null)
                return false;

            int count = 0;
            foreach (DataRow m in messages)
            {
                _ = await SendMessageAssocToUserAsync(m, sender, e, extraInfo: true, count);
                count++;
            }

            return true;
        }

        private static async Task<DataRowCollection> GetMessagesInQueueAsync(TelegramBotAbstract sender, MessageEventArgs e, bool allAssoc)
        {
            Language languageList = null;
            int? messageFromIdEntity = null;
            string conditionOnIdEntity = "";
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
                dict2 = new Dictionary<string, object>() { { "@id", messageFromIdEntity.Value } };
            }

            string q = "SELECT * FROM Messages WHERE " + conditionOnIdEntity + " has_been_sent = FALSE";
            DataTable r = Utils.SqLite.ExecuteSelect(q, dict2);
            if (r == null || r.Rows.Count == 0)
            {
                Language text = new Language(dict: new Dictionary<string, string>() {
                    {"it", "Non ci sono messaggi in coda!" },
                    {"en", "There are no message in the queue!" }
                });
                await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id, e.Message.From.LanguageCode, e.Message.From.Username,
                    text, ParseMode.Html, null);

                return null;
            }

            return r.Rows;
        }

        private static async Task<MessageSend> SendMessageAssocToUserAsync(DataRow m, TelegramBotAbstract sender, MessageEventArgs e, bool extraInfo, int count)
        {
            if (m == null)
            {
                return new MessageSend(false, null, null);
            }

            return await MessageDb.SendMessageFromDataRow(m, e.Message.From.Id, ChatType.Private, extraInfo, sender, count);
        }

        private static bool? CheckIfEntityReachedItsMaxLimit(int messageFromIdEntity)
        {
            switch (messageFromIdEntity)
            {
                case 13: //terna che ci sta aiutando col test (sarà tolto)
                case 2: //polinetwork
                    {
                        return false;
                    }
            }

            string q = "SELECT COUNT (*) " +
                "FROM Messages " +
                "WHERE Messages.from_id_entity = " + messageFromIdEntity + " AND(julianday('now') - 30) <= julianday(Messages.sent_date) ";

            var dt = Utils.SqLite.ExecuteSelect(q);

            if (dt == null)
            {
                return null;
            }

            if (dt.Rows == null)
                return null;

            int? count = null;

            try
            {
                count = Convert.ToInt32(dt.Rows[0].ItemArray[0]);
            }
            catch
            {
                ;
            }

            if (count == null)
                return null;

            return count.Value >= 2;
        }
    }
}
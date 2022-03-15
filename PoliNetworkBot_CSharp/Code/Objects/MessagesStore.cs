#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public static class MessagesStore
    {
        private static Dictionary<string, StoredMessage> Store;

        public static void InitializeMessageStore()
        {
            try
            {
                Store = JsonConvert.DeserializeObject<Dictionary<string, StoredMessage>>(
                    File.ReadAllText(Paths.Data.MessageStore)) ?? new();
            }
            catch (Exception ex)
            {
                Store = new();
            }
        }
        
        /// <summary>
        ///     Adds a new message to the storage
        /// </summary>
        /// <param name="message"></param>
        /// <param name="MessageAllowedStatusEnum"></param>
        /// <param name="timeLater">Allow at a later time starting from now</param>
        /// <returns></returns>
        public static bool AddMessage(string message, MessageAllowedStatusEnum messageAllowedStatus = MessageAllowedStatusEnum.NOT_DEFINED, TimeSpan? timeLater = null)
        {
            if (message == null)
                return false;

            if (string.IsNullOrEmpty(message))
                return false;

            if (Store.ContainsKey(message))
                Store.Remove(message);

            if ( ( timeLater != null && messageAllowedStatus != MessageAllowedStatusEnum.PENDING ) 
                 || (timeLater == null && messageAllowedStatus == MessageAllowedStatusEnum.PENDING ))
                throw new Exception("TimeLater and status mismatch");

            DateTime? allowedTime = null; 
            
            switch (messageAllowedStatus)
            {
                case MessageAllowedStatusEnum.PENDING:
                    allowedTime = DateTime.Now + (timeLater ?? TimeSpan.Zero);
                    break;
                case MessageAllowedStatusEnum.ALLOWED:
                    allowedTime = DateTime.Now;
                    break;
                case MessageAllowedStatusEnum.NOT_ALLOWED:
                case MessageAllowedStatusEnum.NOT_DEFINED:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageAllowedStatus), messageAllowedStatus, null);
            }
            
            lock (Store)
            {
                Store.Add(message,
                    new StoredMessage(message, allowedSpam: messageAllowedStatus,
                        allowedTime: allowedTime));
            }

            return true;
        }

        /// <summary>
        ///     Adds new allowed message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool AllowMessage(string message)
        {
            return AddMessage(message,  MessageAllowedStatusEnum.ALLOWED);
        }

        public static void CheckTimestamp()
        {
            if (Store.Count == 0)
                return;
            foreach (var message in Store.Keys)
            {
                Store.TryGetValue(message, out var storedMessage);
                if (storedMessage != null && !storedMessage.IsOutdated()) continue;
                lock (Store)
                {
                    Store.Remove(message);
                }
            }
        }

        public static void RemoveMessage(string text)
        {
            if (!Store.ContainsKey(text)) return;
            lock (Store)
            {
                Store.Remove(text);
            }
        }

        public static List<StoredMessage> GetAllMessages(Func<StoredMessage, bool> filter = null)
        {
            var r = Store.Values.ToList();
            if (filter != null)
                r = r.Where(filter).ToList();

            return r;
        }

        internal static SpamType StoreAndCheck(MessageEventArgs e, Message message)
        {
            if (message == null)
                return SpamType.UNDEFINED;

            if (!string.IsNullOrEmpty(message.Text))
                return StoreAndCheck2(message, message.Text);
            return !string.IsNullOrEmpty(message.Caption) ? StoreAndCheck2(message, message.Caption) : SpamType.UNDEFINED;
        }

        private static SpamType StoreAndCheck2(Message message, string text)
        {
            if (string.IsNullOrEmpty(text) || message.From == null)
                return SpamType.UNDEFINED;

            if (Store.ContainsKey(text))
            {
                Store[text].LastSeenTime = DateTime.Now;
                Store[text].HowManyTimesWeSawIt++;
            }
            else
            {
                lock (Store)
                {
                    Store[text] = new StoredMessage
                    (
                        lastSeenTime: DateTime.Now,
                        howManyTimesWeSawIt: 1,
                        message: message.Text,
                        allowedSpam:  MessageAllowedStatusEnum.NOT_DEFINED
                    );
                }
            }

            try
            {
                lock (Store[text])
                {
                    if (!Store[text].FromUserId.Contains(message.From.Id))
                        Store[text].FromUserId.Add(message.From.Id);

                    if (!Store[text].GroupsIdItHasBeenSentInto.Contains(message.Chat.Id))
                        Store[text].GroupsIdItHasBeenSentInto.Add(message.Chat.Id);

                    Store[text].Messages.Add(message);

                    return Store.Count == 0 ? SpamType.UNDEFINED : Store[text].IsSpam();
                }
            }
            catch
            {
                ;
            }

            return SpamType.UNDEFINED;
        }

        internal static StoredMessage GetStoredMessageByHash(string hash)
        {
            return Store.Values.FirstOrDefault(storedMessage => storedMessage.GetHash() == hash);
        }

        internal static List<Message> GetMessages(string text)
        {
            try
            {
                return Store[text].Messages;
            }
            catch
            {
                ;
            }

            return null;
        }

        internal static async Task SendMessageDetailsAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            if (e?.Message?.ReplyToMessage == null || string.IsNullOrEmpty(e.Message.ReplyToMessage.Text))
                return;

            if (!Store.ContainsKey(e.Message.ReplyToMessage.Text))
            {
                Language language1 = new(new Dictionary<string, string>
                {
                    {
                        "it", "Non è stato trovato nessun messaggio"
                    },
                    {
                        "en", "There are no messages"
                    }
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, language1, ChatType.Private,
                    e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username);
                return;
            }

            var storedMessage = Store[e.Message.ReplyToMessage.Text];
            var json = storedMessage.ToJson();
            Language language2 = new(new Dictionary<string, string>
            {
                {
                    "en", "Messages"
                },
                {
                    "it", "Messaggi"
                }
            });

            var stream = UtilsFileText.GenerateStreamFromString(json);
            var tf = new TelegramFile(stream, "messagesSent.json", "Messages", "text/plain");
            PeerAbstract peer = new(e.Message.From.Id, e.Message.Chat.Type);
            await sender.SendFileAsync(tf, peer, language2, TextAsCaption.AS_CAPTION, e.Message.From.Username,
                e.Message.From.LanguageCode, null, true);
        }

        public static void VetoMessage(string message)
        {
            Store[message].RemoveMessage(true);
        }

        private static void DisallowMessage(string message)
        {
            Store[message].RemoveMessage(false);
        }

        public static void AllowMessageOwner(string text)
        {
            if (string.IsNullOrEmpty(text) == false)
            {
                AllowMessage(text);
                Store[text].ForceAllowMessage();
                
            }
        }
    }
}
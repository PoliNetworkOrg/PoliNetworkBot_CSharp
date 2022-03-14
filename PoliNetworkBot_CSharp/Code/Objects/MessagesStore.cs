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
        private static readonly Dictionary<string, StoredMessage> Store =
            JsonConvert.DeserializeObject<Dictionary<string, StoredMessage>>(
                File.ReadAllText(Paths.Data.MessageStore));

        /// <summary>
        ///     Adds a new message to the storage
        /// </summary>
        /// <param name="message"></param>
        /// <param name="allowedSpam">true if you want the bot to flag this message as Permitted Spam</param>
        /// <param name="timeLater">Allow at a later time starting from now</param>
        /// <returns></returns>
        public static bool AddMessage(string message, bool allowedSpam = false, TimeSpan? timeLater = null)
        {
            if (message == null)
                return false;

            if (string.IsNullOrEmpty(message))
                return false;

            if (Store.ContainsKey(message))
                Store.Remove(message);

            lock (Store)
            {
                Store.Add(message,
                    new StoredMessage(message, allowedSpam: allowedSpam,
                        allowedTime: DateTime.Now + (timeLater ?? TimeSpan.Zero)));
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
            return AddMessage(message, true);
        }

        public static void CheckTimestamp()
        {
            if (Store.Count == 0)
                return;
            foreach (var message in Store.Keys)
            {
                Store.TryGetValue(message, out var storedMessage);
                if (storedMessage == null || storedMessage.IsOutdated())
                    lock (Store)
                    {
                        Store.Remove(message);
                    }
            }
        }

        public static void RemoveMessage(string text)
        {
            if (Store.ContainsKey(text))
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

        internal static SpamType StoreAndCheck(MessageEventArgs e)
        {
            if (e == null)
                return SpamType.UNDEFINED;
            if (e.Message == null)
                return SpamType.UNDEFINED;

            if (!string.IsNullOrEmpty(e.Message.Text))
                return StoreAndCheck2(e, e.Message.Text);
            if (!string.IsNullOrEmpty(e.Message.Caption))
                return StoreAndCheck2(e, e.Message.Caption);

            return SpamType.UNDEFINED;
        }

        private static SpamType StoreAndCheck2(MessageEventArgs e, string text)
        {
            if (string.IsNullOrEmpty(text))
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
                        message: e.Message.Text,
                        allowedSpam: false
                    );
                }
            }

            try
            {
                lock (Store[text])
                {
                    if (!Store[text].FromUserId.Contains(e.Message.From.Id))
                        Store[text].FromUserId.Add(e.Message.From.Id);

                    if (!Store[text].GroupsIdItHasBeenSentInto.Contains(e.Message.Chat.Id))
                        Store[text].GroupsIdItHasBeenSentInto.Add(e.Message.Chat.Id);

                    Store[text].Messages.Add(e.Message);

                    if (Store.Count == 0)
                        return SpamType.UNDEFINED;

                    return Store[text].IsSpam();
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
            foreach (var storedMessage in Store.Values)
                if (storedMessage.GetHash() == hash)
                    return storedMessage;

            return null;
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
            if (e == null || e.Message == null || e.Message.ReplyToMessage == null ||
                string.IsNullOrEmpty(e.Message.ReplyToMessage.Text))
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
    }
}
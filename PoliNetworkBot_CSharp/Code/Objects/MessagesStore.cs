#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public static class MessagesStore
    {
        private static readonly Dictionary<string, StoredMessage> store = new();

        public static bool AddMessage(string message)
        {
            if (message == null)
                return false;

            if (string.IsNullOrEmpty(message))
                return false;

            if (store.ContainsKey(message))
                store.Remove(message);

            lock (store)
            {
                store.Add(message,
                    new StoredMessage { message = message, insertTime = DateTime.Now, allowedSpam = true });
            }

            return true;
        }

        public static void CheckTimestamp()
        {
            if (store.Count == 0)
                return;
            foreach (var message in store.Keys)
            {
                store.TryGetValue(message, out var storedMessage);
                if (storedMessage.IsOutdated())
                    lock (store)
                    {
                        store.Remove(message);
                    }
            }
        }

        public static void RemoveMessage(string text)
        {
            if (store.ContainsKey(text))
                lock (store)
                {
                    store.Remove(text);
                }
        }

        public static List<StoredMessage> GetAllMessages(Func<StoredMessage, bool> filter = null)
        {
            var r = store.Values.ToList();
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

            if (store.ContainsKey(text))
            {
                store[text].lastSeenTime = DateTime.Now;
                store[text].howManyTimesWeSawIt++;
            }
            else
            {
                lock (store)
                {
                    store[text] = new StoredMessage
                    {
                        insertTime = DateTime.Now,
                        lastSeenTime = DateTime.Now,
                        howManyTimesWeSawIt = 1,
                        message = e.Message.Text,
                        allowedSpam = false
                    };
                }
            }

            try
            {
                lock (store[text])
                {
                    if (!store[text].FromUserId.Contains(e.Message.From.Id))
                        store[text].FromUserId.Add(e.Message.From.Id);

                    if (!store[text].GroupsIdItHasBeenSentInto.Contains(e.Message.Chat.Id))
                        store[text].GroupsIdItHasBeenSentInto.Add(e.Message.Chat.Id);

                    store[text].Messages.Add(e.Message);

                    if (store.Count == 0)
                        return SpamType.UNDEFINED;

                    return store[text].IsSpam();
                }
            }
            catch
            {
                ;
            }

            return SpamType.UNDEFINED;
        }

        internal static List<Message> GetMessages(string text)
        {
            try
            {
                return store[text].Messages;
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

            if (!store.ContainsKey(e.Message.ReplyToMessage.Text))
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

            var storedMessage = store[e.Message.ReplyToMessage.Text];
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
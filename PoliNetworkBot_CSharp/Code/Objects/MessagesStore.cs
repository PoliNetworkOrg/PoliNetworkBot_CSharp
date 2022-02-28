#region

using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public static class MessagesStore
    {
        private static readonly Dictionary<string, StoredMessage> store = new();

        public static bool AddMessage(string message)
        {
            if (message == null)
                return false;
            if (store.ContainsKey(message))
                store.Remove(message);

            lock (store)
            {
                store.Add(message, new StoredMessage() { message = message, insertTime = DateTime.Now, allowedSpam = true });
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
                {
                    lock (store)
                    {
                        store.Remove(message);
                    }
                }
            }
        }

        public static void RemoveMessage(string text)
        {
            if (store.ContainsKey(text))
            {
                lock (store)
                {
                    store.Remove(text);
                }
            }
        }

        public static List<string> GetAllMessages()
        {
            return store.Keys.ToList();
        }

        internal static SpamType StoreAndCheck(MessageEventArgs e)
        {
            if (e == null)
                return SpamType.UNDEFINED;
            if (e.Message == null)
                return SpamType.UNDEFINED;
            if (string.IsNullOrEmpty(e.Message.Text))
                return SpamType.UNDEFINED;

            if (store.ContainsKey(e.Message.Text))
            {
                store[e.Message.Text].lastSeenTime = DateTime.Now;
                store[e.Message.Text].howManyTimesWeSawIt++;
            }
            else
            {
                lock (store)
                {
                    store[e.Message.Text] = new StoredMessage()
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
                lock (store[e.Message.Text])
                {
                    if (!store[e.Message.Text].FromUserId.Contains(e.Message.From.Id))
                        store[e.Message.Text].FromUserId.Add(e.Message.From.Id);

                    if (!store[e.Message.Text].GroupsIdItHasBeenSentInto.Contains(e.Message.Chat.Id))
                        store[e.Message.Text].GroupsIdItHasBeenSentInto.Add(e.Message.Chat.Id);

                    store[e.Message.Text].Messages.Add(e.Message);

                    if (store.Count == 0)
                        return SpamType.UNDEFINED;

                    return store[e.Message.Text].IsSpam();
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
            if (e == null || e.Message == null || e.Message.ReplyToMessage == null || string.IsNullOrEmpty(e.Message.ReplyToMessage.Text))
                return;

            if (!store.ContainsKey(e.Message.ReplyToMessage.Text))
            {
                Language language1 = new(new Dictionary<string, string>() {
                    {
                        "it", "Non è stato trovato nessun messaggio"
                    },
                    {
                        "en", "There are no messages"
                    }
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, language1, Telegram.Bot.Types.Enums.ChatType.Private,
                    e.Message.From.LanguageCode, Telegram.Bot.Types.Enums.ParseMode.Html, null, e.Message.From.Username);
                return;
            }

            var storedMessage = store[e.Message.ReplyToMessage.Text];
            string json = storedMessage.ToJson();
            Language language2 = new(new Dictionary<string, string>() {
                {
                    "en", "Messages"
                },
                {
                    "it", "Messaggi"
                }
            });
            Tuple<TeleSharp.TL.TLAbsInputPeer, long> peer = new(null, e.Message.From.Id);
            var stream = Utils.UtilsMedia.UtilsFileText.GenerateStreamFromString(json);
            var tf = new TelegramFile(stream, "messagesSent.json", "Messages", "text/plain");
            await sender.SendFileAsync(tf, peer, language2, TextAsCaption.AS_CAPTION, e.Message.From.Username, e.Message.From.LanguageCode, null, true);
        }
    }
}
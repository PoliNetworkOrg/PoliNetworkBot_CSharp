#region

using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
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
            store.Add(message, new StoredMessage() { message = message, insertTime = DateTime.Now, allowedSpam = true });
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
                    store.Remove(message);
            }
        }

        public static void RemoveMessage(string text)
        {
            if (store.ContainsKey(text))
                store.Remove(text);
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
                store[e.Message.Text] = new StoredMessage()
                {
                    insertTime = DateTime.Now,
                    lastSeenTime = DateTime.Now,
                    howManyTimesWeSawIt = 1,
                    message = e.Message.Text,
                    allowedSpam = false
                };
            }

            if (!store[e.Message.Text].FromUserId.Contains(e.Message.From.Id))
                store[e.Message.Text].FromUserId.Add(e.Message.From.Id);

            if (!store[e.Message.Text].GroupsIdItHasBeenSentInto.Contains(e.Message.Chat.Id))
                store[e.Message.Text].GroupsIdItHasBeenSentInto.Add(e.Message.Chat.Id);

            store[e.Message.Text].Messages.Add(e.Message);

            if (store.Count == 0)
                return SpamType.UNDEFINED;

            return store[e.Message.Text].IsSpam();
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
    }
}
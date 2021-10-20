using System;
using System.Collections.Generic;
using System.Text;
using PoliNetworkBot_CSharp.Code.Enums;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public static class AllowedMessages
    {
        private static readonly Dictionary<string, DateTime> ApprovedMessages = new Dictionary<string, DateTime>();

        public static bool AddMessage(String message)
        {
            if (message == null)
                return false;
            if (ApprovedMessages.ContainsKey(message))
                ApprovedMessages.Remove(message);
            ApprovedMessages.Add(message, DateTime.Now);
            return true;
        }

        public static SpamType CheckMessage(String message)
        {
            if (message == null)
                return SpamType.UNDEFINED;
            if (ApprovedMessages.Count == 0)
                return SpamType.UNDEFINED;
            if (ApprovedMessages.ContainsKey(message))
            {
                var datetime = ApprovedMessages[message];
                if (datetime.AddHours(24) > DateTime.Now)
                {
                    return SpamType.SPAM_PERMITTED;
                }
            }
            return SpamType.UNDEFINED;
        }

        public static void CheckTimestamp()
        {
            if (ApprovedMessages.Count == 0)
                return;
            foreach (var message in ApprovedMessages.Keys)
            {
                ApprovedMessages.TryGetValue(message, out var datetime);
                if (datetime.AddHours(24) < DateTime.Now)
                    ApprovedMessages.Remove(message);
            }
        }

        public static void removeMessage(string text)
        {
            if (ApprovedMessages.ContainsKey(text))
                ApprovedMessages.Remove(text);
        }
    }
}

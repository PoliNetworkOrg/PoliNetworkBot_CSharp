using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Enums;
using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    internal class StoredMessage
    {
        internal string message;
        internal DateTime? insertTime = null;
        internal DateTime? lastSeenTime = null;
        internal int howManyTimesWeSawIt = 0;

        public List<long> FromUserId = new();
        public List<long> GroupsIdItHasBeenSentInto = new();
        public List<Message> Messages = new();
        internal bool allowedSpam;

        public StoredMessage()
        {
        }

        internal SpamType IsSpam()
        {
            return allowedSpam
                ? insertTime == null
                    ? SpamType.UNDEFINED
                    : insertTime.Value.AddHours(24) > DateTime.Now ? SpamType.SPAM_PERMITTED : SpamType.UNDEFINED
                : GroupsIdItHasBeenSentInto.Count > 1 && howManyTimesWeSawIt > 1 && (FromUserId.Count <= 1 || (FromUserId.Count > 1 && message.Length > 10))
                    ? IsSpam2()
                    : SpamType.UNDEFINED;
        }

        const double averageLimit = 60;

        private SpamType IsSpam2()
        {
            if (insertTime == null || lastSeenTime == null)
                return SpamType.UNDEFINED;

            TimeSpan diff = lastSeenTime.Value - insertTime.Value;
            double average = diff.TotalSeconds / howManyTimesWeSawIt;

            return average < averageLimit ? SpamType.SPAM_LINK : SpamType.UNDEFINED;
        }

        internal bool IsOutdated()
        {
            if (insertTime == null)
                return false;

            return insertTime.Value.AddHours(24) < DateTime.Now;
        }

        internal string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
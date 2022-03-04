#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Enums;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class StoredMessage
    {
        private const double averageLimit = 60;
        internal bool allowedSpam;

        public List<long> FromUserId = new();
        public List<long> GroupsIdItHasBeenSentInto = new();
        internal int howManyTimesWeSawIt;
        internal DateTime? insertTime;
        internal DateTime? lastSeenTime;
        internal string message;
        public List<Message> Messages = new();

        internal SpamType IsSpam()
        {
            return allowedSpam
                ? insertTime == null
                    ? SpamType.UNDEFINED
                    : insertTime.Value.AddHours(24) > DateTime.Now
                        ? SpamType.SPAM_PERMITTED
                        : SpamType.UNDEFINED
                : GroupsIdItHasBeenSentInto.Count > 1 && howManyTimesWeSawIt > 1 &&
                  (FromUserId.Count <= 1 || FromUserId.Count > 1 && message.Length > 10)
                    ? IsSpam2()
                    : SpamType.UNDEFINED;
        }

        private SpamType IsSpam2()
        {
            if (insertTime == null || lastSeenTime == null)
                return SpamType.UNDEFINED;

            var diff = lastSeenTime.Value - insertTime.Value;
            var average = diff.TotalSeconds / howManyTimesWeSawIt;

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
            return JsonConvert.SerializeObject(this);
        }
    }
}
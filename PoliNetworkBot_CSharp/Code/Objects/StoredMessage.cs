#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class StoredMessage
    {
        private const double AverageLimit = 60;
        internal bool AllowedSpam;

        public List<long> FromUserId = new();
        public List<long> GroupsIdItHasBeenSentInto = new();
        internal int HowManyTimesWeSawIt;
        internal DateTime? AllowedTime;
        internal DateTime InsertedTime;
        internal DateTime? LastSeenTime;
        internal string message;
        public List<Message> Messages = new();
        private readonly string hash;

        public StoredMessage(string message, int howManyTimesWeSawIt = 0, bool allowedSpam = false, DateTime? allowedTime = null, DateTime? lastSeenTime = null)
        {
            HowManyTimesWeSawIt = howManyTimesWeSawIt;
            this.message = message;
            AllowedSpam = allowedSpam;
            InsertedTime = DateTime.Now;
            AllowedTime = allowedSpam ? ( allowedTime ?? DateTime.Now ) : null;
            LastSeenTime = lastSeenTime;
            hash = HashUtils.GetHashOf(message);
        }

        internal SpamType IsSpam()
        {
            return AllowedSpam
                ? AllowedTime == null || AllowedTime > DateTime.Now
                    ? SpamType.UNDEFINED
                    : AllowedTime.Value.AddHours(24) > DateTime.Now
                        ? SpamType.SPAM_PERMITTED
                        : SpamType.UNDEFINED
                : GroupsIdItHasBeenSentInto.Count > 1 && HowManyTimesWeSawIt > 1 &&
                  (FromUserId.Count <= 1 || FromUserId.Count > 1 && message.Length > 10)
                    ? IsSpam2()
                    : SpamType.UNDEFINED;
        }

        public string GetHash()
        {
            return hash;
        }

        private SpamType IsSpam2()
        {
            if (LastSeenTime == null)
                return SpamType.UNDEFINED;

            var diff = LastSeenTime.Value - InsertedTime;
            var average = diff.TotalSeconds / HowManyTimesWeSawIt;

            return average < AverageLimit ? SpamType.SPAM_LINK : SpamType.UNDEFINED;
        }

        internal bool IsOutdated()
        {
            if (AllowedTime == null)
                return false;

            return AllowedTime.Value.AddHours(24) < DateTime.Now;
        }

        internal string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
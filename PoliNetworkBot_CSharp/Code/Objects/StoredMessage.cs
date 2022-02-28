using PoliNetworkBot_CSharp.Code.Enums;
using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Objects
{
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
            if (allowedSpam)
            {
                if (insertTime == null)
                    return SpamType.UNDEFINED;

                return insertTime.Value.AddHours(24) > DateTime.Now ? SpamType.SPAM_PERMITTED : SpamType.UNDEFINED;
            }
            else
            {
                //todo: se sono stati inviati troppi messaggi in gruppi diversi in poco tempo è spam
                if (GroupsIdItHasBeenSentInto.Count > 1 && howManyTimesWeSawIt > 1)
                {
                    if (insertTime == null || lastSeenTime == null)
                        return SpamType.UNDEFINED;

                    TimeSpan diff = lastSeenTime.Value - insertTime.Value;
                    double average = diff.TotalSeconds / howManyTimesWeSawIt;

                    if (average < 60)
                        return SpamType.SPAM_LINK;

                    return SpamType.UNDEFINED;
                }

                return SpamType.UNDEFINED;
            }
        }

        internal bool IsOutdated()
        {
            if (insertTime == null)
                return false;

            return insertTime.Value.AddHours(24) < DateTime.Now;
        }
    }
}
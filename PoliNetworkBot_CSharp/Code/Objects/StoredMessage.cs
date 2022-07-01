﻿#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class StoredMessage
{
    private const double AverageLimit = 60;
    internal MessageAllowedStatus AllowedStatus;

    public List<long> FromUserId = new();
    public List<long> GroupsIdItHasBeenSentInto = new();
    internal int HowManyTimesWeSawIt;
    internal DateTime InsertedTime;
    internal DateTime? LastSeenTime;
    internal string message;
    public List<Message?> Messages = new();

    public StoredMessage(string message, int howManyTimesWeSawIt = 0,
        MessageAllowedStatusEnum allowedSpam = MessageAllowedStatusEnum.NOT_DEFINED_ERROR,
        TimeSpan? timeLater = null, DateTime? lastSeenTime = null)
    {
        HowManyTimesWeSawIt = howManyTimesWeSawIt;
        AllowedStatus = new MessageAllowedStatus(allowedSpam, timeLater);
        this.message = message;
        InsertedTime = DateTime.Now;
        LastSeenTime = lastSeenTime;
    }

    internal SpamType IsSpam()
    {
        switch (AllowedStatus.GetStatus())
        {
            case MessageAllowedStatusEnum.ALLOWED:
                return SpamType.SPAM_PERMITTED;

            case MessageAllowedStatusEnum.NOT_ALLOWED:
                return SpamType.SPAM_LINK;

            case MessageAllowedStatusEnum.PENDING:
                throw new Exception("MessageAllowedStatusEnum.PENDING should be hidden behind abstraction!");
            
            case MessageAllowedStatusEnum.NOT_DEFINED_ERROR:
            case MessageAllowedStatusEnum.NOT_DEFINED_FOUND_IN_A_MESSAGE_SENT:
                break;
        }

        return GroupsIdItHasBeenSentInto.Count > 1 && HowManyTimesWeSawIt > 1 &&
               (FromUserId.Count <= 1 || FromUserId.Count > 1 && message.Length > 10)
            ? IsSpam2()
            : SpamType.UNDEFINED;
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
        if (AllowedStatus.isAllowed() ?? true)
            return AllowedStatus.RemovalTime() < DateTime.Now;

        return LastSeenTime != null && LastSeenTime.Value.AddHours(2) < DateTime.Now;
    }

    internal string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    public void ForceAllowMessage()
    {
        AllowedStatus.ForceAllowMessage();
    }

    public void RemoveMessage(bool andFlagAsSpam)
    {
        AllowedStatus.RemoveMessage(andFlagAsSpam);
    }
}
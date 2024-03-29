﻿#region

using System;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Enums;

// ReSharper disable InconsistentNaming

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

/// <summary>
///     Hides PENDING messages responding only ALLOWED, NOT_ALLOWED, NOT_DEFINED
/// </summary>
[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class MessageAllowedStatus
{
    private const int VetoLowerBound = 9;
    private const int VetoHigherBound = 22;
    private readonly DateTime insertedTime = DateTime.Now;
    private MessageAllowedStatusEnum _messageAllowedStatus;
    private DateTime? allowedTime;


    public MessageAllowedStatus(MessageAllowedStatusEnum allowedSpam, TimeSpan? timeSpan)
    {
        _messageAllowedStatus = allowedSpam;
        switch (allowedSpam)
        {
            case MessageAllowedStatusEnum.ALLOWED:
                allowedTime = DateTime.Now;
                return;

            case MessageAllowedStatusEnum.NOT_ALLOWED:
                allowedTime = null;
                return;

            case MessageAllowedStatusEnum.PENDING:
                allowedTime = CalculateTimeSpan(timeSpan);
                break;

            case MessageAllowedStatusEnum.NOT_DEFINED_ERROR:
            case MessageAllowedStatusEnum.NOT_DEFINED_FOUND_IN_A_MESSAGE_SENT:
                allowedTime = null;
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(allowedSpam), allowedSpam, null);
        }
    }

    private static DateTime? CalculateTimeSpan(TimeSpan? timeSpan)
    {
        if (timeSpan == null)
            return null;
        if (timeSpan == TimeSpan.Zero)
            return DateTime.Now;
        var now = DateTime.Now;
        var dayInCount = 0;
        var allowedTimeTemp = DateTime.Now;
        while (!(timeSpan == null || timeSpan == TimeSpan.Zero))
        {
            switch (now.Hour)
            {
                //only necessary in first iteration
                case < VetoLowerBound when dayInCount == 0:
                {
                    var diff = now - DateTime.Today.AddHours(VetoLowerBound);
                    allowedTimeTemp = allowedTimeTemp.Add(diff);
                    break;
                }
                //only necessary in first iteration
                case >= VetoHigherBound when dayInCount == 0:
                    allowedTimeTemp = DateTime.Today.AddDays(1).AddHours(VetoLowerBound);
                    dayInCount++;
                    break;
            }

            var remainingTime = (DateTime.Today.AddDays(dayInCount).AddHours(VetoHigherBound) - allowedTimeTemp)
                .TotalMinutes > 0
                    ? DateTime.Today.AddDays(dayInCount).AddHours(VetoHigherBound) - allowedTimeTemp
                    : TimeSpan.Zero;

            if (remainingTime >= timeSpan)
            {
                allowedTimeTemp = allowedTimeTemp.Add(timeSpan.Value);
                timeSpan = TimeSpan.Zero;
            }
            else
            {
                allowedTimeTemp = allowedTimeTemp.Add(remainingTime)
                    .Add(new TimeSpan(24 - VetoHigherBound + VetoLowerBound, 0, 0));
                timeSpan = timeSpan.Value - remainingTime;
                dayInCount++;
            }
        }

        Logger.Logger.WriteLine("Scheduled message for " + allowedTimeTemp);
        return allowedTimeTemp;
    }

    /// <summary>
    ///     Status of the Message
    /// </summary>
    /// <returns>ALLOWED, NOT_ALLOWED, NOT_DEFINED</returns>
    public MessageAllowedStatusEnum GetStatus()
    {
        switch (_messageAllowedStatus)
        {
            case MessageAllowedStatusEnum.NOT_ALLOWED or MessageAllowedStatusEnum.NOT_DEFINED_ERROR
                or MessageAllowedStatusEnum.NOT_DEFINED_FOUND_IN_A_MESSAGE_SENT:
                return _messageAllowedStatus;

            case MessageAllowedStatusEnum.PENDING or MessageAllowedStatusEnum.ALLOWED:
            {
                if (allowedTime == null || allowedTime > DateTime.Now || allowedTime.Value.AddHours(24) < DateTime.Now)
                    return MessageAllowedStatusEnum.NOT_DEFINED_ERROR;
                return MessageAllowedStatusEnum.ALLOWED;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void ForceAllowMessage()
    {
        allowedTime = DateTime.Now;
        _messageAllowedStatus = MessageAllowedStatusEnum.ALLOWED;
    }

    public void RemoveMessage(bool andFlagAsSpam)
    {
        _messageAllowedStatus =
            andFlagAsSpam ? MessageAllowedStatusEnum.NOT_ALLOWED : MessageAllowedStatusEnum.NOT_DEFINED_ERROR;
    }

    public DateTime RemovalTime()
    {
        return allowedTime?.AddHours(48) ?? insertedTime.AddHours(24);
    }

    public DateTime? GetAllowedTime()
    {
        return allowedTime;
    }

    public bool? IsAllowed()
    {
        return _messageAllowedStatus switch
        {
            MessageAllowedStatusEnum.ALLOWED => true,
            MessageAllowedStatusEnum.NOT_ALLOWED => false,
            MessageAllowedStatusEnum.PENDING => null,
            MessageAllowedStatusEnum.NOT_DEFINED_ERROR => null,
            MessageAllowedStatusEnum.NOT_DEFINED_FOUND_IN_A_MESSAGE_SENT => false,
            _ => null
        };
    }
}
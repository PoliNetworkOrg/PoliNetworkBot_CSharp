﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Utils.Notify;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.Logger;

public static class GroupsFixLog
{
    private static List<string> _bothNull = new();
    private static Dictionary<long, KeyValuePair<string?, Exception?>> _newNull = new();
    private static List<string> _nameChange = new();
    private static List<string> _oldNull = new();
    private static int _countFixed;
    private static int _countIgnored;

    public static void SendLog(TelegramBotAbstract? telegramBotAbstract, EventArgsContainer? messageEventArgs,
        GroupsFixLogUpdatedEnum groupsFixLogUpdatedEnum = GroupsFixLogUpdatedEnum.ALL)
    {
        var message = "Groups Fix Log:";
        message += "\n\n";
        if (groupsFixLogUpdatedEnum == GroupsFixLogUpdatedEnum.ALL)
        {
            message += "-NewTitle null && OldTitle null: [Group ID]";
            message += "\n";
            message = _bothNull.Aggregate(message, (current, group) => current + group + "\n");
            message += "\n\n";
            message += "-NewTitle null:";
            message += "\n";
            message += HandleNewTitleNull(_newNull);
            message += "\n\n";
            message += "-OldTitle null: [newTitle]";
            message += "\n";
            message = _oldNull.Aggregate(message, (current, newTitle) => current + newTitle + "\n");
            message += "\n\n";
        }

        message += "-Name Changed: [oldTitle [->] newTitle]";
        message += "\n";
        message = _nameChange.Aggregate(message, (current, nameChange) => current + nameChange + "\n");
        message += "\n\n";
        message += "Fixed: " + _countFixed;
        message += "\nIgnored (already ok): " + _countIgnored;

        var escaped = HttpUtility.HtmlEncode(message);
        Logger.WriteLine(message);
        NotifyUtil.NotifyOwners_AnError_AndLog3(escaped, telegramBotAbstract, messageEventArgs,
            FileTypeJsonEnum.SIMPLE_STRING, SendActionEnum.SEND_TEXT);
        Reset();
    }

    private static void Reset()
    {
        _bothNull = new List<string>();
        _newNull = new Dictionary<long, KeyValuePair<string?, Exception?>>();
        _oldNull = new List<string>();
        _nameChange = new List<string>();
        _countFixed = 0;
        _countIgnored = 0;
    }

    private static string HandleNewTitleNull(Dictionary<long, KeyValuePair<string?, Exception?>> newNull)
    {
        var toReturn = "";
        var exceptionTypes = new List<Type>();
        foreach (var exception in newNull.Values.ToList()
                     .Where(exception =>
                         exception.Value != null && !exceptionTypes.Contains(exception.Value.GetType())))
            if (exception.Value != null)
                exceptionTypes.Add(exception.Value.GetType());

        foreach (var exceptionType in exceptionTypes)
        {
            toReturn += "#" + exceptionType + ":\n";
            toReturn += "[GroupId , oldTitle]\n";
            toReturn = newNull.Where(group =>
                {
                    var exception = newNull[group.Key].Value;
                    return exception != null && exception.GetType() == exceptionType;
                })
                .Aggregate(toReturn,
                    (current, group) => current + group.Key + " , " + group.Value.Key + "\n");
        }

        return toReturn;
    }

    public static void OldNullNewNull(long? item1Id, long tableRowId)
    {
        if (item1Id != null && item1Id != tableRowId)
        {
            _bothNull.Add(tableRowId + " but GetChat responded with ID: " + item1Id);
            return;
        }

        _bothNull.Add(tableRowId.ToString());
    }

    public static void NewNull(long id, string? oldTitle, Exception? exception)
    {
        _newNull.TryAdd(id, new KeyValuePair<string?, Exception?>(oldTitle, exception));
    }

    public static void OldNull(string newTitle)
    {
        _oldNull.Add(newTitle);
    }

    public static void NameChange(string? oldTitle, string? newTitle)
    {
        _nameChange.Add(oldTitle + " [->] " + newTitle);
        _countFixed++;
    }

    public static void CountIgnored()
    {
        _countIgnored++;
    }
}
﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.CommandDispatcher;

/// <summary>
///     This class represents a command usable by some kind of user
/// </summary>
[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class Command
{
    private readonly Func<MessageEventArgs, TelegramBotAbstract?, string[]?, Task>? _action;
    private readonly Action<MessageEventArgs, TelegramBotAbstract?, string[]?>? _action2;
    private readonly Func<MessageEventArgs, TelegramBotAbstract?, Task>? _action3;
    private readonly Language _helpMessage;
    private readonly Func<MessageEventArgs, bool>? _optionalConditions;
    private readonly Permission _permissionLevel;
    private readonly List<ChatType> _chatTypes;
    private readonly Language _shortDescription;
    private bool _hasBeenTriggered = false;

    // Trigger command
    private List<string> _trigger;



    public Command(IEnumerable<string> trigger, Action<MessageEventArgs, TelegramBotAbstract?, string[]?> action,
        List<ChatType> chatTypes, Permission permissionLevel, Language helpMessage, Language? shortDescription,
        Func<MessageEventArgs, bool>? optionalConditions)
    {
        _optionalConditions = optionalConditions;
        _trigger = trigger.Select(x=> x.ToLower()).ToList();
        _action2 = action;
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _shortDescription = shortDescription ?? helpMessage;
    }

    private Command(IEnumerable<string> trigger, Func<MessageEventArgs, TelegramBotAbstract?, string[]?, Task> action,
        List<ChatType> chatTypes, Permission permissionLevel, Language helpMessage, Language? shortDescription,
        Func<MessageEventArgs, bool>? optionalConditions)
    {
        _trigger = trigger.Select(x=> x.ToLower()).ToList();
        _action = action;
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _optionalConditions = optionalConditions;
        _shortDescription = shortDescription ?? helpMessage;
    }

    public static Command CreateInstance(IEnumerable<string> trigger, Func<MessageEventArgs, TelegramBotAbstract?, string[]?, Task> action, 
        List<ChatType> chatTypes, Permission permissionLevel, Language helpMessage, Language? shortDescription, Func<MessageEventArgs, bool>? optionalConditions)
    {
        return new Command(trigger, action, chatTypes, permissionLevel, helpMessage, shortDescription, optionalConditions);
    }

    public Command(List<string> trigger, Func<MessageEventArgs, TelegramBotAbstract?, Task> action, List<ChatType> chatTypes,
        Permission permissionLevel, Language helpMessage, Language? shortDescription, Func<MessageEventArgs, bool>? optionalConditions)
    {
        _trigger = trigger.Select(x=> x.ToLower()).ToList();
        _action3 = action;
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _optionalConditions = optionalConditions;
        _shortDescription = shortDescription ?? helpMessage;
    }

    public Command(string trigger, Func<MessageEventArgs?, TelegramBotAbstract?, string[]?, Task> action,
        List<ChatType> chatTypes, Permission permissionLevel, L helpMessage, Language? shortDescription,
        Func<MessageEventArgs, bool>? optionalConditions)
    {
        _trigger = new List<string>(){ trigger.ToLower()};
        _action = action;
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _optionalConditions = optionalConditions;
        _shortDescription = shortDescription ?? helpMessage;
    }
    
    public Command(string trigger, Action<MessageEventArgs?, TelegramBotAbstract?, string[]?> action,
        List<ChatType> chatTypes, Permission permissionLevel, Language helpMessage, Language? shortDescription,
        Func<MessageEventArgs, bool>? optionalConditions)
    {
        _optionalConditions = optionalConditions;
        _trigger = new List<string>(){ trigger.ToLower()};
        _action2 = action;
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _shortDescription = shortDescription ?? helpMessage;
    }

    public Command(string trigger,
        Func<MessageEventArgs?, TelegramBotAbstract?, Task> action,
        List<ChatType> chatTypes,
        Permission permissionLevel, L helpMessage,
        Language? shortDescription,
        Func<MessageEventArgs, bool>? optionalConditions)
    {
        _trigger = new List<string>(){ trigger.ToLower()};
        _action3 = action;
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _optionalConditions = optionalConditions;
        _shortDescription = shortDescription ?? helpMessage;
    }


    public Language HelpMessage(){
        return _shortDescription;
    }

    private bool IsTriggered(string command)
    {
        if (command.Contains(' ')) throw new Exception("Commands can't contain blank spaces!");
        var lowMessage = command.ToLower();
        return this._trigger.Any(trigger => string.CompareOrdinal(trigger, lowMessage) == 0);
    }

    public bool HasBeenTriggered(){
        return _hasBeenTriggered;
    }

    public virtual bool TryTrigger(MessageEventArgs e, TelegramBotAbstract telegramBotAbstract, string command,
        string[] args)
    {
        if (!IsTriggered(command))
            return false;
        if (!_chatTypes.Contains(e.Message.Chat.Type))
            return false;
        if (_optionalConditions != null && !_optionalConditions.Invoke(e))
            return false;
        if (!Permissions.CheckPermissions(_permissionLevel, e.Message.From))
            return false;
        _action?.Invoke(e, telegramBotAbstract, args);
        _action2?.Invoke(e, telegramBotAbstract, args);
        _action3?.Invoke(e, telegramBotAbstract);
        _hasBeenTriggered = true;
        return true;
    }
}
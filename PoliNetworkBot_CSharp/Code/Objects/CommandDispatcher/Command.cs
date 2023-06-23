#region

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.CommandDispatcher;

/// <summary>
///     This class represents a command usable by some kind of user
///     helpMessage is the message displayed to the /help command
///     use tags @args and @conditions to specify those
///     you can escape a @ by using \@
/// </summary>
[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class Command
{
    private readonly ActionFuncGeneric? _actionFuncGeneric;
    private readonly List<ChatType> _chatTypes;
    private readonly bool _enabled;
    private readonly Language _helpMessage;
    private readonly Language? _longDescription;
    private readonly Func<MessageEventArgs, bool>? _optionalConditions;
    private readonly Permission _permissionLevel;

    // Trigger command
    private List<string> _trigger;


    private Command(IEnumerable<string> trigger,
        ActionFuncGenericParams? action,
        List<ChatType> chatTypes, Permission permissionLevel, Language helpMessage, Language? longDescription,
        Func<MessageEventArgs, bool>? optionalConditions, bool enabled = true)
    {
        _trigger = trigger.Select(x => x.ToLower()).ToList();
        _actionFuncGeneric = new ActionFuncGeneric(action);
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _optionalConditions = optionalConditions;
        _longDescription = longDescription ?? helpMessage;
        _enabled = enabled;
    }


    public Command(string trigger,
        Action<ActionFuncGenericParams>?
            action,
        List<ChatType> chatTypes, Permission permissionLevel, L helpMessage, Language? longDescription,
        Func<MessageEventArgs, bool>? optionalConditions, bool enabled = true)
    {
        _trigger = new List<string> { trigger.ToLower() };
        _actionFuncGeneric = new ActionFuncGeneric(action);
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _optionalConditions = optionalConditions;
        _longDescription = longDescription ?? helpMessage;
        _enabled = enabled;
    }

    public Command(string trigger,
        ActionFuncGenericParams? action,
        List<ChatType> chatTypes, Permission permissionLevel, Language helpMessage, Language? longDescription,
        Func<MessageEventArgs, bool>? optionalConditions, bool enabled = true)
    {
        _optionalConditions = optionalConditions;
        _trigger = new List<string> { trigger.ToLower() };
        _actionFuncGeneric = new ActionFuncGeneric(action);
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _longDescription = longDescription ?? helpMessage;
        _enabled = enabled;
    }


    public Command(List<string> trigger,
        ActionFuncGenericParams? action,
        List<ChatType> chatTypes,
        Permission permissionLevel, L helpMessage, L? longDescription, Func<MessageEventArgs, bool>? optionalConditions,
        bool enabled = true)
    {
        _trigger = trigger;
        _actionFuncGeneric = new ActionFuncGeneric(action);
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _optionalConditions = optionalConditions;
        _longDescription = longDescription ?? helpMessage;
        _enabled = enabled;
    }

    public Command(List<string> trigger,
        Action<ActionFuncGenericParams> assocWrite,
        List<ChatType> chatTypes,
        Permission permissionLevel,
        L helpMessage,
        L? longDescription,
        Func<MessageEventArgs, bool>? optionalConditions)
    {
        _trigger = trigger;
        _actionFuncGeneric = new ActionFuncGeneric(assocWrite);
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _longDescription = longDescription;
        _optionalConditions = optionalConditions;
    }


    public static Command CreateInstance(IEnumerable<string> trigger,
        ActionFuncGenericParams action,
        List<ChatType> chatTypes, Permission permissionLevel, Language helpMessage, Language? longDescription,
        Func<MessageEventArgs, bool>? optionalConditions, bool enabled = true)
    {
        return new Command(trigger, action, chatTypes, permissionLevel, helpMessage, longDescription,
            optionalConditions, enabled);
    }


    public Language HelpMessage(Permission clearance)
    {
        return CommandsUtils.GenerateMessage(_helpMessage, clearance, _permissionLevel, _trigger);
    }

    public List<string> GetTriggers()
    {
        return _trigger;
    }

    public Language GetLongDescription(Permission clearance)
    {
        return string.IsNullOrEmpty(_longDescription?.Select(""))
            ? HelpMessage(clearance)
            : CommandsUtils.GenerateMessage(_longDescription, clearance, _permissionLevel, _trigger);
    }

    public bool IsTriggered(string command)
    {
        return IsTriggered2(command, _trigger);
    }

    public static bool IsTriggered2(string command, IEnumerable<string> list)
    {
        if (command.Contains(' ')) return false;
        var lowMessage = command.ToLower();
        return list.Any(trigger => string.CompareOrdinal("/" + trigger, lowMessage) == 0);
    }

    public virtual CommandExecutionState? TryTrigger(MessageEventArgs? e,
        TelegramBotAbstract.TelegramBotAbstract? telegramBotAbstract,
        string command,
        string[]? args)
    {
        if (!_enabled)
            return CommandExecutionState.ERROR_NOT_ENABLED;
        if (!IsTriggered(command))
            return CommandExecutionState.NOT_TRIGGERED;
        if (e != null && !_chatTypes.Contains(e.Message.Chat.Type))
            return CommandExecutionState.NOT_TRIGGERED;
        if (e != null && _optionalConditions != null && !_optionalConditions.Invoke(e))
            return CommandExecutionState.UNMET_CONDITIONS;
        if (!CheckPermissions(e?.Message.From))
            return CommandExecutionState.INSUFFICIENT_PERMISSIONS;
        if (_actionFuncGeneric == null)
            throw new Exception("Illegal state exception!");

        if (e == null) throw new Exception("Illegal state exception!");
        if (telegramBotAbstract == null) throw new Exception("Illegal state exception!");

        if (args != null)
            return _actionFuncGeneric.Invoke()?.CommandExecutionState;

        throw new Exception("Illegal state exception!");
    }

    public bool CheckPermissions(User? messageFrom)
    {
        return Permissions.CheckPermissions(_permissionLevel, messageFrom);
    }
}
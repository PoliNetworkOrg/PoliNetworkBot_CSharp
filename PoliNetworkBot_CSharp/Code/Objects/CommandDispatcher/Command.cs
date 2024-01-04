#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
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
    private readonly Language _longDescription;
    private readonly Func<MessageEventArgs, bool>? _optionalConditions;
    private readonly Permission _permissionLevel;

    // Trigger command
    private List<string> _trigger;


    public Command(IEnumerable<string> trigger,
        Func<MessageEventArgs, TelegramBotAbstract?, string[]?, CommandExecutionState> action,
        List<ChatType> chatTypes, Permission permissionLevel, Language helpMessage, Language? longDescription,
        Func<MessageEventArgs, bool>? optionalConditions, bool enabled = true)
    {
        _optionalConditions = optionalConditions;
        _trigger = trigger.Select(x => x.ToLower()).ToList();
        _actionFuncGeneric = new ActionFuncGeneric(action);
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _longDescription = longDescription ?? helpMessage;
        _enabled = enabled;
    }

    private Command(IEnumerable<string> trigger,
        Func<MessageEventArgs, TelegramBotAbstract?, string[]?, Task<CommandExecutionState>> action,
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

    public Command(IEnumerable<string> trigger,
        Func<MessageEventArgs, TelegramBotAbstract?, CommandExecutionState> action,
        List<ChatType> chatTypes,
        Permission permissionLevel, Language helpMessage, Language? longDescription,
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
        Func<MessageEventArgs?, TelegramBotAbstract?, string[]?, Task<CommandExecutionState>>
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
        Func<MessageEventArgs?, TelegramBotAbstract?, string[]?, CommandExecutionState> action,
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

    public Command(string trigger,
        Func<MessageEventArgs?, TelegramBotAbstract?, Task>? action,
        List<ChatType> chatTypes,
        Permission permissionLevel, L helpMessage,
        Language? longDescription,
        Func<MessageEventArgs, bool>? optionalConditions,
        bool enabled = true
    )
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

    public Command(List<string> trigger,
        Func<MessageEventArgs, TelegramBotAbstract?, Task<CommandExecutionState>> action,
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

    public Command(string getConfig,
        Func<MessageEventArgs, TelegramBotAbstract, string[]?, Task>? action,
        List<ChatType> chatTypes, Permission permissionLevel, L helpMessage, Language? longDescription,
        Func<MessageEventArgs, bool>? optionalConditions, bool enabled = true)
    {
        _trigger = new List<string> { getConfig };
        _actionFuncGeneric = new ActionFuncGeneric(action);
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _optionalConditions = optionalConditions;
        _longDescription = longDescription ?? helpMessage;
        _enabled = enabled;
    }

    private Command(IEnumerable<string> getConfig,
        Func<MessageEventArgs, TelegramBotAbstract?, string[]?, Task> action,
        List<ChatType> chatTypes, Permission permissionLevel, Language helpMessage, Language? longDescription,
        Func<MessageEventArgs, bool>? optionalConditions, bool enabled)
    {
        _trigger = getConfig.ToList();
        _actionFuncGeneric = new ActionFuncGeneric(action);
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _optionalConditions = optionalConditions;
        _longDescription = longDescription ?? helpMessage;
        _enabled = enabled;
    }

    public static Command CreateInstance(IEnumerable<string> trigger,
        Func<MessageEventArgs, TelegramBotAbstract?, string[]?, Task> action,
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
        return string.IsNullOrEmpty(_longDescription.Select(""))
            ? HelpMessage(clearance)
            : CommandsUtils.GenerateMessage(_longDescription, clearance, _permissionLevel, _trigger);
    }

    private bool IsTriggered(string command)
    {
        if (command.Contains(' ')) throw new Exception("Commands can't contain blank spaces!");
        var lowMessage = command.ToLower();
        return _trigger.Any(trigger => string.CompareOrdinal("/" + trigger, lowMessage) == 0);
    }

    public virtual CommandExecutionState TryTrigger(MessageEventArgs? e,
        TelegramBotAbstract? telegramBotAbstract,
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
        if (_actionFuncGeneric != null)
            if (e != null)
                if (telegramBotAbstract != null)
                    if (args != null)
                        return _actionFuncGeneric.Invoke(e, telegramBotAbstract, args);
        throw new Exception("Illegal state exception!");
    }

    public bool CheckPermissions(User? messageFrom)
    {
        return Permissions.CheckPermissions(_permissionLevel, messageFrom);
    }
}
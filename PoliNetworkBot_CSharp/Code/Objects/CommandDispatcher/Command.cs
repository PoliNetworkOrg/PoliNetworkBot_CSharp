#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Enums;
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
    private readonly Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, string[]?, Task>? _action;
    private readonly Action<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, string[]?>? _action2;
    private readonly Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, Task>? _action3;
    private readonly List<ChatType> _chatTypes;
    private readonly bool _enabled;
    private readonly Language _helpMessage;
    private readonly Language _longDescription;
    private readonly Func<MessageEventArgs, bool>? _optionalConditions;
    private readonly Permission _permissionLevel;

    // Trigger command
    private List<string> _trigger;


    public Command(IEnumerable<string> trigger,
        Action<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, string[]?> action,
        List<ChatType> chatTypes, Permission permissionLevel, Language helpMessage, Language? longDescription,
        Func<MessageEventArgs, bool>? optionalConditions, bool enabled = true)
    {
        _optionalConditions = optionalConditions;
        _trigger = trigger.Select(x => x.ToLower()).ToList();
        _action2 = action;
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _longDescription = longDescription ?? helpMessage;
        _enabled = enabled;
    }

    private Command(IEnumerable<string> trigger,
        Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, string[]?, Task> action,
        List<ChatType> chatTypes, Permission permissionLevel, Language helpMessage, Language? longDescription,
        Func<MessageEventArgs, bool>? optionalConditions, bool enabled = true)
    {
        _trigger = trigger.Select(x => x.ToLower()).ToList();
        _action = action;
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _optionalConditions = optionalConditions;
        _longDescription = longDescription ?? helpMessage;
        _enabled = enabled;
    }

    public Command(IEnumerable<string> trigger,
        Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, Task> action,
        List<ChatType> chatTypes,
        Permission permissionLevel, Language helpMessage, Language? longDescription,
        Func<MessageEventArgs, bool>? optionalConditions, bool enabled = true)
    {
        _trigger = trigger.Select(x => x.ToLower()).ToList();
        _action3 = action;
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _optionalConditions = optionalConditions;
        _longDescription = longDescription ?? helpMessage;
        _enabled = enabled;
    }

    public Command(string trigger,
        Func<MessageEventArgs?, TelegramBotAbstract.TelegramBotAbstract?, string[]?, Task> action,
        List<ChatType> chatTypes, Permission permissionLevel, L helpMessage, Language? longDescription,
        Func<MessageEventArgs, bool>? optionalConditions, bool enabled = true)
    {
        _trigger = new List<string> { trigger.ToLower() };
        _action = action;
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _optionalConditions = optionalConditions;
        _longDescription = longDescription ?? helpMessage;
        _enabled = enabled;
    }

    public Command(string trigger,
        Action<MessageEventArgs?, TelegramBotAbstract.TelegramBotAbstract?, string[]?> action,
        List<ChatType> chatTypes, Permission permissionLevel, Language helpMessage, Language? longDescription,
        Func<MessageEventArgs, bool>? optionalConditions, bool enabled = true)
    {
        _optionalConditions = optionalConditions;
        _trigger = new List<string> { trigger.ToLower() };
        _action2 = action;
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _longDescription = longDescription ?? helpMessage;
        _enabled = enabled;
    }

    public Command(string trigger,
        Func<MessageEventArgs?, TelegramBotAbstract.TelegramBotAbstract?, Task> action,
        List<ChatType> chatTypes,
        Permission permissionLevel, L helpMessage,
        Language? longDescription,
        Func<MessageEventArgs, bool>? optionalConditions,
        bool enabled = true
    )
    {
        _trigger = new List<string> { trigger.ToLower() };
        _action4 = action;
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _optionalConditions = optionalConditions;
        _longDescription = longDescription ?? helpMessage;
        _enabled = enabled;
    }

    public Command(List<string> trigger, Func<MessageEventArgs, TelegramBotAbstract?, Task<CommandExecutionState>> action, List<ChatType> chatTypes, Permission permissionLevel, L helpMessage, L? longDescription, Func<MessageEventArgs, bool>? optionalConditions, bool enabled = true)
    {
        _trigger = trigger;
        _action4 = action;
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _optionalConditions = optionalConditions;
        _longDescription = longDescription ?? helpMessage;
        _enabled = enabled;
    }

    public static Command CreateInstance(IEnumerable<string> trigger,
        Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, string[]?, Task> action,
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

    public virtual CommandExecutionState TryTrigger(MessageEventArgs e,
        TelegramBotAbstract.TelegramBotAbstract telegramBotAbstract,
        string command,
        string[] args)
    {
        if (!_enabled)
            return CommandExecutionState.ERROR_NOT_ENABLED;
        if (!IsTriggered(command))
            return CommandExecutionState.NOT_TRIGGERED;
        if (!_chatTypes.Contains(e.Message.Chat.Type))
            return CommandExecutionState.NOT_TRIGGERED;
        if (_optionalConditions != null && !_optionalConditions.Invoke(e))
            return CommandExecutionState.UNMET_CONDITIONS;
        if (!CheckPermissions(e.Message.From))
            return CommandExecutionState.INSUFFICIENT_PERMISSIONS;
        if (_action != null)
            return _action.Invoke(e, telegramBotAbstract, args).Result;
        if (_action2 != null)
            return _action2.Invoke(e, telegramBotAbstract, args);
        if (_action3 != null)
            return _action3.Invoke(e, telegramBotAbstract);
        if (_action4 != null)
            return _action4.Invoke(e, telegramBotAbstract).Result;
        throw new Exception("Illegal state exception!");
    }

    public bool CheckPermissions(User? messageFrom)
    {
        return Permissions.CheckPermissions(_permissionLevel, messageFrom);
    }
}
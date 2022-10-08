using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Objects.CommandDispatcher;

/// <summary>
/// This class represents a command usable by some kind of user
/// </summary>
[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class Command
{
    
    // Trigger command
    private string _trigger;

    private readonly Func<MessageEventArgs, TelegramBotAbstract?, string[]?, Task>? _action;
    private readonly Action<MessageEventArgs, TelegramBotAbstract?, string[]?>? _action2;
    private readonly Func<MessageEventArgs, TelegramBotAbstract?, Task>? _action3;
    private readonly Func<MessageEventArgs, bool>? _optionalConditions;
    private List<ChatType> _chatTypes;
    private readonly Permission _permissionLevel;
    private readonly string _helpMessage;
    
    public Command(string trigger, Action<MessageEventArgs, TelegramBotAbstract?, string[]?> action, List<ChatType> chatTypes, Permission permissionLevel, string helpMessage, Func<MessageEventArgs, bool>? optionalConditions)
    {
        _optionalConditions = optionalConditions;
        _trigger = trigger.ToLower();
        _action2 = action;
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
    }
    
    public Command(string trigger, Func<MessageEventArgs, TelegramBotAbstract?, string[]?, Task> action, List<ChatType> chatTypes, Permission permissionLevel, string helpMessage, Func<MessageEventArgs, bool>? optionalConditions)
    {
        _trigger = trigger.ToLower();
        _action = action;
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _optionalConditions = optionalConditions;
    }
    
    public Command(string trigger, Func<MessageEventArgs, TelegramBotAbstract?, Task> action, List<ChatType> chatTypes, Permission permissionLevel, string helpMessage, Func<MessageEventArgs, bool>? optionalConditions)
    {
        _trigger = trigger.ToLower();
        _action3 = action;
        _chatTypes = chatTypes;
        _permissionLevel = permissionLevel;
        _helpMessage = helpMessage;
        _optionalConditions = optionalConditions;
    }
    

    public bool IsTriggered(string command)
    {
        if (command.Contains(' ')) throw new Exception("Commands can't contain blank spaces!");
        var lowMessage = command.ToLower();
        return string.CompareOrdinal(_trigger, lowMessage) == 0;
    }

    public virtual bool TryTrigger(MessageEventArgs e, TelegramBotAbstract telegramBotAbstract, string command, string[] args)
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
        return true;
    }
}

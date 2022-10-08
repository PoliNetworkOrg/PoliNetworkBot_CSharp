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

    private readonly Func<MessageEventArgs, TelegramBotAbstract?, string[], Task> _action;
    private readonly Func<MessageEventArgs, bool>? _optionalConditions;
    private ChatType _chatType;
    private readonly Permission _permissionLevel;
    
    public Command(string trigger, Func<MessageEventArgs, TelegramBotAbstract?, string[], Task> action, ChatType chatType, Permission permissionLevel, string helpMessage, Func<MessageEventArgs, bool> conditions)
        : this(trigger, action, chatType, permissionLevel, helpMessage)
    {
        _optionalConditions = conditions;
    }
    
    public Command(string trigger, Func<MessageEventArgs, TelegramBotAbstract?, string[], Task> action, ChatType chatType, Permission permissionLevel, string helpMessage)
    {
        _trigger = trigger;
        _action = action;
        _chatType = chatType;
        _permissionLevel = permissionLevel;
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
        if (_chatType != e.Message.Chat.Type)
            return false;
        if (_optionalConditions != null && !_optionalConditions.Invoke(e))
            return false;
        if (!Permissions.CheckPermissions(_permissionLevel, e.Message.From)) 
            return false;
        _action.Invoke(e, telegramBotAbstract, args);
        return true;
    }
}

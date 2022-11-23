#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Groups = PoliNetworkBot_CSharp.Code.Data.Constants.GroupsConstants;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

/// <summary>
///     This class represents an automatic bot reply, its action can be activated if IsTriggered(message) == true
/// </summary>
[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class AutomaticAnswer
{
    private Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, Task>? _action;
    private Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, string, Task>? _actionMessage;
    private List<long> _except;
    private string? _response;

    // CONJUNCTIVE NORMAL FORM: conjunction of disjunctions
    private List<List<string>> _trigger;

    public AutomaticAnswer(List<List<string>> trigger, Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, Task> action,
        List<long> except)
    {
        _trigger = trigger;
        _action = action;
        _except = except;
    }

    public AutomaticAnswer(List<List<string>> trigger,
        Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, string, Task> action, List<long> except, string response)
    {
        _trigger = trigger;
        _actionMessage = action;
        _except = except;
        _response = response;
    }

    public bool IsTriggered(string message)
    {
        var lowMessage = message.ToLower();
        return _trigger.Select(disjunction => disjunction
                .Any(clause => lowMessage.Contains(clause)))
            .All(satisfied => satisfied);
    }

    private bool GroupAllowed(long id)
    {
        return _except.All(group => !group.Equals(id));
    }

    public virtual bool TryTrigger(MessageEventArgs e, TelegramBotAbstract.TelegramBotAbstract telegramBotAbstract, string message)
    {
        if (!IsTriggered(message) || !GroupAllowed(e.Message.Chat.Id)) return false;
        if (_response != null)
            _actionMessage?.Invoke(e, telegramBotAbstract, _response);
        else
            _action?.Invoke(e, telegramBotAbstract);
        return true;
    }
}

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class AutomaticAnswerRestricted : AutomaticAnswer
{
    private Func<MessageEventArgs, bool> _condition;

    public AutomaticAnswerRestricted(List<List<string>> trigger,
        Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, Task> action, List<long> except,
        Func<MessageEventArgs, bool> condition) : base(trigger, action, except)
    {
        _condition = condition;
    }

    public AutomaticAnswerRestricted(List<List<string>> trigger,
        Func<MessageEventArgs, TelegramBotAbstract.TelegramBotAbstract?, string, Task> action, List<long> except, string response,
        Func<MessageEventArgs, bool> condition) : base(trigger, action, except, response)
    {
        _condition = condition;
    }

    public override bool TryTrigger(MessageEventArgs e, TelegramBotAbstract.TelegramBotAbstract telegramBotAbstract, string message)
    {
        return _condition(e) && base.TryTrigger(e, telegramBotAbstract, message);
    }
}
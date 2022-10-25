#region

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
///     helpMessage is the message displayed to the /help command
///     use tags @args and @conditions to specify those
///     you can escape a @ by using \@
/// </summary>
[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class Command
{
    private readonly Func<MessageEventArgs, TelegramBotAbstract?, string[]?, Task>? _action;
    private readonly Action<MessageEventArgs, TelegramBotAbstract?, string[]?>? _action2;
    private readonly Func<MessageEventArgs, TelegramBotAbstract?, Task>? _action3;
    private readonly List<ChatType> _chatTypes;
    private readonly bool _enabled;
    private readonly Language _helpMessage;
    private readonly Language _longDescription;
    private readonly Func<MessageEventArgs, bool>? _optionalConditions;
    private readonly Permission _permissionLevel;

    // Trigger command
    private List<string> _trigger;


    public Command(IEnumerable<string> trigger, Action<MessageEventArgs, TelegramBotAbstract?, string[]?> action,
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

    private Command(IEnumerable<string> trigger, Func<MessageEventArgs, TelegramBotAbstract?, string[]?, Task> action,
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

    public Command(IEnumerable<string> trigger, Func<MessageEventArgs, TelegramBotAbstract?, Task> action,
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

    public Command(string trigger, Func<MessageEventArgs?, TelegramBotAbstract?, string[]?, Task> action,
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

    public Command(string trigger, Action<MessageEventArgs?, TelegramBotAbstract?, string[]?> action,
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
        Func<MessageEventArgs?, TelegramBotAbstract?, Task> action,
        List<ChatType> chatTypes,
        Permission permissionLevel, L helpMessage,
        Language? longDescription,
        Func<MessageEventArgs, bool>? optionalConditions,
        bool enabled = true
    )
    {
        _trigger = new List<string> { trigger.ToLower() };
        _action3 = action;
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
        var languages = new Dictionary<string, string?>();
        if (Permissions.Compare(clearance, _permissionLevel) < 0)
            return new Language(languages);

        foreach (var lang in _helpMessage.GetLanguages())
        {
            var body = ParseText(_helpMessage, "body");
            var args = ParseText(_helpMessage, "args");
            var condition = ParseText(_helpMessage, "condition");
            var text = "/<b>" + string.Join(" | /", _trigger.ToArray()) + "</b>:\n" + body.Select(lang);

            if (!string.IsNullOrEmpty(args.Select(lang)))
                text += "<i>\nArguments: </i>" + args.Select(lang);

            if (!string.IsNullOrEmpty(condition.Select(lang)))
                text += "<i>\nConditions: </i>" + condition.Select(lang) + "";

            languages.Add(lang, text + "\n\n");
        }

        return new Language(languages);
    }

    public List<string> GetTriggers()
    {
        return _trigger;
    }

    public Language GetLongDescription(Permission permission)
    {
        return string.IsNullOrEmpty(_longDescription.Select("")) ? HelpMessage(permission) : _longDescription;
    }

    /// <summary>
    ///     body returns everything except the tags
    /// </summary>
    /// <param name="helpMessage"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    private static Language ParseText(Language helpMessage, string tag)
    {
        var toReturn = new Dictionary<string, string?>();
        foreach (var language in helpMessage.GetLanguages())
        {
            var select = helpMessage.Select(language) ?? "";
            if (tag == "body")
            {
                toReturn.Add(language, select.Split(" @")[0].Replace("/@", "@"));
            }
            else
            {
                var tags = select.Split(" @").Skip(1).ToList();
                var innerTags = tags.Where(innerTag => innerTag.Split(": ")[0] == tag)
                    .Select(x => x.Replace(tag + ": ", "").Replace("/@", "@"));

                foreach (var innerTag in innerTags) toReturn.Add(language, innerTag);
            }
        }

        return new Language(toReturn);
    }

    private bool IsTriggered(string command)
    {
        if (command.Contains(' ')) throw new Exception("Commands can't contain blank spaces!");
        var lowMessage = command.ToLower();
        return _trigger.Any(trigger => string.CompareOrdinal("/" + trigger, lowMessage) == 0);
    }

    public virtual bool TryTrigger(MessageEventArgs e, TelegramBotAbstract telegramBotAbstract, string command,
        string[] args)
    {
        if (!_enabled)
            return false;
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
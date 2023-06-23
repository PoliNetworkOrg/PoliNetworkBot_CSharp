#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Utils;
using SampleNuGet.Objects;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class TargetUserObject
{
    private const string TargetNullString = "[target null]";
    private long? _userId;
    private string? _username;

    public TargetUserObject(IReadOnlyList<string?>? stringInfo, SampleNuGet.Objects.TelegramBotAbstract? sender,
        MessageEventArgs? messageEventArgs)
    {
        var target = stringInfo?[0];
        SetStartParam(target);

        var fromIdReply = messageEventArgs?.Message.ReplyToMessage?.From?.Id;
        var fromIdAction = messageEventArgs?.Message.From?.Id;
        if (fromIdReply != null && fromIdAction != null && fromIdAction != fromIdReply)
            _userId = fromIdReply;

        var usernameFromReply = messageEventArgs?.Message.ReplyToMessage?.From?.Username;
        var usernameFromAction = messageEventArgs?.Message.From?.Username;
        if (!string.IsNullOrEmpty(usernameFromReply) && !string.IsNullOrEmpty(usernameFromAction) &&
            usernameFromAction != usernameFromReply)
            _username = usernameFromReply;

        _ = TryGetUserId(sender);
    }

    public TargetUserObject(User stringInfo)
    {
        _userId = stringInfo.Id;
        _username = stringInfo.Username;
    }

    private void SetStartParam(string? s)
    {
        if (string.IsNullOrEmpty(s))
            return;

        try
        {
            _userId = Convert.ToInt64(s);
        }
        catch
        {
            _username = s;
        }
    }

    public string GetTargetHtmlString()
    {
        string? target;
        if (_userId != null)
        {
            target = "<a href=\"tg://user?id=" + _userId + "\">" + _userId + "</a>";
            if (!string.IsNullOrEmpty(_username))
                target += " " + GetUsernameWithAt();
        }
        else if (!string.IsNullOrEmpty(_username))
        {
            target = GetUsernameWithAt();
        }
        else
        {
            target = TargetNullString;
        }

        return target ?? TargetNullString;
    }

    private string? GetUsernameWithAt()
    {
        return string.IsNullOrEmpty(_username) ? null :
            _username.StartsWith("@") ? _username : "@" + _username;
    }

    public async Task<bool> UserIdEmpty(SampleNuGet.Objects.TelegramBotAbstract? telegramBotAbstract)
    {
        if (_userId == null && string.IsNullOrEmpty(_username))
            return true;

        if (!string.IsNullOrEmpty(_username) && _userId == null) await TryGetUserId(telegramBotAbstract);

        return _userId == null;
    }

    private async Task TryGetUserId(SampleNuGet.Objects.TelegramBotAbstract? telegramBotAbstract)
    {
        if (_userId != null)
            return;

        var i2 = await Info.GetIdFromUsernameAsync(_username, telegramBotAbstract);
        var idFound = i2?.GetId();
        if (idFound != null) _userId = idFound;
    }

    public async Task<TargetUserObject> GetTargetUserId(SampleNuGet.Objects.TelegramBotAbstract? telegramBotAbstract)
    {
        if ((string.IsNullOrEmpty(_username) && _userId == null) || _userId != null || string.IsNullOrEmpty(_username))
            return this;

        if (_username[0] >= '0' && _username[0] <= '9')
            return this;

        var i2 = await Info.GetIdFromUsernameAsync(_username, telegramBotAbstract);
        var idFound = i2?.GetId();
        if (idFound != null) _userId = idFound;
        return this;
    }

    public long? GetUserId()
    {
        return _userId;
    }
}
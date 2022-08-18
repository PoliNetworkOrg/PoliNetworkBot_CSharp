#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Utils;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class TargetUserObject
{
    private string? _username;
    private long? _userId;

    public TargetUserObject(IReadOnlyList<string?>? stringInfo, TelegramBotAbstract? sender, MessageEventArgs? messageEventArgs)
    {
        var target = stringInfo?[1];
        SetStartParam(target);

        var fromId = messageEventArgs?.Message?.ReplyToMessage?.From?.Id;
        if (fromId != null)
            _userId = fromId;

        var usernameFrom = messageEventArgs?.Message?.ReplyToMessage?.From?.Username;
        if (!string.IsNullOrEmpty(usernameFrom))
            this._username = usernameFrom;

        _ = TryGetUserId(sender);
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

    private const string TargetNullString = "[target null]";

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

    public async Task<bool> UserIdEmpty(TelegramBotAbstract? telegramBotAbstract)
    {
        if (_userId == null && string.IsNullOrEmpty(_username))
            return true;

        if (!string.IsNullOrEmpty(_username) && _userId == null) await TryGetUserId(telegramBotAbstract);

        return _userId == null;
    }

    private async Task TryGetUserId(TelegramBotAbstract? telegramBotAbstract)
    {
        if (_userId != null)
            return;
        
        var i2 = await Info.GetIdFromUsernameAsync(_username, telegramBotAbstract);
        var idFound = i2?.GetId();
        if (idFound != null) _userId = idFound;
    }

    public async Task<TargetUserObject> GetTargetUserId(TelegramBotAbstract? telegramBotAbstract)
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
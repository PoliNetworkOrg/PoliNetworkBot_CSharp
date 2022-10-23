#region

using System;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Data;

public class TelegramUser
{
    private readonly string? _username;
    public readonly long? Id;

    public TelegramUser(string? v)
    {
        _username = v;
    }

    public TelegramUser(long v)
    {
        Id = v;
    }

    public TelegramUser(long v1, string? v2)
    {
        Id = v1;
        _username = v2;
    }

    internal bool Matches(User? from)
    {
        return from != null && Matches(from.Id, from.Username);
    }

    internal bool Matches(long userIdParam, string? usernameParam)
    {
        return Id switch
        {
            null => !string.IsNullOrEmpty(_username) &&
                    string.Equals(usernameParam, _username, StringComparison.CurrentCultureIgnoreCase),
            _ => Id == userIdParam
        };
    }

    public bool Matches(long userIdParam)
    {
        return Id == userIdParam;
    }


}
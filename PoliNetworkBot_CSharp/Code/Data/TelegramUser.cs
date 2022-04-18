#region

using System;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Data;

public class TelegramUser
{
    public readonly long? id;
    private readonly string username;

    public TelegramUser(string v)
    {
        username = v;
    }

    public TelegramUser(long v)
    {
        id = v;
    }

    public TelegramUser(long v1, string v2)
    {
        id = v1;
        username = v2;
    }

    internal bool Matches(User from)
    {
        return from != null && Matches(from.Id, from.Username);
    }

    internal bool Matches(long userIdParam, string usernameParam)
    {
        return id switch
        {
            null => !string.IsNullOrEmpty(username) &&
                    string.Equals(usernameParam, username, StringComparison.CurrentCultureIgnoreCase),
            _ => id == userIdParam
        };
    }
}
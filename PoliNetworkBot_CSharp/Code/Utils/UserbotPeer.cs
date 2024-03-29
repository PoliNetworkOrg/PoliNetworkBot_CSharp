﻿#region

using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;
using TLSharp.Core;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class UserbotPeer
{
    internal static TLAbsInputPeer GetPeerFromIdAndType(long chatid, ChatType? chatType)
    {
        if (chatType == null) return new TLInputPeerChat { ChatId = (int)chatid };

        return chatType switch
        {
            ChatType.Private => new TLInputPeerUser { UserId = (int)chatid },
            ChatType.Channel => new TLInputPeerChannel { ChannelId = (int)chatid },
            _ => new TLInputPeerChat { ChatId = (int)chatid }
        };
    }

    internal static TLAbsInputChannel? GetPeerChannelFromIdAndType(long? chatId, long? accessHash)
    {
        try
        {
            if (chatId != null)
                return accessHash != null
                    ? new TLInputChannel { ChannelId = (int)chatId, AccessHash = accessHash.Value }
                    : new TLInputChannel { ChannelId = (int)chatId };
        }
        catch
        {
            return null;
        }

        return null;
    }

    internal static TLAbsInputUser? GetPeerUserFromId(long userId)
    {
        try
        {
            return new TLInputUser { UserId = (int)userId };
        }
        catch
        {
            return null;
        }
    }

    public static async Task<TLInputPeerUser?> GetPeerUserWithAccessHash(string? username,
        TelegramClient? telegramClient)
    {
        TLUser? user2 = null;
        if (telegramClient == null)
            return user2 is { AccessHash: not null }
                ? new TLInputPeerUser { AccessHash = user2.AccessHash.Value, UserId = user2.Id }
                : null;

        var r = await telegramClient.ResolveUsernameAsync(username);

        var user = r?.Users?[0];
        if (user is not TLUser)
            return null;

        return user2 is { AccessHash: not null }
            ? new TLInputPeerUser { AccessHash = user2.AccessHash.Value, UserId = user2.Id }
            : null;
    }

    public static string? GetHtmlStringWithUserLink(User? user)
    {
        if (user == null)
            return null;

        var x = new TargetUserObject(user);
        return x.GetTargetHtmlString();
    }
}
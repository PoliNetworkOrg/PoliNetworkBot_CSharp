#region

using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;
using TLSharp.Core;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
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

        internal static TLAbsInputChannel GetPeerChannelFromIdAndType(long chatid, long? accessHash)
        {
            try
            {
                if (accessHash != null)
                    return new TLInputChannel { ChannelId = (int)chatid, AccessHash = accessHash.Value };

                return new TLInputChannel { ChannelId = (int)chatid };
            }
            catch
            {
                return null;
            }
        }

        internal static TLAbsInputUser GetPeerUserFromdId(long userId)
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

        public static async Task<TLInputPeerUser> GetPeerUserWithAccessHash(string username,
            TelegramClient telegramClient)
        {
            var r = await telegramClient.ResolveUsernameAsync(username);
            if (r?.Users == null)
                return null;

            var user = r.Users[0];
            if (user is not TLUser user2)
                return null;

            return user2.AccessHash != null
                ? new TLInputPeerUser { AccessHash = user2.AccessHash.Value, UserId = user2.Id }
                : null;
        }
    }
}
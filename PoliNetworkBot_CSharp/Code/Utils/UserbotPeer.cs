#region

using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class UserbotPeer
    {
        internal static TLAbsInputPeer GetPeerFromIdAndType(long chatid, ChatType chatType)
        {
            return chatType switch
            {
                ChatType.Private => new TLInputPeerUser {UserId = (int) chatid},
                ChatType.Channel => new TLInputPeerChannel {ChannelId = (int) chatid},
                _ => new TLInputPeerChat {ChatId = (int) chatid}
            };
        }

        internal static TLAbsInputChannel GetPeerChannelFromIdAndType(long chatid)
        {
            try
            {
                return new TLInputChannel {ChannelId = (int) chatid};
            }
            catch
            {
                return null;
            }
        }

        internal static TLAbsInputUser GetPeerUserFromdId(int userId)
        {
            try
            {
                return new TLInputUser {UserId = userId};
            }
            catch
            {
                return null;
            }
        }
    }
}
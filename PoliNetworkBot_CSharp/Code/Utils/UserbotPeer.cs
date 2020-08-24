using System;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class UserbotPeer
    {
        internal static TLAbsInputPeer GetPeerFromIdAndType(long chatid, ChatType chatType)
        {
            switch (chatType)
            {
                case Telegram.Bot.Types.Enums.ChatType.Private:
                    {
                        return new TLInputPeerUser() { UserId = (int)chatid };
                    }

                case Telegram.Bot.Types.Enums.ChatType.Channel:
                    {
                        return new TLInputPeerChannel() { ChannelId = (int)chatid };
                    }

                default:
                    {
                        return new TLInputPeerChat() { ChatId = (int)chatid };
                    }
            }
            throw new NotImplementedException();
        }

        internal static TLAbsInputChannel GetPeerChannelFromIdAndType(long chatid)
        {
            try
            {
                return new TLInputChannel() { ChannelId = (int)chatid };
            }
            catch
            {
                return null;
            }
        }

        internal static TLAbsInputUser GetPeerUserFromdId(int user_id)
        {
            try
            {
                return new TLInputUser() { UserId = user_id };
            }
            catch
            {
                return null;
            }
        }
    }
}
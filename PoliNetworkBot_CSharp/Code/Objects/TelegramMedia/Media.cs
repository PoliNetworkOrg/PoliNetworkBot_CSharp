#region

using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramMedia
{
    public abstract class Media
    {
        public abstract MessageType? GetMediaBotType();

        public abstract TLAbsInputMedia GetMediaTl();
    }
}
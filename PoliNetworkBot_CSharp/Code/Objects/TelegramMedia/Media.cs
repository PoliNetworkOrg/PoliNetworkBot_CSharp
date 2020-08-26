#region

using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;
using TLSharp.Core;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramMedia
{
    public abstract class Media
    {
        public abstract MessageType? GetMediaBotType();

        public abstract Task<TlFileToSend> GetMediaTl(TelegramClient client);
    }
}
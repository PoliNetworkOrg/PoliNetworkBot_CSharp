using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using TLSharp.Core;

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramMedia
{
    public class GenericMedia : Objects.TelegramMedia.GenericFile
    {
        internal string _fileId;
        internal int _width;
        internal int _height;
        
        public override MessageType? GetMediaBotType()
        {
            throw new System.NotImplementedException();
        }

        public override Task<TlFileToSend> GetMediaTl(TelegramClient client)
        {
            throw new System.NotImplementedException();
        }
        
        public InputOnlineFile GetTelegramBotInputOnlineFile()
        {
            var r = new InputOnlineFile(_fileId);
            return r;
        }
    }
}
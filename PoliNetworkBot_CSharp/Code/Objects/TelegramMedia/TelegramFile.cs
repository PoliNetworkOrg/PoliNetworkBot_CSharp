#region

using System.IO;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using TeleSharp.TL;
using TLSharp.Core;
using TLSharp.Core.Utils;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramMedia
{
    internal class TelegramFile : Objects.TelegramMedia.Media
    {
        private readonly string _fileName;
        private readonly Stream _stream;
        private string caption;

        public TelegramFile(Stream stream, string fileName)
        {
            _stream = stream;
            _fileName = fileName;
        }

        internal InputOnlineFile GetOnlineFile()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            return new InputOnlineFile(_stream, _fileName);
        }

        public override MessageType? GetMediaBotType()
        {
            return MessageType.Document;
        }

        public override async Task<Objects.TelegramMedia.TlFileToSend> GetMediaTl(TelegramClient client)
        {
            _stream.Seek(0, SeekOrigin.Begin);
            var streamReader = new StreamReader(this._stream);
            var r = await client.UploadFile(this._fileName, streamReader);
            if (r == null)
                return null;

            if (r is TLInputFile r2)
            {
                return new TlFileToSend(r2);
            }

            return null;
        }
    }
}
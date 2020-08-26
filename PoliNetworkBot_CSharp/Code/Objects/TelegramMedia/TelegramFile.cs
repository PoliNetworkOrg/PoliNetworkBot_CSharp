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
        private readonly string _caption;
        private readonly string _mimeType;

        public TelegramFile(Stream stream, string fileName, string caption, string mimeType)
        {
            _stream = stream;
            _fileName = fileName;
            this._caption = caption;
            this._mimeType = mimeType;
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
            
            return r switch
            {
                null => null,
                TLInputFile r2 => new TlFileToSend(r2, _mimeType, null),
                _ => null
            };
        }
    }
}
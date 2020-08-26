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
    public class TelegramFile : Media
    {
        private readonly string _caption;
        private readonly string _fileName;
        private readonly string _mimeType;
        private readonly Stream _stream;

        public TelegramFile(Stream stream, string fileName, string caption, string mimeType)
        {
            _stream = stream;
            _fileName = fileName;
            _caption = caption;
            _mimeType = mimeType;
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

        public override async Task<TlFileToSend> GetMediaTl(TelegramClient client)
        {
            _stream.Seek(0, SeekOrigin.Begin);
            var streamReader = new StreamReader(_stream);
            var r = await client.UploadFile(_fileName, streamReader);

            var attributes = new TLVector<TLAbsDocumentAttribute>();
            TLAbsDocumentAttribute att1 = new TLDocumentAttributeFilename {FileName = _fileName};
            attributes.Add(att1);
            return r switch
            {
                null => null,
                TLInputFile r2 => new TlFileToSend(r2, _mimeType, attributes),
                _ => null
            };
        }
    }
}
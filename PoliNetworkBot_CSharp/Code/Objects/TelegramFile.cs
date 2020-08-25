#region

using System.IO;
using Telegram.Bot.Types.InputFiles;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    internal class TelegramFile
    {
        private readonly string _fileName;
        private readonly Stream _stream;

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
    }
}
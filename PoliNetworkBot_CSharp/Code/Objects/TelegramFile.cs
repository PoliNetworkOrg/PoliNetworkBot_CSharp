using System.IO;
using Telegram.Bot.Types.InputFiles;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    internal class TelegramFile
    {
        private readonly Stream stream;
        private readonly string fileName;

        public TelegramFile(Stream stream, string fileName)
        {
            this.stream = stream;
            this.fileName = fileName;
        }

        internal InputOnlineFile GetOnlineFile()
        {
            stream.Seek(0, SeekOrigin.Begin);
            return new InputOnlineFile(stream, this.fileName);
        }
    }
}
#region

using System.IO;
using Telegram.Bot.Types.InputFiles;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    internal class TelegramFile
    {
        private readonly string fileName;
        private readonly Stream stream;

        public TelegramFile(Stream stream, string fileName)
        {
            this.stream = stream;
            this.fileName = fileName;
        }

        internal InputOnlineFile GetOnlineFile()
        {
            stream.Seek(0, SeekOrigin.Begin);
            return new InputOnlineFile(stream, fileName);
        }
    }
}
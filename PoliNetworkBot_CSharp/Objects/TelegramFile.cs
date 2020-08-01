using System;
using System.IO;
using Telegram.Bot.Types.InputFiles;

namespace PoliNetworkBot_CSharp
{
    internal class TelegramFile
    {
        private Stream stream;
        private string fileName;

        public TelegramFile(Stream stream, string fileName)
        {
            this.stream = stream;
            this.fileName = fileName;
        }

        internal InputOnlineFile GetOnlineFile()
        {
            return new InputOnlineFile(this.stream, this.fileName);
        }
    }
}
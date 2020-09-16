using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using System;
using System.IO;

namespace PoliNetworkBot_CSharp.Code.Utils.UtilsMedia
{
    internal class UtilsFileText
    {
        internal static TelegramFile GenerateFileFromString(string data, string fileName, string caption, string mimeType)
        {
            System.IO.Stream stream = GenerateStreamFromString(data);
            TelegramFile telegramFile = new TelegramFile(stream, fileName, caption, mimeType);
            return telegramFile;
        }



        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
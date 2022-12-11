#region

using System.IO;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;

internal static class UtilsFileText
{
    internal static TelegramFile GenerateFileFromString(string data, string fileName, Language? caption,
        TextAsCaption textAsCaption,
        string? mimeType = "application/json")
    {
        var stream = GenerateStreamFromString(data);
        var telegramFile = new TelegramFile(stream, fileName, caption, mimeType, textAsCaption);
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
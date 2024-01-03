#region

using System.IO;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;
using TLSharp.Core;
using TLSharp.Core.Utils;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;

public class TelegramFile : GenericFile
{
    private readonly Language? _captionOrText;
    private readonly string _fileName;
    private readonly string? _mimeType;
    private readonly Stream? _stream;
    public readonly TextAsCaption TextAsCaption;

    public TelegramFile(Stream? stream, string fileName, Language? captionOrText, string? mimeType,
        TextAsCaption textAsCaption)
    {
        _stream = stream;
        _fileName = fileName;
        _captionOrText = captionOrText;
        _mimeType = mimeType;
        TextAsCaption = textAsCaption;
    }

    internal InputFile? GetOnlineFile()
    {
        _stream?.Seek(0, SeekOrigin.Begin);
        if (_stream != null)
        {
            var onlineFile = new InputFileStream(_stream, _fileName);
            return onlineFile;
        }


        return null;

    }

    public override MessageType? GetMediaBotType()
    {
        return MessageType.Document;
    }

    public override async Task<TlFileToSend?> GetMediaTl(TelegramClient? client)
    {
        if (_stream == null) return null;
        _stream.Seek(0, SeekOrigin.Begin);
        var streamReader = new StreamReader(_stream);
        var r = await client.UploadFile(_fileName, streamReader);

        var attributes = new TLVector<TLAbsDocumentAttribute>();
        TLAbsDocumentAttribute att1 = new TLDocumentAttributeFilename { FileName = _fileName };
        attributes.Add(att1);
        return r switch
        {
            null => null,
            TLInputFile r2 => new TlFileToSend(r2, _mimeType, attributes),
            _ => null
        };
    }

    public static TelegramFile FromString(string json, string fileName, Language caption,
        TextAsCaption textAsCaptionParam)
    {
        return UtilsFileText.GenerateFileFromString(json, fileName, caption, textAsCaptionParam);
    }

    public static TelegramFile FromStreamJson(Stream stream, string filename, Language? caption,
        TextAsCaption textAsCaptionParam)
    {
        return new TelegramFile(stream, filename, caption, "application/json", textAsCaptionParam);
    }

    public string? GetText(string? lang)
    {
        return _captionOrText?.Select(lang);
    }
}
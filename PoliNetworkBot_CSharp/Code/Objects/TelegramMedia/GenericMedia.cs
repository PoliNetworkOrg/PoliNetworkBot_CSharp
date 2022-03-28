#region

using System;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using TLSharp.Core;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;

public class GenericMedia : GenericFile
{
    internal string _fileId;
    internal int _height;
    internal int _width;

    public override MessageType? GetMediaBotType()
    {
        throw new NotImplementedException();
    }

    public override Task<TlFileToSend> GetMediaTl(TelegramClient client)
    {
        throw new NotImplementedException();
    }

    public InputOnlineFile GetTelegramBotInputOnlineFile()
    {
        var r = new InputOnlineFile(_fileId);
        return r;
    }
}
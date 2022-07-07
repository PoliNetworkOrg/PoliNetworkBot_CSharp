#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;

public class ObjectPhoto : GenericMedia
{
    private readonly long _chatId;
    private readonly ChatType _chatType;
    private readonly int _fileSize;
    private readonly int _idPhotoDb;
    private readonly long? _messageIdFrom;
    private readonly string? _uniqueId;

    public ObjectPhoto(int idPhotoDb, string? fileId, int fileSize, int height, int width,
        string? uniqueId, long? messageIdFrom, long chatId, ChatType chatType)
    {
        _idPhotoDb = idPhotoDb;
        FileId = fileId;
        _fileSize = fileSize;
        Height = height;
        Width = width;
        _uniqueId = uniqueId;
        _messageIdFrom = messageIdFrom;
        _chatId = chatId;
        _chatType = chatType;
    }

    public async Task<Tuple<TLAbsInputFile, string>?> GetTelegramUserBotInputPhoto(TelegramClient? userbot)
    {
        if (_messageIdFrom == null)
            return null;
        TLMessagesSlice? tlMessagesSlice = null;
        var filename = "photo" + _uniqueId;
        var peer = UserbotPeer.GetPeerFromIdAndType(_chatId, _chatType);
        const int offsetDate = 0;
        if (userbot != null)
        {
            var r = await userbot.GetHistoryAsync(peer, (int)_messageIdFrom.Value,
                offsetDate, 0, 1);

            if (r is not TLMessagesSlice slice) return null;
            tlMessagesSlice = slice;
        }

        if (tlMessagesSlice != null && tlMessagesSlice.Messages.Count != 1) return null;
        var t = tlMessagesSlice?.Messages[0];
        if (t is not TLMessage t2)
            return null;

        var t3 = t2.Media;

        if (t3 is not TLMessageMediaPhoto tlPhoto) return null;
        var t4 = tlPhoto.Photo;
        if (t4 is not TLPhoto t5) return null;

        var t6 = t5.Sizes;
        var t7 = BestPhoto(t6);
        return t7 switch
        {
            null => null,
            //todo
            TLPhotoSize t8 => new Tuple<TLAbsInputFile, string>(new TLInputFile(), filename),
            //var fileResult = (TLInputFile)await userbot.UploadFile(filename, new StreamReader("tmp/" + filename));
            //return fileResult;
            _ => null
        };
    }

    private static TLAbsPhotoSize? BestPhoto(IEnumerable<TLAbsPhotoSize?> t6)
    {
        TLAbsPhotoSize? r = null;
        var max = -1;
        foreach (var t7 in t6)
            switch (t7)
            {
                case null:
                    continue;
                case TLPhotoSize t8 when t8.H <= max:
                    continue;
                case TLPhotoSize t8:
                    max = t8.H;
                    r = t8;
                    break;
            }

        return r;
    }
}
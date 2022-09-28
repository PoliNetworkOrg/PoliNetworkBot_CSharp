#region

using System;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Utils;
using TeleSharp.TL;
using TLSharp.Core;
using StringUtil = PoliNetworkBot_CSharp.Code.Utils.StringUtil;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;

public class TlFileToSend
{
    private readonly TLVector<TLAbsDocumentAttribute>? _attributes;
    private readonly string? _mimeType;
    private readonly TLAbsInputMedia? _tlAbsInputMedia;
    private readonly TLInputFile? _tlInputFile;

    public TlFileToSend(TLInputFile? r2, string? mimeType, TLVector<TLAbsDocumentAttribute>? attributes)
    {
        _tlInputFile = r2;
        _mimeType = mimeType;
        _attributes = attributes;
    }

    public TlFileToSend(TLAbsInputMedia? r2)
    {
        _tlAbsInputMedia = r2;
    }

    public async Task<TLAbsUpdates?> SendMedia(TLAbsInputPeer? peer, TelegramClient? telegramClient,
        string? caption, string? username)
    {
        TLAbsUpdates? r2 = null;
        try
        {
            r2 = await SendMedia2(peer, telegramClient, caption);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        if (r2 != null)
            return r2;

        if (string.IsNullOrEmpty(username))
            return null;

        try
        {
            peer = await UserbotPeer.GetPeerUserWithAccessHash(username, telegramClient);
            var r = await SendMedia2(peer, telegramClient, caption);
            return r;
        }
        catch (Exception e2)
        {
            Console.WriteLine(e2);
        }

        return null;
    }

    private async Task<TLAbsUpdates?> SendMedia2(TLAbsInputPeer? peer, TelegramClient? telegramClient,
        string? caption)
    {
        if (_tlInputFile == null)
            return _tlAbsInputMedia == null ? null :
                telegramClient != null ? await telegramClient.Messages_SendMedia(peer, _tlAbsInputMedia) : null;
        try
        {
            var caption2 = StringUtil.NotNull(caption);

            if (telegramClient != null)
            {
                var r = await telegramClient.SendUploadedDocument(peer, _tlInputFile, caption2, _mimeType,
                    _attributes);
                return r;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }

        return _tlAbsInputMedia == null ? null :
            telegramClient != null ? await telegramClient.Messages_SendMedia(peer, _tlAbsInputMedia) : null;
    }

    public async Task<TLAbsUpdates?> SendMedia(TLAbsInputPeer? peer, TelegramClient? telegramClient, Language? caption, string? username, string? lang)
    {
        var captionString = StringUtil.NotNull(caption,lang);
        return await SendMedia(peer, telegramClient, captionString, username);
    }
}
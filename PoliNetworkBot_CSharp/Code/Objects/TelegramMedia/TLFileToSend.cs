using System;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Utils;
using TeleSharp.TL;
using TLSharp.Core;

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramMedia
{
    public class TlFileToSend
    {
        private readonly TLInputFile _tlInputFile;
        private readonly TLAbsInputMedia _tlAbsInputMedia;
        private string mimeType;
        private TLVector<TLAbsDocumentAttribute> attributes;
        
        public TlFileToSend(TLInputFile r2, string mimeType, TLVector<TLAbsDocumentAttribute> attributes)
        {
            this._tlInputFile = r2;
            this.mimeType = mimeType;
            this.attributes = attributes;
        }

        public TlFileToSend(TLAbsInputMedia r2)
        {
            this._tlAbsInputMedia = r2;
        }

        public async Task<TLAbsUpdates> SendMedia(TLAbsInputPeer peer, TelegramClient telegramClient,
            string caption, string username)
        {
            TLAbsUpdates r2 = null;
            try
            {
                r2 = await SendMedia2(peer, telegramClient, caption);
            }
            catch (Exception e)
            {
                ;
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
                return null;
            }

            return null;
            
        }

        private async Task<TLAbsUpdates> SendMedia2(TLAbsInputPeer peer, TelegramClient telegramClient, string caption)
        {
            if (this._tlInputFile != null)
            {
                try
                {
                    caption ??= "";
                    
                    var r = await telegramClient.SendUploadedDocument(peer, this._tlInputFile,
                        caption: caption, mimeType: mimeType, attributes: attributes);
                    return r;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            else if (this._tlAbsInputMedia != null)
            {
                return await telegramClient.Messages_SendMedia(peer, this._tlAbsInputMedia);
            }

            return null;
        }
    }
}
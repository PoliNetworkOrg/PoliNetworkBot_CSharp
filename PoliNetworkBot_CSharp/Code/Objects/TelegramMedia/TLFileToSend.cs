using System.Threading.Tasks;
using TeleSharp.TL;
using TLSharp.Core;

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramMedia
{
    public class TlFileToSend
    {
        private readonly TLInputFile _tlInputFile;
        private readonly TLAbsInputMedia _tlAbsInputMedia;
        
        public TlFileToSend(TLInputFile r2)
        {
            this._tlInputFile = r2;
        }

        public TlFileToSend(TLAbsInputMedia r2)
        {
            this._tlAbsInputMedia = r2;
        }

        public async Task<TLAbsUpdates> SendMedia(TLAbsInputPeer peer, TelegramClient telegramClient,
            string caption)
        {
            if (this._tlInputFile != null)
            {
                TLVector<TLAbsDocumentAttribute> attributes = null; //todo
                var mimeType = ""; //todo
                var r = await telegramClient.SendUploadedDocument(peer, this._tlInputFile, 
                    caption: caption, mimeType: mimeType, attributes: attributes);
                return r;
            }
            else if (this._tlAbsInputMedia != null)
            {
                return await telegramClient.Messages_SendMedia(peer, this._tlAbsInputMedia);
            }

            return null;
        }
    }
}
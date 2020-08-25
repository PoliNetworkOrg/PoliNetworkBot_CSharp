using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using TeleSharp.TL;
using TLSharp.Core;
using TLSharp.Core.Utils;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class ObjectPhoto
    {
        private int _idPhotoDb;
        private readonly string _fileId;
        private int _fileSize;
        private int _height;
        private int _width;
        private string _uniqueId;
        private int? messageIdFrom;
        private long chatId;
        private ChatType chatType;

        public ObjectPhoto(int idPhotoDb, string fileId, int fileSize, int height, int width, 
            string uniqueId, int? messageIdFrom, long chatId, ChatType chatType)
        {
            this._idPhotoDb = idPhotoDb;
            this._fileId = fileId;
            this._fileSize = fileSize;
            this._height = height;
            this._width = width;
            this._uniqueId = uniqueId;
            this.messageIdFrom = messageIdFrom;
            this.chatId = chatId;
            this.chatType = chatType;
        }
        
        public InputOnlineFile GetTelegramBotInputOnlineFile()
        {
            var r = new  Telegram.Bot.Types.InputFiles.InputOnlineFile(this._fileId);
            return r;
        }

        public async Task<TLAbsInputFile> GetTelegramUserBotInputPhoto(TelegramClient userbot)
        {
            if (this.messageIdFrom == null)
                return null;
            
            var filename = "photo" + _uniqueId;
            var peer = UserbotPeer.GetPeerFromIdAndType(this.chatId, this.chatType);
            const int offsetDate = 0;
            var r = await userbot.GetHistoryAsync(peer, this.messageIdFrom.Value,
                offsetDate: offsetDate, 0, limit: 1);
            
            if (r == null)
            {
                return null;
            }

            if (r is TeleSharp.TL.Messages.TLMessagesSlice tlMessagesSlice)
            {
                if (tlMessagesSlice.Messages.Count == 1)
                {
                    var t = tlMessagesSlice.Messages[0];
                    if (t == null)
                        return null;

                    if (t is TeleSharp.TL.TLMessage t2)
                    {
                        var t3 = t2.Media;
                        if (t3 == null)
                            return null;

                        if (t3 is TeleSharp.TL.TLMessageMediaPhoto tlPhoto)
                        {
                            var t4 = tlPhoto.Photo;
                            if (t4 == null)
                            {
                                return null;
                            }

                            if (t4 is TLPhoto t5)
                            {
                                var t6 = t5.Sizes;
                                var t7 = BestPhoto(t6);
                                if (t7 == null)
                                    return null;

                                if (t7 is TLPhotoSize t8)
                                {
                                    //todo
                                }
                                
                                //var fileResult = (TLInputFile)await userbot.UploadFile(filename, new StreamReader("tmp/" + filename));
                                //return fileResult;
                            }
                        }
                    }
                }
            }

            return null;
        }

        private static TLAbsPhotoSize BestPhoto(IEnumerable<TLAbsPhotoSize> t6)
        {
            TLAbsPhotoSize r = null;
            var max = -1;
            foreach (var t7 in t6)
            {
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
            }

            return r;
        }
    }
}
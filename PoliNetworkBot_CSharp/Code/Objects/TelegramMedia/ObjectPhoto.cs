#region

using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramMedia
{
    public class ObjectPhoto : GenericMedia
    {
        private readonly string _uniqueId;
        private readonly long _chatId;
        private readonly ChatType _chatType;
        private int _fileSize;
        private int _idPhotoDb;
        private int? _messageIdFrom;

        public ObjectPhoto(int idPhotoDb, string fileId, int fileSize, int height, int width,
            string uniqueId, int? messageIdFrom, long chatId, ChatType chatType)
        {
            _idPhotoDb = idPhotoDb;
            this._fileId = fileId;
            _fileSize = fileSize;
            _height = height;
            _width = width;
            _uniqueId = uniqueId;
            this._messageIdFrom = messageIdFrom;
            this._chatId = chatId;
            this._chatType = chatType;
        }

 

        public async Task<TLAbsInputFile> GetTelegramUserBotInputPhoto(TelegramClient userbot)
        {
            if (_messageIdFrom == null)
                return null;

            var filename = "photo" + _uniqueId;
            var peer = UserbotPeer.GetPeerFromIdAndType(_chatId, _chatType);
            const int offsetDate = 0;
            var r = await userbot.GetHistoryAsync(peer, _messageIdFrom.Value,
                offsetDate, 0, 1);

            if (r == null) return null;

            if (r is TLMessagesSlice tlMessagesSlice)
                if (tlMessagesSlice.Messages.Count == 1)
                {
                    var t = tlMessagesSlice.Messages[0];
                    if (t == null)
                        return null;

                    if (t is TLMessage t2)
                    {
                        var t3 = t2.Media;
                        if (t3 == null)
                            return null;

                        if (t3 is TLMessageMediaPhoto tlPhoto)
                        {
                            var t4 = tlPhoto.Photo;
                            if (t4 == null) return null;

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

            return null;
        }

        private static TLAbsPhotoSize BestPhoto(IEnumerable<TLAbsPhotoSize> t6)
        {
            TLAbsPhotoSize r = null;
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
}
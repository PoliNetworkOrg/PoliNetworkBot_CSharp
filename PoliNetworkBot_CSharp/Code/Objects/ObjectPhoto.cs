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

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class ObjectPhoto
    {
        private readonly string _fileId;
        private int _fileSize;
        private int _height;
        private int _idPhotoDb;
        private readonly string _uniqueId;
        private int _width;
        private readonly long chatId;
        private readonly ChatType chatType;
        private int? messageIdFrom;

        public ObjectPhoto(int idPhotoDb, string fileId, int fileSize, int height, int width,
            string uniqueId, int? messageIdFrom, long chatId, ChatType chatType)
        {
            _idPhotoDb = idPhotoDb;
            _fileId = fileId;
            _fileSize = fileSize;
            _height = height;
            _width = width;
            _uniqueId = uniqueId;
            this.messageIdFrom = messageIdFrom;
            this.chatId = chatId;
            this.chatType = chatType;
        }

        public InputOnlineFile GetTelegramBotInputOnlineFile()
        {
            var r = new InputOnlineFile(_fileId);
            return r;
        }

        public async Task<TLAbsInputFile> GetTelegramUserBotInputPhoto(TelegramClient userbot)
        {
            if (messageIdFrom == null)
                return null;

            var filename = "photo" + _uniqueId;
            var peer = UserbotPeer.GetPeerFromIdAndType(chatId, chatType);
            const int offsetDate = 0;
            var r = await userbot.GetHistoryAsync(peer, messageIdFrom.Value,
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
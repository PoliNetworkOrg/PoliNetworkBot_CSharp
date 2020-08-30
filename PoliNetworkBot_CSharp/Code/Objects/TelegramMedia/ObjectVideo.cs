using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using TeleSharp.TL;
using TLSharp.Core;

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramMedia
{
    public class ObjectVideo : Objects.TelegramMedia.GenericMedia
    {
        private readonly string _uniqueId;
        private readonly long _chatId;
        private readonly ChatType _chatType;
        private int _fileSize;
        private int _idVideoDb;
        private int? _messageIdFrom;
        private readonly int _durationVideo;
        
        
        public ObjectVideo(int idVideoDb, string fileId, int fileSize, int height, int width,
            string uniqueId, int? messageIdFrom, long chatId, ChatType chatType, int durationVideo)
        {
            _idVideoDb = idVideoDb;
            this._fileId = fileId;
            _fileSize = fileSize;
            _height = height;
            _width = width;
            _uniqueId = uniqueId;
            this._messageIdFrom = messageIdFrom;
            this._chatId = chatId;
            this._chatType = chatType;
            this._durationVideo = durationVideo;
        }

        public int GetDuration()
        {
            return _durationVideo;
        }

        public override MessageType? GetMediaBotType()
        {
            return MessageType.Video;
        }

        public int GetHeight()
        {
            return this._height;
        }

        public int GetWidth()
        {
            return _width;
        }

        public async Task<TLAbsInputFile> GetTelegramUserBotInputVideo(TelegramClient userbotClient)
        {
            throw new System.NotImplementedException();
        }

        public TLAbsInputMedia GetTLabsInputMedia()
        {
            TLAbsInputMedia r = new TLInputMediaPhoto();


            return r;
        }
    }
}
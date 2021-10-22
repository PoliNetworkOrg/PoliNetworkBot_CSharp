using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    public class MessageToDelete
    {
        private readonly long? accessHash;
        private readonly long botId;
        private readonly long chatId;
        private readonly ChatType? chatType;
        private readonly int messageId;
        private readonly DateTime timeToDelete;

        public MessageToDelete(TLMessage r3, long chatId, DateTime timeToDelete, long botId, ChatType? chatType,
            long? accessHash)
        {
            messageId = r3.Id;
            this.chatId = chatId;
            this.timeToDelete = timeToDelete;
            this.botId = botId;
            this.chatType = chatType;
            this.accessHash = accessHash;
        }

        public MessageToDelete(Message r4, long chatId, DateTime timeToDelete, long botId, ChatType? chatType,
            long? accessHash)
        {
            messageId = r4.MessageId;
            this.chatId = chatId;
            this.timeToDelete = timeToDelete;
            this.botId = botId;
            this.chatType = chatType;
            this.accessHash = accessHash;
        }

        internal bool ToDelete()
        {
            if (DateTime.Now > timeToDelete)
                return true;
            return false;
        }

        internal async Task<bool> Delete()
        {
            var bot = GlobalVariables.Bots[botId];
            if (bot == null)
                return false;

            try
            {
                return await bot.DeleteMessageAsync(chatId, messageId, chatType, accessHash);
            }
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwners(e, bot);
            }

            return false;
        }
    }
}
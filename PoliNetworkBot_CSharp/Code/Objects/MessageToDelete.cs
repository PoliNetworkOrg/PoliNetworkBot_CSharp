#region

using System;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    public class MessageToDelete
    {
        private readonly long? accessHash;
        private readonly long botId;
        private readonly long chatId;
#pragma warning disable IDE0052 // Rimuovi i membri privati non letti
        private readonly ChatType? chatType;
#pragma warning restore IDE0052 // Rimuovi i membri privati non letti
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
            return DateTime.Now > timeToDelete;
        }

        internal async Task<bool> Delete(MessageEventArgs e2)
        {
            if (GlobalVariables.Bots.ContainsKey(botId) == false)
                return false;

            var bot = GlobalVariables.Bots[botId];
            if (bot == null)
                return false;

            try
            {
                return await bot.DeleteMessageAsync(chatId, messageId, accessHash);
            }
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwners(e, bot, e2);
            }

            return false;
        }
    }
}
#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public static class MessageDb
    {
        private static readonly Dictionary<int, string> MessageTypesInRam = new Dictionary<int, string>();

        internal static bool AddMessage(MessageType type, string messageText,
            int messageFromIdPerson, int? messageFromIdEntity,
            int photoId, long idChatSentInto, DateTime? sentDate,
            bool hasBeenSent, int messageFromIdBot, int messageIdTgFrom)
        {
            const string q = "INSERT INTO Messages " +
                             "(id, from_id_person, from_id_entity, type, " +
                             "id_photo, message_text, id_chat_sent_into, sent_date," +
                             " has_been_sent, from_id_bot, message_id_tg_from) " +
                             "VALUES " +
                             "(@id, @fip, @fie, @t, @idp, @mt, @icsi, @sent_date, @hbs, @fib, @mitf);";

            var typeI = GetMessageTypeByName(type);
            if (typeI == null) return false;

            var id = Tables.GetMaxId("Messages", "id");
            id++;

            SqLite.Execute(q, new Dictionary<string, object>
            {
                {"@id", id},
                {"@fip", messageFromIdPerson},
                {"@fie", messageFromIdEntity},
                {"@t", typeI},
                {"@idp", photoId},
                {"@mt", messageText},
                {"@icsi", idChatSentInto},
                {"@sent_date", sentDate},
                {"@hbs", hasBeenSent},
                {"@fib", messageFromIdBot},
                {"@mitf", messageIdTgFrom}
            });

            return true;
        }

        private static int? GetMessageTypeByName(MessageType type, int times = 1)
        {
            while (true)
            {
                if (times < 0) return null;

                const string q1 = "SELECT id FROM MessageTypes WHERE name = @name";
                var keyValuePairs = new Dictionary<string, object> {{"@name", type.ToString()}};
                var r1 = SqLite.ExecuteSelect(q1, keyValuePairs);
                var r2 = SqLite.GetFirstValueFromDataTable(r1);
                if (r1 == null || r1.Rows.Count == 0 || r2 == null)
                {
                    AddMessageType(type);
                    times--;
                    continue;
                }

                try
                {
                    return Convert.ToInt32(r2);
                }
                catch
                {
                    return null;
                }
            }
        }

        private static void AddMessageType(MessageType type)
        {
            const string q = "INSERT INTO MessageTypes (name) VALUES (@name)";
            var keyValuePairs = new Dictionary<string, object> {{"@name", type.ToString()}};
            SqLite.Execute(q, keyValuePairs);
            Tables.FixIdTable("MessageTypes", "id", "name");
        }

        public static async Task CheckMessagesToSend()
        {
            const string q =
                "SELECT * FROM Messages WHERE Messages.has_been_sent IS FALSE AND Messages.sent_date IS NOT NULL AND julianday('now') - julianday(Messages.sent_date) <= 0";
            var dt = SqLite.ExecuteSelect(q);
            if (dt == null || dt.Rows.Count == 0)
                return;

            foreach (DataRow dr in dt.Rows)
                try
                {
                    await SendMessageToSend(dr);
                }
                catch
                {
                    //ignored
                }
        }

        private static async Task SendMessageToSend(DataRow dr)
        {
            var done = await SendMessageFromDataRow(dr);
            if (!done)
                return;

            var q2 = "UPDATE Messages SET has_been_sent = TRUE WHERE id = " + dr["id"];
            SqLite.Execute(q2);
        }

        private static async Task<bool> SendMessageFromDataRow(DataRow dr)
        {
            var botId = Convert.ToInt32(dr["from_id_bot"]);
            var botClass = GlobalVariables.Bots[botId];

            var typeI = Convert.ToInt32(dr["type"]);
            var typeT = GetMessageTypeClassById(typeI);
            if (typeT == null)
                return false;

            switch (typeT.Value)
            {
                case MessageType.Unknown:
                    break;
                case MessageType.Text:
                    SendTextFromDataRow(dr, botClass);
                    return true;
                case MessageType.Photo:
                    return await SendPhotoFromDataRow(dr, botClass);
                case MessageType.Audio:
                    break;
                case MessageType.Video:
                    return await SendVideoFromDataRow(dr, botClass);
                case MessageType.Voice:
                    break;
                case MessageType.Document:
                    break;
                case MessageType.Sticker:
                    break;
                case MessageType.Location:
                    break;
                case MessageType.Contact:
                    break;
                case MessageType.Venue:
                    break;
                case MessageType.Game:
                    break;
                case MessageType.VideoNote:
                    break;
                case MessageType.Invoice:
                    break;
                case MessageType.SuccessfulPayment:
                    break;
                case MessageType.WebsiteConnected:
                    break;
                case MessageType.ChatMembersAdded:
                    break;
                case MessageType.ChatMemberLeft:
                    break;
                case MessageType.ChatTitleChanged:
                    break;
                case MessageType.ChatPhotoChanged:
                    break;
                case MessageType.MessagePinned:
                    break;
                case MessageType.ChatPhotoDeleted:
                    break;
                case MessageType.GroupCreated:
                    break;
                case MessageType.SupergroupCreated:
                    break;
                case MessageType.ChannelCreated:
                    break;
                case MessageType.MigratedToSupergroup:
                    break;
                case MessageType.MigratedFromGroup:
                    break;
                case MessageType.Animation:
                    break;
                case MessageType.Poll:
                    break;
                case MessageType.Dice:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        public static MessageType? GetMessageTypeClassById(in int typeI)
        {
            var typeS = GetMessageTypeNameById(typeI);

            if (string.IsNullOrEmpty(typeS))
                return null;

            var messageType = Enum.TryParse(typeof(MessageType), typeS, out var typeT);
            if (messageType == false || typeT == null)
                return null;

            if (typeT is MessageType t)
                return t;
            return null;
        }

        private static void SendTextFromDataRow(DataRow dr, TelegramBotAbstract botClass)
        {
            throw new NotImplementedException();
        }

        private static string GetMessageTypeNameById(in int typeI)
        {
            if (MessageTypesInRam.ContainsKey(typeI))
                return MessageTypesInRam[typeI];

            var q = "SELECT name FROM MessageTypes WHERE id = " + typeI;
            var dt = SqLite.ExecuteSelect(q);
            if (dt == null || dt.Rows.Count == 0) return null;

            var value = SqLite.GetFirstValueFromDataTable(dt).ToString();
            if (string.IsNullOrEmpty(value)) return null;

            MessageTypesInRam[typeI] = value;
            return value;
        }

        private static async Task<bool> SendVideoFromDataRow(DataRow dr, TelegramBotAbstract botClass)
        {
            throw new NotImplementedException();
        }

        private static async Task<bool> SendPhotoFromDataRow(DataRow dr, TelegramBotAbstract botClass)
        {
            var photoId = SqLite.GetIntFromColumn(dr, "id_photo");
            if (photoId == null)
                return false;

            var chatIdToSendTo = (long) dr["id_chat_sent_into"];
            var caption = dr["message_text"].ToString();
            var chatIdFromIdPerson = Convert.ToInt64(dr["from_id_person"]);
            int? messageIdFrom = null;
            try
            {
                messageIdFrom = Convert.ToInt32(dr["message_id_tg_from"]);
            }
            catch
            {
                //ignored
            }

            var photo = UtilsPhoto.GetPhotoByIdFromDb(
                photoId.Value,
                messageIdFrom,
                chatIdFromIdPerson,
                ChatType.Private);

            var done = await botClass.SendPhotoAsync(chatIdToSendTo, photo, caption);
            return done;
        }
    }
}
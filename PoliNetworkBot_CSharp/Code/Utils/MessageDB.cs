#region

using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public static class MessageDb
    {
        private static readonly Dictionary<int, string> MessageTypesInRam = new Dictionary<int, string>();

        internal static bool AddMessage(MessageType type, string messageText,
            int messageFromIdPerson, int? messageFromIdEntity,
            long idChatSentInto, DateTime? sentDate,
            bool hasBeenSent, int messageFromIdBot, 
            int messageIdTgFrom, ChatType type_chat_sent_into,
            int? photo_id, int? video_id)
        {
            const string q = "INSERT INTO Messages " +
                             "(id, from_id_person, from_id_entity, type, " +
                             "id_photo, id_video, message_text, id_chat_sent_into, sent_date," +
                             " has_been_sent, from_id_bot, message_id_tg_from, type_chat_sent_into) " +
                             "VALUES " +
                             "(@id, @fip, @fie, @t, @idp, @idv, @mt, @icsi, @sent_date, @hbs, @fib, @mitf, @tcsi);";

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
                {"@idp", photo_id},
                {"@idv", video_id},
                {"@mt", messageText},
                {"@icsi", idChatSentInto},
                {"@sent_date", sentDate},
                {"@hbs", hasBeenSent},
                {"@fib", messageFromIdBot},
                {"@mitf", messageIdTgFrom},
                {"@tcsi", type_chat_sent_into.ToString()}
            });

            return true;
        }

        private static int? GetMessageTypeByName(MessageType type, int times = 1)
        {
            while (true)
            {
                if (times < 0) return null;

                const string q1 = "SELECT id FROM MessageTypes WHERE name = @name";
                var keyValuePairs = new Dictionary<string, object> { { "@name", type.ToString() } };
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
            var keyValuePairs = new Dictionary<string, object> { { "@name", type.ToString() } };
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
                    _ = await SendMessageToSend(dr, null, schedule: true);
                }
                catch
                {
                    //ignored
                }
        }

        private static async Task<Code.Enums.ScheduleMessageSentResult> SendMessageToSend(DataRow dr, TelegramBotAbstract telegramBotAbstract, bool schedule)
        {
            DateTime? dt = null;

            try
            {
                dt = (DateTime)dr["sent_date"];
            }
            catch
            {
                ;
            }

            if (schedule && dt == null)
                return Enums.ScheduleMessageSentResult.THE_MESSAGE_IS_NOT_SCHEDULED;

            if (schedule && dt < DateTime.Now)
                return Enums.ScheduleMessageSentResult.NOT_THE_RIGHT_TIME;

            var done = await SendMessageFromDataRow(dr, null, null, extraInfo: false, telegramBotAbstract, 0);
            if (done.IsSuccess() == false)
                return Enums.ScheduleMessageSentResult.FAILED_SEND;

            var q2 = "UPDATE Messages SET has_been_sent = TRUE WHERE id = " + dr["id"];
            SqLite.Execute(q2);

            return Enums.ScheduleMessageSentResult.SUCCESS;
        }

        public static async Task<MessageSend> SendMessageFromDataRow(DataRow dr, int? chatIdToSendTo,
            ChatType? chatTypeToSendTo, bool extraInfo, TelegramBotAbstract telegramBotAbstract, int count)
        {
            var r1 = await SendMessageFromDataRowSingle(dr, chatIdToSendTo, chatTypeToSendTo);

            if (extraInfo)
            {
                var r2 = await SendExtraInfoDbForThisMessage(r1, dr, chatIdToSendTo, chatTypeToSendTo, telegramBotAbstract, count);
                return r2;
            }

            return r1;
        }

        private async static Task<MessageSend> SendExtraInfoDbForThisMessage(MessageSend r1, DataRow dr,
            int? chatIdToSendTo, ChatType? chatTypeToSendTo, TelegramBotAbstract telegramBotAbstract, int count)
        {
            if (r1 == null || r1.IsSuccess() == false)
            {
                return r1;
            }

            if (chatIdToSendTo == null)
            {
                return new MessageSend(false, null, chatTypeToSendTo);
            }

            object dto = dr["sent_date"];
            object fieo = dr["from_id_entity"];
            object fipo = dr["from_id_person"];

            DateTime? dt = null;
            int? from_id_entity = null;
            int? from_id_person = null;

            try
            {
                dt = (DateTime?)dto;
            }
            catch
            {
                ;
            }

            try
            {
                from_id_entity = (int?)fieo;
            }
            catch
            {
                ;
            }

            try
            {
                from_id_person = (int?)fipo;
            }
            catch
            {
                ;
            }

            string text1 = "📌 ID: " + count.ToString() + "\n";
            if (dt != null)
            {
                text1 += "📅 " + Utils.DateTimeClass.DateTimeToItalianFormat(dt) + "\n";
            }
            if (from_id_entity != null)
            {
                string entity_name = Utils.Assoc.GetNameOfEntityFromItsID(from_id_entity.Value);
                text1 += "👥 " + entity_name + "\n";
            }
            if (from_id_person != null)
            {
                text1 += "✍ " + from_id_person.ToString() + "\n";
            }

            Dictionary<string, string> dict = new Dictionary<string, string>() {
                {"en", text1 }
            };
            Language text2 = new Language(dict: dict);
            return await telegramBotAbstract.SendTextMessageAsync(chatIdToSendTo.Value, text2, chatTypeToSendTo, "", ParseMode.Html,
                null, null, r1.GetMessageID(), true);
        }

        private static async Task<Objects.MessageSend> SendMessageFromDataRowSingle(DataRow dr, int? chatIdToSendTo, ChatType? chatTypeToSendTo)
        {
            var botId = Convert.ToInt32(dr["from_id_bot"]);
            var botClass = GlobalVariables.Bots[botId];

            var typeI = Convert.ToInt32(dr["type"]);
            var typeT = GetMessageTypeClassById(typeI);
            if (typeT == null)
                return new MessageSend(false, null, chatTypeToSendTo);

            switch (typeT.Value)
            {
                case MessageType.Unknown:
                    break;

                case MessageType.Text:
                    return SendTextFromDataRow(dr, botClass);

                case MessageType.Photo:
                    return await SendPhotoFromDataRow(dr, botClass, ParseMode.Html, chatIdToSendTo, chatTypeToSendTo);

                case MessageType.Audio:
                    break;

                case MessageType.Video:
                    return await SendVideoFromDataRow(dr, botClass, ParseMode.Html, chatIdToSendTo, chatTypeToSendTo);

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

            return new MessageSend(false, null, chatTypeToSendTo);
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

        private static MessageSend SendTextFromDataRow(DataRow dr, TelegramBotAbstract botClass)
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

        private static async Task<MessageSend> SendVideoFromDataRow(DataRow dr, TelegramBotAbstract botClass,
            ParseMode parseMode, int? chatIdToSendTo2, ChatType? chatTypeToSendTo)
        {
            var videoId = SqLite.GetIntFromColumn(dr, "id_video");
            if (videoId == null)
                return new MessageSend(false, null, chatTypeToSendTo);

            var chatIdToSendTo = (long)dr["id_chat_sent_into"];
            if (chatIdToSendTo2 != null)
                chatIdToSendTo = chatIdToSendTo2.Value;

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

            var typeOfChatSentInto = ChatTypeUtil.GetChatTypeFromString(dr["type_chat_sent_into"]);

            if (chatTypeToSendTo != null)
                typeOfChatSentInto = chatTypeToSendTo;

            if (typeOfChatSentInto == null)
                return new MessageSend(false, null, chatTypeToSendTo);

            var video = UtilsVideo.GetVideoByIdFromDb(
                videoId.Value,
                messageIdFrom,
                chatIdFromIdPerson,
                ChatType.Private);

            return await botClass.SendVideoAsync(chatIdToSendTo, video,
                caption, parseMode, typeOfChatSentInto.Value);
        }

        private static async Task<MessageSend> SendPhotoFromDataRow(DataRow dr, TelegramBotAbstract botClass,
            ParseMode parseMode, int? chatIdToSendTo2, ChatType? chatTypeToSendTo)
        {
            var photoId = SqLite.GetIntFromColumn(dr, "id_photo");
            if (photoId == null)
                return new MessageSend(false, null, chatTypeToSendTo);

            var chatIdToSendTo = (long)dr["id_chat_sent_into"];
            if (chatIdToSendTo2 != null)
                chatIdToSendTo = chatIdToSendTo2.Value;

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

            var typeOfChatSentInto = ChatTypeUtil.GetChatTypeFromString(dr["type_chat_sent_into"]);

            if (chatTypeToSendTo != null)
                typeOfChatSentInto = chatTypeToSendTo;

            if (typeOfChatSentInto == null)
                return new MessageSend(false, null, chatTypeToSendTo);

            var photo = UtilsPhoto.GetPhotoByIdFromDb(
                photoId.Value,
                messageIdFrom,
                chatIdFromIdPerson,
                ChatType.Private);

            return await botClass.SendPhotoAsync(chatIdToSendTo, photo,
                caption, parseMode, typeOfChatSentInto.Value);
        }

        internal async static Task CheckMessageToDelete()
        {
            if (Code.Data.GlobalVariables.MessagesToDelete == null)
            {
                return;
            }

            for (int i = 0; i < Code.Data.GlobalVariables.MessagesToDelete.Count;)
            {
                var m = Code.Data.GlobalVariables.MessagesToDelete[i];
                if (m.ToDelete())
                {
                    bool success = await m.Delete();
                    if (success)
                    {
                        lock (Code.Data.GlobalVariables.MessagesToDelete)
                        {
                            Code.Data.GlobalVariables.MessagesToDelete.RemoveAt(i);
                            Utils.FileSerialization.WriteToBinaryFile(Code.Data.Constants.Paths.Bin.MessagesToDelete, Code.Data.GlobalVariables.MessagesToDelete);
                            continue;
                        }
                    }
                }

                i++;
            }
        }
    }
}
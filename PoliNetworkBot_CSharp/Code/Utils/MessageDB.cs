#region

using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
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
        private static readonly Dictionary<int, string> MessageTypesInRam = new();

        internal static bool AddMessage(MessageType type, string messageText,
            long messageFromIdPerson, int? messageFromIdEntity,
            long idChatSentInto, DateTime? sentDate,
            bool hasBeenSent, long messageFromIdBot,
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

        public static async Task<bool> CheckMessagesToSend(bool force_send_everything_in_queue,
            TelegramBotAbstract telegramBotAbstract)
        {
            DataTable dt = null;
            var q = "SELECT * " +
                    "FROM Messages ";

            dt = SqLite.ExecuteSelect(q);
            if (dt == null || dt.Rows.Count == 0)
                return false;

            foreach (DataRow dr in dt.Rows)
                try
                {
                    var botToReportException = FindBotIfNeeded(null, telegramBotAbstract);
                    var r1 = await SendMessageToSend(dr, null, !force_send_everything_in_queue, botToReportException);
                    telegramBotAbstract = FindBotIfNeeded(r1, telegramBotAbstract);
                    if (telegramBotAbstract != null &&
                        r1 != null) // && r1.scheduleMessageSentResult != Enums.ScheduleMessageSentResult.ALREADY_SENT)
                        switch (r1.scheduleMessageSentResult)
                        {
                            case ScheduleMessageSentResult.NOT_THE_RIGHT_TIME:
                            case ScheduleMessageSentResult.FAILED_SEND:
                            case ScheduleMessageSentResult.SUCCESS:
                            case ScheduleMessageSentResult.WE_DONT_KNOW_IF_IT_HAS_BEEN_SENT:
                                {
                                    await NotifyOwnersOfResultAsync(r1, telegramBotAbstract);
                                    break;
                                }

                            case ScheduleMessageSentResult.THE_MESSAGE_IS_NOT_SCHEDULED:
                            case ScheduleMessageSentResult.ALREADY_SENT:
                                break;
                        }
                }
                catch (Exception e)
                {
                    await NotifyUtil.NotifyOwners(e, BotUtil.GetFirstModerationRealBot(telegramBotAbstract));
                }

            return true;
        }


        private static TelegramBotAbstract FindBotIfNeeded(MessageSendScheduled r1,
            TelegramBotAbstract telegramBotAbstract)
        {
            if (telegramBotAbstract != null)
                return telegramBotAbstract;

            if (r1 == null)
                return BotUtil.GetFirstModerationRealBot();

            var r2 = r1.scheduleMessageSentResult;

            switch (r2)
            {
                case ScheduleMessageSentResult.NOT_THE_RIGHT_TIME:
                    return null;

                case ScheduleMessageSentResult.THE_MESSAGE_IS_NOT_SCHEDULED:
                    return null;

                case ScheduleMessageSentResult.FAILED_SEND:
                    break;

                case ScheduleMessageSentResult.SUCCESS:
                    return null;

                case ScheduleMessageSentResult.WE_DONT_KNOW_IF_IT_HAS_BEEN_SENT:
                    break;

                case ScheduleMessageSentResult.ALREADY_SENT:
                    return null;
            }

            return BotUtil.GetFirstModerationRealBot();
        }

        private static async Task NotifyOwnersOfResultAsync(MessageSendScheduled r1,
            TelegramBotAbstract telegramBotAbstract)
        {
            var s3 = r1.ToString();
            var s4 = r1?.r1?.Item2.ToString();
            if (string.IsNullOrEmpty(s4))
                s4 = "[NULL(1)]";
            s3 += "\n[Id1]: " + s4 + "\n";
            var s5 = r1?.r1?.Item3;
            if (string.IsNullOrEmpty(s5)) s5 = "[NULL(2)]";
            s3 += "[Id2]: " + s5 + "\n";
            s3 += "[Id3]: " + r1?.scheduleMessageSentResult + "\n";
            s3 += "CheckMessagesToSend\n\n";
            var e3 = new Exception(s3);
            await NotifyUtil.NotifyOwners(e3, telegramBotAbstract);
        }

        private static async Task<MessageSendScheduled> SendMessageToSend(DataRow dr,
            TelegramBotAbstract telegramBotAbstract,
            bool schedule, TelegramBotAbstract botToReportException)
        {
            bool? has_been_sent = null;
            Tuple<bool?, int, string> r1 = null;
            try
            {
                r1 = await GetHasBeenSentAsync(dr, telegramBotAbstract);
            }
            catch (Exception e3)
            {
                await NotifyUtil.NotifyOwners(e3, botToReportException);
            }

            if (r1 != null) has_been_sent = r1.Item1;

            if (has_been_sent == null)
                return new MessageSendScheduled(ScheduleMessageSentResult.WE_DONT_KNOW_IF_IT_HAS_BEEN_SENT, null, null,
                    r1);

            if (has_been_sent.Value)
                return new MessageSendScheduled(ScheduleMessageSentResult.ALREADY_SENT, null, null, r1);

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
                return new MessageSendScheduled(ScheduleMessageSentResult.THE_MESSAGE_IS_NOT_SCHEDULED, null, null, r1);

            if (schedule && dt > DateTime.Now)
                return new MessageSendScheduled(ScheduleMessageSentResult.NOT_THE_RIGHT_TIME, null, null, r1);

            var done = await SendMessageFromDataRow(dr, null, null, false, telegramBotAbstract, 0);
            if (done.IsSuccess() == false)
                return new MessageSendScheduled(ScheduleMessageSentResult.FAILED_SEND, null, null, r1);

            var q2 = "UPDATE Messages SET has_been_sent = TRUE WHERE id = " + dr["id"];
            SqLite.Execute(q2);

            return new MessageSendScheduled(ScheduleMessageSentResult.SUCCESS, null, null, r1);
        }

        private static async Task<Tuple<bool?, int, string>> GetHasBeenSentAsync(DataRow dr, TelegramBotAbstract sender)
        {
            try
            {
                var b1 = (bool)dr["has_been_sent"];

                var s1 = b1 ? "S" : "N";
                s1 += "\n";
                s1 += "GetHasBeenSentAsync";
                var e1 = new Exception(s1);
                await NotifyUtil.NotifyOwners(e1, sender);
                return new Tuple<bool?, int, string>(b1, 1, s1); //todo: change to "return b1"
            }
            catch
            {
                ;
            }

            try
            {
                var s = dr["has_been_sent"].ToString();
                var b2 = s == "1" || s == "S";

                var s2 = b2 ? "S" : "N";
                s2 += "\n";
                s2 += "GetHasBeenSentAsync";
                var e2 = new Exception(s2);
                await NotifyUtil.NotifyOwners(e2, sender);
                return new Tuple<bool?, int, string>(b2, 2, s2); //todo: change to "return b2"
            }
            catch
            {
                ;
            }

            var s4 = "[WE DON'T KNOW]";
            try
            {
                s4 = dr["has_been_sent"].ToString();
            }
            catch
            {
                ;
            }

            var s3 = s4;
            s3 += "\n";
            s3 += "GetHasBeenSentAsync";
            var e3 = new Exception(s3);
            await NotifyUtil.NotifyOwners(e3, sender);
            return new Tuple<bool?, int, string>(null, 3, s3);
        }

        public static async Task<MessageSentResult> SendMessageFromDataRow(DataRow dr, long? chatIdToSendTo,
            ChatType? chatTypeToSendTo, bool extraInfo, TelegramBotAbstract telegramBotAbstract, int count)
        {
            var r1 = await SendMessageFromDataRowSingle(dr, chatIdToSendTo, chatTypeToSendTo);

            if (extraInfo)
            {
                var r2 = await SendExtraInfoDbForThisMessage(r1, dr, chatIdToSendTo, chatTypeToSendTo,
                    telegramBotAbstract, count);
                return r2;
            }

            return r1;
        }

        private static async Task<MessageSentResult> SendExtraInfoDbForThisMessage(MessageSentResult r1, DataRow dr,
            long? chatIdToSendTo, ChatType? chatTypeToSendTo, TelegramBotAbstract telegramBotAbstract, int count)
        {
            if (r1 == null || r1.IsSuccess() == false) return r1;

            if (chatIdToSendTo == null) return new MessageSentResult(false, null, chatTypeToSendTo);

            var dto = dr["sent_date"];
            var fieo = dr["from_id_entity"];
            var fipo = dr["from_id_person"];

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

            var text1 = "📌 ID: " + count + "\n";
            if (dt != null) text1 += "📅 " + DateTimeClass.DateTimeToItalianFormat(dt) + "\n";
            if (from_id_entity != null)
            {
                var entity_name = Assoc.GetNameOfEntityFromItsID(from_id_entity.Value);
                text1 += "👥 " + entity_name + "\n";
            }

            if (from_id_person != null) text1 += "✍ " + from_id_person + "\n";

            var dict = new Dictionary<string, string>
            {
                {"en", text1}
            };
            var text2 = new Language(dict);
            return await telegramBotAbstract.SendTextMessageAsync(chatIdToSendTo.Value, text2, chatTypeToSendTo, "",
                ParseMode.Html,
                null, null, r1.GetMessageID(), true);
        }

        private static async Task<MessageSentResult> SendMessageFromDataRowSingle(DataRow dr, long? chatIdToSendTo,
            ChatType? chatTypeToSendTo)
        {
            var botId = Convert.ToInt32(dr["from_id_bot"]);
            var botClass = GlobalVariables.Bots[botId];

            var typeI = Convert.ToInt32(dr["type"]);
            var typeT = GetMessageTypeClassById(typeI);
            if (typeT == null)
                return new MessageSentResult(false, null, chatTypeToSendTo);

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


                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new MessageSentResult(false, null, chatTypeToSendTo);
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

        private static MessageSentResult SendTextFromDataRow(DataRow dr, TelegramBotAbstract botClass)
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

        private static async Task<MessageSentResult> SendVideoFromDataRow(DataRow dr, TelegramBotAbstract botClass,
            ParseMode parseMode, long? chatIdToSendTo2, ChatType? chatTypeToSendTo)
        {
            var videoId = SqLite.GetIntFromColumn(dr, "id_video");
            if (videoId == null)
                return new MessageSentResult(false, null, chatTypeToSendTo);

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
                return new MessageSentResult(false, null, chatTypeToSendTo);

            var video = UtilsVideo.GetVideoByIdFromDb(
                videoId.Value,
                messageIdFrom,
                chatIdFromIdPerson,
                ChatType.Private);

            return await botClass.SendVideoAsync(chatIdToSendTo, video,
                caption, parseMode, typeOfChatSentInto.Value);
        }

        private static async Task<MessageSentResult> SendPhotoFromDataRow(DataRow dr, TelegramBotAbstract botClass,
            ParseMode parseMode, long? chatIdToSendTo2, ChatType? chatTypeToSendTo)
        {
            var photoId = SqLite.GetIntFromColumn(dr, "id_photo");
            if (photoId == null)
                return new MessageSentResult(false, null, chatTypeToSendTo);

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
                return new MessageSentResult(false, null, chatTypeToSendTo);

            var photo = UtilsPhoto.GetPhotoByIdFromDb(
                photoId.Value,
                messageIdFrom,
                chatIdFromIdPerson,
                ChatType.Private);

            return await botClass.SendPhotoAsync(chatIdToSendTo, photo,
                caption, parseMode, typeOfChatSentInto.Value);
        }

        internal static async Task CheckMessageToDelete()
        {
            if (GlobalVariables.MessagesToDelete == null) return;

            for (var i = 0; i < GlobalVariables.MessagesToDelete.Count;)
            {
                var m = GlobalVariables.MessagesToDelete[i];
                if (m.ToDelete())
                {
                    var success = await m.Delete();
                    if (success)
                        lock (GlobalVariables.MessagesToDelete)
                        {
                            GlobalVariables.MessagesToDelete.RemoveAt(i);
                            FileSerialization.WriteToBinaryFile(Paths.Bin.MessagesToDelete,
                                GlobalVariables.MessagesToDelete);
                            continue;
                        }
                }

                i++;
            }
        }
    }
}
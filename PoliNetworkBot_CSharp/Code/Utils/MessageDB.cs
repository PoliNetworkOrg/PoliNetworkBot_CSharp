﻿#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TmpResults;
using PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class MessageDb
{
    private static readonly Dictionary<long, string?> MessageTypesInRam = new();

    internal static bool AddMessage(MessageType type, string? messageText,
        long messageFromIdPerson, long? messageFromIdEntity,
        long? idChatSentInto, DateTime? sentDate,
        bool hasBeenSent, long? messageFromIdBot,
        int messageIdTgFrom, ChatType? typeChatSentInto,
        long? photoId, long? videoId, TelegramBotAbstract? sender)
    {
        const string? q = "INSERT INTO Messages " +
                          "(id, from_id_person, from_id_entity, type, " +
                          "id_photo, id_video, message_text, id_chat_sent_into, sent_date," +
                          " has_been_sent, from_id_bot, message_id_tg_from, type_chat_sent_into) " +
                          "VALUES " +
                          "(@id, @fip, @fie, @t, @idp, @idv, @mt, @icsi, @sent_date, @hbs, @fib, @mitf, @tcsi);";

        var typeI = GetMessageTypeByName(type, sender);
        if (typeI == null) return false;

        var id = Tables.GetMaxId("Messages", "id", sender?.DbConfig);
        id++;

        Database.Execute(q, sender?.DbConfig, new Dictionary<string, object?>
        {
            { "@id", id },
            { "@fip", messageFromIdPerson },
            { "@fie", messageFromIdEntity },
            { "@t", typeI },
            { "@idp", photoId },
            { "@idv", videoId },
            { "@mt", messageText },
            { "@icsi", idChatSentInto },
            { "@sent_date", sentDate },
            { "@hbs", hasBeenSent },
            { "@fib", messageFromIdBot },
            { "@mitf", messageIdTgFrom },
            { "@tcsi", typeChatSentInto.ToString() }
        });

        return true;
    }

    private static long? GetMessageTypeByName(MessageType type, TelegramBotAbstract? sender, int times = 1)
    {
        while (true)
        {
            if (times < 0) return null;

            const string? q1 = "SELECT id FROM MessageTypes WHERE name = @name";
            var keyValuePairs = new Dictionary<string, object?> { { "@name", type.ToString() } };
            var r1 = Database.ExecuteSelect(q1, sender?.DbConfig, keyValuePairs);
            var r2 = Database.GetFirstValueFromDataTable(r1);
            if (r1 == null || r1.Rows.Count == 0 || r2 == null)
            {
                AddMessageType(type, sender);
                times--;
                continue;
            }

            try
            {
                return Convert.ToInt64(r2);
            }
            catch
            {
                return null;
            }
        }
    }

    private static void AddMessageType(MessageType type, TelegramBotAbstract? bot)
    {
        const string? q = "INSERT INTO MessageTypes (name) VALUES (@name)";
        var keyValuePairs = new Dictionary<string, object?> { { "@name", type.ToString() } };
        Database.Execute(q, bot?.DbConfig, keyValuePairs);
        Tables.FixIdTable("MessageTypes", "id", "name", bot?.DbConfig);
    }

    public static async Task<bool> CheckMessagesToSend(bool forceSendEverythingInQueue,
        TelegramBotAbstract? telegramBotAbstract, MessageEventArgs? messageEventArgs)
    {
        const string? q = "SELECT * FROM Messages WHERE has_been_sent != 1;";

        var dt = Database.ExecuteSelectUnlogged(
            q, telegramBotAbstract?.DbConfig ?? GlobalVariables.DbConfig
        );

        if (dt == null || dt.Rows.Count == 0)
            return false;

        foreach (DataRow dr in dt.Rows)
            try
            {
                var botToReportException = FindBotIfNeeded(null, telegramBotAbstract);
                var r1 = await SendMessageToSend(dr, telegramBotAbstract, !forceSendEverythingInQueue,
                    botToReportException,
                    messageEventArgs);
                telegramBotAbstract = FindBotIfNeeded(r1, telegramBotAbstract);
                if (telegramBotAbstract !=
                    null) // && r1.scheduleMessageSentResult != Enums.ScheduleMessageSentResult.ALREADY_SENT)
                    switch (r1.ScheduleMessageSentResult)
                    {
                        case ScheduleMessageSentResult.FAILED_SEND:
                        case ScheduleMessageSentResult.SUCCESS:
                        case ScheduleMessageSentResult.WE_DONT_KNOW_IF_IT_HAS_BEEN_SENT:
                        {
                            await NotifyOwnersOfResultAsync(r1, telegramBotAbstract, messageEventArgs);
                            break;
                        }


                        case ScheduleMessageSentResult.NOT_THE_RIGHT_TIME:
                        case ScheduleMessageSentResult.THE_MESSAGE_IS_NOT_SCHEDULED:
                        case ScheduleMessageSentResult.ALREADY_SENT:
                            break;
                    }
            }
            catch (Exception? e)
            {
                await NotifyUtil.NotifyOwnerWithLog2(e, BotUtil.GetFirstModerationRealBot(telegramBotAbstract),
                    EventArgsContainer.Get(messageEventArgs));
            }

        return true;
    }

    private static TelegramBotAbstract? FindBotIfNeeded(MessageSendScheduled? r1,
        TelegramBotAbstract? telegramBotAbstract)
    {
        if (telegramBotAbstract != null)
            return telegramBotAbstract;

        if (r1 == null)
            return BotUtil.GetFirstModerationRealBot();

        var r2 = r1.ScheduleMessageSentResult;

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
        TelegramBotAbstract? telegramBotAbstract, MessageEventArgs? messageEventArgs)
    {
        var s3 = r1.ToString();
        var s4 = r1.R1?.I.ToString();
        if (string.IsNullOrEmpty(s4))
            s4 = "[NULL(1)]";
        s3 += "\n[Id1]: " + s4 + "\n";
        var s5 = r1.R1?.S;
        if (string.IsNullOrEmpty(s5)) s5 = "[NULL(2)]";
        s3 += "[Id2]: " + s5 + "\n";
        s3 += "[Id3]: " + r1.ScheduleMessageSentResult + "\n";
        s3 += "CheckMessagesToSend\n\n";
        var e3 = new Exception(s3);
        await NotifyUtil.NotifyOwnerWithLog2(e3, telegramBotAbstract, EventArgsContainer.Get(messageEventArgs));
    }

    private static async Task<MessageSendScheduled> SendMessageToSend(DataRow dr,
        TelegramBotAbstract? telegramBotAbstract,
        bool schedule, TelegramBotAbstract? botToReportException, MessageEventArgs? messageEventArgs)
    {
        bool? hasBeenSent = null;
        HasBeenSent? r1 = null;
        try
        {
            r1 = await GetHasBeenSentAsync(dr, telegramBotAbstract, messageEventArgs);
        }
        catch (Exception? e3)
        {
            await NotifyUtil.NotifyOwnerWithLog2(e3, botToReportException, EventArgsContainer.Get(messageEventArgs));
        }

        if (r1 != null) hasBeenSent = r1.B;

        if (hasBeenSent == null)
            return new MessageSendScheduled(ScheduleMessageSentResult.WE_DONT_KNOW_IF_IT_HAS_BEEN_SENT, null, null,
                r1);

        if (hasBeenSent.Value)
            return new MessageSendScheduled(ScheduleMessageSentResult.ALREADY_SENT, null, null, r1);

        var dt = GetDateTime(dr, "sent_date");

        switch (schedule)
        {
            case true when dt == null:
                return new MessageSendScheduled(ScheduleMessageSentResult.THE_MESSAGE_IS_NOT_SCHEDULED,
                    null, null, r1);

            case true when dt > DateTime.Now:
                return new MessageSendScheduled(ScheduleMessageSentResult.NOT_THE_RIGHT_TIME,
                    null, null, r1);
        }

        var done = await SendMessageFromDataRow(dr, null, null, false, telegramBotAbstract, 0);
        if (done != null && done.IsSuccess() == false)
            return new MessageSendScheduled(ScheduleMessageSentResult.FAILED_SEND, null, null, r1);

        var o = dr["id"].ToString();
        var q2 = $"UPDATE Messages SET has_been_sent = TRUE WHERE id = {o}";
        Database.Execute(q2, telegramBotAbstract?.DbConfig);

        return new MessageSendScheduled(ScheduleMessageSentResult.SUCCESS, null, null, r1);
    }

    private static DateTime? GetDateTime(DataRow dr, string v)
    {
        try
        {
            var s = dr[v].ToString();
            var r = DateTimeClass.GetDateTimeFromString(s);
            if (r is { Item2: null, Item1: not null })
            {
            }

            if (r is { Item1: not null })
                return r.Item1.Value;
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static async Task<HasBeenSent> GetHasBeenSentAsync(DataRow dr, TelegramBotAbstract? sender,
        MessageEventArgs? messageEventArgs)
    {
        try
        {
            var b1 = (bool)dr["has_been_sent"];

            var s1 = b1 ? "S" : "N";
            s1 += "\n";
            s1 += "GetHasBeenSentAsync";
            //var e1 = new Exception(s1);
            //await NotifyUtil.NotifyOwners(e1, sender, messageEventArgs);
            return new HasBeenSent(b1, 1, s1);
        }
        catch
        {
            // ignored
        }

        try
        {
            var s = dr["has_been_sent"].ToString();
            var b2 = s is "1" or "S";

            var s2 = b2 ? "S" : "N";
            s2 += "\n";
            s2 += "GetHasBeenSentAsync";
            //var e2 = new Exception(s2);
            //await NotifyUtil.NotifyOwners(e2, sender, messageEventArgs);
            return new HasBeenSent(b2, 2, s2); //todo: change to "return b2"
        }
        catch
        {
            // ignored
        }

        var s4 = "[WE DON'T KNOW]";
        try
        {
            s4 = dr["has_been_sent"].ToString();
        }
        catch
        {
            // ignored
        }

        var s3 = s4;
        s3 += "\n";
        s3 += "GetHasBeenSentAsync";
        var e3 = new Exception(s3);
        await NotifyUtil.NotifyOwnerWithLog2(e3, sender, EventArgsContainer.Get(messageEventArgs));
        return new HasBeenSent(null, 3, s3);
    }

    public static async Task<MessageSentResult?> SendMessageFromDataRow(DataRow dr, long? chatIdToSendTo,
        ChatType? chatTypeToSendTo, bool extraInfo, TelegramBotAbstract? telegramBotAbstract, int count)
    {
        var r1 = await SendMessageFromDataRowSingle(dr, chatIdToSendTo, chatTypeToSendTo, telegramBotAbstract);

        if (!extraInfo) return r1;
        var r2 = await SendExtraInfoDbForThisMessage(r1, dr, chatIdToSendTo, chatTypeToSendTo,
            telegramBotAbstract, count);
        return r2;
    }

    private static async Task<MessageSentResult?> SendExtraInfoDbForThisMessage(MessageSentResult? r1, DataRow dr,
        long? chatIdToSendTo, ChatType? chatTypeToSendTo, TelegramBotAbstract? telegramBotAbstract, int count)
    {
        if (r1 == null || r1.IsSuccess() == false) return r1;

        if (chatIdToSendTo == null) return new MessageSentResult(false, null, chatTypeToSendTo);

        var dto = dr["sent_date"];
        var fieo = dr["from_id_entity"];
        var fipo = dr["from_id_person"];

        DateTime? dt = null;
        long? fromIdEntity = null;
        long? fromIdPerson = null;

        try
        {
            dt = (DateTime?)dto;
        }
        catch
        {
            // ignored
        }

        try
        {
            fromIdEntity = (long?)fieo;
        }
        catch
        {
            // ignored
        }

        try
        {
            fromIdPerson = (long?)fipo;
        }
        catch
        {
            // ignored
        }

        var text1 = "📌 ID: " + count + "\n";
        if (dt != null) text1 += "📅 " + DateTimeClass.DateTimeToAmericanFormat(dt) + "\n";
        if (fromIdEntity != null)
        {
            var entityName = Assoc.GetNameOfEntityFromItsId(fromIdEntity.Value, telegramBotAbstract);
            text1 += "👥 " + entityName + "\n";
        }

        if (fromIdPerson != null) text1 += "✍ " + fromIdPerson + "\n";

        var dict = new Dictionary<string, string?>
        {
            { "en", text1 }
        };
        var text2 = new Language(dict);
        if (telegramBotAbstract == null) return null;
        var messageOptions = new MessageOptions
        {
            ChatId = chatIdToSendTo.Value,
            Text = text2,
            ChatType = chatTypeToSendTo,
            ReplyToMessageId = r1.GetMessageId(),
            DisablePreviewLink = true
        };
        return await telegramBotAbstract.SendTextMessageAsync(messageOptions);
    }

    private static async Task<MessageSentResult?> SendMessageFromDataRowSingle(DataRow dr, long? chatIdToSendTo,
        ChatType? chatTypeToSendTo, TelegramBotAbstract? sender)
    {
        var botId = Convert.ToInt64(dr["from_id_bot"]);
        if (GlobalVariables.Bots != null && !GlobalVariables.Bots.ContainsKey(botId))
            return new MessageSentResult(false, null, chatTypeToSendTo);

        var botClass = GlobalVariables.Bots?[botId];

        var typeI = Convert.ToInt64(dr["type"]);
        var typeT = GetMessageTypeClassById(typeI, sender);
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

    private static MessageType? GetMessageTypeClassById(in long typeI, TelegramBotAbstract? sender)
    {
        var typeS = GetMessageTypeNameById(typeI, sender);

        if (string.IsNullOrEmpty(typeS))
            return null;

        var messageType = Enum.TryParse(typeof(MessageType), typeS, out var typeT);
        if (messageType == false || typeT == null)
            return null;

        if (typeT is MessageType t)
            return t;
        return null;
    }

    // ReSharper disable once UnusedParameter.Local
    // ReSharper disable once UnusedParameter.Local
    private static MessageSentResult SendTextFromDataRow(DataRow dr, TelegramBotAbstract? botClass)
    {
        throw new NotImplementedException();
        //non serve perché le assoc mandano solo immagini
    }

    private static string? GetMessageTypeNameById(in long typeI, TelegramBotAbstract? sender)
    {
        if (MessageTypesInRam.TryGetValue(typeI, out var id))
            return id;

        var q = "SELECT name FROM MessageTypes WHERE id = " + typeI;
        var dt = Database.ExecuteSelect(q, sender?.DbConfig);
        if (dt == null || dt.Rows.Count == 0) return null;

        var firstValueFromDataTable = Database.GetFirstValueFromDataTable(dt);
        var value = firstValueFromDataTable?.ToString();
        if (string.IsNullOrEmpty(value)) return null;

        MessageTypesInRam[typeI] = value;
        return value;
    }

    private static async Task<MessageSentResult?> SendVideoFromDataRow(DataRow dr, TelegramBotAbstract? botClass,
        ParseMode parseMode, long? chatIdToSendTo2, ChatType? chatTypeToSendTo)
    {
        var videoId = Database.GetIntFromColumn(dr, "id_video");
        if (videoId == null)
            return new MessageSentResult(false, null, chatTypeToSendTo);

        var chatIdToSendTo = (long)dr["id_chat_sent_into"];
        if (chatIdToSendTo2 != null)
            chatIdToSendTo = chatIdToSendTo2.Value;

        var caption = dr["message_text"].ToString();
        var chatIdFromIdPerson = Convert.ToInt64(dr["from_id_person"]);
        long? messageIdFrom = null;
        try
        {
            messageIdFrom = Convert.ToInt64(dr["message_id_tg_from"]);
        }
        catch
        {
            //ignored
        }

        var typeOfChatSentInto = ChatTypeUtil.GetChatTypeFromString(dr["type_chat_sent_into"]);

        if (chatTypeToSendTo != null)
            typeOfChatSentInto = chatTypeToSendTo;

        if (typeOfChatSentInto == null)
            return new MessageSentResult(false, null, null);

        var video = UtilsVideo.GetVideoByIdFromDb(
            videoId.Value,
            messageIdFrom,
            chatIdFromIdPerson,
            ChatType.Private, botClass);

        if (botClass != null)
            return await botClass.SendVideoAsync(chatIdToSendTo, video,
                caption, parseMode, typeOfChatSentInto.Value);
        return null;
    }

    private static async Task<MessageSentResult?> SendPhotoFromDataRow(DataRow dr, TelegramBotAbstract? botClass,
        ParseMode parseMode, long? chatIdToSendTo2, ChatType? chatTypeToSendTo)
    {
        var photoId = Database.GetIntFromColumn(dr, "id_photo");
        if (photoId == null)
            return new MessageSentResult(false, null, chatTypeToSendTo);

        var chatIdToSendTo = (long)dr["id_chat_sent_into"];
        if (chatIdToSendTo2 != null)
            chatIdToSendTo = chatIdToSendTo2.Value;

        var caption = dr["message_text"].ToString();
        var chatIdFromIdPerson = Convert.ToInt64(dr["from_id_person"]);
        long? messageIdFrom = null;
        try
        {
            messageIdFrom = Convert.ToInt64(dr["message_id_tg_from"]);
        }
        catch
        {
            //ignored
        }

        var typeOfChatSentInto = ChatTypeUtil.GetChatTypeFromString(dr["type_chat_sent_into"]);

        if (chatTypeToSendTo != null)
            typeOfChatSentInto = chatTypeToSendTo;

        if (typeOfChatSentInto == null)
            return new MessageSentResult(false, null, null);

        var photo = UtilsPhoto.GetPhotoByIdFromDb(
            photoId.Value,
            messageIdFrom,
            chatIdFromIdPerson,
            ChatType.Private, botClass);

        if (botClass != null)
            return await botClass.SendPhotoAsync(chatIdToSendTo, photo,
                caption, parseMode, typeOfChatSentInto.Value, null);

        return null;
    }

    internal static async Task CheckMessageToDelete(TelegramBotAbstract telegramBotAbstract)
    {
        var id = telegramBotAbstract.GetId();
        if (id == null)
            return;

        var q = "SELECT * FROM MessagesToRemove WHERE bot_id = @bot_id AND delete_when >= NOW()";
        var dt = Database.ExecuteSelect(q, telegramBotAbstract.DbConfig,
            new Dictionary<string, object?> { { "@bot_id", id.Value } });
        if (dt == null)
            return;
        foreach (DataRow VARIABLE in dt.Rows)
            try
            {
                if (VARIABLE != null)
                {
                    var message_id = Convert.ToInt64(VARIABLE["message_id"]);
                    var chat_id = Convert.ToInt64(VARIABLE["chat_id"]);
                    await DeleteAndUpdateTodoTable(message_id, chat_id, telegramBotAbstract);
                }
            }
            catch
            {
                //ignored
            }
    }

    private static async Task<bool> DeleteAndUpdateTodoTable(long messageId, long chatId,
        TelegramBotAbstract telegramBotAbstract)
    {
        GlobalVariables.Bots ??= new Dictionary<long, TelegramBotAbstract?>();

        try
        {
            var s = await telegramBotAbstract.DeleteMessageAsync(chatId, messageId, null);
            if (s)
            {
                var q = "DELETE FROM MessagesToRemove WHERE message_id = @message_id AND chat_id = @chat_id";
                Database.Execute(q, telegramBotAbstract.DbConfig,
                    new Dictionary<string, object?>
                    {
                        { "@message_id", messageId },
                        { "@chat_id", chatId }
                    });
                return true;
            }
        }
        catch (Exception? e)
        {
            await NotifyUtil.NotifyOwnerWithLog2(e, telegramBotAbstract, null);
        }

        return false;
    }
}
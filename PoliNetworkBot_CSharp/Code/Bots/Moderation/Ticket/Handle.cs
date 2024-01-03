using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket;

public static class Handle
{
    private static readonly List<ChatIdTgWith100>
        AllowedGroups = new() { new ChatIdTgWith100 { Id = 2124790858, VaAggiuntoMeno100 = true } };

    public static void HandleMethod(TelegramBotAbstract t, MessageEventArgs e)
    {
        if (e.Message.Chat.Type is not (ChatType.Group or ChatType.Supergroup))
            return;


        var (found, chatIdTgWith100) = AllowedGroupsContains(e.Message.Chat.Id);
        if (!found)
            return;

        try
        {
            var messageReplyToMessage = e.Message.ReplyToMessage;
            if (messageReplyToMessage != null)

                return;

            var messageText = e.Message.Text;
            if (string.IsNullOrEmpty(messageText))
                return;

            var date = GetItalianDateTime(e);


            var chatId = chatIdTgWith100?.Id;
            var body = "Link to first message: https://t.me/c/" + chatId + "/" + e.Message.MessageId;
            body += "\n\n\n";

            body += "When: " + date.ToString(CultureInfo.InvariantCulture);
            body += "\n\n\n";
            body += "Chat title: " + e.Message.Chat.Title;
            body += "\n\n\n";
            body += "Chat type: " + e.Message.Chat.Type;
            body += "\n\n\n";
            body += "Message type: " + e.Message.Type;
            body += "\n\n\n";
            body += "From user id: " + e.Message.From?.Id;
            body += "\n\n\n";
            body += "Body:\n\n";
            body += messageText;


            const int maxLengthTitle = 25;
            var substring = messageText.Length > maxLengthTitle ? messageText[..maxLengthTitle] : messageText;
            CreateIssue.Create(substring, body, e.Message.Chat.Id, e.Message.From?.Id, t, chatIdTgWith100);
        }
        catch (Exception ex)
        {
            NotifyUtil.NotifyOwnerWithLog2(ex, t, new EventArgsContainer { MessageEventArgs = e });
        }
    }

    private static DateTime GetItalianDateTime(MessageEventArgs e)
    {
        var messageDate = e.Message.Date;
        var diff = DateTime.Now - DateTime.UtcNow;
        var date = messageDate.AddHours(diff.TotalHours);
        return date;
    }

    private static Tuple<bool, ChatIdTgWith100?> AllowedGroupsContains(long chatId)
    {
        var b = AllowedGroups.FirstOrDefault(variable => variable.GetString() == chatId.ToString());

        return b == null ? new Tuple<bool, ChatIdTgWith100?>(false, null) : new Tuple<bool, ChatIdTgWith100?>(true, b);
    }
}
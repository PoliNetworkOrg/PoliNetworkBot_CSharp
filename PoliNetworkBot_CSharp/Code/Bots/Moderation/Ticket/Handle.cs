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
    private static readonly List<ChatIdTg>
        AllowedGroups = new() { new ChatIdTg { Id = 2124790858, VaAggiuntoMeno100 = true } };

    public static void HandleMethod(TelegramBotAbstract t, MessageEventArgs e)
    {
        if (e.Message.Chat.Type is not (ChatType.Group or ChatType.Supergroup))
            return;


        var allowedGroupsContains = AllowedGroupsContains(e.Message.Chat.Id);
        if (!allowedGroupsContains.Item1)
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


            var chatId = allowedGroupsContains.Item2?.Id;
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
            CreateIssue.Create(substring, body, e.Message.Chat.Id, e.Message.From?.Id, t);
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

    private static Tuple<bool, ChatIdTg?> AllowedGroupsContains(long chatId)
    {
        var b = AllowedGroups.FirstOrDefault(variable => variable.GetString() == chatId.ToString());
<<<<<<< HEAD
        return b == null ? new Tuple<bool, ChatIdTg?>(false, null) : new Tuple<bool, ChatIdTg?>(true, b);
=======
        if (b == null)
            return new Tuple<bool, ChatIdTg?>(false, null);

        return new Tuple<bool, ChatIdTg?>(true, b);
>>>>>>> master
    }
}

internal class ChatIdTg
{
    public long Id;
    public bool VaAggiuntoMeno100;

    public string GetString()
    {
        return VaAggiuntoMeno100 ? "-100" + Id : Id.ToString();
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket;

public static class Handle
{
    private static readonly List<long> AllowedGroups = new List<long>() { 123 };

    public static void HandleMethod(TelegramBotAbstract t, MessageEventArgs e)
    {
        if (!AllowedGroups.Contains(e.Message.Chat.Id))
            return;

        if (e.Message.Chat.Type is not (ChatType.Group or ChatType.Supergroup))
            return;

        try
        {
            if (e.Message.ReplyToMessage != null)
                return;

            if (string.IsNullOrEmpty(e.Message.Text))
                return;

            var body = "Link to first message: https://t.me/c/" + e.Message.Chat.Id + "/" + e.Message.MessageId;
            body += "\n\n\n";
            body += "When: " +e.Message.Date.ToString(CultureInfo.InvariantCulture);
            body += "\n\n\n";
            body += "From user id: " +  e.Message.From?.Id;
            body += "\n\n\n";
            body += "Body:\n\n";
            body += e.Message.Text;


            var substring = e.Message.Text[..20];
            CreateIssue.Create(substring, body, e.Message.Chat.Id, e.Message.From?.Id);
        }
        catch (Exception ex)
        {
            NotifyUtil.NotifyOwnerWithLog2(ex, t, new EventArgsContainer() { MessageEventArgs = e });
        }
    }
}
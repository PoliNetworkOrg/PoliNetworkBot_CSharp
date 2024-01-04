using System;
using System.Globalization;
using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Utils;

public static class BodyClass
{
    public static string GetBody(MessageEventArgs e, long chatId, DateTime date, string? messageText)
    {
        var body = "Link to message: https://t.me/c/" + chatId + "/" + e.Message.MessageId;
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
        body += "## Body:\n\n";
        body += messageText;
        return body;
    }
}
using System;
using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Utils;

public static class BodyClass
{
    public static string GetBody(MessageEventArgs e, long chatId, DateTime date, string? messageText)
    {
        var eMessage = e.Message;

        var body = "Link to message: https://t.me/c/" + chatId + "/" + eMessage.MessageId;
        body += "\n\n\n";

        body += "When: " + date.ToString("dd/MM/yyyy HH:mm:ss");
        body += "\n";
        body += "Chat title: " + eMessage.Chat.Title;
        body += "\n";
        body += "Chat type: " + eMessage.Chat.Type;
        body += "\n";
        body += "Message type: " + eMessage.Type;
        body += "\n";
        body += "From user id: " + eMessage.From?.Id;
        body += "\n\n";
        body += "## Body:\n\n";
        body += messageText;
        return body;
    }
}
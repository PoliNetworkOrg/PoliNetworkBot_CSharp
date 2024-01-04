using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Data;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Model;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Utils;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket;

public static class Handle
{
    private const int MaxLengthTitleIssue = 200;

    private static readonly List<MessageThread> Threads = new();

    public static void HandleMethod(TelegramBotAbstract t, MessageEventArgs e)
    {
        HandleRemoveOutdatedThreadsFromRam();

        if (e.Message.Chat.Type is not (ChatType.Group or ChatType.Supergroup))
            return;


        var chatIdTgWith100 = AllowedGroupsContains(e.Message.Chat.Id);
        if (chatIdTgWith100 == null)
            return;


        var messageReplyToMessage = e.Message.ReplyToMessage;
        if (messageReplyToMessage != null)
        {
            HandleReply(messageReplyToMessage, t, e);
            return;
        }

        HandleCreateIssue(t, e, chatIdTgWith100);
    }

    private static void HandleReply(Message messageReplyToMessage, TelegramBotAbstract telegramBotAbstract,
        MessageEventArgs messageEventArgs)
    {
        MessageThread? messageThread = FindOrigin(messageReplyToMessage);

        if (messageThread == null)
        {
            return;
        }

        Console.WriteLine("todo: add comment to issue " + messageThread.IssueNumber);
        //todo: add comment to issue
    }

    private static MessageThread? FindOrigin(Message messageReplyToMessage)
    {
        lock (Threads)
        {
            foreach (var variable in Threads)
            {
                variable.Children ??= new List<MessageThread>();

                var messageId = messageReplyToMessage.MessageId;
                var chatId = messageReplyToMessage.Chat.Id;

                if (variable.MessageId == messageId &&
                    variable.ChatId == chatId)
                {
                    variable.Children.Add(new MessageThread() { MessageId = messageId, ChatId = chatId });
                    return variable;
                }


                foreach (var variable2 in variable.Children)
                {
                    if (variable2.MessageId == messageId &&
                        variable2.ChatId == chatId)
                    {
                        variable.Children.Add(new MessageThread() { MessageId = messageId, ChatId = chatId });
                        return variable2;
                    }
                }
            }
        }

        return null;
    }

    private static void HandleRemoveOutdatedThreadsFromRam()
    {
        lock (Threads)
        {
            for (var index = 0; index < Threads.Count; index++)
            {
                var variable = Threads[index];
                if (variable.DateTime != null && variable.DateTime.Value.AddDays(3) >= DateTime.Now) continue;

                Threads.RemoveAt(index);
                index--;
            }
        }
    }

    private static void HandleCreateIssue(TelegramBotAbstract t, MessageEventArgs e, ChatIdTgWith100 chatIdTgWith100)
    {
        var messageText = e.Message.Text;
        if (string.IsNullOrEmpty(messageText))
            return;

        try
        {
            var date = GetItalianDateTime(e);

            var chatId = chatIdTgWith100.Id;

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


            var substring = messageText.Length > MaxLengthTitleIssue ? messageText[..MaxLengthTitleIssue] : messageText;

            var issue = CreateIssue.Create(substring, body, e.Message.Chat.Id, e.Message.From?.Id, t, chatIdTgWith100);

            var messageThread = new MessageThread
            {
                DateTime = DateTime.Now,
                MessageId = e.Message.MessageId,
                ChatId = e.Message.Chat.Id,
                IssueNumber = issue.Number
            };

            lock (Threads)
            {
                Threads.Add(messageThread);
            }
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

    private static ChatIdTgWith100? AllowedGroupsContains(long chatId) =>
        DataTicketClass.AllowedGroups.FirstOrDefault(a => ChatIdTgWith100Equales(a, chatId));

    private static bool ChatIdTgWith100Equales(ChatIdTgWith100 a, long b) => a.GetString() == b.ToString();
}
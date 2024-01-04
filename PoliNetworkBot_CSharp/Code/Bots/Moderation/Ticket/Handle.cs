using System;
using System.Collections.Generic;
using System.Linq;
using Octokit;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Data;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Model;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Utils;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket;

public static class Handle
{
    private const int MaxLengthTitleIssue = 200;
    private const int MaxTimeThreadInRamDays = 3;


    public static void HandleTicketMethod(TelegramBotAbstract t, MessageEventArgs e)
    {
        HandleRemoveOutdatedThreadsFromRam(t);


        if (e.Message.Chat.Type is not (ChatType.Group or ChatType.Supergroup))
            return;


        var chatIdTgWith100 = AllowedGroupsContains(e.Message.Chat.Id);
        if (chatIdTgWith100 == null)
            return;


        var messageReplyToMessage = e.Message.ReplyToMessage;
        if (messageReplyToMessage != null)
        {
            HandleReply(messageReplyToMessage, t, e, chatIdTgWith100.GithubInfo);
            return;
        }

        HandleCreateIssue(t, e, chatIdTgWith100);
    }

    private static void HandleReply(Message messageReplyToMessage, TelegramBotAbstract telegramBotAbstract,
        MessageEventArgs messageEventArgs, GithubInfo? githubInfo)
    {
        var messageThread = FindOrigin(messageReplyToMessage, messageEventArgs.Message, githubInfo);


        if (messageThread == null) return;


        HandleWriteComment(telegramBotAbstract, messageEventArgs, messageThread, githubInfo);
    }

    private static void HandleWriteComment(TelegramBotAbstract telegramBotAbstract, MessageEventArgs messageEventArgs,
        MessageThread messageThread, GithubInfo? githubInfo)
    {
        var messageThreadIssueNumber = messageThread.IssueNumber;
        var messageThreadChatId = messageThread.ChatId;
        if (messageThreadIssueNumber == null) return;
        if (messageThreadChatId == null) return;

        var threadChatId = messageThreadChatId.Value;
        var messageText = messageEventArgs.Message.Text;
        var body = BodyClass.GetBody(
            messageEventArgs,
            threadChatId,
            DateTime.Now,
            messageText
        );

        var threadIssueNumber = messageThreadIssueNumber.Value;

        Comments.CreateComment(
            telegramBotAbstract,
            threadIssueNumber,
            body,
            githubInfo
        );
    }


    private static MessageThread? FindOrigin(Message messageReplyToMessage, Message newMessage, GithubInfo? githubInfo)

    {
        GlobalVariables.Threads ??= new MessageThreadStore();
        GlobalVariables.Threads.Dict ??= new Dictionary<DateTime, List<MessageThread>>();
        lock (GlobalVariables.Threads)
        {
            foreach (var key in GlobalVariables.Threads.Dict.Keys)
            {
                var startMessage2 = GlobalVariables.Threads.Dict[key];
                foreach (var startMessage in startMessage2)
                {
                    startMessage.Children ??= new List<MessageThread>();

                    var messageId = messageReplyToMessage.MessageId;
                    var chatId = messageReplyToMessage.Chat.Id;


                    var variableChildren = startMessage.Children;

                    if (startMessage.MessageId == messageId &&
                        startMessage.ChatId == chatId)
                    {
                        variableChildren.Add(new MessageThread
                            { MessageId = newMessage.MessageId, ChatId = chatId, GithubInfo = githubInfo });

                        UpdateKeyTime(key, startMessage);
                        WriteThreadsToFile();

                        return startMessage;
                    }


                    foreach (var childMessage in variableChildren)
                        if (childMessage.MessageId == messageId &&
                            childMessage.ChatId == chatId)
                        {
                            variableChildren.Add(
                                new MessageThread
                                    { MessageId = newMessage.MessageId, ChatId = chatId, GithubInfo = githubInfo });

                            UpdateKeyTime(key, startMessage);
                            WriteThreadsToFile();

                            return startMessage;
                        }
                }
            }
        }

        return null;
    }

    private static void UpdateKeyTime(DateTime key, MessageThread startMessage)
    {
        GlobalVariables.Threads ??= new MessageThreadStore();
        GlobalVariables.Threads.Dict ??= new Dictionary<DateTime, List<MessageThread>>();

        lock (GlobalVariables.Threads)
        {
            var list = GlobalVariables.Threads.Dict[key];
            list.Remove(startMessage);

            var newDate = DateTime.Now;

            if (!GlobalVariables.Threads.Dict.ContainsKey(newDate))
                GlobalVariables.Threads.Dict[newDate] = new List<MessageThread>();

            GlobalVariables.Threads.Dict[newDate].Add(startMessage);
        }
    }

    private static void HandleRemoveOutdatedThreadsFromRam(TelegramBotAbstract telegramBotAbstract)
    {
        GlobalVariables.Threads ??= new MessageThreadStore();
        GlobalVariables.Threads.Dict ??= new Dictionary<DateTime, List<MessageThread>>();


        var deletedList = new List<List<MessageThread>>();
        lock (GlobalVariables.Threads)
        {
            var dateTimes = GlobalVariables.Threads.Dict.Keys
                .Where(variable => variable.AddDays(MaxTimeThreadInRamDays) < DateTime.Now)
                .ToList();


            foreach (var variable in dateTimes)
            {
                var messageThreads = GlobalVariables.Threads.Dict[variable];
                GlobalVariables.Threads.Dict.Remove(variable);

                deletedList.Add(messageThreads);
            }

            if (deletedList.Count > 0) WriteThreadsToFile();
        }

        foreach (var v1 in deletedList)
        foreach (var v2 in v1)
            CommentAndClose(v2, telegramBotAbstract);
    }

    private static void CommentAndClose(MessageThread v2, TelegramBotAbstract telegramBotAbstract)
    {
        try
        {
            if (v2.IssueNumber == null)
                return;

            var g = DataTicketClass.GetGitHubClient(telegramBotAbstract);
            var owner = v2.GithubInfo?.CustomOwnerGithub ?? DataTicketClass.OwnerRepo;
            var repo = v2.GithubInfo?.CustomRepoGithub ?? DataTicketClass.NameRepo;
            var issueNumber = v2.IssueNumber.Value;
            var i = g.Issue.Get(owner, repo, issueNumber).Result;

            if (i.State != ItemState.Open) return;


            Comments.CreateComment(telegramBotAbstract, issueNumber, "Closed issue for inactivity.",
                v2.GithubInfo);
            var issueUpdate = new IssueUpdate { State = ItemState.Closed, StateReason = ItemStateReason.Completed };
            g.Issue.Update(owner, repo, issueNumber, issueUpdate);
        }
        catch (Exception ex)
        {
            NotifyUtil.NotifyOwnerWithLog2(ex, telegramBotAbstract, null);
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

            var body = BodyClass.GetBody(e, chatId, date, messageText);


            var titleIssue = messageText.Length > MaxLengthTitleIssue
                ? messageText[..MaxLengthTitleIssue]
                : messageText;

            var issue = CreateIssue.Create(titleIssue, body, e.Message.Chat.Id, e.Message.From?.Id, t, chatIdTgWith100);

            var messageThread = new MessageThread
            {
                MessageId = e.Message.MessageId,
                ChatId = e.Message.Chat.Id,
                IssueNumber = issue.Number,
                GithubInfo = chatIdTgWith100.GithubInfo
            };

            GlobalVariables.Threads ??= new MessageThreadStore();
            GlobalVariables.Threads.Dict ??= new Dictionary<DateTime, List<MessageThread>>();

            lock (GlobalVariables.Threads)
            {
                var dateTime = DateTime.Now;
                if (!GlobalVariables.Threads.Dict.ContainsKey(dateTime))
                    GlobalVariables.Threads.Dict[dateTime] = new List<MessageThread>();


                GlobalVariables.Threads.Dict[dateTime].Add(messageThread);

                WriteThreadsToFile();
            }
        }
        catch (Exception ex)
        {
            NotifyUtil.NotifyOwnerWithLog2(ex, t, new EventArgsContainer { MessageEventArgs = e });
        }
    }

    private static void WriteThreadsToFile()
    {
        try
        {
            FileSerialization.WriteToBinaryFile(
                Paths.Bin.MessagesThread,
                GlobalVariables.Threads
            );
        }
        catch
        {
            // ignored
        }
    }


    private static DateTime GetItalianDateTime(MessageEventArgs e)
    {
        var messageDate = e.Message.Date;
        var diff = DateTime.Now - DateTime.UtcNow;
        var date = messageDate.AddHours(diff.TotalHours);
        return date;
    }

    private static ChatIdTgWith100? AllowedGroupsContains(long chatId)
    {
        return DataTicketClass.AllowedGroups.FirstOrDefault(a => ChatIdTgWith100Equales(a, chatId));
    }

    private static bool ChatIdTgWith100Equales(ChatIdTgWith100 a, long b)
    {
        return a.GetString() == b.ToString();
    }
}
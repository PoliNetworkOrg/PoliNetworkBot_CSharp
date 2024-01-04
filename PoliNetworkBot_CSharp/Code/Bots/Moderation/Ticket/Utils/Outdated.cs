using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Data;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Model;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using PoliNetworkBot_CSharp.Code.Utils.Notify;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Utils;

public static class Outdated
{
    public static void HandleRemoveOutdatedThreadsFromRam(TelegramBotAbstract telegramBotAbstract)
    {
        GlobalVariables.Threads ??= new MessageThreadStore();
        GlobalVariables.Threads.Dict ??= new Dictionary<DateTime, List<MessageThread>>();


        var deletedList = new List<List<MessageThread>>();
        lock (GlobalVariables.Threads)
        {
            bool FindOld(DateTime variable)
            {
                return variable.AddDays(DataTicketClass.MaxTimeThreadInRamDays) < DateTime.Now;
            }

            var dateTimes = GlobalVariables.Threads.Dict.Keys
                .Where(FindOld)
                .ToList();

            foreach (var variable in dateTimes)
            {
                var messageThreads = GlobalVariables.Threads.Dict[variable];
                GlobalVariables.Threads.Dict.Remove(variable);

                deletedList.Add(messageThreads);
            }

            if (deletedList.Count > 0) Write.WriteThreadsToFile();
        }


        try
        {
            var task = new Task(() => { CommentAndCloseDeletedList(telegramBotAbstract, deletedList); });
            task.Start();
        }
        catch (Exception ex)
        {
            NotifyUtil.NotifyOwnerWithLog2(ex, telegramBotAbstract, null);
        }
    }

    private static void CommentAndCloseDeletedList(TelegramBotAbstract telegramBotAbstract,
        List<List<MessageThread>> deletedList)
    {
        var messageThreadsDeleted = deletedList.SelectMany(v1 => v1);
        foreach (var v2 in messageThreadsDeleted)
            CommentAndCloseOutdated(v2, telegramBotAbstract);
    }


    private static void CommentAndCloseOutdated(MessageThread v2, TelegramBotAbstract telegramBotAbstract)
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

            const string msg = "Closed issue for inactivity.";
            Comments.CreateComment(telegramBotAbstract, issueNumber, msg, v2.GithubInfo);
            var issueUpdate = new IssueUpdate { State = ItemState.Closed, StateReason = ItemStateReason.Completed };
            g.Issue.Update(owner, repo, issueNumber, issueUpdate);
        }
        catch (Exception ex)
        {
            NotifyUtil.NotifyOwnerWithLog2(ex, telegramBotAbstract, null);
        }
    }
}
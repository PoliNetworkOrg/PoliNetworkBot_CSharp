using System;
using System.Collections.Generic;
using System.Linq;
using Octokit;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Data;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Model;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using PoliNetworkBot_CSharp.Code.Utils.Notify;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Utils;

public class Outdated
{
    public static void HandleRemoveOutdatedThreadsFromRam(TelegramBotAbstract telegramBotAbstract)
    {
        GlobalVariables.Threads ??= new MessageThreadStore();
        GlobalVariables.Threads.Dict ??= new Dictionary<DateTime, List<MessageThread>>();


        var deletedList = new List<List<MessageThread>>();
        lock (GlobalVariables.Threads)
        {
            var dateTimes = GlobalVariables.Threads.Dict.Keys
                .Where(variable => variable.AddDays(DataTicketClass.MaxTimeThreadInRamDays) < DateTime.Now)
                .ToList();


            foreach (var variable in dateTimes)
            {
                var messageThreads = GlobalVariables.Threads.Dict[variable];
                GlobalVariables.Threads.Dict.Remove(variable);

                deletedList.Add(messageThreads);
            }

            if (deletedList.Count > 0) Write.WriteThreadsToFile();
        }

        foreach (var v1 in deletedList)
        foreach (var v2 in v1)
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
}
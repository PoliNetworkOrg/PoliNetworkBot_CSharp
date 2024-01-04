using System.Collections.Generic;
using Octokit;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Data;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Utils;

public static class GetIssues
{
    public static IReadOnlyList<Issue> GetIssuesMethod(GitHubClient client)
    {
        var issuesForOctokit = client.Issue.GetAllForRepository(DataTicketClass.OwnerRepo, DataTicketClass.NameRepo).Result;
        return issuesForOctokit;
    }
}
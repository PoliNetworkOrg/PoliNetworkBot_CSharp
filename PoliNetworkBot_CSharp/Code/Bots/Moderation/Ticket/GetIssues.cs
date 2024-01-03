using System.Collections.Generic;
using Octokit;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket;

public class GetIssues
{
    public static IReadOnlyList<Issue> GetIssuesMethod(GitHubClient client)
    {
        var issuesForOctokit = client.Issue.GetAllForRepository(Data.OwnerRepo, Data.NameRepo).Result;
        return issuesForOctokit;
    }
}
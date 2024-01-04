using System.Security.Cryptography;
using System.Text;
using Octokit;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Data;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Model;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Utils;

public static class CreateIssue
{
    public static Issue Create(string title, string body, long telegramChatId, long? fromId,
        TelegramBotAbstract telegramBotAbstract, ChatIdTgWith100? chatIdTgWith100)

    {
        var githubInfo = chatIdTgWith100?.GithubInfo;
        var githubClient = DataTicketClass.GetGitHubClient(telegramBotAbstract);
        var newIssue = new NewIssue(title)
        {
            Body = body
        };
        CreateAndAddLabel(GetLabelIdTelegramName(telegramChatId, "id"), newIssue, telegramBotAbstract, githubInfo);
        CreateAndAddLabel(GetLabelIdTelegramName(fromId, "u"), newIssue, telegramBotAbstract, githubInfo);


        CreateAndAddLabel(githubInfo?.CategoryGithub, newIssue, telegramBotAbstract, githubInfo);

        var owner = githubInfo?.CustomOwnerGithub ?? DataTicketClass.OwnerRepo;
        var repo = githubInfo?.CustomRepoGithub ?? DataTicketClass.NameRepo;
        var task = githubClient.Issue.Create(owner, repo, newIssue);
        task.Wait();
        return task.Result;
    }

    private static void CreateAndAddLabel(string? labelIdTelegramName, NewIssue newIssue,
        TelegramBotAbstract telegramBotAbstract, GithubInfo? githubInfo)
    {
        if (string.IsNullOrEmpty(labelIdTelegramName))
            return;

        CreateLabel(labelIdTelegramName, telegramBotAbstract, githubInfo);
        newIssue.Labels.Add(labelIdTelegramName);
    }

    private static void CreateLabel(string? labelIdTelegramName, TelegramBotAbstract telegramBotAbstract,
        GithubInfo? githubInfo)
    {
        if (string.IsNullOrEmpty(labelIdTelegramName))
            return;

        try
        {
            var generateHexColor = GenerateHexColor(labelIdTelegramName);
            var label = new NewLabel(labelIdTelegramName, generateHexColor);
            var githubClient = DataTicketClass.GetGitHubClient(telegramBotAbstract);
            var owner = githubInfo?.CustomOwnerGithub ?? DataTicketClass.OwnerRepo;
            var repo = githubInfo?.CustomRepoGithub ?? DataTicketClass.NameRepo;

            var labelCreated = githubClient.Issue.Labels
                .Create(owner, repo, label).Result;
        }
        catch
        {
            // ignored
        }
    }

    private static string? GetLabelIdTelegramName(long? longVar, string prefix)
    {
        if (longVar == null)
            return null;

        var t = longVar.ToString()?.Replace("-", "_");
        var labelIdTelegramName = prefix + "_" + t;
        return labelIdTelegramName;
    }

    private static string GenerateHexColor(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return "FFFFFF";

        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);

        // Take the first 3 bytes of the hash to determine RGB values
        var r = hashBytes[0];
        var g = hashBytes[1];
        var b = hashBytes[2];

        // Convert RGB values to hex
        var hexColor = $"{r:X2}{g:X2}{b:X2}";

        return hexColor;
    }
}
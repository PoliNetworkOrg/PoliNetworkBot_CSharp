using System.Security.Cryptography;
using System.Text;
using Octokit;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket;

public static class CreateIssue
{
    public static Issue Create(string title, string body, long telegramChatId, long? fromId)
    {
        var githubClient = Data.GetGitHubClient();
        var newIssue = new NewIssue(title)
        {
            Body = body
        };
        CreateAndAddLabel(GetLabelIdTelegramName(telegramChatId, "id"), newIssue);
        CreateAndAddLabel(GetLabelIdTelegramName(fromId, "u"), newIssue);
        var task = githubClient.Issue.Create(Data.OwnerRepo, Data.NameRepo, newIssue);
        task.Wait();
        return task.Result;
    }

    private static void CreateAndAddLabel(string? labelIdTelegramName, NewIssue newIssue)
    {
        if (string.IsNullOrEmpty(labelIdTelegramName))
            return;

        CreateLabel(labelIdTelegramName);
        newIssue.Labels.Add(labelIdTelegramName);
    }

    private static void CreateLabel(string? labelIdTelegramName)
    {
        if (string.IsNullOrEmpty(labelIdTelegramName))
            return;

        try
        {
            var generateHexColor = GenerateHexColor(labelIdTelegramName);
            NewLabel label = new NewLabel(labelIdTelegramName, generateHexColor);
            var githubClient = Data.GetGitHubClient();
            githubClient.Issue.Labels.Create(Data.OwnerRepo, Data.NameRepo, label).Wait();
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

        using MD5 md5 = MD5.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        // Take the first 3 bytes of the hash to determine RGB values
        byte r = hashBytes[0];
        byte g = hashBytes[1];
        byte b = hashBytes[2];

        // Convert RGB values to hex
        string hexColor = $"{r:X2}{g:X2}{b:X2}";

        return hexColor;
    }
}
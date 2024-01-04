using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octokit;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Data;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Utils;

public static class Comments
{
    public static JArray? GetComments(Issue issue, TelegramBotAbstract telegramBotAbstract)
    {
        if (issue.Comments <= 0) return null;

        var httpClient = DataTicketClass.GetHttpClient(telegramBotAbstract);
        var r = httpClient.GetAsync(issue.CommentsUrl).Result;
        var responseBody = r.Content.ReadAsStringAsync().Result;
        var jArray = (JArray?)JsonConvert.DeserializeObject(responseBody);
        return jArray;
    }

    public static void CreateComment(TelegramBotAbstract telegramBotAbstract, int issueNumber, string body)
    {
        //https://docs.github.com/en/rest/issues/comments?apiVersion=2022-11-28#create-an-issue-comment
        var client = DataTicketClass.GetHttpClient(telegramBotAbstract);
        var jObject = new JObject
        {
            ["body"] = body
        };
        var serializeObject = JsonConvert.SerializeObject(jObject);
        const string applicationJson = "application/json";
        var content = new StringContent(serializeObject, Encoding.UTF8, applicationJson);

        var uri =
            $"https://api.github.com/repos/{DataTicketClass.OwnerRepo}/{DataTicketClass.NameRepo}/issues/{issueNumber}/comments";

        client.PostAsync(uri,
            content);
    }
}
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
}
using Newtonsoft.Json.Linq;
using Octokit;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket;

public static class Comments
{
    public static JArray? GetComments(Issue issue)
    {
        if (issue.Comments <= 0) return null;

        var httpClient = Data.GetHttpClient();
        var r = httpClient.GetAsync(issue.CommentsUrl).Result;
        var responseBody = r.Content.ReadAsStringAsync().Result;
        var jArray = (JArray?)Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
        return jArray;
    }
}
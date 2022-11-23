#region

using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Main;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Anon;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
internal class WebPost
{
    public char? approved;
    public string? password;

    public long? photoid;
    //public JObject r4;

    public long postid;
    public char seen;
    public string? text;
    public DateTime? whensubmitted;

    public WebPost(JObject r4)
    {
        //this.r4 = r4;
        var r23 = r4["PostID"];
        if (r23 != null)
        {
            var x = r23.Values()[0];
        }

        foreach (var r5 in r4.Children())
        {
            if (r5 is not JProperty r6) continue;

            if (r6.Value is JValue r7)
            {
                var r8 = r7.Value;
                switch (r6.Name)
                {
                    case "PostID":
                    {
                        postid = Convert.ToInt64(r8);
                        break;
                    }

                    case "Text":
                    {
                        text = r7.Value?.ToString();
                        break;
                    }

                    case "PhotoID":
                    {
                        long? p = null;
                        try
                        {
                            p = Convert.ToInt64(r7.Value);
                        }
                        catch
                        {
                            // ignored
                        }

                        photoid = p;

                        if (photoid <= 0) photoid = null;

                        break;
                    }

                    case "Approved":
                    {
                        var r9 = r8?.ToString();
                        if (r8 != null)
                            if (r9 != null)
                                approved = r9[0];
                        break;
                    }

                    case "Password":
                    {
                        var r9 = r8?.ToString();
                        password = r9;
                        break;
                    }

                    case "Seen":
                    {
                        var r9 = r8?.ToString();
                        if (r9 != null) seen = r9[0];
                        break;
                    }

                    case "WhenSubmitted":
                    {
                        whensubmitted = Convert.ToDateTime(r7.Value);
                        break;
                    }
                }
            }
        }
    }

    internal async Task<bool> PlaceInQueue()
    {
        var telegramBotAbstract = await GetAnonBotAsync();
        if (telegramBotAbstract == null)
            return false;
        var e = new MessageAnonToSendInQueue(this);
        return await MainAnon.PlaceMessageInQueue(telegramBotAbstract, e, 0, null);
    }

    public static async Task<TelegramBotAbstract?> GetAnonBotAsync()
    {
        var bots = GlobalVariables.Bots;
        if (bots != null)
            return (from key in bots.Keys
                let telegramBotAbstract = bots[key]
                let m = telegramBotAbstract.GetMode()
                where m == BotStartMethods.Anon.Item1
                select bots[key]).FirstOrDefault();
        try
        {
            await ProgramUtil.StartBotsAsync(false, false, true);
        }
        catch
        {
            // ignored
        }

        if (bots != null)
            return (from key in bots.Keys
                let telegramBotAbstract = bots[key]
                let m = telegramBotAbstract.GetMode()
                where m == BotStartMethods.Anon.Item1
                select telegramBotAbstract).FirstOrDefault();
        return null;
    }

    internal async Task<bool> SetAsSeenAsync()
    {
        var url = "https://spottedpolimi.altervista.org/s/setseen.php?id=" + postid + "&password=" +
                  ConfigAnon.Password + "&seen=Y";
        var x = await Web.DownloadHtmlAsync(url);
        if (!x.IsValid())
            return false;

        seen = 'Y';
        return true;
    }

    internal static async Task<bool> SetApprovedStatusAsync(CallBackDataAnon x)
    {
        var approved = Approved(x);
        if (x.AuthorId == null) return false;
        var url = "https://spottedpolimi.altervista.org/s/setapproved.php?id=" + x.AuthorId.Value + "&password=" +
                  ConfigAnon.Password + "&approved=" + approved;
        var x2 = await Web.DownloadHtmlAsync(url);
        if (!x2.IsValid())
            return false;

        if (ThreadAsyncAnon.DictionaryWebpost != null)
            ThreadAsyncAnon.DictionaryWebpost[x.AuthorId.Value].approved = approved;
        ThreadAsyncAnon.WriteDict();
        return true;
    }

    private static char? Approved(CallBackDataAnon x)
    {
        var s = CallBackDataAnon.ResultToString(x.GetResultEnum());
        return s?[0];
    }
}
#region

using System;
using System.Linq;
using System.Net.Cache;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.MainProgram;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    [Serializable]
    internal class WebPost
    {
        public char approved;
        public string password;

        public long? photoid;
        //public JObject r4;

        public long postid;
        public char seen;
        public string text;
        public DateTime? whensubmitted;

        public WebPost(JObject r4)
        {
            //this.r4 = r4;
            ;
            var x = r4["PostID"].Values()[0];
            ;

            foreach (var r5 in r4.Children())
            {
                ;
                if (r5 is not JProperty r6) continue;
                ;

                if (r6.Value is JValue r7)
                    switch (r6.Name)
                    {
                        case "PostID":
                        {
                            postid = Convert.ToInt64(r7.Value);
                            break;
                        }

                        case "Text":
                        {
                            text = r7.Value.ToString();
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
                                ;
                            }

                            photoid = p;

                            if (photoid <= 0) photoid = null;

                            break;
                        }

                        case "Approved":
                        {
                            approved = r7.Value.ToString()[0];
                            break;
                        }

                        case "Password":
                        {
                            password = r7.Value.ToString();
                            break;
                        }

                        case "Seen":
                        {
                            seen = r7.Value.ToString()[0];
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

        internal async Task<bool> PlaceInQueue()
        {
            var telegramBotAbstract = await GetAnonBotAsync();
            if (telegramBotAbstract == null)
                return false;
            var e = new MessaggeAnonToSendInQueue(this);
            return await MainAnon.PlaceMessageInQueue(telegramBotAbstract, e, 0, null);
        }

        public static async Task<TelegramBotAbstract> GetAnonBotAsync()
        {
            if (GlobalVariables.Bots != null)
                return (from key in GlobalVariables.Bots.Keys
                    let m = GlobalVariables.Bots[key].GetMode()
                    where m == BotStartMethods.Anon
                    select GlobalVariables.Bots[key]).FirstOrDefault();
            try
            {
                await Program.StartBotsAsync(false, false, true);
            }
            catch
            {
                ;
            }

            return (from key in GlobalVariables.Bots.Keys
                let m = GlobalVariables.Bots[key].GetMode()
                where m == BotStartMethods.Anon
                select GlobalVariables.Bots[key]).FirstOrDefault();
        }

        internal async Task SetAsSeenAsync()
        {
            var url = "https://spottedpolimi.altervista.org/s/setseen.php?id=" + postid + "&password=" +
                      ConfigAnon.password + "&seen=Y";
            var x = await Web.DownloadHtmlAsync(url, RequestCacheLevel.NoCacheNoStore);
            seen = 'Y';
        }

        internal static async Task<bool> SetApprovedStatusAsync(CallBackDataAnon x)
        {
            var approved = Approved(x);
            if (x.userId == null) return false;
            var url = "https://spottedpolimi.altervista.org/s/setapproved.php?id=" + x.userId.Value + "&password=" +
                      ConfigAnon.password + "&approved=" + approved;
            var x2 = await Web.DownloadHtmlAsync(url, RequestCacheLevel.NoCacheNoStore);
            ThreadAsync.dictionary_webpost[x.userId.Value].approved = approved;
            ThreadAsync.WriteDict();
            return true;
        }

        private static char Approved(CallBackDataAnon x)
        {
            var s = CallBackDataAnon.ResultToString(x.resultQueueEnum);
            return s[0];
        }
    }
}
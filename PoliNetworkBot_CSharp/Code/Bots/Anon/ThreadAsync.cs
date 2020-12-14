using Newtonsoft.Json;
using System;
using System.Threading;

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    internal class ThreadAsync
    {
        
        internal static async void DoThingsAsyncBotAsync(object obj)
        {
            while (true)
            {
                try
                {
                    string url = "https://spottedpolimi.altervista.org/s/getposts.php?password=";


                    string urlFinal = url + Anon.ConfigAnon.password;

                    Objects.WebObject.WebReply x = await Utils.Web.DownloadHtmlAsync(urlFinal);
                    if (x == null)
                    {
                        continue;
                    }

                    if (x.IsValid() == false)
                    {
                        continue;
                    }

                    string data = x.GetData();

                    await DoThingsAsyncBotAsync2Async(data);
                }
                catch (Exception e)
                {
                    ;
                }

                Thread.Sleep(1000 * 30);
            }
        }

        public static async System.Threading.Tasks.Task DoThingsAsyncBotAsync2Async(string data)
        {
            ;

            try
            {
                var result = JsonConvert.DeserializeObject<object>(data);
                ;

                if (result is Newtonsoft.Json.Linq.JArray r2)
                {
                    ;
                    foreach (Newtonsoft.Json.Linq.JToken r3 in r2)
                    {
                        ;

                        if (r3 is Newtonsoft.Json.Linq.JObject r4)
                        {
                            ;

                            WebPost webPost = new WebPost(r4);
                            await DoThingsAsyncBotAsync3Async(webPost);
                        }
                    }
                }
            }
            catch
            {
                ;
            }
        }

        private static async System.Threading.Tasks.Task DoThingsAsyncBotAsync3Async(WebPost webPost)
        {
            if (webPost == null)
                return;

            ;
            try
            {
                await webPost.setAsSeenAsync();

                await webPost.PlaceInQueue();
            }
            catch (Exception e)
            {
                ;
            }

        }
    }
}
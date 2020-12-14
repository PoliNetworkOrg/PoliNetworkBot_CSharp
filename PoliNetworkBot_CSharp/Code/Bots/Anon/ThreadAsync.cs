using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    internal class ThreadAsync
    {
        
        internal static async void DoThingsAsyncBotAsync(object obj)
        {
            var bot = await Code.Bots.Anon.WebPost.GetAnonBotAsync();
            if (bot == null)
                return;

            if (string.IsNullOrEmpty(Anon.ConfigAnon.password))
                return;

            try
            {
                await Utils.NotifyUtil.NotifyOwners(new Exception(message: "Check anon message started."), bot);
            }
            catch
            {
                ;
            }

            while (true)
            {
                try
                {
                    string url = "https://spottedpolimi.altervista.org/s/getposts.php?password=";


                    string urlFinal = url + Anon.ConfigAnon.password;

                    Objects.WebObject.WebReply x = await Utils.Web.DownloadHtmlAsync(urlFinal, System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
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
                    await Utils.ExceptionNumbered.SendExceptionAsync(e, bot);
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

        public static Dictionary<long, WebPost> dictionary_webpost = new Dictionary<long, WebPost>();

        private static async System.Threading.Tasks.Task DoThingsAsyncBotAsync3Async(WebPost webPost)
        {
            lock (dictionary_webpost)
            {
                _ = DoThingsAsyncBotAsync4Async(webPost);
            }

        }

        private static async System.Threading.Tasks.Task DoThingsAsyncBotAsync4Async(WebPost webPost)
        {
            if (webPost == null)
                return;

            if (webPost.seen == 'Y')
                return;

            if (dictionary_webpost == null)
            {
                dictionary_webpost = new Dictionary<long, WebPost>();
            }

            if (dictionary_webpost.ContainsKey(webPost.postid) && dictionary_webpost[webPost.postid].seen == 'Y')
            {
                return;
            }

            ;

            try
            {
                await webPost.SetAsSeenAsync();

                await webPost.PlaceInQueue();

                dictionary_webpost[webPost.postid] = webPost;
            }
            catch (Exception e)
            {
                ;
            }
        }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    internal class ThreadAsync
    {
        public static Random random = new Random();
        public const int timesleep = 1000 * 30;

        private static string GenerateRandomString(int length)
        {
            string r = "";
            for (int i = 0; i < length; i++)
            {
                double r2 = random.NextDouble() * 26;
                int r3 = (int)r2;
                int r4 = 'A' + r3;
                char r5 = (char)r4;
                r += r5;
            }
            return r;
        }

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

            if (dictionary_webpost == null)
            {
                dictionary_webpost = GetDictionary();
            }

            while (true)
            {
                lock (random)
                {
                    _ = IterationAsync2Async(bot);
                    Thread.Sleep(timesleep);
                }
            }
        }

        private static async System.Threading.Tasks.Task IterationAsync2Async(Objects.TelegramBotAbstract bot)
        {
            await IterationAsync(bot);
        }

        private static async System.Threading.Tasks.Task IterationAsync(Objects.TelegramBotAbstract bot)
        {
            try
            {
                string url = "https://spottedpolimi.altervista.org/s/getposts.php?password=";

                string urlFinal = url + Anon.ConfigAnon.password;
                string randomstring = GenerateRandomString(30);
                urlFinal += "&random=" + randomstring;

                Objects.WebObject.WebReply x = await Utils.Web.DownloadHtmlAsync(urlFinal, System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                if (x == null || x.IsValid() == false)
                {
                    return;
                }

                string data = x.GetData();

                await DoThingsAsyncBotAsync2Async(data);
            }
            catch (Exception e)
            {
                await Utils.ExceptionNumbered.SendExceptionAsync(e, bot);
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

        public static Dictionary<long, WebPost> dictionary_webpost = null;

        private static async System.Threading.Tasks.Task DoThingsAsyncBotAsync3Async(WebPost webPost)
        {
            if (dictionary_webpost == null)
            {
                dictionary_webpost = GetDictionary();
            }

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
                dictionary_webpost = GetDictionary();
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
                WriteDict();
            }
            catch (Exception e)
            {
                ;
            }
        }

        public static void WriteDict()
        {
            try
            {
                Utils.FileSerialization.WriteToBinaryFile(pathwebdict, dictionary_webpost);
            }
            catch
            {
                ;
            }
        }

        public const string pathwebdict = "webposts.bin";

        private static Dictionary<long, WebPost> GetDictionary()
        {
            bool done = false;
            try
            {
                dictionary_webpost = Utils.FileSerialization.ReadFromBinaryFile<Dictionary<long, WebPost>>(pathwebdict);
                if (dictionary_webpost != null)
                    done = true;
            }
            catch
            {
                ;
            }

            if (!done)
            {
                dictionary_webpost = new Dictionary<long, WebPost>();
            }

            WriteDict();
            return dictionary_webpost;
        }
    }
}
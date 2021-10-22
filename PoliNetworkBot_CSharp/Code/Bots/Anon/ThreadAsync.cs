using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    internal class ThreadAsync
    {
        public const int timesleep = 1000 * 30;

        public const string pathwebdict = "webposts.bin";
        public static Random random = new Random();

        public static Dictionary<long, WebPost> dictionary_webpost;

        private static string GenerateRandomString(int length)
        {
            var r = "";
            for (var i = 0; i < length; i++)
            {
                var r2 = random.NextDouble() * 26;
                var r3 = (int)r2;
                var r4 = 'A' + r3;
                var r5 = (char)r4;
                r += r5;
            }

            return r;
        }

        internal static async void DoThingsAsyncBotAsync(object obj)
        {
            var bot = await WebPost.GetAnonBotAsync();
            if (bot == null)
                return;

            if (string.IsNullOrEmpty(ConfigAnon.password))
                return;

            try
            {
                await NotifyUtil.NotifyOwners(new Exception("Check anon message started."), bot);
            }
            catch
            {
                ;
            }

            if (dictionary_webpost == null) dictionary_webpost = GetDictionary();

            while (true)
                lock (random)
                {
                    _ = IterationAsync2Async(bot);
                    Thread.Sleep(timesleep);
                }
        }

        private static async Task IterationAsync2Async(TelegramBotAbstract bot)
        {
            await IterationAsync(bot);
        }

        private static async Task IterationAsync(TelegramBotAbstract bot)
        {
            try
            {
                var url = "https://spottedpolimi.altervista.org/s/getposts.php?password=";

                var urlFinal = url + ConfigAnon.password;
                var randomstring = GenerateRandomString(30);
                urlFinal += "&random=" + randomstring;

                var x = await Web.DownloadHtmlAsync(urlFinal, RequestCacheLevel.NoCacheNoStore);
                if (x == null || x.IsValid() == false) return;

                var data = x.GetData();

                await DoThingsAsyncBotAsync2Async(data);
            }
            catch (Exception e)
            {
                await ExceptionNumbered.SendExceptionAsync(e, bot);
            }
        }

        public static async Task DoThingsAsyncBotAsync2Async(string data)
        {
            ;

            try
            {
                var result = JsonConvert.DeserializeObject<object>(data);
                ;

                if (result is JArray r2)
                {
                    ;
                    foreach (var r3 in r2)
                    {
                        ;

                        if (r3 is JObject r4)
                        {
                            ;

                            var webPost = new WebPost(r4);
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

        private static async Task DoThingsAsyncBotAsync3Async(WebPost webPost)
        {
            if (dictionary_webpost == null) dictionary_webpost = GetDictionary();

            lock (dictionary_webpost)
            {
                _ = DoThingsAsyncBotAsync4Async(webPost);
            }
        }

        private static async Task DoThingsAsyncBotAsync4Async(WebPost webPost)
        {
            if (webPost == null)
                return;

            if (webPost.seen == 'Y')
                return;

            if (dictionary_webpost == null) dictionary_webpost = GetDictionary();

            if (dictionary_webpost.ContainsKey(webPost.postid) &&
                dictionary_webpost[webPost.postid].seen == 'Y') return;

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
                Console.WriteLine(e);
            }
        }

        public static void WriteDict()
        {
            try
            {
                FileSerialization.WriteToBinaryFile(pathwebdict, dictionary_webpost);
            }
            catch
            {
                ;
            }
        }

        private static Dictionary<long, WebPost> GetDictionary()
        {
            var done = false;
            try
            {
                dictionary_webpost = FileSerialization.ReadFromBinaryFile<Dictionary<long, WebPost>>(pathwebdict);
                if (dictionary_webpost != null)
                    done = true;
            }
            catch
            {
                ;
            }

            if (!done) dictionary_webpost = new Dictionary<long, WebPost>();

            WriteDict();
            return dictionary_webpost;
        }
    }
}
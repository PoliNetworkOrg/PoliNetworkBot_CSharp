#region

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Anon;

internal class ThreadAsync
{
    public const int timesleep = 1000 * 30;

    public const string pathwebdict = "webposts.bin";
    private static readonly Random random = new();

    public static Dictionary<long, WebPost> dictionary_webpost;

    private static string GenerateRandomString(int length)
    {
        var r = "";
        for (var i = 0; i < length; i++)
        {
            var r2 = random.NextDouble() * 26;
            var r3 = (long)r2;
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
            await NotifyUtil.NotifyOwners(new Exception("Check anon message started."), bot, null);
        }
        catch
        {
            ;
        }

        dictionary_webpost ??= GetDictionary();

        while (true)
            lock (random)
            {
                _ = IterationAsync2Async(bot, null);
                Thread.Sleep(timesleep);
            }
    }

    private static async Task IterationAsync2Async(TelegramBotAbstract bot, MessageEventArgs messageEventArgs)
    {
        await IterationAsync(bot, messageEventArgs);
    }

    private static async Task IterationAsync(TelegramBotAbstract bot, MessageEventArgs messageEventArgs)
    {
        try
        {
            const string url = "https://spottedpolimi.altervista.org/s/getposts.php?password=";

            var urlFinal = url + ConfigAnon.password;
            var randomstring = GenerateRandomString(30);
            urlFinal += "&random=" + randomstring;

            var x = await Web.DownloadHtmlAsync(urlFinal, RequestCacheLevel.NoCacheNoStore);
            if (x == null || x.IsValid() == false) return;

            var data = x.GetData();

            DoThingsAsyncBotAsync2(data);
        }
        catch (Exception e)
        {
            await ExceptionNumbered.SendExceptionAsync(e, bot, messageEventArgs);
        }
    }

    public static void DoThingsAsyncBotAsync2(string data)
    {
        ;

        try
        {
            var result = JsonConvert.DeserializeObject<object>(data);
            ;

            if (result is not JArray r2) return;
            ;
            foreach (var r3 in r2)
            {
                ;

                if (r3 is not JObject r4) continue;
                ;

                var webPost = new WebPost(r4);
                DoThingsAsyncBotAsync3(webPost);
            }
        }
        catch
        {
            ;
        }
    }

    private static void DoThingsAsyncBotAsync3(WebPost webPost)
    {
        dictionary_webpost ??= GetDictionary();

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

        dictionary_webpost ??= GetDictionary();

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
            Logger.WriteLine(e);
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
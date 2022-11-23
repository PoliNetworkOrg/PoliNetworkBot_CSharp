#region

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using PoliNetworkBot_CSharp.Code.Utils.Notify;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Anon;

internal static class ThreadAsyncAnon
{
    private const int Timesleep = 1000 * 30;

    private const string Pathwebdict = "webposts.bin";
    private static readonly Random Random = new();

    public static Dictionary<long, WebPost>? DictionaryWebpost;

    private static string GenerateRandomString(int length)
    {
        var r = "";
        for (var i = 0; i < length; i++)
        {
            var r2 = Random.NextDouble() * 26;
            var r3 = (long)r2;
            var r4 = 'A' + r3;
            var r5 = (char)r4;
            r += r5;
        }

        return r;
    }

    public static async void DoThingsAsyncBot_Anon_First_Async(object? obj)
    {
        var bot = await WebPost.GetAnonBotAsync();
        if (bot == null)
            return;

        if (string.IsNullOrEmpty(ConfigAnon.Password))
            return;

        try
        {
            await NotifyUtil.NotifyOwnerWithLog2(new Exception("Check anon message started."), bot, null);
        }
        catch
        {
            // ignored
        }

        DictionaryWebpost ??= GetDictionary();

        while (true)
            lock (Random)
            {
                _ = IterationAsync2Async(bot, null);
                Thread.Sleep(Timesleep);
            }
    }

    private static async Task<bool> IterationAsync2Async(TelegramBotAbstract? bot, MessageEventArgs? messageEventArgs)
    {
        return await IterationAsync(bot, messageEventArgs);
    }

    private static async Task<bool> IterationAsync(TelegramBotAbstract? bot, MessageEventArgs? messageEventArgs)
    {
        try
        {
            const string url = "https://spottedpolimi.altervista.org/s/getposts.php?password=";

            var urlFinal = url + ConfigAnon.Password;
            var randomString = GenerateRandomString(30);
            urlFinal += "&random=" + randomString;

            var x = await Web.DownloadHtmlAsync(urlFinal);
            if (x.IsValid() == false)
                return false;

            var data = x.GetData();

            DoThingsAsyncBotAsync2(data);
        }
        catch (Exception? e)
        {
            return await ExceptionNumbered.SendExceptionAsync(e, bot, EventArgsContainer.Get(messageEventArgs));
        }

        return true;
    }

    private static void DoThingsAsyncBotAsync2(string? data)
    {
        try
        {
            JArray? r2 = null;
            if (data != null)
            {
                var result = JsonConvert.DeserializeObject<object>(data);

                if (result is not JArray array) return;
                r2 = array;
            }

            if (r2 == null) return;
            foreach (var r3 in r2)
            {
                if (r3 is not JObject r4) continue;

                var webPost = new WebPost(r4);
                DoThingsAsyncBotAsync3(webPost);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void DoThingsAsyncBotAsync3(WebPost? webPost)
    {
        DictionaryWebpost ??= GetDictionary();

        if (DictionaryWebpost == null) return;
        lock (DictionaryWebpost)
        {
            _ = DoThingsAsyncBotAsync4Async(webPost);
        }
    }

    private static async Task DoThingsAsyncBotAsync4Async(WebPost? webPost)
    {
        if (webPost == null)
            return;

        if (webPost.seen == 'Y')
            return;

        DictionaryWebpost ??= GetDictionary();

        if (DictionaryWebpost != null && DictionaryWebpost.ContainsKey(webPost.postid) &&
            DictionaryWebpost[webPost.postid].seen == 'Y') return;

        try
        {
            var b = await webPost.SetAsSeenAsync();
            if (b)
            {
                await webPost.PlaceInQueue();

                if (DictionaryWebpost != null) DictionaryWebpost[webPost.postid] = webPost;
                WriteDict();
            }
            else
            {
                throw new Exception();
            }
        }
        catch (Exception? e)
        {
            Logger.WriteLine(e);
        }
    }

    public static void WriteDict()
    {
        try
        {
            FileSerialization.WriteToBinaryFile(Pathwebdict, DictionaryWebpost);
        }
        catch
        {
            // ignored
        }
    }

    private static Dictionary<long, WebPost>? GetDictionary()
    {
        var done = false;
        try
        {
            DictionaryWebpost = FileSerialization.ReadFromBinaryFile<Dictionary<long, WebPost>>(Pathwebdict);
            if (DictionaryWebpost != null)
                done = true;
        }
        catch
        {
            // ignored
        }

        if (!done) DictionaryWebpost = new Dictionary<long, WebPost>();

        WriteDict();
        return DictionaryWebpost;
    }
}
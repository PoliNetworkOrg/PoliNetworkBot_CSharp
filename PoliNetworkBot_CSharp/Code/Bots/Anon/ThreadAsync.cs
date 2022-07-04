﻿#region

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Anon;

internal static class ThreadAsync
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

    internal static async void DoThingsAsyncBotAsync(object? obj)
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

        DictionaryWebpost ??= GetDictionary();

        while (true)
            lock (Random)
            {
                _ = IterationAsync2Async(bot, null);
                Thread.Sleep(Timesleep);
            }
    }

    private static async Task IterationAsync2Async(TelegramBotAbstract? bot, MessageEventArgs? messageEventArgs)
    {
        await IterationAsync(bot, messageEventArgs);
    }

    private static async Task IterationAsync(TelegramBotAbstract? bot, MessageEventArgs? messageEventArgs)
    {
        try
        {
            const string url = "https://spottedpolimi.altervista.org/s/getposts.php?password=";

            var urlFinal = url + ConfigAnon.password;
            var randomstring = GenerateRandomString(30);
            urlFinal += "&random=" + randomstring;

            var x = await Web.DownloadHtmlAsync(urlFinal);
            if (x == null || x.IsValid() == false) return;

            var data = x.GetData();

            DoThingsAsyncBotAsync2(data);
        }
        catch (Exception? e)
        {
            await ExceptionNumbered.SendExceptionAsync(e, bot, messageEventArgs);
        }
    }

    private static void DoThingsAsyncBotAsync2(string? data)
    {
        ;

        try
        {
            JArray? r2 = null;
            if (data != null)
            {
                var result = JsonConvert.DeserializeObject<object>(data);
                ;

                if (result is not JArray) return;
                r2 = (JArray?)result;
            }

            ;
            if (r2 != null)
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
        DictionaryWebpost ??= GetDictionary();

        if (DictionaryWebpost != null)
            lock (DictionaryWebpost)
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

        DictionaryWebpost ??= GetDictionary();

        if (DictionaryWebpost != null && DictionaryWebpost.ContainsKey(webPost.postid) && DictionaryWebpost[webPost.postid].seen == 'Y') return;

        ;

        try
        {
            await webPost.SetAsSeenAsync();

            await webPost.PlaceInQueue();

            if (DictionaryWebpost != null) DictionaryWebpost[webPost.postid] = webPost;
            WriteDict();
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
            ;
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
            ;
        }

        if (!done) DictionaryWebpost = new Dictionary<long, WebPost>();

        WriteDict();
        return DictionaryWebpost;
    }
}
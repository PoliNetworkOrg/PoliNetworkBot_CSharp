using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HtmlAgilityPack;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Bots.RoomsBot.Data;
using PoliNetworkBot_CSharp.Code.Utils;

namespace PoliNetworkBot_CSharp.Code.Bots.RoomsBot;

public class Fetcher
{
    private static readonly int MaximumApiCallsPerSecond = 5;
    private static readonly int MaximumApiCallsPerMinute = 30;
    private static readonly object Lock = new();

    private static int ApiCallsCounterPerSeconds;
    private static int ApiCallsSecondTracker;

    private static int ApiCallsCounterPerMinute;
    private static int ApiCallsMinuteTracker;

    private static readonly TimeSpan CacheInvalidationTime = TimeSpan.FromHours(1);
    private static readonly Dictionary<string, Dictionary<DateTime, DateTime>> FetchCacheAge = new();
    private static readonly Dictionary<string, Dictionary<DateTime, HtmlDocument>> RawFetchedFile = new();

    public static string? GetRawOccupancies(string campus, DateTime dateTime)
    {
        var doc = FetchOccupationData(campus, dateTime);
        var parsedDoc = doc.DocumentNode.SelectNodes("//table[contains(@class, 'BoxInfoCard')]");
        var text = parsedDoc.Nodes().Aggregate(Const.CssStyles, (current, node) => current + node.OuterHtml);
        return text;
    }

    public static List<string>? GetFreeClassrooms(string campus, DateTime rawDateTime, double startingTime,
        double endingTime)
    {
        var dateTime = rawDateTime.Date;
        var doc = FetchOccupationData(campus, dateTime.Date);
        var t1 = HtmlUtil.GetElementsByTagAndClassName(doc.DocumentNode, "", "BoxInfoCard", 1);

        var t3 = HtmlUtil.GetElementsByTagAndClassName(t1?[0], "", "scrollContent");

        return Rooms.GetFreeRooms(t3?[0], dateTime.AddHours(startingTime), dateTime.AddHours(endingTime), 1);
    }


    public static List<string>? GetAllClassrooms(string campus, DateTime dateTime)
    {
        return GetFreeClassrooms(campus, dateTime, 8, 8);
    }

    public static string? GetSingleClassroom(string campus, string roomName, DateTime dateTime)
    {
        var doc = FetchOccupationData(campus, dateTime);
        var htmlNodeCollection = doc.DocumentNode.SelectNodes("//tr[contains(@class, 'normalRow')]");
        foreach (var classNode in htmlNodeCollection)
            if (classNode.ChildNodes[1].InnerText.Contains(roomName))
                return Const.CssStyles + Const.HtmlTableInit + Const.HtmlClockLine + classNode.OuterHtml +
                       Const.HtmlTableEnd;

        return null;
    }

    private static void FixHyperlinks(HtmlNode classNode)
    {
        foreach (var link in classNode.SelectNodes("//a[@href]"))
        {
            var att = link.Attributes["href"];
            att.Value = Const.HrefRepairLink + att.Value;
        }
    }

    private static HtmlDocument FetchOccupationData(string campus, DateTime dateTime)
    {
        lock (Lock)
        {
            if (FetchCacheAge.ContainsKey(campus)
                && FetchCacheAge[campus].ContainsKey(dateTime)
                && DateTime.Now - FetchCacheAge[campus][dateTime] < CacheInvalidationTime)
                return RawFetchedFile[campus][dateTime];
            if (dateTime != DateTime.Today && dateTime != DateTime.Today + TimeSpan.FromDays(1))
                CheckApiRateLimit();
            var doc = new HtmlDocument();
            var web = new HtmlWeb();
            Console.WriteLine("### Requested passed to API endpoint ###");
            doc = web.Load(Const.PolimiController +
                           $"?csic={campus}&tipologia={Data.Enums.RoomType.tutte}&categoria=tutte" +
                           $"&giorno_day={dateTime.Day}" +
                           $"&giorno_month={dateTime.Month}" +
                           $"&giorno_year={dateTime.Year}" +
                           "&jaf_giorno_date_format=dd%2FMM%2Fyyyy&&evn_visualizza=");
            FixHyperlinks(doc.DocumentNode);
            RawFetchedFile.Remove(campus);
            RawFetchedFile.Add(campus, new Dictionary<DateTime, HtmlDocument>());
            RawFetchedFile[campus].Add(dateTime, doc);
            FetchCacheAge.Remove(campus);
            FetchCacheAge.Add(campus, new Dictionary<DateTime, DateTime>());
            FetchCacheAge[campus].Add(dateTime, DateTime.Now);
            Thread.Sleep(50);
            return doc;
        }
    }

    private static void CheckApiRateLimit()
    {
        var now = DateTime.Now;
        if (now.Second == ApiCallsSecondTracker)
        {
            if (ApiCallsCounterPerSeconds > MaximumApiCallsPerSecond)
                throw new TooManyRequestsException();
            ApiCallsCounterPerSeconds++;
        }
        else
        {
            ApiCallsSecondTracker = DateTime.Now.Second;
            ApiCallsCounterPerSeconds = 1;
        }

        if (now.Minute == ApiCallsMinuteTracker)
        {
            if (ApiCallsCounterPerMinute > MaximumApiCallsPerMinute)
                throw new TooManyRequestsException();
            ApiCallsCounterPerMinute++;
        }
        else
        {
            ApiCallsMinuteTracker = DateTime.Now.Minute;
            ApiCallsCounterPerMinute = 1;
        }
    }
}

internal class TooManyRequestsException : Exception
{
}
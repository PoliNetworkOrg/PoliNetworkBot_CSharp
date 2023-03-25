using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HtmlAgilityPack;
using PoliNetworkBot_CSharp.Code.Bots.RoomsBot.Data;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Bots.RoomsBot;

public class Fetcher
{

    private static readonly int MaximumApiCallsPerSecond = 5;
    private static readonly int MaximumApiCallsPerMinute = 30;
    private static readonly object Lock = new();
    
    private static int ApiCallsCounterPerSeconds = 0;
    private static int ApiCallsSecondTracker = 0;
    
    private static int ApiCallsCounterPerMinute = 0;
    private static int ApiCallsMinuteTracker = 0;

    private static readonly TimeSpan CacheInvalidationTime = TimeSpan.FromHours(1);
    private static readonly Dictionary<string, Dictionary<DateTime, DateTime>> FetchCacheAge = new();
    private static readonly Dictionary<string, Dictionary<DateTime, HtmlDocument>> RawFetchedFile = new();

    public static string? GetRawOccupancies(string campus, DateTime dateTime)
    {
        
        var doc = FetchOccupationData(campus, dateTime);
        var parsedDoc = doc.DocumentNode.SelectNodes("//table[contains(@class, 'BoxInfoCard')]");
        var text =  parsedDoc.Nodes().Aggregate(@"<head>	<link rel=""stylesheet\""
        href=""https://webcommons.polimi.it/webcommons/assets/ateneo2014.css.jsp?v=5&lang=it&dt_version=1.10""
        type=""text/css"" />
            <link rel=""stylesheet""
        href=""https://webcommons.polimi.it/webcommons/ajax/libs/jqueryui/1.12.1/themes/polij.css.jsp?v=5&lang=it""
        type=""text/css"" />
            <link rel=""stylesheet"" href=""https://webcommons.polimi.it/webcommons/assets/desktop.css.jsp?v=5&lang=it""
        type=""text/css"" /></head><body>", (current, node) => current + node.OuterHtml);
        text += "</body>";
        return text;
    }

    public static List<string> GetFreeClassrooms(string campus, DateTime dateTime, int startingTime, int endingTime)
    {
        var doc = FetchOccupationData(campus, dateTime);
        var classroomRows = doc.DocumentNode.SelectNodes("//tr[contains(@class, 'normalRow')]");
        var toReturn = new List<string>();
        foreach (var row in classroomRows)
        {
            var freeClassroom = true;
            for (var i = 2 + startingTime - 8; i < row.OuterLength && i < 2 + endingTime - 8; i++)
            {
                if (!row.ChildNodes[i].HasClass("slot")) continue;
                freeClassroom = false;
                break;
            }
            if (!freeClassroom) continue;
            var roomName = row.ChildNodes[1].InnerText;
            toReturn.Add(roomName);
        }

        return toReturn;
    }

    public static List<string> GetAllClassrooms(string campus, DateTime dateTime)
    {
        return GetFreeClassrooms(campus, dateTime,8, 8);
    }

    public static string? GetSingleClassroom(string campus, string roomName, DateTime dateTime)
    {
        var doc = FetchOccupationData(campus, dateTime);
        foreach (var classNode in doc.DocumentNode.SelectNodes("//tr[contains(@class, 'normalRow')]"))
        {
            if (classNode.ChildNodes[1].InnerText == roomName)
            {
                return classNode.ToString();
            }
        }

        return null;
    }

    private static HtmlDocument FetchOccupationData(string campus, DateTime dateTime)
    {
        lock (Lock)
        {
            if (FetchCacheAge.ContainsKey(campus) 
                && FetchCacheAge[campus].ContainsKey(dateTime) 
                && (DateTime.Now - FetchCacheAge[campus][dateTime] < CacheInvalidationTime))
                return RawFetchedFile[campus][dateTime];
            if(dateTime != DateTime.Today && dateTime != DateTime.Today + TimeSpan.FromDays(1))
                CheckApiRateLimit();
            var doc = new HtmlDocument(); 
            var web = new HtmlWeb();
            Console.WriteLine("### Requested passed to API endpoint ###");
            doc = web.Load(Const.PolimiController + 
                           $"?csic={campus}&tipologia={Data.Enums.RoomType.tutte}&categoria=tutte" +
                           $"&giorno_day={dateTime.Day}" +
                           $"&giorno_month={dateTime.Month}" +
                           $"&giorno_year={dateTime.Year}" +
                           $"&jaf_giorno_date_format=dd%2FMM%2Fyyyy&&evn_visualizza=" );
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
using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using PoliNetworkBot_CSharp.Code.Bots.RoomsBot.Data;
using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Bots.RoomsBot;

public class Fetcher
{
 
    private static readonly TimeSpan CacheInvalidationTime = TimeSpan.FromHours(1);
    private static readonly Dictionary<string, DateTime> FetchCacheAge = new();
    private static readonly Dictionary<string, HtmlDocument> RawFetchedFile = new();

    public static string? GetRawOccupancy(string campus)
    {
        
        var parsedCampus = Data.Enums.GetCampusByName(campus);
        var doc = FetchOccupationData(parsedCampus);
        var parsedDoc = doc.DocumentNode.SelectNodes("//table[contains(@class, 'BoxInfoCard')]");
        return parsedDoc?.ToString();
    }

    private static List<string> GetFreeClassrooms(string campus, int startingTime, int endingTime)
    {
        var parsedCampus = Data.Enums.GetCampusByName(campus);
        var doc = FetchOccupationData(parsedCampus);
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

    public List<string> GetAllClassrooms(string campus)
    {
        return GetFreeClassrooms(campus, 8, 8);
    }

    public string? GetSingleClassroom(string campus, string roomName)
    {
        var parsedCampus = Data.Enums.GetCampusByName(campus);
        var doc = FetchOccupationData(parsedCampus);
        return doc.DocumentNode.SelectSingleNode($"//div[.='{roomName}']")?.ToString();
    }

    private static HtmlDocument FetchOccupationData(string campus)
    {
        if (DateTime.Now - FetchCacheAge[campus] < CacheInvalidationTime)
            return RawFetchedFile[campus];
        var doc = new HtmlDocument();
        doc.LoadHtml(Const.PolimiController + $"/?csic={campus}?tipologia={Data.Enums.RoomType.tutte}?categoria=tutte" );
        RawFetchedFile[campus] = doc;
        FetchCacheAge[campus] = DateTime.Now;
        return doc;
    }
}
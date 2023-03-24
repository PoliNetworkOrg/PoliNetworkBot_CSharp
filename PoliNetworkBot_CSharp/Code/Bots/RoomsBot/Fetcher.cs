using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using PoliNetworkBot_CSharp.Code.Bots.RoomsBot.Data;
using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Bots.RoomsBot;

public class Fetcher
{
 
    private static TimeSpan _cacheInvalidationTime = TimeSpan.FromHours(1);
    private static List<string> _freeClassroomsCache = new();
    private static Dictionary<string, DateTime> _fetchCacheAge = new();
    private static Dictionary<string, HtmlDocument> _rawFetchedFile = new();
    private static Dictionary<string, Dictionary<string, bool>> _occupancyPerCampus = new();
    private static Dictionary<string, DateTime> _occupancyCacheAge = new();

    public static string? GetRawOccupancy(string campus)
    {
        
        var parsedCampus = Data.Enums.GetCampusByName(campus);
        var doc = FetchData(parsedCampus);
        var parsedDoc = doc.DocumentNode.SelectNodes("//table[contains(@class, 'BoxInfoCard')]");
        return parsedDoc?.ToString();
    }

    private static HtmlDocument FetchData(string campus)
    {
        if (DateTime.Now - _fetchCacheAge[campus] < _cacheInvalidationTime)
            return _rawFetchedFile[campus];
        var doc = new HtmlDocument();
        doc.LoadHtml(Const.PolimiController + $"/?csic={campus}?tipologia={Data.Enums.RoomType.tutte}?categoria=tutte" );
        _rawFetchedFile[campus] = doc;
        _fetchCacheAge[campus] = DateTime.Now;
        return doc;
    }
}
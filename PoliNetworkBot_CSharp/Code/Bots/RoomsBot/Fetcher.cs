using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using PoliNetworkBot_CSharp.Code.Bots.RoomsBot.Data;
using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Bots.RoomsBot;

public class Fetcher
{
    
    private static List<string> _freeClassroomsCache = new();
    private static Dictionary<string, DateTime> _fetchCacheAge = new();
    private static Dictionary<string, string> _rawFetchedFile = new();
    private static Dictionary<string, Dictionary<string, bool>> _occupancyPerCampus = new();
    private static Dictionary<string, DateTime> _occupancyCacheAge = new();

    public static string GetRawOccupancy(string campus, string roomType)
    {
        
        var parsedCampus = Data.Enums.GetCampusByName(campus);
        var parsedRoomType = Data.Enums.GetRoomType(roomType);
        var doc = new HtmlDocument();
        doc.LoadHtml(Const.PolimiController + $"/?csic={parsedCampus}?tipologia={parsedRoomType}?categoria=tutte" );
        return "";
    }

}
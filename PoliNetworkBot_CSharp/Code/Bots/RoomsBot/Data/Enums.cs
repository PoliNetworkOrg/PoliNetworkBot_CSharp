using System;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;

namespace PoliNetworkBot_CSharp.Code.Bots.RoomsBot.Data;

public class Enums
{
    private static readonly Dictionary<string, string> Campuses = new()
    {
        ["COE"] = "Como",
        ["CRG"] = "Cremona",
        ["LCF"] = "Lecco",
        ["MNI"] = "Mantova",
        ["MIB"] = "Milano Bovisa",
        ["MIA"] = "Milano Leonardo"
    };

    public enum RoomType
    {
        A, D, N, F, S, tutte
    }

    public static string GetCampusByName(string campus)
    {
        if (Campuses.TryGetValue(campus, out var toReturn))
            return toReturn;
        throw new ArgumentException("Selected campus does not exist");
    }

    //Disabled for now as Polimi basically only supports All queries
    public static RoomType GetRoomType(string room)
    {
        return RoomType.tutte;
    }
}
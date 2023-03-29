using System;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Bots.RoomsBot.Data;

public class Enums
{
    public enum ConversationState
    {
        START,
        MAIN,
        SELECT_CAMPUS,
        SELECT_CLASSROOM,
        SELECT_DATE,
        SELECT_START_HOUR,
        SELECT_END_HOUR
    }

    public enum Function
    {
        NULL_FUNCTION,
        OCCUPANCIES,
        FREE_CLASSROOMS,
        ROOM_OCCUPANCY,
        SETTINGS,
        FREE_CLASSROOMS_NOW
    }

    public static readonly Dictionary<string, string> Campuses = new()
    {
        ["Milano Leonardo"] = "MIA",
        ["Milano Bovisa"] = "MIB",
        ["Como"] = "COE",
        ["Cremona"] = "CRG",
        ["Lecco"] = "LCF",
        ["Mantova"] = "MNI", 
        
    };
    
    public static readonly Dictionary<string, Function> MainMenuOptionsToFunction = new()
    {
        ["📅 Occupazione Giornaliera"] = Function.OCCUPANCIES,
        ["🏫 Occupazione Aula"] = Function.ROOM_OCCUPANCY,
        ["🆓 Aule libere"] = Function.FREE_CLASSROOMS,
        ["🕒 Ora"] = Function.FREE_CLASSROOMS_NOW,
        ["⚙ Imposta preferenze"] = Function.SETTINGS,
    };

    public static readonly Dictionary<string, Function> MainMenuOptionsToStateEn = new()
    {
        ["📅 Daily Occupancies"] = Function.OCCUPANCIES,
        ["🏫 Classroom Occupancy"] = Function.ROOM_OCCUPANCY,
        ["🆓 Free Classrooms"] = Function.FREE_CLASSROOMS,
        ["🕒 Now"] = Function.FREE_CLASSROOMS_NOW,
        ["⚙ Set defaults"] = Function.SETTINGS,
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
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
        SELECT_END_HOUR,
        END
    }

    public enum Function
    {
        OCCUPANCIES,
        FREE_CLASSROOMS,
        FIND_CLASSROOM,
        SETTINGS
    }

    public static readonly Dictionary<string, string> Campuses = new()
    {
        ["Como"] = "COE",
        ["Cremona"] = "CRG",
        ["Lecco"] = "LCF",
        ["Mantova"] = "MNI", 
        ["Milano Bovisa"] = "MIB",
        ["Milano Leonardo"] = "MIA"
    };
    
    public static readonly Dictionary<string, Function> MainMenuOptionsToFunction = new()
    {
        ["Occupazione aule"] = Function.OCCUPANCIES,
        ["Aule libere"] = Function.FREE_CLASSROOMS,
        ["Trova aula"] = Function.FIND_CLASSROOM,
        ["imposta preferenze"] = Function.SETTINGS
    };

    public static readonly Dictionary<string, Function> MainMenuOptionsToStateEn = new()
    {
        ["Classroom Occupancies"] = Function.OCCUPANCIES,
        ["Free Classrooms"] = Function.FREE_CLASSROOMS,
        ["Find Classroom"] = Function.FIND_CLASSROOM,
        ["set defaults"] = Function.SETTINGS
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
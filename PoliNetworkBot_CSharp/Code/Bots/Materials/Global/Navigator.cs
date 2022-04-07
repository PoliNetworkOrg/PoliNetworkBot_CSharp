#region

using PoliNetworkBot_CSharp.Code.Bots.Materials.Enums;
using PoliNetworkBot_CSharp.Code.Bots.Materials.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Global;

public static class Navigator
{
    public static readonly Dictionary<string, string[]> ScuoleCorso = new()
    {
        ["3I"] = new[]
        {
            "MatNano",
            "Info",
            "MobilityMD",
            "AES",
            "Electronics",
            "Automazione",
            "Chimica",
            "Elettrica"
        },
        ["AUIC"] = Array.Empty<string>(),
        ["ICAT"] = Array.Empty<string>(),
        ["Design"] = Array.Empty<string>()
    };

    public static bool CourseHandler(Conversation conversation, string messageText)
    {
        foreach (var course in from scuola in ScuoleCorso.Values
                               where scuola != null
                               from course in scuola
                               where messageText == course
                               select course)
        {
            conversation.SetCourse(course);
            conversation.SetState(UserState.FOLDER);
            return true;
        }

        return false;
    }

    public static bool SchoolHandler(Conversation conversation, string messageText)
    {
        foreach (var school in ScuoleCorso.Keys.Where(school => messageText == school))
        {
            conversation.SetSchool(school);
            conversation.SetState(UserState.COURSE);
            return true;
        }

        return false;
    }
}
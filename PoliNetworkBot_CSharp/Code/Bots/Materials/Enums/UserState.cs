#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Enums;

[Serializable]
public enum UserState
{
    START = 1,
    SCHOOL = 2,
    COURSE = 3,
    FOLDER = 4,
    NEW_FOLDER = 5,
    WAITING_FILE = 6
}
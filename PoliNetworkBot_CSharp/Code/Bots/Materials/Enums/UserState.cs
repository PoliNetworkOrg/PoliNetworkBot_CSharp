#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Enums;

[Serializable]
public enum UserState
{
    START,
    SCHOOL,
    COURSE,
    FOLDER,
    NEW_FOLDER,
    WAITING_FILE
}
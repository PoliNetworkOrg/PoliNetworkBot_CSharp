#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Enums;

[Serializable]
public enum BotProgramTypeEnum
{
    MODERATION = 1,
    PRIMO = 2,
    ANON = 3,
    MATERIALS = 4,
    ADMIN = 5
}

//see PoliNetworkBot_CSharp.Code.Data.Constants.BotStartMethods
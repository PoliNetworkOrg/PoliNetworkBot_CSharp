#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Enums;

[Serializable]
public enum BotProgramType
{
    MODERATION = 1,
    PRIMO = 2,
    ANON = 3,
    ADMIN = 4,
    MATERIALS = 5
}
#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    public class BotInfo : BotInfoAbstract
    {
        internal new bool SetIsBot(Enums.BotTypeApi v)
        {
            return false;
        }

        internal new Enums.BotTypeApi? IsBot()
        {
            return Enums.BotTypeApi.REAL_BOT;
        }
    }
}
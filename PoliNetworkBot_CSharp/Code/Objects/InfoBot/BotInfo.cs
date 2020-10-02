#region

using PoliNetworkBot_CSharp.Code.Enums;
using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.InfoBot
{
    [Serializable]
    public class BotInfo : BotInfoAbstract
    {
        internal new bool SetIsBot(BotTypeApi v)
        {
            return false;
        }

        internal new BotTypeApi? IsBot()
        {
            return BotTypeApi.REAL_BOT;
        }
    }
}
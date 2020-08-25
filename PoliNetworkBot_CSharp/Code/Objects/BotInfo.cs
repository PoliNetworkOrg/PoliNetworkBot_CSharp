#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    public class BotInfo : BotInfoAbstract
    {
        internal new bool SetIsBot(bool v)
        {
            return false;
        }

        internal new bool IsBot()
        {
            return true;
        }
    }
}
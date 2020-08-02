using System;
using System.Collections.Generic;
using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Objects
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
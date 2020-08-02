using PoliNetworkBot_CSharp.Objects;
using System;

namespace PoliNetworkBot_CSharp
{
    public class UserBotInfo : BotInfoAbstract
    {

        internal new bool SetIsBot(bool v)
        {
            return false;
        }

        internal new bool IsBot()
        {
            return false;
        }

        internal int GetApiId()
        {
            throw new NotImplementedException();
        }

        internal string GetApiHash()
        {
            throw new NotImplementedException();
        }

        internal string GetSessionUserId()
        {
            throw new NotImplementedException();
        }

        internal long GetUserId()
        {
            throw new NotImplementedException();
        }
    }
}
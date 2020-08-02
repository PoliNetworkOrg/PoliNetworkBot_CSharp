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

        internal int? GetApiId()
        {
            try
            {
                return Convert.ToInt32(this.keyValuePairs[api_id]);
            }
            catch
            {
                return null;
            }
        }

        internal string? GetApiHash()
        {
            try
            {
                return this.keyValuePairs[api_hash].ToString();
            }
            catch
            {
                return null;
            }
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
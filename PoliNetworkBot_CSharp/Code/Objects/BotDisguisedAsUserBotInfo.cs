using System;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    public class BotDisguisedAsUserBotInfo : BotInfoAbstract
    {
        internal new bool SetIsBot(Enums.BotTypeApi v)
        {
            return false;
        }

        internal new Enums.BotTypeApi? IsBot()
        {
            return Enums.BotTypeApi.DISGUISED_BOT;
        }

        internal long? GetUserId()
        {
            try
            {
                return Convert.ToInt32(KeyValuePairs[ConstConfigBot.UserId]);
            }
            catch
            {
                return null;
            }
        }

        internal int? GetApiId()
        {
            try
            {
                return Convert.ToInt32(KeyValuePairs[ConstConfigBot.ApiId]);
            }
            catch
            {
                return null;
            }
        }

        internal string GetApiHash()
        {
            try
            {
                return KeyValuePairs[ConstConfigBot.ApiHash].ToString();
            }
            catch
            {
                return null;
            }
        }

        private string GenerateSessionUserId_From_UserId()
        {
            var id = GetUserId();
            return id == null ? null : Convert.ToString(id.Value, 16).ToLower();
        }

        internal bool SetApiId(string v)
        {
            try
            {
                KeyValuePairs[ConstConfigBot.ApiId] = Convert.ToInt32(v);
                return true;
            }
            catch
            {
                ;
            }

            return false;
        }


        internal string GetSessionUserId()
        {
            if (string.IsNullOrEmpty(SessionUserId)) SessionUserId = GenerateSessionUserId_From_UserId();

            return SessionUserId;
        }

        public string? SessionUserId { get; set; }


        internal void SetApiHash(string v)
        {
            KeyValuePairs[ConstConfigBot.ApiHash] = v;
        }

        
        internal bool SetUserId(string v)
        {
            try
            {
                KeyValuePairs[ConstConfigBot.UserId] = Convert.ToInt32(v);
                return true;
            }
            catch
            {
                ;
            }

            return false;
        }
    }
}
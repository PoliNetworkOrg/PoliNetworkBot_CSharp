#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    public class UserBotInfo : BotInfoAbstract
    {
        public string SessionUserId;


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
                return Convert.ToInt32(KeyValuePairs[ConstConfigBot.ApiId]);
            }
            catch
            {
                return null;
            }
        }

        internal string GetPhoneNumber()
        {
            try
            {
                return "+" + KeyValuePairs[ConstConfigBot.NumberCountry] + " " + KeyValuePairs[ConstConfigBot.NumberNumber];
            }
            catch
            {
                return null;
            }
        }

        internal string GetPasswordToAuthenticate()
        {
            return KeyValuePairs[ConstConfigBot.PasswordToAuthenticate].ToString();
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

        internal void SetNumberNumber(string v)
        {
            KeyValuePairs[ConstConfigBot.NumberNumber] = v;
        }

        internal void SetPasswordToAuthenticate(string v)
        {
            KeyValuePairs[ConstConfigBot.PasswordToAuthenticate] = v;
        }

        internal void SetNumberCountry(string v)
        {
            KeyValuePairs[ConstConfigBot.NumberCountry] = v;
        }

        internal string GetSessionUserId()
        {
            if (string.IsNullOrEmpty(SessionUserId)) SessionUserId = GenerateSessionUserId_From_UserId();

            return SessionUserId;
        }

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

        private string GenerateSessionUserId_From_UserId()
        {
            var id = GetUserId();
            return id == null ? null : Convert.ToString(id.Value, 16).ToLower();
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
    }
}
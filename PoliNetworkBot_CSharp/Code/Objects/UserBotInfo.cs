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
                return Convert.ToInt32(KeyValuePairs[ApiId]);
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
                return "+" + KeyValuePairs[NumberCountry] + " " + KeyValuePairs[NumberNumber];
            }
            catch
            {
                return null;
            }
        }

        internal string GetPasswordToAuthenticate()
        {
            return KeyValuePairs[PasswordToAuthenticate].ToString();
        }

        internal string GetApiHash()
        {
            try
            {
                return KeyValuePairs[ApiHash].ToString();
            }
            catch
            {
                return null;
            }
        }

        internal void SetNumberNumber(string v)
        {
            KeyValuePairs[NumberNumber] = v;
        }

        internal void SetPasswordToAuthenticate(string v)
        {
            KeyValuePairs[PasswordToAuthenticate] = v;
        }

        internal void SetNumberCountry(string v)
        {
            KeyValuePairs[NumberCountry] = v;
        }

        internal string GetSessionUserId()
        {
            if (string.IsNullOrEmpty(SessionUserId)) SessionUserId = GenerateSessionUserId_From_UserId();

            return SessionUserId;
        }

        internal void SetApiHash(string v)
        {
            KeyValuePairs[ApiHash] = v;
        }

        internal bool SetUserId(string v)
        {
            try
            {
                KeyValuePairs[UserId] = Convert.ToInt32(v);
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
                KeyValuePairs[ApiId] = Convert.ToInt32(v);
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
                return Convert.ToInt32(KeyValuePairs[UserId]);
            }
            catch
            {
                return null;
            }
        }
    }
}
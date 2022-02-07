#region

using PoliNetworkBot_CSharp.Code.Enums;
using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.InfoBot
{
    [Serializable]
    public class UserBotInfo : BotInfoAbstract
    {
        public string SessionUserId;

        internal new static BotTypeApi? IsBot()
        {
            return BotTypeApi.USER_BOT;
        }

        internal long? GetApiId()
        {
            try
            {
                return Convert.ToInt64(KeyValuePairs[ConstConfigBot.ApiId]);
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
                return "+" + KeyValuePairs[ConstConfigBot.NumberCountry] + " " +
                       KeyValuePairs[ConstConfigBot.NumberNumber];
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
                KeyValuePairs[ConstConfigBot.UserId] = Convert.ToInt64(v);
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
                KeyValuePairs[ConstConfigBot.ApiId] = Convert.ToInt64(v);
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
                return Convert.ToInt64(KeyValuePairs[ConstConfigBot.UserId]);
            }
            catch
            {
                return null;
            }
        }

        internal bool SetMethod(string v)
        {
            try
            {
                KeyValuePairs[ConstConfigBot.Method] = Convert.ToString(v);
                return true;
            }
            catch
            {
                ;
            }

            return false;
        }

        internal char? GetMethod()
        {
            try
            {
                var s = Convert.ToString(KeyValuePairs[ConstConfigBot.Method]);
                if (string.IsNullOrEmpty(s))
                    return null;

                return s[0];
            }
            catch
            {
                return null;
            }
        }
    }
}
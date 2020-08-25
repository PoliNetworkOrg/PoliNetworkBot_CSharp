#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    public class UserBotInfo : BotInfoAbstract
    {
        public string session_user_id;

#pragma warning disable IDE0060 // Rimuovere il parametro inutilizzato

        internal new bool SetIsBot(bool v)
#pragma warning restore IDE0060 // Rimuovere il parametro inutilizzato
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
                return Convert.ToInt32(keyValuePairs[api_id]);
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
                return "+" + keyValuePairs[number_contry] + " " + keyValuePairs[number_number];
            }
            catch
            {
                return null;
            }
        }

        internal string GetPasswordToAuthenticate()
        {
            return keyValuePairs[password_to_authenticate].ToString();
        }

        internal string GetApiHash()
        {
            try
            {
                return keyValuePairs[api_hash].ToString();
            }
            catch
            {
                return null;
            }
        }

        internal void SetNumberNumber(string v)
        {
            keyValuePairs[number_number] = v;
        }

        internal void SetPasswordToAuthenticate(string v)
        {
            keyValuePairs[password_to_authenticate] = v;
        }

        internal void SetNumberCountry(string v)
        {
            keyValuePairs[number_contry] = v;
        }

        internal string GetSessionUserId()
        {
            if (string.IsNullOrEmpty(session_user_id)) session_user_id = GenerateSessionUserId_From_UserId();

            return session_user_id;
        }

        internal void SetApiHash(string v)
        {
            keyValuePairs[api_hash] = v;
        }

        internal bool SetUserId(string v)
        {
            try
            {
                keyValuePairs[user_id] = Convert.ToInt32(v);
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
                keyValuePairs[api_id] = Convert.ToInt32(v);
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
            if (id == null)
                return null;

            return Convert.ToString(id.Value, 16).ToLower();
        }

        internal long? GetUserId()
        {
            try
            {
                return Convert.ToInt32(keyValuePairs[user_id]);
            }
            catch
            {
                return null;
            }
        }
    }
}
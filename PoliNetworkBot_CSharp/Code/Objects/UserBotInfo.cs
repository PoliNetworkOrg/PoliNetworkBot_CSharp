using System;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    public class UserBotInfo : BotInfoAbstract
    {
        public string session_user_id = null;

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
                return Convert.ToInt32(this.keyValuePairs[api_id]);
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
                return "+" + this.keyValuePairs[number_contry].ToString() + " " + this.keyValuePairs[number_number].ToString();
            }
            catch
            {
                return null;
            }
        }

        internal string GetPasswordToAuthenticate()
        {
            return this.keyValuePairs[password_to_authenticate].ToString();
        }

        internal string GetApiHash()
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

        internal void SetNumberNumber(string v)
        {
            this.keyValuePairs[number_number] = v;
        }

        internal void SetPasswordToAuthenticate(string v)
        {
            this.keyValuePairs[password_to_authenticate] = v;
        }

        internal void SetNumberCountry(string v)
        {
            this.keyValuePairs[number_contry] = v;
        }

        internal string GetSessionUserId()
        {
            if (string.IsNullOrEmpty(this.session_user_id))
            {
                this.session_user_id = GenerateSessionUserId_From_UserId();
            }

            return this.session_user_id;
        }

        internal void SetApiHash(string v)
        {
            this.keyValuePairs[api_hash] = v;
        }

        internal bool SetUserId(string v)
        {
            try
            {
                this.keyValuePairs[user_id] = Convert.ToInt32(v);
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
                this.keyValuePairs[api_id] = Convert.ToInt32(v);
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
            var id = this.GetUserId();
            if (id == null)
                return null;

            return Convert.ToString(id.Value, 16).ToString().ToLower();
        }

        internal long? GetUserId()
        {
            try
            {
                return Convert.ToInt32(this.keyValuePairs[user_id]);
            }
            catch
            {
                return null;
            }
        }
    }
}
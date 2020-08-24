using System;
using System.Collections.Generic;
using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    public class BotInfoAbstract
    {
        protected readonly Dictionary<string, object> keyValuePairs;

        public const string token = "t";
        public const string is_bot = "b";
        public const string accepts_messages = "a";
        public const string on_messages = "o";
        public const string website = "w";
        public const string contactString = "c";
        public const string api_id = "ai";
        public const string api_hash = "ah";
        public const string user_id = "u";
        public const string number_contry = "nc";
        public const string number_number = "nn";
        public const string password_to_authenticate = "pta";

        public BotInfoAbstract()
        {
            keyValuePairs = new Dictionary<string, object>();
        }

        internal bool IsBot()
        {
            return (bool)this.keyValuePairs[is_bot];
        }

        internal bool SetIsBot(bool v)
        {
            this.keyValuePairs[is_bot] = v;
            return true;
        }

        internal string GetToken()
        {
            return this.keyValuePairs[token].ToString();
        }

        internal void SetWebsite(string v)
        {
            this.keyValuePairs[website] = v;
        }

        internal void SetContactString(string v)
        {
            this.keyValuePairs[contactString] = v;
        }

        internal void SetOnMessages(string v)
        {
            this.keyValuePairs[on_messages] = v;
        }

        internal void SetAcceptMessages(bool v)
        {
            this.keyValuePairs[accepts_messages] = v;
        }

        internal void SetToken(string v)
        {
            this.keyValuePairs[token] = v;
        }

        internal EventHandler<MessageEventArgs> GetOnMessage()
        {
            string s = this.keyValuePairs[on_messages].ToString();
            return Data.Constants.BotStartMethods.GetMethodFromString(s);
        }

        internal bool AcceptsMessages()
        {
            return (bool)this.keyValuePairs[accepts_messages];
        }

        internal string GetWebsite()
        {
            try
            {
                return this.keyValuePairs[website].ToString();
            }
            catch
            {
                return null;
            }
        }

        internal string GetContactString()
        {
            try
            {
                return this.keyValuePairs[contactString].ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
#region

using System;
using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using Telegram.Bot.Args;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    public class BotInfoAbstract
    {
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
        protected readonly Dictionary<string, object> keyValuePairs;

        public BotInfoAbstract()
        {
            keyValuePairs = new Dictionary<string, object>();
        }

        internal bool IsBot()
        {
            return (bool) keyValuePairs[is_bot];
        }

        internal bool SetIsBot(bool v)
        {
            keyValuePairs[is_bot] = v;
            return true;
        }

        internal string GetToken()
        {
            return keyValuePairs[token].ToString();
        }

        internal void SetWebsite(string v)
        {
            keyValuePairs[website] = v;
        }

        internal void SetContactString(string v)
        {
            keyValuePairs[contactString] = v;
        }

        internal void SetOnMessages(string v)
        {
            keyValuePairs[on_messages] = v;
        }

        internal void SetAcceptMessages(bool v)
        {
            keyValuePairs[accepts_messages] = v;
        }

        internal void SetToken(string v)
        {
            keyValuePairs[token] = v;
        }

        internal EventHandler<MessageEventArgs> GetOnMessage()
        {
            var s = keyValuePairs[on_messages].ToString();
            return BotStartMethods.GetMethodFromString(s);
        }

        internal bool AcceptsMessages()
        {
            return (bool) keyValuePairs[accepts_messages];
        }

        internal string GetWebsite()
        {
            try
            {
                return keyValuePairs[website].ToString();
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
                return keyValuePairs[contactString].ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
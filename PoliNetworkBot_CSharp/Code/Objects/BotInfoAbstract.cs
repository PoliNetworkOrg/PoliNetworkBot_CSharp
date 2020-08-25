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
        public const string Token = "t";
        public const string is_bot = "b";
        public const string accepts_messages = "a";
        public const string OnMessages = "o";
        public const string Website = "w";
        public const string ContactString = "c";
        public const string ApiId = "ai";
        public const string ApiHash = "ah";
        public const string UserId = "u";
        public const string NumberCountry = "nc";
        public const string NumberNumber = "nn";
        public const string PasswordToAuthenticate = "pta";
        protected readonly Dictionary<string, object> KeyValuePairs;

        public BotInfoAbstract()
        {
            KeyValuePairs = new Dictionary<string, object>();
        }

        internal bool IsBot()
        {
            return (bool) KeyValuePairs[is_bot];
        }

        internal bool SetIsBot(bool v)
        {
            KeyValuePairs[is_bot] = v;
            return true;
        }

        internal string GetToken()
        {
            return KeyValuePairs[Token].ToString();
        }

        internal void SetWebsite(string v)
        {
            KeyValuePairs[Website] = v;
        }

        internal void SetContactString(string v)
        {
            KeyValuePairs[ContactString] = v;
        }

        internal void SetOnMessages(string v)
        {
            KeyValuePairs[OnMessages] = v;
        }

        internal void SetAcceptMessages(bool v)
        {
            KeyValuePairs[accepts_messages] = v;
        }

        internal void SetToken(string v)
        {
            KeyValuePairs[Token] = v;
        }

        internal EventHandler<MessageEventArgs> GetOnMessage()
        {
            var s = KeyValuePairs[OnMessages].ToString();
            return BotStartMethods.GetMethodFromString(s);
        }

        internal bool AcceptsMessages()
        {
            return (bool) KeyValuePairs[accepts_messages];
        }

        internal string GetWebsite()
        {
            try
            {
                return KeyValuePairs[Website].ToString();
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
                return KeyValuePairs[ContactString].ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Objects
{
    [Serializable]
    public class BotInfo
    {
        private readonly Dictionary<string, object> keyValuePairs;

        public BotInfo()
        {
            keyValuePairs = new Dictionary<string, object>();
        }

        public const string token = "t";
        public const string is_bot = "b";
        public const string accepts_messages = "a";
        public const string on_messages = "o";

        internal string GetToken()
        {
            return this.keyValuePairs[token].ToString();
        }

        internal bool IsBot()
        {
            return (bool)this.keyValuePairs[is_bot];
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

        internal void SetIsBot(bool v)
        {
            this.keyValuePairs[is_bot] = v;
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
    }
}
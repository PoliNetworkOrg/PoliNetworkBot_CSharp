#region

using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using System;
using Telegram.Bot.Args;

#endregion

namespace PoliNetworkBot_CSharp.Code.Data.Constants
{
    public static class BotStartMethods
    {
        public const string Moderation = "m";
        public const string Primo = "p";
        public const string Anon = "a";

        internal static EventHandler<MessageEventArgs> GetMethodFromString(string s)
        {
            return s switch
            {
                Moderation => Main.MainMethod,
                Primo => Code.Bots.Primo.Main.MainMethod,
                Anon => Code.Bots.Anon.MainAnon.MainMethod,
                _ => null
            };
        }
    }
}
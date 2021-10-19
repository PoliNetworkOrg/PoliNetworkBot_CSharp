#region

using System;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
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
                Primo => Bots.Primo.Main.MainMethod,
                Anon => MainAnon.MainMethod,
                _ => null
            };
        }
    }
}
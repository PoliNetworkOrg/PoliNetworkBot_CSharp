#region

using System;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using Telegram.Bot.Args;

#endregion

namespace PoliNetworkBot_CSharp.Code.Data.Constants
{
    public static class BotStartMethods
    {
        private const string Moderation = "m";

        internal static EventHandler<MessageEventArgs> GetMethodFromString(string s)
        {
            return s switch
            {
                Moderation => Main.MainMethod,
                _ => null
            };
        }
    }
}
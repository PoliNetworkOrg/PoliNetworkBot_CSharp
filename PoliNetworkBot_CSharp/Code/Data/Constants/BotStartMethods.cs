#region

using System;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using Telegram.Bot.Args;

#endregion

namespace PoliNetworkBot_CSharp.Code.Data.Constants
{
    public class BotStartMethods
    {
        public const string moderation = "m";

        internal static EventHandler<MessageEventArgs> GetMethodFromString(string s)
        {
            switch (s)
            {
                case moderation:
                {
                    return Main.MainMethod;
                }
            }

            return null;
        }
    }
}
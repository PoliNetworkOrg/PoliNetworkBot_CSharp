using System;
using Telegram.Bot.Args;

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
                        return Bots.Moderation.Main.MainMethod;
                    }
            }

            return null;
        }
    }
}
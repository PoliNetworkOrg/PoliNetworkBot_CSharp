#region

using System;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Bots.Materials;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;

#endregion

namespace PoliNetworkBot_CSharp.Code.Data.Constants;

public static class BotStartMethods
{
    public const string Moderation = "m";
    public const string Primo = "p";
    public const string Anon = "a";
    public const string Material = "mat";

    internal static EventHandler<MessageEventArgs> GetMethodFromString(string s)
    {
        return s switch
        {
            Moderation => Main.MainMethod,
            Primo => Bots.Primo.Main.MainMethod,
            Anon => MainAnon.MainMethod,
            Material => Program.BotClient_OnMessageAsync,
            _ => null
        };
    }
}
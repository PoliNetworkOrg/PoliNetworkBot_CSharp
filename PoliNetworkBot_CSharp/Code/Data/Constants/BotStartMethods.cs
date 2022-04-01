#region

using System;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Bots.Materials;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Data.Constants;

public static class BotStartMethods
{
    public static readonly Tuple<string, int> Moderation = new("m", 1);
    public static readonly Tuple<string, int> Primo = new("p", 2);
    public static readonly Tuple<string, int> Anon = new("a", 3);
    public static readonly Tuple<string, int> Material = new("mat", 4);
    public static readonly Tuple<string, int> Admin = new("ad", 5);


    internal static EventHandler<MessageEventArgs> GetMethodFromString(string s)
    {
        return s == Moderation.Item1 ? Main.MainMethod :
            s == Primo.Item1 ? Bots.Primo.Main.MainMethod :
            s == Anon.Item1 ? MainAnon.MainMethod :
            s == Material.Item1 ? Program.BotClient_OnMessageAsync : null;
    }
}

//see PoliNetworkBot_CSharp.Code.Enums.BotProgramTypeEnum
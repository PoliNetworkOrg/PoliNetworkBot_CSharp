#region

using System;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Bots.Materials;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;

#endregion

namespace PoliNetworkBot_CSharp.Code.Data.Constants;

public static class BotStartMethods
{
    public static readonly Tuple<string, int> Moderation = new("m", 1);
    public static readonly Tuple<string, int> Primo = new("p", 2);
    public static readonly Tuple<string, int> Anon = new("a", 3);
    public static readonly Tuple<string, int> Material = new("mat", 4);
    public static readonly Tuple<string, int> Admin = new("ad", 5);
    public static readonly Tuple<string, int> Rooms = new("au", 6);

    internal static ActionMessageEvent GetMethodFromString(string? s)
    {
        Action<object?, MessageEventArgs?>? x = null;
        if (s == Moderation.Item1)
            x = Main.MainMethod;
        else if (s == Primo.Item1)
            x = Bots.Primo.Main.MainMethod;
        else if (s == Anon.Item1)
            x = MainAnon.MainMethod;
        else if (s == Material.Item1)
            x = Program.BotClient_OnMessageAsync;
        else if (s == Rooms.Item1)
            x = Bots.RoomsBot.RoomsBot.MainMethod;

        return new ActionMessageEvent(x);
    }
}

//see PoliNetworkBot_CSharp.Code.Enums.BotProgramTypeEnum
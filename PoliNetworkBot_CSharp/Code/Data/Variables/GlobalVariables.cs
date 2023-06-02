#region

using System;
using System.Collections.Generic;
using System.Linq;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils;
using SampleNuGet.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Data.Variables;

public static class GlobalVariables
{
    public static Dictionary<long, TelegramBotAbstract?>? Bots;
    public static List<TelegramUser>? Creators;
    public static List<TelegramUser>? SubCreators;
    public static List<TelegramUser>? AllowedBanAll;
    public static List<TelegramUser>? AllowedMuteAll;
    public static List<long>? AllowedNoUsernameFromThisUserId;
    public static List<TelegramUser>? Owners;
    public static List<TelegramUser>? AllowedSpam;
    public static List<MessageToDelete>? MessagesToDelete;
    public static List<WordToBeFirst>? WordToBeFirsts;
    public static List<long>? ExcludedChatsForBot;
    public static List<long>? NoUsernameCheckInThisChats;
    public static List<string>? AllowedTags;
    public static Dictionary<long, DateTime>? UsernameWarningDictSent;

    private static bool _alreadyLoaded;

    public static DbConfigConnection? DbConfig { get; set; }

    internal static void LoadToRam()
    {
        if (_alreadyLoaded)
            return;

        _alreadyLoaded = true;

        LoadMessagesToDelete();

        Creators = new List<TelegramUser>
        {
            new("policreator"),
            new("policreator2"),
            new("policreator3"),
            new("policreator4"),
            new("policreator5"),
            new(5651789),
            new("poliadmin"),
            new("eliamaggioni")
        };

        SubCreators = new List<TelegramUser>
        {
            new("giovannieffe777"),
            new("testpolinetwork"),
            new("albus25"),
            new("deet98"),
            new("alberto_fattori"),
            new("scala98"),
            new("giulia_ye"),
            new("andre_crc"),
            new("fllippo"),
            new("marcol_8"),
            new("andre_crc"),
            new("lucreziaal"),
            new("giada_marti"),
            new("raif9"),
            new("diegoaldarese"),
            new(992285066) // Tommaso
        };

        AllowedBanAll = new List<TelegramUser>
        {
            new(5651789),
            new("raif9"),
            new("eliamaggioni"),
            new(992285066) // Tommaso
        };

        AllowedMuteAll = new List<TelegramUser>
        {
            new(5651789),
            new("raif9"),
            new("eliamaggioni"),
            new(992285066) // Tommaso
        };

        AllowedSpam = new List<TelegramUser>
        {
            new(5651789),
            new("raif9"),
            new("eliamaggioni"),
            new(992285066), // Tommaso
            new(349275135), //policreator
            new(1051414781), //polinetwork,
            new(1087968824) //@GroupAnonymousBot (è quello di telegram per retrocompatibilità
        };

        AllowedNoUsernameFromThisUserId = new List<long>
        {
            777000 //telegram
        };

        AllowedTags = new List<string>
        {
            "poligruppo", "polirules", "polibook", "poliextra", "askpolimi"
        };

        Owners = new List<TelegramUser>
        {
            new(5651789),
            new(107050697, "eliamaggioni"),
            new(992285066) // Tommaso
        };

        WordToBeFirsts = new List<WordToBeFirst>
        {
            new("primo", new List<string> { "prima" }),
            new("secondo", new List<string> { "seconda" }),
            new("terzo", new List<string> { "terza" }),
            new("kebabbaro", new List<string> { "kebabbara" }),
            //new WordToBeFirst("foco"),
            new("boomer"),
            //new WordToBeFirst("upkara"),
            //new WordToBeFirst("snitch"),
            new("pizzaiolo", new List<string> { "pizzaiola" })
            //new WordToBeFirst("lasagna")
        };

        ExcludedChatsForBot = new List<long>
        {
            -230287457,
            -1454214112
        };

        NoUsernameCheckInThisChats = new List<long>
        {
            -1443285113
        };
    }

    private static void LoadMessagesToDelete()
    {
        var m = FileSerialization.ReadFromBinaryFile<List<MessageToDelete>>(Paths.Bin.MessagesToDelete);
        if (m is null or null)
        {
            MessagesToDelete = new List<MessageToDelete>();
            return;
        }

        MessagesToDelete = m;
    }

    internal static bool IsOwner(long id)
    {
        return Owners != null && Owners.Any(x => x.Id == id);
    }

    public static bool IsAdmin(long id)
    {
        return (Owners != null && Owners.Any(x => x.Id == id))
               || (Creators != null && Creators.Any(x => x.Id == id))
               || (SubCreators != null && SubCreators.Any(x => x.Id == id))
               || (AllowedSpam != null && AllowedSpam.Any(x => x.Id == id));
    }
}
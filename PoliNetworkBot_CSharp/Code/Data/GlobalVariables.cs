#region

using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;

#endregion

namespace PoliNetworkBot_CSharp.Code.Data;

public static class GlobalVariables
{
    public static Dictionary<long, TelegramBotAbstract> Bots;
    public static List<TelegramUser> Creators;
    public static List<TelegramUser> SubCreators;
    public static List<TelegramUser> AllowedBanAll;
    public static List<TelegramUser> AllowedMuteAll;
    public static List<long> AllowedNoUsernameFromThisUserId;
    public static List<TelegramUser> Owners;
    public static List<TelegramUser> AllowedSpam;
    public static List<MessageToDelete> MessagesToDelete;
    public static List<WordToBeFirst> wordToBeFirsts;
    public static List<long> ExcludedChatsForBot;
    public static List<long> NoUsernameCheckInThisChats;
    public static List<string> AllowedTags;

    private static bool alreadyLoaded;

    public static DbConfig DbConfig { get; set; }
    public static MySqlConnection DbConnection { get; set; }

    internal static void LoadToRam()
    {
        if (alreadyLoaded)
            return;

        alreadyLoaded = true;

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
            new("carlogiova"),
            new("giovannieffe777"),
            new("testpolinetwork"),
            new("albus25"),
            new("deet98"),
            new("alberto_fattori"),
            new("scala98"),
            new("giulia_ye"),
            new("andre_crc"),
            new("perularrabeiti"),
            new("fllippo"),
            new("marcol_8"),
            new("andre_crc"),
            new("lucreziaal"),
            new("giada_marti"),
            new("raif9"),
            new("diegoaldarese")
        };

        AllowedBanAll = new List<TelegramUser>
        {
            new(5651789),
            new("raif9"),
            new("eliamaggioni")
        };

        AllowedMuteAll = new List<TelegramUser>
        {
            new(5651789),
            new("raif9"),
            new("eliamaggioni")
        };

        AllowedSpam = new List<TelegramUser>
        {
            new(5651789),
            new("raif9"),
            new("eliamaggioni"),
            new("tlpats")
        };

        AllowedNoUsernameFromThisUserId = new List<long>
        {
            777000 //telegram
        };

        AllowedTags = new List<string>
        {
            "poligruppo", "polirules", "polibook", "poliextra"
        };

        Owners = new List<TelegramUser>
        {
            new(5651789),
            new(107050697, "eliamaggioni")
        };

        wordToBeFirsts = new List<WordToBeFirst>
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
        return Owners.Any(x => x.id == id);
    }
}
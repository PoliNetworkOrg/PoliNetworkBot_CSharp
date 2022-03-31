#region

using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System.Collections.Generic;
using System.Linq;

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

    public static bool alreadyLoaded;

    internal static void LoadToRam()
    {
        if (alreadyLoaded)
            return;

        alreadyLoaded = true;

        LoadMessagesToDelete();

        Creators = new List<TelegramUser>
        {
            new TelegramUser( "policreator"),
             new TelegramUser( "policreator2"),
             new TelegramUser( "policreator3"),
             new TelegramUser( "policreator4"),
             new TelegramUser( "policreator5"),
                      new TelegramUser(5651789),
             new TelegramUser( "poliadmin"),
             new TelegramUser( "eliamaggioni")
        };

        SubCreators = new List<TelegramUser>
        {
                new TelegramUser("carlogiova"),
                new TelegramUser("giovannieffe777"),
                new TelegramUser("testpolinetwork"),
                    new TelegramUser(            "albus25"),
                new TelegramUser("deet98"),
                new TelegramUser("alberto_fattori"),
                new TelegramUser("scala98"),
                    new TelegramUser(            "giulia_ye"),
                new TelegramUser("andre_crc"),
                new TelegramUser("perularrabeiti"),
                new TelegramUser("fllippo"),
                new TelegramUser("marcol_8"),
                new TelegramUser("andre_crc"),
                new TelegramUser("lucreziaal"),
                    new TelegramUser(            "giada_marti"),
                new TelegramUser("raif9"),
                new TelegramUser("diegoaldarese")
        };

        AllowedBanAll = new List<TelegramUser>
        {
                 new TelegramUser(5651789),
                new TelegramUser("raif9"),
                new TelegramUser("eliamaggioni")
        };

        AllowedMuteAll = new List<TelegramUser>
        {
                  new TelegramUser(5651789),
                new TelegramUser("raif9"),
                new TelegramUser("eliamaggioni")
        };

        AllowedSpam = new List<TelegramUser>
        {
                new TelegramUser(5651789),
                new TelegramUser("raif9"),
                new TelegramUser("eliamaggioni"),
                new TelegramUser("tlpats")
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
                     new TelegramUser(5651789),
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
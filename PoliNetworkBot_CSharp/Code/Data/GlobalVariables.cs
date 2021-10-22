#region

using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;

#endregion

namespace PoliNetworkBot_CSharp.Code.Data
{
    public static class GlobalVariables
    {
        public static Dictionary<long, TelegramBotAbstract> Bots;
        public static List<string> Creators;
        public static List<string> SubCreators;
        public static List<string> AllowedBanAll;
        public static List<string> AllowedMuteAll;
        public static List<long> AllowedNoUsernameFromThisUserId;
        public static List<Tuple<long, string>> Owners;
        public static List<string> AllowedSpam;
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

            Creators = new List<string>
            {
                "policreator", "policreator2", "policreator3",
                "policreator4", "policreator5", "armef97", "poliadmin",
                "eliamaggioni"
            };

            SubCreators = new List<string>
            {
                "carlogiova", "giovannieffe777", "testpolinetwork",
                "albus25", "deet98", "alberto_fattori", "scala98",
                "giulia_ye", "andre_crc", "perularrabeiti", "fllippo", "marcol_8", "@andre_crc", "@lucreziaal"
            };

            AllowedBanAll = new List<string>
            {
                "armef97", "raif9", "eliamaggioni"
            };

            AllowedMuteAll = new List<string>
            {
                "armef97", "raif9", "eliamaggioni"
            };

            AllowedSpam = new List<string>
            {
                "armef97", "raif9", "eliamaggioni", "tlpats"
            };

            AllowedNoUsernameFromThisUserId = new List<long>
            {
                777000 //telegram
            };

            AllowedTags = new List<string>
            {
                "poligruppo", "polirules", "polibook", "poliextra"
            };

            Owners = new List<Tuple<long, string>>
            {
                new Tuple<long, string>(5651789, "armef97"),
                new Tuple<long, string>(107050697, "eliamaggioni")
            };

            wordToBeFirsts = new List<WordToBeFirst>
            {
                new WordToBeFirst("primo", new List<string> {"prima"}),
                new WordToBeFirst("secondo", new List<string> {"seconda"}),
                new WordToBeFirst("terzo", new List<string> {"terza"}),
                new WordToBeFirst("kebabbaro", new List<string> {"kebabbara"}),
                //new WordToBeFirst("foco"),
                new WordToBeFirst("boomer"),
                //new WordToBeFirst("upkara"),
                //new WordToBeFirst("snitch"),
                new WordToBeFirst("pizzaiolo", new List<string> {"pizzaiola"})
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
            if (m == default || m == null)
            {
                MessagesToDelete = new List<MessageToDelete>();
                return;
            }

            MessagesToDelete = m;
        }

        internal static bool IsOwner(long id)
        {
            foreach (var x in Owners)
                if (x.Item1 == id)
                    return true;

            return false;
        }
    }
}
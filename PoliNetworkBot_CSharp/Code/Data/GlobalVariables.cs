#region

using PoliNetworkBot_CSharp.Code.Objects;
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
        public static List<long> AllowedNoUsername;
        public static List<Tuple<long, string>> Owners;
        public static List<string> AllowedSpam;
        public static List<MessageToDelete> MessagesToDelete;
        public static List<Code.Objects.WordToBeFirst> wordToBeFirsts;
        public static List<long> ExcludedChatsForBot;

        internal static void LoadToRam()
        {
            LoadMessagesToDelete();

            Creators = new List<string>
            {
                "policreator", "policreator2", "policreator3",
                "policreator4", "policreator5", "armef97", "poliadmin"
            };

            SubCreators = new List<string>
            {
                "carlogiova", "giovannieffe777", "testpolinetwork",
               "albus25", "deet98", "alberto_fattori", "scala98",
               "giulia_ye", "andre_crc", "perularrabeiti", "fllippo"
            };

            AllowedBanAll = new List<string>
            {
                "armef97", "raif9", "eliamaggioni"
            };

            AllowedSpam = new List<string>()
            {
                "armef97", "raif9", "eliamaggioni"
            };

            AllowedNoUsername = new List<long>()
            {
                777000 //telegram
            };

            Owners = new List<Tuple<long, string>>()
            {
                new Tuple<long, string>(5651789, "armef97"),
                new Tuple<long, string>(107050697, "eliamaggioni")
            };

            wordToBeFirsts = new List<WordToBeFirst>()
            {
                new WordToBeFirst("primo", new List<string>() { "prima" }),
                new WordToBeFirst("secondo", new List<string>() {"seconda" }),
                new WordToBeFirst("terzo", new List<string>(){ "terza"}),
                new WordToBeFirst("kebabbaro", new List<string>(){ "kebabbara"}),
                new WordToBeFirst("foco"),
                new WordToBeFirst("boomer"),
                new WordToBeFirst("upkara"),
                new WordToBeFirst("snitch"),
                new WordToBeFirst("pizzaiolo", new List<string>(){"pizzaiola"}),
                new WordToBeFirst("lasagna")
            };

            ExcludedChatsForBot = new List<long>()
            {
                -230287457,
                -1454214112
            };
        }

        private static void LoadMessagesToDelete()
        {
            var m = Utils.FileSerialization.ReadFromBinaryFile<List<MessageToDelete>>(Data.Constants.Paths.Bin.MessagesToDelete);
            if (m == default || m == null)
            {
                MessagesToDelete = new List<MessageToDelete>();
                return;
            }

            MessagesToDelete = m;
        }
    }
}
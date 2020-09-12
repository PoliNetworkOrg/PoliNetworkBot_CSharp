#region

using System;
using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Data
{
    public static class GlobalVariables
    {
        public static Dictionary<long, TelegramBotAbstract> Bots;
        public static List<string> Creators;
        public static List<string> SubCreators;
        public static List<long> AllowedNoUsername;
        public static List<Tuple<long,string>> Owners;
        internal static void LoadToRam()
        {
            Creators = new List<string>
            {
                "policreator", "policreator2", "policreator3",
                "policreator4", "policreator5", "armef97", "poliadmin"
            };

            SubCreators = new List<string> 
            {
                "carlogiova", "giovannieffe777", "testpolinetwork",
                "albus25", "deet98", "alberto_fattori", "scala98",
                "giulia_ye"
            };

            AllowedNoUsername = new List<long>()
            {
                777000 //telegram
            };

            Owners = new List<Tuple<long, string>>()
            {
                new Tuple<long, string>(5651789, "armef97")
            };
        }
    }
}
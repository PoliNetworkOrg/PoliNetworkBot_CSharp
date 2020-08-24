using PoliNetworkBot_CSharp.Code.Objects;
using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Data
{
    public class GlobalVariables
    {
        public static Dictionary<long, TelegramBotAbstract> Bots;
        public static List<long> Creators;

        internal static void LoadToRam()
        {
            Creators = new List<long>() {
                5651789 // @ArmeF97
            };
        }
    }
}
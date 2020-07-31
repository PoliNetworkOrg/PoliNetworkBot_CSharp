using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Data
{
    public class GlobalVariables
    {
        public static List<TelegramBotAbstract> Bots;
        public static List<long> Creators;

        internal static void LoadToRam()
        {
            Creators = new List<long>() {
                5651789 // @ArmeF97
            };
        }
    }
}
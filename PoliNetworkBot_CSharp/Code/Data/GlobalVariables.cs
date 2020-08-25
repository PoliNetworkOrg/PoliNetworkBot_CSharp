#region

using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Data
{
    public class GlobalVariables
    {
        public static Dictionary<long, TelegramBotAbstract> Bots;
        public static List<long> Creators;

        internal static void LoadToRam()
        {
            Creators = new List<long>
            {
                5651789 // @ArmeF97
            };
        }
    }
}
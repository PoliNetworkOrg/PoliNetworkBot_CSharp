#region

using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Data;

#endregion

namespace PoliNetworkBot_CSharp.Test.Spam
{
    internal class SpamTest
    {
        public static void Main2()
        {
            ;
            GlobalVariables.LoadToRam();
            var d1 = Blacklist.IsSpam("https://t.me/PoliGruppo/1", null);
            var d2 = Blacklist.IsSpam("¯\\_(ツ)_/¯", null);

            ;
        }
    }
}
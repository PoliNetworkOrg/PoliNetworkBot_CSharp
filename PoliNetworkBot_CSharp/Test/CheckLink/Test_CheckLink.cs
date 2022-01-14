using JsonPolimi_Core_nf.Data;
using JsonPolimi_Core_nf.Tipi;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;

namespace PoliNetworkBot_CSharp.Test.CheckLink
{
    public static class Test_CheckLink
    {
        public static void Test_CheckLink2()
        {
            var r1 = Blacklist.IsSpam("https://t.me/Doctor", null);
            ;

            if (Variabili.L == null)
                Variabili.L = new ListaGruppo();

            Variabili.L.Add(new Gruppo() { Platform = "TG", IdLink = "+gVzS0kgvwuA5OTlk", TipoLink = TipoLink.PLUS }, true);
            Code.Bots.Moderation.CommandDispatcher.CheckSeILinkVanno2(5, true, 10);
            ;
        }
    }
}
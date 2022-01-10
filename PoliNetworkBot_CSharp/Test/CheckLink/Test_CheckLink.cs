using JsonPolimi_Core_nf.Tipi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoliNetworkBot_CSharp.Test.CheckLink
{
    public static class Test_CheckLink
    {
        public static void Test_CheckLink2()
        {
            ListaGruppo listaGruppo = new();
            listaGruppo.Add(new Gruppo() { Platform = "TG", IdLink= "+gVzS0kgvwuA5OTlk", TipoLink= TipoLink.PLUS }, true);
            listaGruppo.CheckSeILinkVanno(10, true);
            ;
        }
    }
}

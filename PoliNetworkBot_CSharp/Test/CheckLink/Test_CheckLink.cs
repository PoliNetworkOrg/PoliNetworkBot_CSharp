﻿using JsonPolimi_Core_nf.Data;
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
            if (Variabili.L == null)
                Variabili.L = new ListaGruppo();

            Variabili.L.Add(new Gruppo() { Platform = "TG", IdLink = "+gVzS0kgvwuA5OTlk", TipoLink = TipoLink.PLUS }, true);
            Code.Bots.Moderation.CommandDispatcher.CheckSeILinkVanno2(5, true);
            ;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PoliNetworkBot_CSharp.Code.Bots.Primo
{
    class Main
    {
        public void MainPrimo()
        {
            Thread t = new Thread(MainPrimo2);
            t.Start();
        }

        private async void MainPrimo2()
        {
            return;
        }
    }
}

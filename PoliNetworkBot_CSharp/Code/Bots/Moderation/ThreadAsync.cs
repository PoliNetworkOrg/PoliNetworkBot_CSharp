using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    public class ThreadAsync
    {
        public static void DoThingsAsyncBot(object obj)
        {
            var t = new Thread(CheckMessagesToSend);
            t.Start();

            var t2 = new Thread(CheckMessagesToDeleteAsync);
            t2.Start();
        }

        private static async void CheckMessagesToDeleteAsync()
        {
            while (true)
            {
                await MessageDb.CheckMessageToDelete();
                Thread.Sleep(20 * 1000); //20 sec
            }
        }


        private static async void CheckMessagesToSend()
        {
            while (true)
            {
                await MessageDb.CheckMessagesToSend();
                Thread.Sleep(20 * 1000); //20 sec
            }

            // ReSharper disable once FunctionNeverReturns
        }
    }
}

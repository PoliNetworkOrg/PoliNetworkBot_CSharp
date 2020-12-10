using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Test.Spam
{
    class SpamTest
    {
        public static void Main2()
        {
            ;
            GlobalVariables.LoadToRam();
            var d1 = Blacklist.IsSpam("https://t.me/PoliGruppo/1");
            var d2 = Blacklist.IsSpam("¯\\_(ツ)_/¯");
            ;
        }
    }
}

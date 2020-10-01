using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoliNetworkBot_CSharp.Code.Bots.Administration
{
    class Main
    {
        internal static async System.Threading.Tasks.Task MainMethodAsync(TelegramBotAbstract telegramBotAbstract)
        {
            ;

            await telegramBotAbstract.FixTheFactThatSomeGroupsDoesNotHaveOurModerationBotAsync();
        }
    }
}

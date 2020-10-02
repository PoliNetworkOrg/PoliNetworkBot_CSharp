using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System;

namespace PoliNetworkBot_CSharp.Code.Bots.Administration
{
    internal class Main
    {
        internal static async System.Threading.Tasks.Task MainMethodAsync(TelegramBotAbstract telegramBotAbstract)
        {
            ;

            try
            {
                await telegramBotAbstract.FixTheFactThatSomeGroupsDoesNotHaveOurModerationBotAsync();
            }
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwners(e, telegramBotAbstract);
            }
        }
    }
}
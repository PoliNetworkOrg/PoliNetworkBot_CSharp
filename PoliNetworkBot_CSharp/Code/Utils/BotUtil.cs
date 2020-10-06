using PoliNetworkBot_CSharp.Code.Objects;
using System;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class BotUtil
    {
        internal static TelegramBotAbstract GetFirstModerationRealBot(TelegramBotAbstract telegramBotAbstract = null)
        {
            if (telegramBotAbstract != null)
                return telegramBotAbstract;

            foreach (var x in Data.GlobalVariables.Bots.Keys)
            {
                var bot = Data.GlobalVariables.Bots[x];
                var botType = bot.GetBotType();
                switch (botType)
                {
                    case Enums.BotTypeApi.REAL_BOT:
                        {
                            string botMode = bot.GetMode();
                            if (botMode == Code.Data.Constants.BotStartMethods.Moderation)
                                return bot;

                            break;
                        }
                    case Enums.BotTypeApi.USER_BOT:
                        break;
                    case Enums.BotTypeApi.DISGUISED_BOT:
                        break;
                }
            }

            return null;
        }
    }
}
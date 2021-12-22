using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class BotUtil
    {
        internal static TelegramBotAbstract GetFirstModerationRealBot(TelegramBotAbstract telegramBotAbstract = null)
        {
            if (telegramBotAbstract != null)
                return telegramBotAbstract;

            foreach (var x in GlobalVariables.Bots.Keys)
            {
                var bot = GlobalVariables.Bots[x];
                var botType = bot.GetBotType();
                switch (botType)
                {
                    case BotTypeApi.REAL_BOT:
                        {
                            var botMode = bot.GetMode();
                            if (botMode == BotStartMethods.Moderation)
                                return bot;

                            break;
                        }
                    case BotTypeApi.USER_BOT:
                        break;

                    case BotTypeApi.DISGUISED_BOT:
                        break;
                }
            }

            return null;
        }

        public static List<TelegramBotAbstract> GetBotFromType(BotTypeApi botTypeApi, string botModeParam)
        {
            List<TelegramBotAbstract> r = new();
            foreach (var x in GlobalVariables.Bots.Keys)
            {
                var bot = GlobalVariables.Bots[x];
                var botType = bot.GetBotType();
                if (botType == botTypeApi)
                {
                    var botMode = bot.GetMode();
                    if (botMode == botModeParam)
                        r.Add(bot);
                }
            }

            return r;
        }
    }
}
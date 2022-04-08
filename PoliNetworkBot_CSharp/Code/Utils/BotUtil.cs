﻿#region

using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

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
                        if (botMode == BotStartMethods.Moderation.Item1)
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
        return (from x in GlobalVariables.Bots.Keys
                select GlobalVariables.Bots[x]
            into bot
                let botType = bot.GetBotType()
                where botType == botTypeApi
                let botMode = bot.GetMode()
                where botMode == botModeParam
                select bot).ToList();
    }
}
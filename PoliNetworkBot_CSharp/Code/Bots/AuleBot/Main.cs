#region

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.AuleBot;

internal static class AuleBot
{
    internal static void MainMethod(object? sender, MessageEventArgs? e)
    {
        var t = new Thread(() =>
        {
            if (sender != null) _ = MainMethod2(sender, e);
        });
        t.Start();
    }

    private static async Task MainMethod2(object sender, MessageEventArgs? e)
    {
        if (e?.Message == null)
            return;

        if (e.Message.Chat.Type != ChatType.Private) return;

        if (sender is not TelegramBotClient t2)
            return;

        var telegramBotAbstract = TelegramBotAbstract.GetFromRam(t2);

    }

}
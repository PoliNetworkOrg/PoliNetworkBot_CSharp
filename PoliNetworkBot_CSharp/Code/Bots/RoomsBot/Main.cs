#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.IsisMtt.X509;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.RoomsBot;

internal static class RoomsBot
{
    internal static void MainMethod(object? sender, MessageEventArgs? e)
    {
        var t = new Thread(() =>
        {
            if (sender != null && e != null) _ = MainMethod2(new TelegramBotParam(sender, false), e);
        });
        t.Start();
        //var t1 = new Thread(() => _ = CheckAllowedMessageExpiration(sender, e));
        //t1.Start();
    }

    public static async Task<ActionDoneObject> MainMethod2(TelegramBotParam sender, MessageEventArgs? e)
    {
        TelegramBotAbstract? telegramBotClient = null;

        telegramBotClient = sender.GetTelegramBot();

        if (telegramBotClient == null || e?.Message?.From == null || e.Message.Text == null)
            return new ActionDoneObject(ActionDoneEnum.NONE, null, null);


        return await MessageHandler.HandleMessage(telegramBotClient, e.Message);
    }
}
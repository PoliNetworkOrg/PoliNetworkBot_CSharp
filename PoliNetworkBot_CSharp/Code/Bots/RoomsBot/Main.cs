#region

using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using SampleNuGet.Objects;

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

    private static async Task<ActionDoneObject> MainMethod2(TelegramBotParam sender, MessageEventArgs? e)
    {
        TelegramBotAbstract? telegramBotClient = null;

        telegramBotClient = sender.GetTelegramBot();

        if (telegramBotClient == null || e?.Message.From == null || e.Message.Text == null)
            return new ActionDoneObject(ActionDoneEnum.NONE, null, null);


        return await MessageHandler.HandleMessage(telegramBotClient, e.Message);
    }
}
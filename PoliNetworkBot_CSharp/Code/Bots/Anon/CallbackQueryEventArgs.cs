#region

using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Anon;

public class CallbackQueryEventArgs
{
    internal readonly CallbackQuery? CallbackQuery;

    public CallbackQueryEventArgs(CallbackQuery? callbackQuery)
    {
        CallbackQuery = callbackQuery;
    }
}
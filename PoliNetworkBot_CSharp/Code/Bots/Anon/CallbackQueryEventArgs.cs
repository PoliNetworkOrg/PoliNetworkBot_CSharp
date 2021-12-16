using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    internal class CallbackQueryEventArgs
    {
        internal CallbackQuery CallbackQuery;

        public CallbackQueryEventArgs(CallbackQuery callbackQuery)
        {
            CallbackQuery = callbackQuery;
        }
    }
}
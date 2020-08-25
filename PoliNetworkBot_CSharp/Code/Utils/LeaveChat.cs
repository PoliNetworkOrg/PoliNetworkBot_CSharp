#region

using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Args;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class LeaveChat
    {
        internal static void ExitFromChat(TelegramBotAbstract sender, MessageEventArgs e)
        {
            sender.LeaveChatAsync(e.Message.Chat.Id);
        }
    }
}
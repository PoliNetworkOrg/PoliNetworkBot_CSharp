using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class LeaveChat
    {
        internal static void ExitFromChat(TelegramBotAbstract sender, MessageEventArgs e)
        {
            sender.LeaveChatAsync(e.Message.Chat.Id);
        }
    }
}
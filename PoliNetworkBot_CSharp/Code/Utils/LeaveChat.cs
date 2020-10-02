#region

using PoliNetworkBot_CSharp.Code.Objects;
using System.Threading.Tasks;
using Telegram.Bot.Args;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class LeaveChat
    {
        internal static async Task ExitFromChat(TelegramBotAbstract sender, MessageEventArgs e)
        {
            await sender.LeaveChatAsync(e.Message.Chat.Id);
        }
    }
}
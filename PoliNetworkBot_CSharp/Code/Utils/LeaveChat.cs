#region

using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Objects;
using System.Threading.Tasks;

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
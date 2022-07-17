#region

using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class LeaveChat
{
    internal static async Task<bool> ExitFromChat(TelegramBotAbstract sender, MessageEventArgs e)
    {
        return e.Message != null && await sender.LeaveChatAsync(e.Message.Chat.Id);
    }
}
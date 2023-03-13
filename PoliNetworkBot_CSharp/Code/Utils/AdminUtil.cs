using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using Telegram.Bot.Types;
using TeleSharp.TL;

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class AdminUtil
{
    public static async Task<CommandExecutionState> PromoteUser(MessageEventArgs? arg1, TelegramBotAbstract? arg2, string[]? arg3)
    {
        var fromId = arg1?.Message.From?.Id;
        var chatId = arg1?.Message.Chat.Id;
        if (fromId == null || chatId == null)
            return (CommandExecutionState.NOT_TRIGGERED);


        var promoteChatMember = arg2?.PromoteChatMember(new TelegramUser(fromId.Value), new ChatId(chatId.Value), null);
        if (promoteChatMember == null)
            return (CommandExecutionState.NOT_TRIGGERED);
        
        var b = await promoteChatMember;
        return b ? CommandExecutionState.SUCCESSFUL : CommandExecutionState.ERROR_DEFAULT;
    }
}
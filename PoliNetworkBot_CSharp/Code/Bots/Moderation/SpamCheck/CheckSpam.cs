using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.SpamCheck;

public static class CheckSpam
{
    public static async Task<bool?> CheckSpamMethod(MessageEventArgs e, TelegramBotAbstract? telegramBotClient)
    {

        var checkSpam = await ModerationCheck.CheckSpamAsync(e, telegramBotClient);
        if (checkSpam != SpamType.ALL_GOOD && checkSpam != SpamType.SPAM_PERMITTED)
            return await ModerationCheck.AntiSpamMeasure(telegramBotClient, e, checkSpam);

        if (checkSpam == SpamType.SPAM_PERMITTED)
            return await ModerationCheck.PermittedSpamMeasure(telegramBotClient, EventArgsContainer.Get(e));

        return null;
    }
}
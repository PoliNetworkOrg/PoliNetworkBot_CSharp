using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.SpamCheck;

public static class CheckSpam
{
    public static async Task<bool?> CheckSpamMethod(MessageEventArgs e, TelegramBotAbstract? telegramBotClient)
    {
        var checkSpam = await CheckSpamAsync(e, telegramBotClient);
        

        
        return checkSpam switch
        {
            SpamType.SPAM_LINK => await ModerationCheck.AntiSpamMeasure(telegramBotClient, e, checkSpam),
            SpamType.NOT_ALLOWED_WORDS => await ModerationCheck.AntiSpamMeasure(telegramBotClient, e, checkSpam),
            SpamType.FOREIGN => await ModerationCheck.AntiSpamMeasure(telegramBotClient, e, checkSpam),
            SpamType.FORMAT_INCORRECT => await ModerationCheck.AntiSpamMeasure(telegramBotClient, e, checkSpam),
            SpamType.SPAM_PERMITTED => await ModerationCheck.PermittedSpamMeasure(telegramBotClient,
                EventArgsContainer.Get(e)),
            SpamType.UNDEFINED => null,
            SpamType.ALL_GOOD => null,
            _ => null
        };
    }


    private static async Task<SpamType> CheckSpamAsync(MessageEventArgs? e, TelegramBotAbstract? telegramBotClient)
    {
        var checkIfHeIsAllowedResult = ModerationCheck.CheckIfHeIsAllowedSpam(e);
        if (checkIfHeIsAllowedResult)
            return SpamType.ALL_GOOD;

        var isSpamStored = ModerationCheck.CheckIfSpamStored(e, telegramBotClient);
        if (isSpamStored != null)
            return isSpamStored.Value;


        if (string.IsNullOrEmpty(e?.Message.Text))
        {
            var s1 = SpamTypeUtil.Merge(
                await Blacklist.Blacklist.IsSpam(e?.Message.Caption, e?.Message.Chat.Id, telegramBotClient, false, e),
                Blacklist.Blacklist.IsSpam(e?.Message.Photo));
            if (s1 != null)
                return s1.Value;
        }

        if (e?.Message.Text != null && e.Message.Text.StartsWith("/"))
            return SpamType.ALL_GOOD;

        var isForeign = ModerationCheck.DetectForeignLanguage(e);

        if (isForeign)
            return SpamType.FOREIGN;

        var spamType1 = await Blacklist.Blacklist.IsSpam(e?.Message.Text,
            e?.Message.Chat.Id, telegramBotClient, false, e);
        var spamType2 = Blacklist.Blacklist.IsSpam(e?.Message.Photo);
        var s2 = SpamTypeUtil.Merge(spamType1, spamType2);
        return s2 ?? SpamType.ALL_GOOD;
    }

}
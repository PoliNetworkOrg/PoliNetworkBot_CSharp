using System;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Notify;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.SpamCheck;

public static class CheckSpam
{
    public static async Task<Tuple<SpamType, bool?>?> CheckSpamMethod(MessageEventArgs e,
        TelegramBotAbstract? telegramBotClient)
    {
        var checkSpam = await CheckSpamAsync(e, telegramBotClient, true);


        bool? x = null;
        switch (checkSpam)
        {
            case SpamType.SPAM_LINK:
            case SpamType.NOT_ALLOWED_WORDS:
            case SpamType.FOREIGN:
            case SpamType.FORMAT_INCORRECT:
            {
                ;
                try
                {
                    x = await ModerationCheck.AntiSpamMeasure(telegramBotClient, e, checkSpam);
                }
                catch (Exception ex)
                {
                    await NotifyUtil.NotifyOwnersWithLog(ex, telegramBotClient, null,
                        new EventArgsContainer { MessageEventArgs = e });

                    x = true;
                }

                break;
            }
            case SpamType.SPAM_PERMITTED:
                x = await ModerationCheck.PermittedSpamMeasure(telegramBotClient, EventArgsContainer.Get(e));
                break;
            case SpamType.UNDEFINED:
            case SpamType.ALL_GOOD:
            default:
                x = null;
                break;
        }

        return new Tuple<SpamType, bool?>(checkSpam, x);
    }


    public static async Task<SpamType> CheckSpamAsync(MessageEventArgs? e, TelegramBotAbstract? telegramBotClient,
        bool checkSender)
    {
        if (checkSender)
        {
            var checkIfHeIsAllowedResult = ModerationCheck.CheckIfHeIsAllowedSpam(e);
            if (checkIfHeIsAllowedResult)
                return SpamType.ALL_GOOD;
        }

        var isSpamStored = ModerationCheck.CheckIfSpamStored(e, telegramBotClient);
        if (isSpamStored != null)
            return isSpamStored.Value;


        if (string.IsNullOrEmpty(e?.Message.Text))
        {
            var s1 = SpamTypeUtil.Merge(
                await Blacklist.Blacklist.IsSpam(e?.Message.Caption, e?.Message.Chat.Id, telegramBotClient, false, e),
                Blacklist.Blacklist.IsSpam(e?.Message.Photo));
            var s3 = SpamTypeUtil.Merge(s1, Blacklist.Blacklist.CheckCaptionElements(e?.Message, telegramBotClient));
            if (s3 != null)
                return s3.Value;
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
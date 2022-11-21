#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class TimeUtils
{
    public static async Task ExecuteAtLaterTime(TimeSpan time, Action task)
    {
        try
        {
            await Task.Delay(time);
            task.Invoke();
        }
        catch (Exception? ex)
        {
            Logger.Logger.WriteLine(ex, LogSeverityLevel.ERROR);
        }
    }

    public static async Task<bool> GetRunningTime(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        try
        {
            var lang = new Language(new Dictionary<string, string?>
            {
                { "", await CommandDispatcher.GetRunningTime() }
            });
            if (e != null)
                await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                    e.Message.From?.LanguageCode,
                    e.Message.From?.Username, lang, ParseMode.Html,
                    null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
            return false;
        }
        catch (Exception? ex)
        {
            _ = NotifyUtil.NotifyOwnerWithLog2(ex, sender, EventArgsContainer.Get(e));
        }

        return false;
    }

    public static async Task TestTime(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        var time = await CommandDispatcher.TestTime(sender, e);
        Console.WriteLine(time);
    }

    public static async Task<bool> GetTime(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        var lang = new Language(new Dictionary<string, string?>
        {
            { "", DateTimeClass.NowAsStringAmericanFormat() }
        });
        if (e != null)
            await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                e.Message.From?.LanguageCode,
                e.Message.From?.Username, lang, ParseMode.Html,
                null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));

        return false;
    }
}
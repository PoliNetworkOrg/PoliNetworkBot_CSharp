﻿#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Dispatcher;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
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

    public static async Task<CommandExecutionState> GetRunningTime(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        var eventArgsContainer = EventArgsContainer.Get(e);
        try
        {
            var lang = new Language(new Dictionary<string, string?>
            {
                { "", await CommandDispatcher.GetRunningTime() }
            });
            if (e != null)
            {
                var eMessage = e.Message;
                var eMessageFrom = eMessage.From;
                await SendMessage.SendMessageInPrivate(sender, eMessageFrom?.Id,
                    eMessageFrom?.LanguageCode,
                    eMessageFrom?.Username, lang, ParseMode.Html,
                    null, InlineKeyboardMarkup.Empty(), eventArgsContainer, eMessage.MessageThreadId);
            }

            return CommandExecutionState.SUCCESSFUL;
        }
        catch (Exception? ex)
        {
            _ = NotifyUtil.NotifyOwnerWithLog2(ex, sender, eventArgsContainer);
        }

        return CommandExecutionState.ERROR_DEFAULT;
    }

    public static async Task<CommandExecutionState> TestTime(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        var time = await CommandDispatcher.TestTime(sender, e);
        Logger.Logger.WriteLine(time);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> GetTime(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        var lang = new Language(new Dictionary<string, string?>
        {
            { "", DateTimeClass.NowAsStringAmericanFormat() }
        });
        if (e != null)
        {
            var eventArgsContainer = EventArgsContainer.Get(e);
            var eMessage = e.Message;
            await SendMessage.SendMessageInPrivate(sender, eMessage.From?.Id,
                eMessage.From?.LanguageCode,
                eMessage.From?.Username, lang, ParseMode.Html,
                null, InlineKeyboardMarkup.Empty(), eventArgsContainer, messageThreadId: eMessage.MessageThreadId);
        }

        return CommandExecutionState.SUCCESSFUL;
    }
}
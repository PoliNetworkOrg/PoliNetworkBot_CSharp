#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Dispatcher;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using SampleNuGet.Objects;
using SampleNuGet.Utils;
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

    public static void GetRunningTime(ActionFuncGenericParams actionFuncGenericParams)
    {
        var eventArgsContainer = EventArgsContainer.Get(actionFuncGenericParams.MessageEventArgs);
        try
        {
            var runningTime = CommandDispatcher.GetRunningTime();
            var lang = new Language(new Dictionary<string, string?>
            {
                { "", runningTime.Result }
            });
            if (actionFuncGenericParams.MessageEventArgs == null)
            {
                actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
                return;
            }

            var eMessage = actionFuncGenericParams.MessageEventArgs.Message;
            var eMessageFrom = eMessage.From;
            var sendMessageInPrivate = SendMessage.SendMessageInPrivate(actionFuncGenericParams.TelegramBotAbstract,
                eMessageFrom?.Id,
                eMessageFrom?.LanguageCode,
                eMessageFrom?.Username, lang, ParseMode.Html,
                null, InlineKeyboardMarkup.Empty(), eventArgsContainer, eMessage.MessageThreadId);
            sendMessageInPrivate.Wait();
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
            return;
        }
        catch (Exception? ex)
        {
            _ = NotifyUtil.NotifyOwnerWithLog2(ex, actionFuncGenericParams.TelegramBotAbstract, eventArgsContainer);
        }

        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.ERROR_DEFAULT;
    }

    public static void TestTime(ActionFuncGenericParams actionFuncGenericParams)
    {
        var testTime = CommandDispatcher.TestTime(actionFuncGenericParams.TelegramBotAbstract,
            actionFuncGenericParams.MessageEventArgs);
        var time = testTime.Result;
        Logger.Logger.WriteLine(time);
        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }

    public static void GetTime(ActionFuncGenericParams actionFuncGenericParams)
    {
        var lang = new Language(new Dictionary<string, string?>
        {
            { "", DateTimeClass.NowAsStringAmericanFormat() }
        });
        if (actionFuncGenericParams.MessageEventArgs == null)
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
            return;
        }

        var eventArgsContainer = EventArgsContainer.Get(actionFuncGenericParams.MessageEventArgs);
        var eMessage = actionFuncGenericParams.MessageEventArgs.Message;
        var sendMessageInPrivate = SendMessage.SendMessageInPrivate(actionFuncGenericParams.TelegramBotAbstract,
            eMessage.From?.Id,
            eMessage.From?.LanguageCode,
            eMessage.From?.Username, lang, ParseMode.Html,
            null, InlineKeyboardMarkup.Empty(), eventArgsContainer, eMessage.MessageThreadId);
        sendMessageInPrivate.Wait();

        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }
}
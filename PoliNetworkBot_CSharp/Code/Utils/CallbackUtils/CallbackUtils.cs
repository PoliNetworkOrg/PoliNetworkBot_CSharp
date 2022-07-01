#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;

public static class CallbackUtils
{
    private const string SEPARATOR = "-";
    public static CallBackDataFull? CallBackDataFull = new();

    public static async Task<MessageSentResult?> SendMessageWithCallbackQueryAsync(
        CallbackGenericData callbackGenericData,
        long chatToSendTo,
        Language text, TelegramBotAbstract? telegramBotAbstract, ChatType chatType, string? lang, string? username,
        bool splitMessage, long? replyToMessageId = null)
    {
        callbackGenericData.Bot = telegramBotAbstract;
        callbackGenericData.InsertedTime = DateTime.Now;

        var newLast = GetLast();
        var key = GetKeyFromNumber(newLast);
        callbackGenericData.Id = key;
        CallBackDataFull?.Add(key, callbackGenericData);

        var replyMarkupObject = GetReplyMarkupObject(callbackGenericData, key);
        if (telegramBotAbstract != null)
        {
            var messageSent = await telegramBotAbstract.SendTextMessageAsync(chatToSendTo, text, chatType, lang,
                ParseMode.Html, replyMarkupObject, username, splitMessage: splitMessage,
                replyToMessageId: replyToMessageId);
            callbackGenericData.MessageSent = messageSent;

            return messageSent;
        }

        return null;
    }

    internal static void DoCheckCallbackDataExpired()
    {
        while (true)
        {
            try
            {
                CallBackDataFull?.ChechCallbackDataExpired();
            }
            catch
            {
                ;
            }

            Thread.Sleep(1000 * 60 * 60 * 24); //every day
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private static ReplyMarkupObject? GetReplyMarkupObject(CallbackGenericData callbackGenericData, string? key)
    {
        var x2 = callbackGenericData.Options.Select((option, i1) => new List<InlineKeyboardButton>
            { new(option.displayed) { CallbackData = key + SEPARATOR + i1 } }).ToList();

        var inlineKeyboardMarkup = new InlineKeyboardMarkup(x2);
        return new ReplyMarkupObject(inlineKeyboardMarkup);
    }

    private static string? GetKeyFromNumber(BigInteger? newLast)
    {
        if (newLast != null)
        {
            var r = newLast.Value.ToString("X");
            return r;
        }

        return null;
    }

    private static BigInteger? GetLast()
    {
        if (CallBackDataFull != null) return CallBackDataFull.GetLast();
        return null;
    }

#pragma warning disable CS8632 // L'annotazione per i tipi riferimento nullable deve essere usata solo nel codice in un contesto di annotations '#nullable'.

    public static void CallbackMethodStart(object? sender, CallbackQueryEventArgs e)
#pragma warning restore CS8632 // L'annotazione per i tipi riferimento nullable deve essere usata solo nel codice in un contesto di annotations '#nullable'.
    {
        try
        {
            var t = new Thread(() => _ = CallbackMethodHandle(sender, e));
            t.Start();
        }
        catch (Exception? ex)
        {
            Logger.Logger.WriteLine(ex);
        }
    }

#pragma warning disable CS8632 // L'annotazione per i tipi riferimento nullable deve essere usata solo nel codice in un contesto di annotations '#nullable'.

    private static async Task<bool> CallbackMethodHandle(object? sender, CallbackQueryEventArgs callbackQueryEventArgs)
#pragma warning restore CS8632 // L'annotazione per i tipi riferimento nullable deve essere usata solo nel codice in un contesto di annotations '#nullable'.
    {
        TelegramBotAbstract? telegramBotClient = null;

        try
        {
            if (sender is TelegramBotClient tmp) telegramBotClient = new TelegramBotAbstract(tmp);

            if (telegramBotClient == null)
                return false;

            await CallbackMethodRun(telegramBotClient, callbackQueryEventArgs);
        }
        catch (Exception? exc)
        {
            await NotifyUtil.NotifyOwners(exc, telegramBotClient);
        }

        return false;
    }

    private static async Task CallbackMethodRun(TelegramBotAbstract? telegramBotClientBot,
        CallbackQueryEventArgs callbackQueryEventArgs)
    {
        try
        {
            if (callbackQueryEventArgs.CallbackQuery != null)
            {
                var data = callbackQueryEventArgs.CallbackQuery.Data;
                if (string.IsNullOrEmpty(data) == false)
                {
                    string?[] datas = data.Split(SEPARATOR);
                    var key = datas[0];
                    var answer = Convert.ToInt32(datas[1]);
                    CallBackDataFull?.UpdateAndRun(callbackQueryEventArgs, answer, key);
                }
            }
        }
        catch (Exception? exception)
        {
            await NotifyUtil.NotifyOwners(exception, telegramBotClientBot);
        }
    }

    internal static void InitializeCallbackDatas()
    {
        try
        {
            try
            {
                CallBackDataFull = JsonConvert.DeserializeObject<CallBackDataFull>(
                    File.ReadAllText(Paths.Data.CallbackData));
                Logger.Logger.WriteLine("Callbackdata file is empty");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                CallBackDataFull = new CallBackDataFull();
            }
        }
        catch (Exception? ex)
        {
            Logger.Logger.WriteLine(ex);
        }
    }
}
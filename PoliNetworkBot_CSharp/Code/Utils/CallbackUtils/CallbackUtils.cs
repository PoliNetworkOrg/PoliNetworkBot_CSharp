using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;

public class CallbackUtils
{
    public static CallBackDataFull callBackDataFull = new();

    public const string SEPARATOR = "-";

    public static async Task<MessageSentResult> SendMessageWithCallbackQueryAsync(CallbackGenericData callbackGenericData,
        long chatToSendTo,
        Language text, TelegramBotAbstract telegramBotAbstract, ChatType chatType, string lang, string username,
        bool splitMessage, long? replyToMessageId = null)
    {
        callbackGenericData.Bot = telegramBotAbstract;
        callbackGenericData.InsertedTime = DateTime.Now;

        var newLast = CallbackUtils.GetLast();
        var key = GetKeyFromNumber(newLast);
        callbackGenericData.id = key;
        callBackDataFull.Add(key, callbackGenericData);

        ReplyMarkupObject replyMarkupObject = GetReplyMarkupObject(callbackGenericData, key);
        var messageSent = await telegramBotAbstract.SendTextMessageAsync(chatToSendTo, text, chatType, lang, ParseMode.Html, replyMarkupObject, username, splitMessage: splitMessage, replyToMessageId: replyToMessageId);
        callbackGenericData.MessageSent = messageSent;

        return messageSent;
    }

    internal static void DoCheckCallbackDataExpired()
    {
        while (true)
        {
            try
            {
                callBackDataFull.ChechCallbackDataExpired();
            }
            catch
            {
                ;
            }

            Thread.Sleep(1000 * 60 * 60 * 24); //every day
        }
    }

    private static ReplyMarkupObject GetReplyMarkupObject(CallbackGenericData callbackGenericData, string key)
    {
        var x2 = callbackGenericData.options.Select((option, i1) => new List<InlineKeyboardButton> { new(option.displayed) { CallbackData = key + SEPARATOR + i1 } }).ToList();

        var inlineKeyboardMarkup = new InlineKeyboardMarkup(x2);
        return new ReplyMarkupObject(inlineKeyboardMarkup);
    }

    private static string GetKeyFromNumber(BigInteger newLast)
    {
        string r = newLast.ToString("X");
        return r;
    }

    private static BigInteger GetLast()
    {
        return callBackDataFull.GetLast();
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
        catch (Exception ex)
        {
            Logger.WriteLine(ex);
        }
    }

#pragma warning disable CS8632 // L'annotazione per i tipi riferimento nullable deve essere usata solo nel codice in un contesto di annotations '#nullable'.

    public static async Task<bool> CallbackMethodHandle(object? sender, CallbackQueryEventArgs callbackQueryEventArgs)
#pragma warning restore CS8632 // L'annotazione per i tipi riferimento nullable deve essere usata solo nel codice in un contesto di annotations '#nullable'.
    {
        TelegramBotClient telegramBotClientBot = null;
        TelegramBotAbstract telegramBotClient = null;

        try
        {
            if (sender is TelegramBotClient tmp) telegramBotClientBot = tmp;

            if (telegramBotClientBot == null)
                return false;

            await CallbackMethodRun(telegramBotClient, callbackQueryEventArgs);
        }
        catch (Exception exc)
        {
            await NotifyUtil.NotifyOwners(exc, telegramBotClient);
        }

        return false;
    }

    internal static async Task CallbackMethodRun(TelegramBotAbstract telegramBotClientBot, CallbackQueryEventArgs callbackQueryEventArgs)
    {
        try
        {
            string data = callbackQueryEventArgs.CallbackQuery.Data;
            var datas = data.Split(SEPARATOR);
            var key = datas[0];
            var answer = Convert.ToInt32(datas[1]);
            callBackDataFull.UpdateAndRun(callbackQueryEventArgs, answer, key);
        }
        catch (Exception exception)
        {
            await NotifyUtil.NotifyOwners(exception, telegramBotClientBot);
        }
    }

    internal static void InitializeCallbackDatas()
    {
        try
        {
            callBackDataFull = JsonConvert.DeserializeObject<CallBackDataFull>(
                File.ReadAllText(Paths.Data.CallbackData)) ?? new CallBackDataFull();
        }
        catch (Exception ex)
        {
            callBackDataFull = new CallBackDataFull();
            Logger.WriteLine(ex);
        }
    }
}
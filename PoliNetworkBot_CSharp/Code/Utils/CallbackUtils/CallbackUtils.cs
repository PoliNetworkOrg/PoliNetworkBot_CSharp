using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.IO;
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

        BigInteger newLast = CallbackUtils.GetLast();
        string key = GetKeyFromNumber(newLast);
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
        var x2 = new List<List<InlineKeyboardButton>>();
        for (int i1 = 0; i1 < callbackGenericData.options.Count; i1++)
        {
            CallbackOption option = callbackGenericData.options[i1];
            x2.Add(new List<InlineKeyboardButton>
                {
                    new(option.displayed)
                    {
                        CallbackData = key + SEPARATOR + i1.ToString()
                    }
                });
        }

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

    public static void CallbackMethodStart(object? sender, CallbackQueryEventArgs e)
    {
        var t = new Thread(() => _ = CallbackMethodHandle(sender, e));
        t.Start();
    }

    public static async Task<bool> CallbackMethodHandle(object? sender, CallbackQueryEventArgs callbackQueryEventArgs)
    {
        TelegramBotClient telegramBotClientBot = null;
        TelegramBotAbstract telegramBotClient = null;

        try
        {
            if (sender is TelegramBotClient tmp) telegramBotClientBot = tmp;

            if (telegramBotClientBot == null)
                return false;

            await Utils.CallbackUtils.CallbackUtils.CallbackMethodRun(telegramBotClient, callbackQueryEventArgs);
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
                File.ReadAllText(Paths.Data.CallbackData)) ?? new();
        }
        catch (Exception ex)
        {
            callBackDataFull = new();
        }
    }
}
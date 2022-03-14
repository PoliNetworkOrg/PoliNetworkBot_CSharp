﻿using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using Telegram.Bot;
using PoliNetworkBot_CSharp.Code.Bots.Anon;

namespace PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;

public class CallbackUtils
{
    public static BigInteger last = 0;
    public static Dictionary<string, CallbackGenericData> callbackDatas;
    public static object callbackLock = new();

    public const string SEPARATOR = "-";

    public static async Task SendMessageWithCallbackQueryAsync(CallbackGenericData callbackGenericData, long chatToSendTo, 
        Language text, TelegramBotAbstract telegramBotAbstract, ChatType  chatType, string lang, string username)
    {
        callbackGenericData.Bot = telegramBotAbstract;
        callbackGenericData.insertedTime = DateTime.Now;

        BigInteger newLast = CallbackUtils.GetLast();
        string key = GetKeyFromNumber(newLast);
        callbackGenericData.id = key;
        callbackDatas[key] = callbackGenericData;

        ReplyMarkupObject replyMarkupObject = GetReplyMarkupObject(callbackGenericData, key);
        var messageSent = await telegramBotAbstract.SendTextMessageAsync(chatToSendTo, text, chatType, lang, ParseMode.Html, replyMarkupObject, username);
        callbackGenericData.messageSent = messageSent;
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
        BigInteger r = 0;
        lock (callbackLock)
        {
            r = last;
            last++;
        }

        return r;
    }

    internal static void CallbackMethod(TelegramBotClient telegramBotClientBot, CallbackQueryEventArgs callbackQueryEventArgs)
    {
        string data = callbackQueryEventArgs.CallbackQuery.Data;
        var datas = data.Split(SEPARATOR);
        var key = datas[0];
        var answer = Convert.ToInt32(datas[1]);
        callbackDatas[key].callBackQueryFromTelegram = callbackQueryEventArgs.CallbackQuery;
        callbackDatas[key].selectedAnswer = answer;
        callbackDatas[key].RunAfterSelection(callbackDatas[key]);
    }
}


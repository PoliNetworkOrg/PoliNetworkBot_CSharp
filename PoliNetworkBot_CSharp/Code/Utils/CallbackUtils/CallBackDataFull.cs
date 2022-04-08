﻿#region

using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class CallBackDataFull
{
    public Dictionary<string, CallbackGenericData> callbackDatas = new();
    public BigInteger last = 0;

    internal void Add(string key, CallbackGenericData callbackGenericData)
    {
        callbackDatas.Add(key, callbackGenericData);
    }

    internal void BackupToFile()
    {
        try
        {
            File.WriteAllText(Paths.Data.CallbackData, JsonConvert.SerializeObject(this));
        }
        catch
        {
            ;
        }
    }

    internal BigInteger GetLast()
    {
        BigInteger r = 0;
        lock (this)
        {
            r = last;
            last++;
        }

        return r;
    }

    internal void UpdateAndRun(CallbackQueryEventArgs callbackQueryEventArgs, int answer, string key)
    {
        callbackDatas[key].CallBackQueryFromTelegram = callbackQueryEventArgs.CallbackQuery;
        callbackDatas[key].SelectedAnswer = answer;
        callbackDatas[key].RunAfterSelection(callbackDatas[key]);
    }

    internal void ChechCallbackDataExpired()
    {
        List<string> toRemove = new();
        toRemove.AddRange(callbackDatas.Where(v => v.Value.IsExpired()).Select(v => v.Key));
        lock (this)
        {
            foreach (var v in toRemove) callbackDatas.Remove(v);
        }
    }
}
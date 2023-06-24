#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class AnswerWithTask
{
    private readonly Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string?>?>> _dictionary;

    public AnswerWithTask()
    {
        _dictionary = new Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string?>?>>();
    }

    public bool ContainsKey(long botId)
    {
        return _dictionary.ContainsKey(botId);
    }

    public void InitializeIfKeyNotPresent(long botId)
    {
        _dictionary.TryAdd(botId, new Couple<AnswerTelegram, TaskCompletionSource<string?>?>());
    }

    public void ResetItem1(long botId)
    {
        _dictionary[botId].Item1 = null;
        _dictionary[botId].Item1 = new AnswerTelegram();
    }

    public AnswerTelegram? GetItem1(long botId)
    {
        return _dictionary[botId].Item1;
    }

    public void Delete(long botId)
    {
        _dictionary[botId].Item1 = null;
        _dictionary[botId].Item2 = null;
    }

    public TaskCompletionSource<string?>? GetItem2(long botId)
    {
        return _dictionary[botId].Item2;
    }

    public void ResetItem2(long botId)
    {
        _dictionary[botId].Item2 = new TaskCompletionSource<string?>();
    }
}
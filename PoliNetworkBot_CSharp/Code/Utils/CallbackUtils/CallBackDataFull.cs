using PoliNetworkBot_CSharp.Code.Bots.Anon;
using System.Collections.Generic;
using System.Numerics;

namespace PoliNetworkBot_CSharp.Code.Utils.CallbackUtils
{
    public class CallBackDataFull
    {
        public BigInteger last = 0;
        public Dictionary<string, CallbackGenericData> callbackDatas = new();

        internal void Add(string key, CallbackGenericData callbackGenericData)
        {
            this.callbackDatas.Add(key, callbackGenericData);
        }

        internal BigInteger GetLast()
        {
            BigInteger r = 0;
            lock (this)
            {
                r = this.last;
                this.last++;
            }

            return r;
        }

        internal void UpdateAndRun(CallbackQueryEventArgs callbackQueryEventArgs, int answer, string key)
        {
            callbackDatas[key].CallBackQueryFromTelegram = callbackQueryEventArgs.CallbackQuery;
            callbackDatas[key].SelectedAnswer = answer;
            callbackDatas[key].RunAfterSelection(callbackDatas[key]);
        }
    }
}
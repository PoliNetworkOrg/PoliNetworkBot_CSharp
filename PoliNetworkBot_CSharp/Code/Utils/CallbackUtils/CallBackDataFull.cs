using PoliNetworkBot_CSharp.Code.Bots.Anon;
using System.Collections.Generic;
using System.Linq;
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

        internal void ChechCallbackDataExpired()
        {
            List<string> toRemove = new();
            toRemove.AddRange(callbackDatas.Where(v => v.Value.IsExpired()).Select(v => v.Key));
            lock (this)
            {
                foreach (var v in toRemove)
                {
                    this.callbackDatas.Remove(v);
                }
            }
        }
    }
}
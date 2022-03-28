using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Utils.CallbackUtils
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class CallbackGenericData
    {
        public string id;
        public List<CallbackOption> options = new();
        internal TelegramBotAbstract Bot;
        internal MessageSentResult MessageSent;
        internal DateTime? InsertedTime;
        public Action<CallbackGenericData> RunAfterSelection;
        internal int SelectedAnswer;
        internal CallbackQuery CallBackQueryFromTelegram;

        public CallbackGenericData(List<CallbackOption> options, Action<CallbackGenericData> runAfterSelection)
        {
            this.options = options;
            this.RunAfterSelection = runAfterSelection;
        }

        internal bool IsExpired()
        {
            if (InsertedTime == null)
                return false;

            return this.InsertedTime.Value.AddDays(7) <= DateTime.Now;
        }
    }
}
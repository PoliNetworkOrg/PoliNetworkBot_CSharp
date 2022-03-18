#region

using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Bots.Materials;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.InfoBot
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class BotInfoAbstract
    {
        public bool? acceptedMessages;
        public string apiHash;
        public long? apiId;
        public BotTypeApi? botTypeApi;
        public string contactString;
        public string method;
        public string NumberCountry;
        public string NumberNumber;
        public string onMessages;
        public string passwordToAuthenticate;
        public string SessionUserId;
        public string token;
        public long? userId;
        public string website;

        internal EventHandler<CallbackQueryEventArgs> GetCallbackEvent()
        {
            return onMessages switch
            {
                "a" => Utils.CallbackUtils.CallbackUtils.CallbackMethodStart,
                "m" => Utils.CallbackUtils.CallbackUtils.CallbackMethodStart,
                "mat" => Program.BotOnCallbackQueryReceived,
                _ => null
            };
        }

        internal string GetToken()
        {
            return token;
        }

        internal Tuple<EventHandler<MessageEventArgs>, string> GetOnMessage()
        {
            try
            {
                var s = onMessages;
                var r1 = BotStartMethods.GetMethodFromString(s);
                return new Tuple<EventHandler<MessageEventArgs>, string>(r1, s);
            }
            catch
            {
                ;
            }

            return new Tuple<EventHandler<MessageEventArgs>, string>(null, null);
        }

        internal bool? AcceptsMessages()
        {
            return acceptedMessages;
        }

        internal string GetWebsite()
        {
            try
            {
                return website;
            }
            catch
            {
                return null;
            }
        }

        internal string GetContactString()
        {
            try
            {
                return contactString;
            }
            catch
            {
                return null;
            }
        }
    }
}
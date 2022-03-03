#region

using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using System;
using System.Collections.Generic;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.InfoBot
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class BotInfoAbstract
    {
        public BotTypeApi? botTypeApi;
        public string token;
        public string website;
        public string contactString;
        public string onMessages;
        public bool? acceptedMessages;
        public string SessionUserId;
        public long? userId;
        public long? apiId;
        public string apiHash;
        public string NumberCountry;
        public string NumberNumber;
        public string passwordToAuthenticate;
        public string method;

        public BotInfoAbstract()
        {

        }

        internal EventHandler<CallbackQueryEventArgs> GetCallbackEvent()
        {
            return onMessages switch
            {
                "a" => MainAnon.CallbackMethod,
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
            return  acceptedMessages;
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
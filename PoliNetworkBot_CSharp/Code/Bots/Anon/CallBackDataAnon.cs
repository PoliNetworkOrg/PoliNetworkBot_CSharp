using System;

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    internal class CallBackDataAnon
    {
#pragma warning disable IDE0052 // Rimuovi i membri privati non letti
        private readonly string data;
#pragma warning restore IDE0052 // Rimuovi i membri privati non letti
        public bool? from_telegram;
        public long? identity;
        public string langUser;
        public long? messageIdGroup;
        internal Tuple<long?, ResultQueueEnum?> messageIdToReplyTo;
        public long? messageIdUser;

        public ResultQueueEnum? resultQueueEnum;
        public long? userId;
        public string username;

        public CallBackDataAnon(string data)
        {
            this.data = data;

            var s = data.Split(ConfigAnon.splitCallback);
            resultQueueEnum = GetResult(s[0]);

            try
            {
                messageIdGroup = Convert.ToInt64(s[1]);
            }
            catch
            {
                messageIdGroup = null;
            }

            try
            {
                userId = Convert.ToInt64(s[2]);
            }
            catch
            {
                userId = null;
            }

            try
            {
                identity = Convert.ToInt64(s[3]);
            }
            catch
            {
                identity = null;
            }

            langUser = s[4];
            username = s[5];

            try
            {
                messageIdUser = Convert.ToInt64(s[6]);
            }
            catch
            {
                messageIdUser = null;
            }

            try
            {
                messageIdToReplyTo = new Tuple<long?, ResultQueueEnum?>(Convert.ToInt64(s[7]), GetChosenQueue(s[8]));
            }
            catch
            {
                messageIdToReplyTo = null;
            }

            try
            {
                var s9 = s[9];
                from_telegram = s9 == "S" || s9 == "Y";
            }
            catch
            {
                from_telegram = null;
            }
        }

        public CallBackDataAnon(ResultQueueEnum v, long? messageIdGroup1, long? userId, long identity, string langcode,
            string username, long? messageIdUser1, Tuple<long?, ResultQueueEnum?> messageIdToReplyTo,
            bool from_telegram)
        {
            resultQueueEnum = v;
            messageIdGroup = messageIdGroup1;
            this.userId = userId;
            this.identity = identity;
            langUser = langcode;
            this.username = username;
            messageIdUser = messageIdUser1;
            this.messageIdToReplyTo = messageIdToReplyTo;
            this.from_telegram = from_telegram;
        }

        private static ResultQueueEnum? GetChosenQueue(string v)
        {
            return GetResult(v);
        }

        private static ResultQueueEnum? GetResult(string v)
        {
            return v switch
            {
                null => null,
                "" => null,
                "a" => ResultQueueEnum.APPROVED_MAIN,
                "u" => ResultQueueEnum.GO_TO_UNCENSORED,
                "d" => ResultQueueEnum.DELETE,
                _ => null
            };
        }

        internal string ToDataString()
        {
            ;
            // split
            var r = "";

            r += ResultToString(resultQueueEnum);
            r += ConfigAnon.splitCallback;
            r += messageIdGroup == null ? "null" : messageIdGroup.Value.ToString();
            r += ConfigAnon.splitCallback;
            r += userId;
            r += ConfigAnon.splitCallback;
            r += identity;
            r += ConfigAnon.splitCallback;
            r += langUser;
            r += ConfigAnon.splitCallback;
            r += username;
            r += ConfigAnon.splitCallback;
            r += messageIdUser == null ? "null" : messageIdUser.Value.ToString();
            r += ConfigAnon.splitCallback;
            r += PrintReplyTo();
            r += ConfigAnon.splitCallback;
            r += from_telegram != null && from_telegram == true ? "Y" : "N";

            return r;
        }

        private string PrintReplyTo()
        {
            if (messageIdToReplyTo == null) return "null" + ConfigAnon.splitCallback + "null";

            var r = "";

            if (messageIdToReplyTo.Item1 == null)
                r += "null";
            else
                r += messageIdToReplyTo.Item1.ToString();

            r += ConfigAnon.splitCallback;

            if (messageIdToReplyTo.Item2 == null)
                r += "null";
            else
                r += ResultToString(messageIdToReplyTo.Item2);

            return r;
        }

        public static string ResultToString(ResultQueueEnum? item2)
        {
            return item2 switch
            {
                ResultQueueEnum.APPROVED_MAIN => "a",
                ResultQueueEnum.GO_TO_UNCENSORED => "u",
                ResultQueueEnum.DELETE => "d",
                _ => null
            };
        }
    }
}
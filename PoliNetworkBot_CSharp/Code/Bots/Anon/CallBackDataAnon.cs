using System;

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    internal class CallBackDataAnon
    {
        private readonly string data;

        public ResultQueueEnum? resultQueueEnum;
        public long? messageIdGroup;
        public long? userId;
        public int? identity;
        public string langUser;
        public string username;
        public long? messageIdUser;
        internal Tuple<long?, Anon.ResultQueueEnum?> messageIdToReplyTo;
        public bool? from_telegram;

        public CallBackDataAnon(string data)
        {
            this.data = data;

            var s = data.Split(ConfigAnon.splitCallback);
            this.resultQueueEnum = GetResult(s[0]);

            try
            {
                this.messageIdGroup = Convert.ToInt32(s[1]);
            }
            catch
            {
                this.messageIdGroup = null;
            }

            try
            {
                this.userId = Convert.ToInt32(s[2]);
            }
            catch
            {
                this.userId = null;
            }

            try
            {
                this.identity = Convert.ToInt32(s[3]);
            }
            catch
            {
                this.identity = null;
            }

            this.langUser = s[4];
            this.username = s[5];

            try
            {
                this.messageIdUser = Convert.ToInt32(s[6]);
            }
            catch
            {
                this.messageIdUser = null;
            }

            try
            {
                this.messageIdToReplyTo = new Tuple<long?, ResultQueueEnum?>(Convert.ToInt64(s[7]), GetChosenQueue(s[8]));
            }
            catch
            {
                this.messageIdToReplyTo = null;
            }

            try
            {
                string s9 = s[9];
                this.from_telegram = s9 == "S" || s9 == "Y";
            }
            catch
            {
                this.from_telegram = null;
            }
        }

        private ResultQueueEnum? GetChosenQueue(string v)
        {
            return GetResult(v);
        }

        public CallBackDataAnon(ResultQueueEnum v, long? messageIdGroup1, long? userId, int identity, string langcode,
            string username, long? messageIdUser1, Tuple<long?, Anon.ResultQueueEnum?> messageIdToReplyTo, bool from_telegram)
        {
            this.resultQueueEnum = v;
            this.messageIdGroup = messageIdGroup1;
            this.userId = userId;
            this.identity = identity;
            this.langUser = langcode;
            this.username = username;
            this.messageIdUser = messageIdUser1;
            this.messageIdToReplyTo = messageIdToReplyTo;
            this.from_telegram = from_telegram;
        }

        private ResultQueueEnum? GetResult(string v)
        {
            return v switch
            {
                null => null,
                "" => null,
                "a" => Anon.ResultQueueEnum.APPROVED_MAIN,
                "u" => Anon.ResultQueueEnum.GO_TO_UNCENSORED,
                "d" => Anon.ResultQueueEnum.DELETE,
                _ => null,
            };
        }

        internal string ToDataString()
        {
            ;
            // split
            string r = "";

            r += ResultToString(this.resultQueueEnum);
            r += Anon.ConfigAnon.splitCallback;
            r += (messageIdGroup == null ? "null" : messageIdGroup.Value.ToString());
            r += Anon.ConfigAnon.splitCallback;
            r += userId;
            r += Anon.ConfigAnon.splitCallback;
            r += identity;
            r += Anon.ConfigAnon.splitCallback;
            r += langUser;
            r += Anon.ConfigAnon.splitCallback;
            r += username;
            r += Anon.ConfigAnon.splitCallback;
            r += (messageIdUser == null ? "null" : messageIdUser.Value.ToString());
            r += Anon.ConfigAnon.splitCallback;
            r += PrintReplyTo();
            r += Anon.ConfigAnon.splitCallback;
            r += from_telegram != null && from_telegram == true ? "Y" : "N";

            return r;
        }

        private string PrintReplyTo()
        {
            if (this.messageIdToReplyTo == null)
            {
                return "null" + Anon.ConfigAnon.splitCallback + "null";
            }

            string r = "";

            if (this.messageIdToReplyTo.Item1 == null)
            {
                r += "null";
            }
            else
            {
                r += this.messageIdToReplyTo.Item1.ToString();
            }

            r += Anon.ConfigAnon.splitCallback;

            if (this.messageIdToReplyTo.Item2 == null)
            {
                r += "null";
            }
            else
            {
                r += ResultToString(this.messageIdToReplyTo.Item2);
            }

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
using System;

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    internal class CallBackDataAnon
    {
        private string data;

        public ResultQueueEnum? resultQueueEnum;
        public long? messageIdGroup;
        public int? userId;     
        public int? identity;
        public string langUser;
        public string username;
        public long? messageIdUser;

        public CallBackDataAnon(string data)
        {
            this.data = data;

            var s = data.Split(ConfigAnon.splitCallback);
            this.resultQueueEnum = getResult(s[0]);

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
        }

        public CallBackDataAnon(ResultQueueEnum v, long? messageIdGroup1, int userId, int identity, string langcode, string username, long? messageIdUser1)
        {
            this.resultQueueEnum = v;
            this.messageIdGroup = messageIdGroup1;
            this.userId = userId;
            this.identity = identity;
            this.langUser = langcode;
            this.username = username;
            this.messageIdUser = messageIdUser1;
        }

        private ResultQueueEnum? getResult(string v)
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
            switch (this.resultQueueEnum)
            {
                case ResultQueueEnum.APPROVED_MAIN:
                    {
                        r += "a";
                        break;
                    }
                case ResultQueueEnum.GO_TO_UNCENSORED:
                    {
                        r += "u";
                        break;
                    }
                case ResultQueueEnum.DELETE:
                    {
                        r += "d";
                        break;
                    }
            }

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

            return r;
        }
    }
}
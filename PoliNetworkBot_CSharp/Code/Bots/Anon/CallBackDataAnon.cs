using System;

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    internal class CallBackDataAnon
    {
        private string data;

        public ResultQueueEnum? ResultQueueEnum;
        public int? messageIdGroup;
        public int? userId;     
        public int? identity;
        public string langUser;
        public string username;
        public int? messageIdUser;

        public CallBackDataAnon(string data)
        {
            this.data = data;

            var s = data.Split(ConfigAnon.splitCallback);
            this.ResultQueueEnum = getResult(s[0]);

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
    }
}
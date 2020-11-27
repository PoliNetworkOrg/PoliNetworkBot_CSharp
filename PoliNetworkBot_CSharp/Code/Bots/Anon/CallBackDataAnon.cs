using System;

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    internal class CallBackDataAnon
    {
        private string data;

        public ResultQueueEnum? ResultQueueEnum;
        public int? messageId;
        public int? userId;     
        public int? identity;
        public string langUser;
        public string username;

        public CallBackDataAnon(string data)
        {
            this.data = data;

            var s = data.Split(ConfigAnon.splitCallback);
            this.ResultQueueEnum = getResult(s[0]);

            try
            {
                this.messageId = Convert.ToInt32(s[1]);
            }
            catch
            {
                this.messageId = null;
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
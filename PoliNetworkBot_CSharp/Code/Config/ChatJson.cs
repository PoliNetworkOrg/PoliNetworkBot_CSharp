namespace PoliNetworkBot_CSharp.Code.Config
{
    internal class ChatJson
    {
        public long? id;
        public string type;
        public string title;
        public string invite_link;

        public ChatJson(long? id, string type, string title, string invite_link)
        {
            this.id = id;
            this.type = type;
            this.title = title;
            this.invite_link = invite_link;
        }
    }
}
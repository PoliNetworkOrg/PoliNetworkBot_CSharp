namespace PoliNetworkBot_CSharp.Code.Config
{
    internal class ChatJson
    {
        public readonly string invite_link;
        public readonly string title;
        public readonly string type;
        public long? id;

        public ChatJson(long? id, string type, string title, string invite_link)
        {
            this.id = id;
            this.type = type;
            this.title = title;
            this.invite_link = invite_link;
        }
    }
}
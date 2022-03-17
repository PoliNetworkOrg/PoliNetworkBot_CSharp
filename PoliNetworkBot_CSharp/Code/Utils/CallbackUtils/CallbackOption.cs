using Newtonsoft.Json;

namespace PoliNetworkBot_CSharp.Code.Utils.CallbackUtils
{
    [System.Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class CallbackOption
    {
        public string displayed;
        public object value;
        internal int id;

        public CallbackOption(string display, object value = null)
        {
            this.displayed = display;
            this.value = value;
        }
    }
}
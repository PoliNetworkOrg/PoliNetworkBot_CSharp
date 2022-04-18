#region

using System;
using Newtonsoft.Json;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class CallbackOption
{
    public string displayed;
    internal int id;
    public object value;

    public CallbackOption(string display, int id, object value)
    {
        displayed = display;
        this.id = id;
        this.value = value;
    }
}
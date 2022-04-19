using Newtonsoft.Json;
using System;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    internal class MutableTuple<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;

        public MutableTuple(T1 p1, T2 p2)
        {
            this.Item1 = p1;
            this.Item2 = p2;
        }
    }
}
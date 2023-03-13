using System;
using Newtonsoft.Json;

namespace PoliNetworkBot_CSharp.Code.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class Couple<T, TS>
{
    public T? Item1;
    public TS? Item2;
}
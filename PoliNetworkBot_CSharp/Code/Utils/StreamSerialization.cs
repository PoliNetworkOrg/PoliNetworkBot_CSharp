﻿#region

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal class StreamSerialization
{
    public static MemoryStream SerializeToStream(object? o)
    {
        var stream = new MemoryStream();
        IFormatter formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011 // Il tipo o il membro è obsoleto
        if (o != null) formatter.Serialize(stream, o);
#pragma warning restore SYSLIB0011 // Il tipo o il membro è obsoleto
        return stream;
    }
}
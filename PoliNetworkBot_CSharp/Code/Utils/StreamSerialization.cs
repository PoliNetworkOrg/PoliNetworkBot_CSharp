using System.IO;
using System.Runtime.Serialization;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class StreamSerialization
    {
        public static MemoryStream SerializeToStream(object o)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            formatter.Serialize(stream, o);
            return stream;
        }
    }
}
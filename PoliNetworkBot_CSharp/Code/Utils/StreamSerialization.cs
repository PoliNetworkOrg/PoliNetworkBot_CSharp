#region

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class StreamSerialization
    {
        public static MemoryStream SerializeToStream(object o)
        {
            var stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, o);
            return stream;
        }
    }
}
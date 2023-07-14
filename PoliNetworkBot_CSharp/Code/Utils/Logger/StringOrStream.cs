using System.IO;
using System.Text;

namespace PoliNetworkBot_CSharp.Code.Utils.Logger;

public class StringOrStream
{
    public string? StringValue;
    public Stream? StreamValue;

    public Stream GetStream()
    {
        return this.StreamValue ??   new MemoryStream(Encoding.UTF8.GetBytes(StringValue ?? ""));

    }
}
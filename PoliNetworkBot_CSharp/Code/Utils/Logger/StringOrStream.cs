using System.IO;
using System.Text;

namespace PoliNetworkBot_CSharp.Code.Utils.Logger;

public class StringOrStream
{
    public Stream? StreamValue;
    public string? StringValue;

    public Stream GetStream()
    {
        return StreamValue ?? new MemoryStream(Encoding.UTF8.GetBytes(StringValue ?? ""));
    }
}
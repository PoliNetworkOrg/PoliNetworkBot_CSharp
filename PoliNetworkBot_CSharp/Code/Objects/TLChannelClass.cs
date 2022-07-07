#region

using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class TlChannelClass
{
    public readonly TLChannel Channel;

    public TlChannelClass(TLChannel channel)
    {
        Channel = channel;
    }
}
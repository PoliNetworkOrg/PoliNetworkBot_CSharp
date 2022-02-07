#region

using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class TLChannelClass
    {
        public TLChannel channel;

        public TLChannelClass(TLChannel channel)
        {
            this.channel = channel;
        }
    }
}
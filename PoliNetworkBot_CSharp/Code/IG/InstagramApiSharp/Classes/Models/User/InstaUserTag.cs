using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Media;

namespace InstagramApiSharp.Classes.Models;

public class InstaUserTag
{
    public InstaPosition Position { get; set; }

    public string TimeInVideo { get; set; }

    public InstaUserShort User { get; set; }
}
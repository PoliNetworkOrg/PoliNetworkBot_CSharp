#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Business;

public class InstaBrandedContent
{
    public bool RequireApproval { get; set; }

    public List<InstaUserShort?> WhitelistedUsers { get; set; } = new();
}
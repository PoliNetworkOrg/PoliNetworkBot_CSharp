#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaPresenceConverter : IObjectConverter<InstaPresence, InstaPresenceResponse>
{
    public InstaPresenceResponse SourceObject { get; set; }

    public InstaPresence Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("SourceObject");

        var presence = new InstaPresence
        {
            PresenceDisabled = SourceObject.Disabled ?? false,
            ThreadPresenceDisabled = SourceObject.ThreadPresenceDisabled ?? false
        };

        return presence;
    }
}
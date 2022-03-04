#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Helpers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaSingleUserPresenceConverter : IObjectConverter<InstaUserPresence, InstaUserPresenceResponse>
    {
        public InstaUserPresenceResponse SourceObject { get; set; }

        public InstaUserPresence Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var userPresence = new InstaUserPresence
            {
                Pk = SourceObject.Pk,
                IsActive = SourceObject.IsActive ?? false,
                LastActivity = (SourceObject.LastActivityAtMs ?? 0).FromUnixTimeMiliSeconds()
            };
            return userPresence;
        }
    }
}
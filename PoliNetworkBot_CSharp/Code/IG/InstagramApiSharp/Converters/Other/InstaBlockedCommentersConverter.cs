#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaBlockedCommentersConverter : IObjectConverter<InstaUserShortList, InstaBlockedCommentersResponse>
    {
        public InstaBlockedCommentersResponse SourceObject { get; set; }

        public InstaUserShortList Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("SourceObject");

            var users = new InstaUserShortList();

            if (SourceObject.BlockedCommenters?.Count > 0)
                users.AddRange(SourceObject.BlockedCommenters.Select(user =>
                    ConvertersFabric.GetUserShortConverter(user).Convert()));

            return users;
        }
    }
}
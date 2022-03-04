#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

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
                foreach (var user in SourceObject.BlockedCommenters)
                    users.Add(ConvertersFabric.Instance.GetUserShortConverter(user).Convert());

            return users;
        }
    }
}
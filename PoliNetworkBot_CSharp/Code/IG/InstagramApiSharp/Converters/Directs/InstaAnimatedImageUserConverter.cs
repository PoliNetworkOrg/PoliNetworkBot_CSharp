#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters;

internal class
    InstaAnimatedImageUserConverter : IObjectConverter<InstaAnimatedImageUser, InstaAnimatedImageUserResponse>
{
    public InstaAnimatedImageUserResponse SourceObject { get; set; }

    public InstaAnimatedImageUser Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");

        var user = new InstaAnimatedImageUser
        {
            IsVerified = SourceObject.IsVerified,
            Username = SourceObject.Username
        };

        return user;
    }
}
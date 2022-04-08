#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Converters;
using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Converters.Discover;

internal class InstaUserContactListConverter : IObjectConverter<InstaContactUserList, InstaContactUserListResponse>
{
    public InstaContactUserListResponse SourceObject { get; init; }

    public InstaContactUserList Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        var userList = new InstaContactUserList();
        try
        {
            foreach (var item in SourceObject.Items)
                try
                {
                    userList.Add(ConvertersFabric.GetSingleUserContactConverter(item.User).Convert());
                }
                catch
                {
                }
        }
        catch
        {
        }

        return userList;
    }
}
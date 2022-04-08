#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters.Users;

internal class
    InstaUserChainingListConverter : IObjectConverter<InstaUserChainingList, InstaUserChainingContainerResponse>
{
    public InstaUserChainingContainerResponse SourceObject { get; set; }

    public InstaUserChainingList Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        var users = new InstaUserChainingList
        {
            Status = SourceObject.Status,
            IsBackup = SourceObject.IsBackup
        };
        if (SourceObject.Users == null || !SourceObject.Users.Any()) return users;
        foreach (var u in SourceObject.Users)
            try
            {
                users.Add(ConvertersFabric.GetSingleUserChainingConverter(u).Convert());
            }
            catch
            {
            }

        return users;
    }
}
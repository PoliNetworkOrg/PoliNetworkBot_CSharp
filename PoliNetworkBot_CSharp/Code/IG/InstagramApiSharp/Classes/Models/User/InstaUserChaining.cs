#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaUserChaining : InstaUserShort
{
    public string ChainingInfo { get; set; }

    public string ProfileChainingSecondaryLabel { get; set; }
}

public class InstaUserChainingList : List<InstaUserChaining>
{
    internal string Status { get; set; }

    public bool IsBackup { get; set; }
}
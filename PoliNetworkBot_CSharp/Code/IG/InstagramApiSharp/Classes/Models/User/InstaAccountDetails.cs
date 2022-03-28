#region

using System;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaAccountDetails
{
    public DateTime DateJoined { get; set; }

    public bool HasFormerUsernames { get; set; } = false;

    public InstaPrimaryCountryInfo PrimaryCountryInfo { get; set; }

    public bool HasSharedFollowerAccounts { get; set; } = false;

    public InstaAdsInfo AdsInfo { get; set; }
}
#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Helpers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaAccountDetailsConverter : IObjectConverter<InstaAccountDetails, InstaAccountDetailsResponse>
    {
        public InstaAccountDetailsResponse SourceObject { get; set; }

        public InstaAccountDetails Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var details = new InstaAccountDetails
            {
                DateJoined = DateTimeHelper.FromUnixTimeSeconds(SourceObject.DateJoined ?? 0)
            };
            if (SourceObject.FormerUsernameInfo != null)
                details.HasFormerUsernames = SourceObject.FormerUsernameInfo.HasFormerUsernames ?? false;

            if (SourceObject.SharedFollowerAccountsInfo != null)
                details.HasSharedFollowerAccounts =
                    SourceObject.SharedFollowerAccountsInfo.HasSharedFollowerAccounts ?? false;

            if (SourceObject.AdsInfo != null)
                try
                {
                    details.AdsInfo = ConvertersFabric.Instance.GetAdsInfoConverter(SourceObject.AdsInfo).Convert();
                }
                catch
                {
                }

            if (SourceObject.PrimaryCountryInfo != null)
                try
                {
                    details.PrimaryCountryInfo = ConvertersFabric.Instance
                        .GetPrimaryCountryInfoConverter(SourceObject.PrimaryCountryInfo).Convert();
                }
                catch
                {
                }

            return details;
        }
    }
}
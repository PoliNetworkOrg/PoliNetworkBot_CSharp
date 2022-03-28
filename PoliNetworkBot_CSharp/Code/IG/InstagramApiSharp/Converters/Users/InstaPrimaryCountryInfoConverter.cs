#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters;

internal class
    InstaPrimaryCountryInfoConverter : IObjectConverter<InstaPrimaryCountryInfo, InstaPrimaryCountryInfoResponse>
{
    public InstaPrimaryCountryInfoResponse SourceObject { get; set; }

    public InstaPrimaryCountryInfo Convert()
    {
        return new InstaPrimaryCountryInfo
        {
            CountryName = SourceObject.CountryName,
            HasCountry = SourceObject.HasCountry ?? false,
            IsVisible = SourceObject.IsVisible ?? false
        };
    }
}
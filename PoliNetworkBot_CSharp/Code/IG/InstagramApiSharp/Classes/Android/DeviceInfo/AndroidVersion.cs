#region

using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Classes.Android.DeviceInfo;

[Serializable]
public class AndroidVersion
{
    private static readonly Random Rnd = new();

    private static AndroidVersion LastAndriodVersion =
        AndroidVersionList.AndroidVersions()[^2];

    internal AndroidVersion()
    {
    }

    public string Codename { get; set; }
    public string VersionNumber { get; set; }
    public string APILevel { get; set; }

    public static AndroidVersion FromString(string versionString)
    {
        var version = new Version(versionString);
        return AndroidVersionList.AndroidVersions()
            .FirstOrDefault(androidVersion => version.CompareTo(new Version(androidVersion.VersionNumber)) == 0 ||
                                              version.CompareTo(new Version(androidVersion.VersionNumber)) > 0 &&
                                              androidVersion != AndroidVersionList.AndroidVersions().Last() &&
                                              version.CompareTo(new Version(
                                                  AndroidVersionList.AndroidVersions()[
                                                          AndroidVersionList.AndroidVersions()
                                                              .IndexOf(androidVersion) + 1]
                                                      .VersionNumber)) < 0);
    }

    public static AndroidVersion GetRandomAndriodVersion()
    {
    TryLabel:
        var randomDeviceIndex = Rnd.Next(0, AndroidVersionList.AndroidVersions().Count);
        var androidVersion = AndroidVersionList.AndroidVersions().ElementAt(randomDeviceIndex);
        if (LastAndriodVersion != null)
            if (androidVersion.APILevel == LastAndriodVersion.APILevel)
                goto TryLabel;
        LastAndriodVersion = androidVersion;
        return androidVersion;
    }

    public static AndroidVersion GetAndroidVersion(string apiLevel)
    {
        if (string.IsNullOrEmpty(apiLevel)) return null;

        return AndroidVersionList.AndroidVersions()
            .FirstOrDefault(api => api.APILevel == apiLevel);
    }
}
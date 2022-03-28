#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Android.DeviceInfo
{
    public class AndroidVersionList
    {
        public static AndroidVersionList GetVersionList()
        {
            return new AndroidVersionList();
        }

        public static List<AndroidVersion> AndroidVersions()
        {
            return new List<AndroidVersion>
            {
                new()
                {
                    Codename = "Ice Cream Sandwich",
                    VersionNumber = "4.0",
                    APILevel = "14"
                },
                new()
                {
                    Codename = "Ice Cream Sandwich",
                    VersionNumber = "4.0.3",
                    APILevel = "15"
                },
                new()
                {
                    Codename = "Jelly Bean",
                    VersionNumber = "4.1",
                    APILevel = "16"
                },
                new()
                {
                    Codename = "Jelly Bean",
                    VersionNumber = "4.2",
                    APILevel = "17"
                },
                new()
                {
                    Codename = "Jelly Bean",
                    VersionNumber = "4.3",
                    APILevel = "18"
                },
                new()
                {
                    Codename = "KitKat",
                    VersionNumber = "4.4",
                    APILevel = "19"
                },
                new()
                {
                    Codename = "KitKat",
                    VersionNumber = "5.0",
                    APILevel = "21"
                },
                new()
                {
                    Codename = "Lollipop",
                    VersionNumber = "5.1",
                    APILevel = "22"
                },
                new()
                {
                    Codename = "Marshmallow",
                    VersionNumber = "6.0",
                    APILevel = "23"
                },
                new()
                {
                    Codename = "Nougat",
                    VersionNumber = "7.0",
                    APILevel = "24"
                },
                new()
                {
                    Codename = "Nougat",
                    VersionNumber = "7.1",
                    APILevel = "25"
                },
                new()
                {
                    Codename = "Oreo",
                    VersionNumber = "8.0",
                    APILevel = "26"
                },
                new()
                {
                    Codename = "Oreo",
                    VersionNumber = "8.1",
                    APILevel = "27"
                },
                new()
                {
                    Codename = "Pie",
                    VersionNumber = "9.0",
                    APILevel = "27"
                }
            };
        }
    }
}
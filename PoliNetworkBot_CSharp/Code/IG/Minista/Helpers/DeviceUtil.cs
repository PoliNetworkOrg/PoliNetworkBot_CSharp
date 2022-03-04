#region

using Windows.System.Profile;

#endregion

namespace Minista
{
    public static class DeviceUtil
    {
        public static bool IsDesktop => AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop";

        public static bool IsMobile => AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile";

        public static bool IsIoT => AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.IoT";

        public static bool IsXbox => AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox";

        public static string OSVersion
        {
            get
            {
                var sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
                var v = ulong.Parse(sv);
                var v1 = (v & 0xFFFF000000000000L) >> 48;
                var v2 = (v & 0x0000FFFF00000000L) >> 32;
                var v3 = (v & 0x00000000FFFF0000L) >> 16;
                var v4 = v & 0x000000000000FFFFL;
                return $"{v1}.{v2}.{v3}.{v4}";
            }
        }

        public static bool OverRS2OS
        {
            get
            {
                var versions = GetDeviceOsVersion();
                int.TryParse(versions[2], out var versionCode);

                return versionCode > 15063; // >=
            }
        }

        private static string[] GetDeviceOsVersion()
        {
            var sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            var v = ulong.Parse(sv);
            var v1 = (v & 0xFFFF000000000000L) >> 48;
            var v2 = (v & 0x0000FFFF00000000L) >> 32;
            var v3 = (v & 0x00000000FFFF0000L) >> 16;
            var v4 = v & 0x000000000000FFFFL;
            return new[] { v1.ToString(), v2.ToString(), v3.ToString(), v4.ToString() };
        }
    }
}
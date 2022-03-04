#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

#endregion

namespace Minista.Helpers
{
    internal enum LottieTypes
    {
        Dislike,
        Dislike2,
        CheckMark,
        Hearts,
        Heart,
        Love
    }

    internal static class AssetHelper
    {
        public static string Get(LottieTypes type)
        {
            switch (type)
            {
                case LottieTypes.CheckMark:
                    return "Assets\\LottieAssets\\checkmark.json";
                case LottieTypes.Dislike:
                    return "Assets\\LottieAssets\\dislike.json";
                case LottieTypes.Dislike2:
                    return "Assets\\LottieAssets\\dislike2.json";
                case LottieTypes.Hearts:
                    return "Assets\\LottieAssets\\hearts.json";
                case LottieTypes.Heart:
                    return "Assets\\LottieAssets\\heart.json";
                default:
                case LottieTypes.Love:
                    return "Assets\\LottieAssets\\love.zip";
            }
        }

        public static async Task<StorageFile> GetAsync(LottieTypes type)
        {
            switch (type)
            {
                case LottieTypes.CheckMark:
                    return await GetSingleAssetsByName("checkmark.json");
                case LottieTypes.Dislike:
                    return await GetSingleAssetsByName("dislike.json");
                case LottieTypes.Dislike2:
                    return await GetSingleAssetsByName("dislike2.json");
                case LottieTypes.Hearts:
                    return await GetSingleAssetsByName("hearts.json");
                default:
                case LottieTypes.Love:
                    return await GetSingleAssetsByName("love.zip");
            }
        }

        public static async Task<StorageFile> GetSingleAssetsByName(string name)
        {
            var assets = await GetAssets();

            return assets.SingleOrDefault(a => a.Name.ToLower() == name.ToLower());
        }

        public static async Task<List<StorageFile>> GetAssets()
        {
            var localizationDirectory = await Package.Current.InstalledLocation.GetFolderAsync(@"Assets");

            return await GetAssetsFromFolder(localizationDirectory);
        }

        private static async Task<List<StorageFile>> GetAssetsFromFolder(StorageFolder folder)
        {
            var files = new List<StorageFile>();
            foreach (var asset in await folder.GetItemsAsync())
                if (asset is StorageFile file &&
                    (file.Name.ToLower().EndsWith(".json", StringComparison.Ordinal) ||
                     file.Name.ToLower().EndsWith(".zip", StringComparison.Ordinal)))
                    files.Add(file);
                else if (asset is StorageFolder storageFolder) files.AddRange(await GetAssetsFromFolder(storageFolder));
            return files;
        }
    }
}
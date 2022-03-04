#region

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Notifications;

#endregion

namespace Minista.Helpers
{
    internal class DownloadHelper
    {
        private static readonly Regex EmojiRegex = new("&#x?[A-Fa-f0-9]+;");

        private static string ReplaceInvalidXmlCharacterReferences(string input)
        {
            if (input.IndexOf("&#") == -1)
                return input;

            return EmojiRegex.Replace(input, match =>
            {
                var ncr = match.Value;
                var frmt = NumberFormatInfo.InvariantInfo;

                var isParsed =
                    ncr[2] == 'x'
                        ? // the x must be lowercase in XML documents
                        uint.TryParse(ncr.Substring(3, ncr.Length - 4), NumberStyles.AllowHexSpecifier, frmt,
                            out var num)
                        : uint.TryParse(ncr.Substring(2, ncr.Length - 3), NumberStyles.Integer, frmt, out num);

                return isParsed /*&& !System.Xml.XmlConvert.IsXmlChar((char)num)*/ ? "" : ncr;
            });
        }

        public static async void Download(string url, string thumbnail,
            bool isVideo = false, string username = null, string caption = null, bool sendNotify = false,
            bool story = false, bool userDownloader = false)
        {
            try
            {
                StorageFolder folder;
                if (story)
                    folder = await Helper.GetPictureFolderForStories();
                else
                    folder = await Helper.GetPictureFolder();
                var date = DateTime.UtcNow;

                //DateTime.Now.ToString("yyyy-dd-MMTh:mm:ss-0fffZ")
                var name =
                    $"{username?.ToUpper()}_IMG_{date.ToString("yyyyddMM_hmmssfff", CultureInfo.CurrentCulture)}.jpg";
                if (isVideo)
                    name =
                        $"{username?.ToUpper()}_VID_{date.ToString("yyyyddMM_hmmssfff", CultureInfo.CurrentCulture)}.mp4";

                var destinationFile = await folder.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);


                if (!string.IsNullOrEmpty(caption))
                    if (caption.Length > 110)
                        caption = caption.Substring(0, 108);
                if (caption == null)
                    caption = string.Empty;
                caption = ReplaceInvalidXmlCharacterReferences(caption);
                ToastNotification failed = null;
                ToastNotification success = null;
                if (userDownloader)
                {
                    failed = NotificationHelper.GetSingleUserNotify(username, thumbnail, "Download failed");

                    success = NotificationHelper.GetSuccessUserNotify(username, thumbnail);
                }
                else
                {
                    try
                    {
                        failed = NotificationHelper.GetFailedNotify(caption, thumbnail);
                    }
                    catch
                    {
                        failed = NotificationHelper.GetFailedNotify(null, thumbnail);
                    }

                    try
                    {
                        success = NotificationHelper.GetSuccessNotify(caption, thumbnail);
                    }
                    catch
                    {
                        success = NotificationHelper.GetSuccessNotify(null, thumbnail);
                    }
                }

                var downloader = new BackgroundDownloader
                {
                    FailureToastNotification = failed,
                    SuccessToastNotification = success
                };
                if (sendNotify)
                    MainPage.Current.ShowInAppNotify("Download started...", 1200);

                var download = downloader.CreateDownload(new Uri(url), destinationFile);
                await download.StartAsync().AsTask(new CancellationTokenSource().Token,
                    new Progress<DownloadOperation>(DownloadProgress));
            }
            catch (Exception ex)
            {
                ex.PrintException("Download");
            }
        }

        private static void DownloadProgress(DownloadOperation download)
        {
            // DownloadOperation.Progress is updated in real-time while the operation is ongoing. Therefore,
            // we must make a local copy so that we can have a consistent view of that ever-changing state
            // throughout this method's lifetime.
            var currentProgress = download.Progress;

            string.Format(CultureInfo.CurrentCulture, "Progress: {0}, Status: {1}", download.Guid,
                currentProgress.Status).PrintDebug();

            double percent = 100;
            if (currentProgress.TotalBytesToReceive > 0)
                percent = currentProgress.BytesReceived * 100 / currentProgress.TotalBytesToReceive;

            string.Format(
                CultureInfo.CurrentCulture,
                " - Transferred bytes: {0} of {1}, {2}%",
                currentProgress.BytesReceived,
                currentProgress.TotalBytesToReceive,
                percent).PrintDebug();

            if (currentProgress.HasRestarted) " - Download restarted".PrintDebug();

            if (currentProgress.HasResponseChanged)
            {
                // We have received new response headers from the server.
                // Be aware that GetResponseInformation() returns null for non-HTTP transfers (e.g., FTP).
                var response = download.GetResponseInformation();
                var headersCount = response != null ? response.Headers.Count : 0;

                (" - Response updated; Header count: " + headersCount).PrintDebug();

                // If you want to stream the response data this is a good time to start.
                // download.GetResultStreamAt(0);
            }
        }
    }
}
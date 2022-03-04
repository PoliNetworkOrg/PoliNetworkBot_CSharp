#region

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Minista.Helpers
{
    internal class StoryPhotoUploaderHelper
    {
        private CancellationTokenSource cts;
        private StorageFile NotifyFile;

        public StoryPhotoUploaderHelper()
        {
            Name = Guid.NewGuid().ToString();
        }

        public string Name { get; }

        public string UploadId { get; private set; }
        public string Caption { get; set; }

        internal string GenerateUploadId()
        {
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            var uploadId = (long)timeSpan.TotalMilliseconds;
            return uploadId.ToString();
        }

        private static Uri GetMediaConfigureUri()
        {
            return new Uri("https://i.instagram.com/api/v1/media/configure_to_story/", UriKind.RelativeOrAbsolute);
        }

        private static Uri GetUploadPhotoUri(string uploadId, int fileHashCode)
        {
            return new Uri(string.Format("https://i.instagram.com/rupload_igphoto/{0}_0_{1}", uploadId, fileHashCode),
                UriKind.RelativeOrAbsolute);
        }

        private string GetRetryContext()
        {
            return new JObject
            {
                { "num_step_auto_retry", 0 },
                { "num_reupload", 0 },
                { "num_step_manual_retry", 0 }
            }.ToString(Formatting.None);
        }

        public async void UploadSinglePhoto(StorageFile File, string caption)
        {
            Caption = caption;

            if (!string.IsNullOrEmpty(Caption))
                Caption = Caption.Replace("\r", "\n");

            UploadId = GenerateUploadId();
            try
            {
                var cacheFolder = await SessionHelper.LocalFolder.GetFolderAsync("Cache");
                NotifyFile = await cacheFolder.CreateFileAsync(15.GenerateRandomStringStatic() + ".jpg");
                NotifyFile = await File.CopyAsync(cacheFolder);
            }
            catch
            {
            }

            var photoHashCode = Path.GetFileName(File.Path ?? $"C:\\{13.GenerateRandomStringStatic()}.jpg")
                .GetHashCode();
            var photoEntityName = $"{UploadId}_0_{photoHashCode}";
            var instaUri = GetUploadPhotoUri(UploadId, photoHashCode);

            var device = InstaApi.GetCurrentDevice();
            var httpRequestProcessor = InstaApi.HttpRequestProcessor;

            var BGU = new BackgroundUploader();

            var cookies =
                httpRequestProcessor.HttpHandler.CookieContainer.GetCookies(httpRequestProcessor.Client.BaseAddress);
            var strCookies = string.Empty;
            foreach (Cookie cook in cookies)
                strCookies += $"{cook.Name}={cook.Value}; ";
            // header haye asli in ha hastan faghat
            BGU.SetRequestHeader("Cookie", strCookies);
            var r = InstaApi._httpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, device);
            foreach (var item in r.Headers) BGU.SetRequestHeader(item.Key, string.Join(" ", item.Value));


            var photoUploadParamsObj = new JObject
            {
                { "upload_id", UploadId },
                { "media_type", "1" },
                { "retry_context", GetRetryContext() },
                { "image_compression", "{\"lib_name\":\"moz\",\"lib_version\":\"3.1.m\",\"quality\":\"95\"}" }
            };
            var photoUploadParams = JsonConvert.SerializeObject(photoUploadParamsObj);
            var openedFile = await File.OpenAsync(FileAccessMode.Read);

            BGU.SetRequestHeader("X-Entity-Type", "image/jpeg");
            BGU.SetRequestHeader("Offset", "0");
            BGU.SetRequestHeader("X-Instagram-Rupload-Params", photoUploadParams);
            BGU.SetRequestHeader("X-Entity-Name", photoEntityName);
            BGU.SetRequestHeader("X-Entity-Length", openedFile.AsStream().Length.ToString());
            BGU.SetRequestHeader("X_FB_PHOTO_WATERFALL_ID", Guid.NewGuid().ToString());
            BGU.SetRequestHeader("Content-Transfer-Encoding", "binary");
            BGU.SetRequestHeader("Content-Type", "application/octet-stream");

            Debug.WriteLine("----------------------------------------Start upload----------------------------------");

            //var uploadX = await BGU.CreateUploadAsync(instaUri, parts, "", UploadId);
            var upload = BGU.CreateUpload(instaUri, File);
            //upload.Priority = BackgroundTransferPriority.High;
            upload.Start();
        }

        private async Task<IResult<bool>> ConfigurePhotoAsync()
        {
            try
            {
                Debug.WriteLine(
                    "----------------------------------------ConfigurePhotoAsync----------------------------------");
                var instaUri = GetMediaConfigureUri();
                var retryContext = GetRetryContext();
                var _user = InstaApi.GetLoggedUser();
                var _deviceInfo = InstaApi.GetCurrentDevice();
                var data0 = new JObject
                {
                    //{"date_time_digitalized", DateTime.UtcNow.ToString("yyyy:MM:dd+hh:mm:ss")},
                    //{"date_time_original", DateTime.UtcNow.ToString("yyyy:MM:dd+hh:mm:ss")},
                    //{"is_suggested_venue", "false"},
                    { "timezone_offset", InstaApi.GetTimezoneOffset().ToString() },
                    { "_csrftoken", _user.CsrfToken },
                    { "media_folder", "Camera" },
                    { "source_type", "4" },
                    { "_uid", _user.LoggedInUser.Pk.ToString() },
                    { "_uuid", _deviceInfo.DeviceGuid.ToString() },
                    { "device_id", _deviceInfo.DeviceId },
                    //{"caption", Caption ?? string.Empty},
                    { "upload_id", UploadId },
                    {
                        "device", new JObject
                        {
                            { "manufacturer", _deviceInfo.HardwareManufacturer },
                            { "model", _deviceInfo.DeviceModelIdentifier },
                            { "android_release", _deviceInfo.AndroidVer.VersionNumber },
                            { "android_version", int.Parse(_deviceInfo.AndroidVer.APILevel) }
                        }
                    },
                    {
                        "extra", new JObject
                        {
                            { "source_width", 0 },
                            { "source_height", 0 }
                        }
                    }
                };
                var rnd = new Random();
                var data = new JObject
                {
                    { "supported_capabilities_new", InstaApiConstants.SupportedCapabalities.ToString(Formatting.None) },
                    { "allow_multi_configures", "1" },
                    { "timezone_offset", InstaApi.GetTimezoneOffset().ToString() },
                    { "_csrftoken", _user.CsrfToken },
                    { "client_shared_at", (DateTime.UtcNow.ToUnixTime() - rnd.Next(10, 25)).ToString() },
                    { "configure_mode", "1" },
                    { "source_type", "3" },
                    { "_uid", _user.LoggedInUser.Pk.ToString() },
                    { "device_id", _deviceInfo.DeviceId },
                    { "_uuid", _deviceInfo.DeviceGuid.ToString() },
                    { "audience", "default" },
                    //{"caption", caption},
                    { "upload_id", UploadId },
                    { "client_timestamp", DateTime.UtcNow.ToUnixTime().ToString() },
                    //{"edits", new JObject()},
                    //{"disable_comments", false},
                    { "camera_position", "unknown" },
                    {
                        "device", new JObject
                        {
                            { "manufacturer", _deviceInfo.HardwareManufacturer },
                            { "model", _deviceInfo.DeviceModelIdentifier },
                            { "android_release", _deviceInfo.AndroidVer.VersionNumber },
                            { "android_version", _deviceInfo.AndroidVer.APILevel }
                        }
                    },
                    {
                        "extra", new JObject
                        {
                            { "source_width", 0 },
                            { "source_height", 0 }
                        }
                    }
                };


                var _httpRequestProcessor = InstaApi.HttpRequestProcessor;
                var request = InstaApi._httpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                request.Headers.Add("retry_context", GetRetryContext());
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                //var mediaResponse = JsonConvert.DeserializeObject<InstaMediaItemResponse>(json, new InstaMediaDataConverter());
                //var converter = ConvertersFabric.Instance.GetSingleMediaConverter(mediaResponse);
                //try
                //{
                //    var obj = converter.Convert();
                //    if (obj != null)
                //    {
                //        Views.Main.MainView.Current?.MainVM.PostsGenerator.Items.Insert(0, new InstaPost
                //        {
                //            Media = obj,
                //            Type = InstagramApiSharp.Enums.InstaFeedsType.Media
                //        });
                ShowNotify("Your story uploaded successfully...", 3500);
                //        NotificationHelper.ShowNotify(Caption?.Truncate(50), NotifyFile?.Path, "Media uploaded");
                //    }
                //}
                //catch { }
                SendNotify(102);
                RemoveThis();


                return Result.Success(true);
            }
            catch (HttpRequestException httpException)
            {
                SendNotify(102);
                RemoveThis();


                return Result.Fail(httpException, false, ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                SendNotify(102);
                RemoveThis();

                return Result.Fail<bool>(exception);
            }
        }

        private void RemoveThis()
        {
            try
            {
                var single = AppUploadHelper.SinglePhotoUploads.FirstOrDefault(s => s.Name == Name);
                if (single != null)
                    AppUploadHelper.SinglePhotoUploads.Remove(single);
            }
            catch
            {
            }
        }

        public void LogStatus(string Text)
        {
            Debug.WriteLine(Text);
        }

        private async void SendNotify(double value)
        {
            try
            {
                //if (MediaShare == null) return;
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //MediaShare.Percentage = value;
                });
            }
            catch
            {
            }
        }
    }
}
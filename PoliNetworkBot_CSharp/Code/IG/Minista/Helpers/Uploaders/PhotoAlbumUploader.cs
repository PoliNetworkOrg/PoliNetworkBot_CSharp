#region

using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;

//using static Helper;

#endregion

namespace Minista.Helpers
{
    internal class PhotoAlbumUploader
    {
        private readonly Random Rnd = new();
        private readonly List<string> Uploaded = new();
        private readonly Dictionary<string, SinglePhotoUploader> Uploads = new();
        public InstaApi instaApi;

        public PhotoAlbumUploader()
        {
            ReportCompleted += PhotoAlbumUploader_ReportCompleted;
        }

        public string Caption { get; set; }

        public event EventHandler<string> ReportCompleted;

        internal static string GenerateUploadId()
        {
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            var uploadId = (long)timeSpan.TotalMilliseconds;
            return uploadId.ToString();
        }

        private static Uri GetMediaConfigureUri()
        {
            return new Uri("https://i.instagram.com/api/v1/media/configure_sidecar/", UriKind.RelativeOrAbsolute);
        }

        public async Task SetFiles(StorageFile[] files, string caption, InstaApi instaApi)
        {
            Caption = caption;
            this.instaApi = instaApi;

            if (!string.IsNullOrEmpty(Caption))
                Caption = Caption.Replace("\r", "");
            foreach (var f in files)
            {
                var uploadId = GenerateUploadId();
                var b = new SinglePhotoUploader(this, instaApi);
                await b.UploadSinglePhotoAsync(f, uploadId, instaApi);
                Uploads.Add(uploadId, b);
                await Task.Delay(120);
            }
        }

        public void InvokeReport(string id)
        {
            ReportCompleted?.Invoke(this, id);
        }

        private async void PhotoAlbumUploader_ReportCompleted(object sender, string e)
        {
            if (!Uploaded.Contains(e))
                Uploaded.Add(e);

            if (Uploaded.Count == Uploads.Count)
            {
                await Task.Delay(10000);
                await ConfigurePhotoAsync();
            }
        }

        private async Task<IResult<bool>> ConfigurePhotoAsync()
        {
            try
            {
                Debug.WriteLine(
                    "----------------------------------------ConfigurePhotoAsync----------------------------------");
                var instaUri = GetMediaConfigureUri();
                var retryContext = GetRetryContext();
                var _user = instaApi.GetLoggedUser();
                var _deviceInfo = instaApi.GetCurrentDevice();
                var clientSidecarId = GenerateUploadId();
                var childrenArray = new JArray();

                foreach (var al in Uploads)
                    //if (al.Value.IsImage)
                    //    childrenArray.Add(_instaApi.HelperProcessor.GetImageConfigure(al.Key, al.Value.ImageToUpload));
                    //else if (al.Value.IsVideo)
                    //    childrenArray.Add(_instaApi.HelperProcessor.GetVideoConfigure(al.Key, al.Value.VideoToUpload));

                    childrenArray.Add(GetImageConfigure(al.Key));

                var data = new JObject
                {
                    { "_uuid", _deviceInfo.DeviceGuid.ToString() },
                    { "_uid", _user.LoggedInUser.Pk.ToString() },
                    { "_csrftoken", _user.CsrfToken },
                    { "caption", Caption },
                    { "client_sidecar_id", clientSidecarId },
                    { "upload_id", clientSidecarId },
                    { "timezone_offset", InstaApi.GetTimezoneOffset() },
                    { "source_type", "4" },
                    { "device_id", _deviceInfo.DeviceId },
                    { "creation_logger_session_id", Guid.NewGuid().ToString() },
                    {
                        "device", new JObject
                        {
                            { "manufacturer", _deviceInfo.HardwareManufacturer },
                            { "model", _deviceInfo.DeviceModelIdentifier },
                            { "android_release", _deviceInfo.AndroidVer.VersionNumber },
                            { "android_version", int.Parse(_deviceInfo.AndroidVer.APILevel) }
                        }
                    },
                    { "children_metadata", childrenArray }
                };
                //if (location != null)
                //{
                //    data.Add("location", location.GetJson());
                //    data.Add("date_time_digitalized", DateTime.Now.ToString("yyyy:dd:MM+h:mm:ss"));
                //}

                var _httpRequestProcessor = instaApi.HttpRequestProcessor;
                var request = instaApi.HttpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                request.Headers.Add("retry_context", GetRetryContext());
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                //SendCompleted();
                //IsUploading = false;
                return Result.Success(true);
            }
            catch (HttpRequestException httpException)
            {
                SendError(
                    $"HttpException thrown: {httpException.Message}\r\nSource: {httpException.Source}\r\nTrace: {httpException.StackTrace}");
                return Result.Fail(httpException, false, ResponseType.NetworkProblem);
            }
            catch (Exception exception)
            {
                SendError(
                    $"Exception thrown: {exception.Message}\r\nSource: {exception.Source}\r\nTrace: {exception.StackTrace}");
                return Result.Fail<bool>(exception);
            }
        }

        public JObject GetImageConfigure(string uploadId /*,InstaImageUpload image*/)
        {
            var _deviceInfo = instaApi.GetCurrentDevice();
            var imgData = new JObject
            {
                { "timezone_offset", InstaApi.GetTimezoneOffset() },
                { "source_type", "4" },
                { "upload_id", uploadId },
                { "caption", "" },
                {
                    "extra", JsonConvert.SerializeObject(new JObject
                    {
                        { "source_width", 0 },
                        { "source_height", 0 }
                    })
                },
                {
                    "device", JsonConvert.SerializeObject(new JObject
                    {
                        { "manufacturer", _deviceInfo.HardwareManufacturer },
                        { "model", _deviceInfo.DeviceModelIdentifier },
                        { "android_release", _deviceInfo.AndroidVer.VersionNumber },
                        { "android_version", _deviceInfo.AndroidVer.APILevel }
                    })
                }
            };
            //if (image.UserTags?.Count > 0)
            //{
            //    var tagArr = new JArray();
            //    foreach (var tag in image.UserTags)
            //    {
            //        if (tag.Pk != -1)
            //        {
            //            var position = new JArray(tag.X, tag.Y);
            //            var singleTag = new JObject
            //                        {
            //                            {"user_id", tag.Pk},
            //                            {"position", position}
            //                        };
            //            tagArr.Add(singleTag);
            //        }
            //    }

            //    var root = new JObject
            //    {
            //        {"in",  tagArr}
            //    };
            //    imgData.Add("usertags", root.ToString(Formatting.None));
            //}
            return imgData;
        }

        /*
        public JObject GetVideoConfigure(string uploadId, InstaVideoUpload video)
        {
            var vidData = new JObject
            {
                {"timezone_offset", InstaApiConstants.TIMEZONE_OFFSET.ToString()},
                {"caption", ""},
                {"upload_id", uploadId},
                {"date_time_original", DateTime.Now.ToString("yyyy-dd-MMTh:mm:ss-0fffZ")},
                {"source_type", "4"},
                {
                    "extra", JsonConvert.SerializeObject(new JObject
                    {
                        {"source_width", 0},
                        {"source_height", 0}
                    })
                },
                {
                    "clips", JsonConvert.SerializeObject(new JArray{
                        new JObject
                        {
                            {"length", video.Video.Length},
                            {"source_type", "4"},
                        }
                    })
                },
                {
                    "device", JsonConvert.SerializeObject(new JObject{
                        {"manufacturer", _deviceInfo.HardwareManufacturer},
                        {"model", _deviceInfo.DeviceModelIdentifier},
                        {"android_release", _deviceInfo.AndroidVer.VersionNumber},
                        {"android_version", _deviceInfo.AndroidVer.APILevel}
                    })
                },
                {"length", video.Video.Length.ToString()},
                {"poster_frame_index", "0"},
                {"audio_muted", "false"},
                {"filter_type", "0"},
                {"video_result", ""},
            };
            if (video.UserTags?.Count > 0)
            {
                var tagArr = new JArray();
                foreach (var tag in video.UserTags)
                {
                    if (tag.Pk != -1)
                    {
                        var position = new JArray(0.0, 0.0);
                        var singleTag = new JObject
                        {
                            {"user_id", tag.Pk},
                            {"position", position}
                        };
                        tagArr.Add(singleTag);
                    }
                }

                var root = new JObject
                {
                    {"in",  tagArr}
                };
                vidData.Add("usertags", root.ToString(Formatting.None));
            }
            return vidData;
        }
        */

        private static string GetRetryContext()
        {
            return new JObject
            {
                { "num_step_auto_retry", 0 },
                { "num_reupload", 0 },
                { "num_step_manual_retry", 0 }
            }.ToString(Formatting.None);
        }

        private static void SendError(string text)
        {
            try
            {
                //IsUploading = false;
                //OnError?.Invoke(this, text);
            }
            catch
            {
            }
        }
    }

    internal class SinglePhotoUploader
    {
        public static Random Rnd = new();
        private readonly PhotoAlbumUploader Album;

        private StorageFile File;
        private readonly InstaApi InstaApi;

        public SinglePhotoUploader(PhotoAlbumUploader album, InstaApi instaApi)
        {
            Album = album;
            InstaApi = instaApi;
        }

        public string UploadId { get; private set; }

#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

        private static Uri GetMediaConfigureUri()
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
        {
            return new Uri("https://i.instagram.com/api/v1/media/configure/", UriKind.RelativeOrAbsolute);
        }

        private static Uri GetUploadPhotoUri(string uploadId, int fileHashCode)
        {
            return new Uri(string.Format("https://i.instagram.com/rupload_igphoto/{0}_0_{1}", uploadId, fileHashCode),
                UriKind.RelativeOrAbsolute);
        }

        private static string GetRetryContext()
        {
            return new JObject
            {
                { "num_step_auto_retry", 0 },
                { "num_reupload", 0 },
                { "num_step_manual_retry", 0 }
            }.ToString(Formatting.None);
        }

        public async Task UploadSinglePhotoAsync(StorageFile file, string uploadId, InstaApi instaApi)
        {
            File = file;
            UploadId = uploadId;
            await UploadSinglePhoto(instaApi);
        }

        public static string GenerateRandomString(int length = 10)
        {
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            var chars = Enumerable.Range(0, length)
                .Select(x => pool[Rnd.Next(0, pool.Length)]);
            return "Telegram" + new string(chars.ToArray());
        }

        public async Task UploadSinglePhoto(InstaApi instaApi)
        {
            var photoHashCode = Path.GetFileName(File.Path ?? $"C:\\{GenerateRandomString(13)}.jpg").GetHashCode();
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
            var r = InstaApi.HttpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, device);
            foreach (var item in r.Headers) BGU.SetRequestHeader(item.Key, string.Join(' ', item.Value));

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
            var upload = BGU.CreateUpload(instaUri, File, instaApi);
            //upload.Priority = BackgroundTransferPriority.High;
            await upload.StartAsync();
        }
    }
}
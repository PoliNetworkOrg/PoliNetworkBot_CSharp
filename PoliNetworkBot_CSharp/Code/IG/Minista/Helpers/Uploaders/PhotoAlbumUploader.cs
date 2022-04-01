#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Minista.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.API;

//using static Helper;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.Minista.Helpers.Uploaders;

internal class PhotoAlbumUploader
{
    private readonly List<string> _uploaded = new();
    private readonly Dictionary<string, SinglePhotoUploader> _uploads = new();
    private InstaApi instaApi;

    public PhotoAlbumUploader()
    {
        ReportCompleted += PhotoAlbumUploader_ReportCompleted;
    }

    private string Caption { get; set; }

    public event EventHandler<string> ReportCompleted;

    private static string GenerateUploadId()
    {
        var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
        var uploadId = (long)timeSpan.TotalMilliseconds;
        return uploadId.ToString();
    }

    private static Uri GetMediaConfigureUri()
    {
        return new Uri("https://i.instagram.com/api/v1/media/configure_sidecar/", UriKind.RelativeOrAbsolute);
    }

    public async Task SetFiles(IEnumerable<StorageFile> files, string caption, InstaApi instaApi)
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
            _uploads.Add(uploadId, b);
            await Task.Delay(120);
        }
    }

    private async void PhotoAlbumUploader_ReportCompleted(object sender, string e)
    {
        if (!_uploaded.Contains(e))
            _uploaded.Add(e);

        if (_uploaded.Count != _uploads.Count) return;
        await Task.Delay(10000);
        await ConfigurePhotoAsync();
    }

    private async Task ConfigurePhotoAsync()
    {
        try
        {
            Debug.WriteLine(
                "----------------------------------------ConfigurePhotoAsync----------------------------------");
            var instaUri = GetMediaConfigureUri();
            var retryContext = GetRetryContext();
            var user = instaApi.GetLoggedUser();
            var deviceInfo = instaApi.GetCurrentDevice();
            var clientSidecarId = GenerateUploadId();
            var childrenArray = new JArray();

            foreach (var al in _uploads)
                //if (al.Value.IsImage)
                //    childrenArray.Add(_instaApi.HelperProcessor.GetImageConfigure(al.Key, al.Value.ImageToUpload));
                //else if (al.Value.IsVideo)
                //    childrenArray.Add(_instaApi.HelperProcessor.GetVideoConfigure(al.Key, al.Value.VideoToUpload));

                childrenArray.Add(GetImageConfigure(al.Key));

            var data = new JObject
            {
                { "_uuid", deviceInfo.DeviceGuid.ToString() },
                { "_uid", user.LoggedInUser.Pk.ToString() },
                { "_csrftoken", user.CsrfToken },
                { "caption", Caption },
                { "client_sidecar_id", clientSidecarId },
                { "upload_id", clientSidecarId },
                { "timezone_offset", InstaApi.GetTimezoneOffset() },
                { "source_type", "4" },
                { "device_id", deviceInfo.DeviceId },
                { "creation_logger_session_id", Guid.NewGuid().ToString() },
                {
                    "device", new JObject
                    {
                        { "manufacturer", deviceInfo.HardwareManufacturer },
                        { "model", deviceInfo.DeviceModelIdentifier },
                        { "android_release", deviceInfo.AndroidVer.VersionNumber },
                        { "android_version", int.Parse(deviceInfo.AndroidVer.APILevel) }
                    }
                },
                { "children_metadata", childrenArray }
            };
            //if (location != null)
            //{
            //    data.Add("location", location.GetJson());
            //    data.Add("date_time_digitalized", DateTime.Now.ToString("yyyy:dd:MM+h:mm:ss"));
            //}

            var httpRequestProcessor = instaApi.HttpRequestProcessor;
            var request = instaApi.HttpHelper.GetSignedRequest(instaUri, deviceInfo, data);
            request.Headers.Add("retry_context", GetRetryContext());
            var response = await httpRequestProcessor.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            //SendCompleted();
            //IsUploading = false;
            Result.Success(true);
        }
        catch (HttpRequestException httpException)
        {
            Result.Fail(httpException, false, ResponseType.NetworkProblem);
        }
        catch (Exception exception)
        {
            Result.Fail<bool>(exception);
        }
    }

    private JObject GetImageConfigure(string uploadId /*,InstaImageUpload image*/)
    {
        var deviceInfo = instaApi.GetCurrentDevice();
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
                    { "manufacturer", deviceInfo.HardwareManufacturer },
                    { "model", deviceInfo.DeviceModelIdentifier },
                    { "android_release", deviceInfo.AndroidVer.VersionNumber },
                    { "android_version", deviceInfo.AndroidVer.APILevel }
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
}

internal class SinglePhotoUploader
{
    private static readonly Random Rnd = new();
    private readonly InstaApi _instaApi;

    private StorageFile _file;

    public SinglePhotoUploader(PhotoAlbumUploader album, InstaApi instaApi)
    {
        _instaApi = instaApi;
    }

    private string UploadId { get; set; }

#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

    private static Uri GetMediaConfigureUri()
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
    {
        return new Uri("https://i.instagram.com/api/v1/media/configure/", UriKind.RelativeOrAbsolute);
    }

    private static Uri GetUploadPhotoUri(string uploadId, int fileHashCode)
    {
        return new Uri($"https://i.instagram.com/rupload_igphoto/{uploadId}_0_{fileHashCode}",
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
        _file = file;
        UploadId = uploadId;
        await UploadSinglePhoto(instaApi);
    }

    private static string GenerateRandomString(int length = 10)
    {
        const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
        var chars = Enumerable.Range(0, length)
            .Select(x => pool[Rnd.Next(0, pool.Length)]);
        return "Telegram" + new string(chars.ToArray());
    }

    private async Task UploadSinglePhoto(InstaApi instaApi)
    {
        var photoHashCode = Path.GetFileName(_file.Path ?? $"C:\\{GenerateRandomString(13)}.jpg").GetHashCode();
        var photoEntityName = $"{UploadId}_0_{photoHashCode}";
        var instaUri = GetUploadPhotoUri(UploadId, photoHashCode);

        var device = _instaApi.GetCurrentDevice();
        var httpRequestProcessor = _instaApi.HttpRequestProcessor;

        var bgu = new BackgroundUploader();

        var cookies =
            httpRequestProcessor.HttpHandler.CookieContainer.GetCookies(httpRequestProcessor.Client.BaseAddress);
        var strCookies = string.Empty;
        foreach (Cookie cook in cookies)
            strCookies += $"{cook.Name}={cook.Value}; ";
        // header haye asli in ha hastan faghat
        bgu.SetRequestHeader("Cookie", strCookies);
        var r = _instaApi.HttpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, device);
        foreach (var (key, value) in r.Headers) bgu.SetRequestHeader(key, string.Join(' ', value));

        var photoUploadParamsObj = new JObject
        {
            { "upload_id", UploadId },
            { "media_type", "1" },
            { "retry_context", GetRetryContext() },
            { "image_compression", "{\"lib_name\":\"moz\",\"lib_version\":\"3.1.m\",\"quality\":\"95\"}" }
        };
        var photoUploadParams = JsonConvert.SerializeObject(photoUploadParamsObj);
        var openedFile = await _file.OpenAsync(FileAccessMode.Read);

        bgu.SetRequestHeader("X-Entity-Type", "image/jpeg");
        bgu.SetRequestHeader("Offset", "0");
        bgu.SetRequestHeader("X-Instagram-Rupload-Params", photoUploadParams);
        bgu.SetRequestHeader("X-Entity-Name", photoEntityName);
        bgu.SetRequestHeader("X-Entity-Length", openedFile.Length.ToString());
        bgu.SetRequestHeader("X_FB_PHOTO_WATERFALL_ID", Guid.NewGuid().ToString());
        bgu.SetRequestHeader("Content-Transfer-Encoding", "binary");
        bgu.SetRequestHeader("Content-Type", "application/octet-stream");

        Debug.WriteLine("----------------------------------------Start upload----------------------------------");

        //var uploadX = await BGU.CreateUploadAsync(instaUri, parts, "", UploadId);
        var upload = bgu.CreateUpload(instaUri, _file, instaApi);
        //upload.Priority = BackgroundTransferPriority.High;
        await upload.StartAsync();
    }
}
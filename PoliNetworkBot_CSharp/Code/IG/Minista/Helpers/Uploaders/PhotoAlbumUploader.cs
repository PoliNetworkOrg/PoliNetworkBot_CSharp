#region

using Minista.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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
    }

    private string Caption { get; set; }

    private static string GenerateUploadId()
    {
        var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
        var uploadId = (long)timeSpan.TotalMilliseconds;
        return uploadId.ToString();
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
#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Versions;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using InstagramApiSharp.Enums;
using InstagramApiSharp.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.API;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Helpers;

public class HttpHelper
{
    private static readonly CultureInfo EnglishCulture = new("en-us");
    private readonly InstaApi _instaApi;
    private readonly IHttpRequestProcessor HttpRequestProcessor;
    private readonly Random Rnd = new();
    public InstaApiVersion _apiVersion;

    internal HttpHelper(InstaApiVersion apiVersionType, IHttpRequestProcessor httpRequestProcessor,
        InstaApi instaApi)
    {
        _apiVersion = apiVersionType;
        HttpRequestProcessor = httpRequestProcessor;
        _instaApi = instaApi;
    }

    private bool IsNewerApis => _instaApi.InstaApiVersionType > InstaApiVersionType.Version126;

    public HttpRequestMessage GetDefaultRequest(HttpMethod method, Uri uri, AndroidDevice deviceInfo)
    {
        var currentCulture = GetCurrentCulture();
#if !NET452
        CultureInfo.CurrentCulture = EnglishCulture;
#endif
        var userAgent = deviceInfo.GenerateUserAgent(_apiVersion);

        var request = new HttpRequestMessage(method, uri);
        var currentUser = _instaApi.GetLoggedUser();
        if (HttpRequestProcessor.Client
                .BaseAddress == null) return request;
        var cookies = HttpRequestProcessor.HttpHandler.CookieContainer.GetCookies(HttpRequestProcessor.Client
            .BaseAddress);
        var mid = currentUser.XMidHeader;
        var rur = currentUser.RurHeader;
        var dsUserId = cookies[InstaApiConstants.COOKIES_DS_USER_ID]?.Value ?? string.Empty;
        var shbid = cookies[InstaApiConstants.COOKIES_SHBID]?.Value ?? string.Empty;
        var shbts = cookies[InstaApiConstants.COOKIES_SHBTS]?.Value ?? string.Empty;
        var igDirectRegionHint = cookies[InstaApiConstants.COOKIES_IG_DIRECT_REGION_HINT]?.Value ?? string.Empty;

        request.Headers.Add(InstaApiConstants.HEADER_X_IG_APP_LOCALE,
            InstaApiConstants.ACCEPT_LANGUAGE.Replace("-", "_"));

        request.Headers.Add(InstaApiConstants.HEADER_X_IG_DEVICE_LOCALE,
            InstaApiConstants.ACCEPT_LANGUAGE.Replace("-", "_"));

        request.Headers.Add(InstaApiConstants.HEADER_X_IG_MAPPED_LOCALE,
            InstaApiConstants.ACCEPT_LANGUAGE.Replace("-", "_"));

        request.Headers.Add(InstaApiConstants.HEADER_PIGEON_SESSION_ID, deviceInfo.PigeonSessionId.ToString());

        request.Headers.Add(InstaApiConstants.HEADER_PIGEON_RAWCLINETTIME,
            $"{DateTime.UtcNow.ToUnixTime()}.0{Rnd.Next(10, 99)}");

        request.Headers.Add(InstaApiConstants.HEADER_X_IG_CONNECTION_SPEED, "-1kbps");

        request.Headers.Add(InstaApiConstants.HEADER_X_IG_BANDWIDTH_SPEED_KBPS, deviceInfo.IGBandwidthSpeedKbps);

        request.Headers.Add(InstaApiConstants.HEADER_X_IG_BANDWIDTH_TOTALBYTES_B,
            deviceInfo.IGBandwidthTotalBytesB);

        request.Headers.Add(InstaApiConstants.HEADER_X_IG_BANDWIDTH_TOTALTIME_MS,
            deviceInfo.IGBandwidthTotalTimeMS);

        request.Headers.Add(InstaApiConstants.HEADER_IG_APP_STARTUP_COUNTRY,
            InstaApiConstants.HEADER_IG_APP_STARTUP_COUNTRY_VALUE);

        request.Headers.Add(InstaApiConstants.HEADER_X_IG_BLOKS_VERSION_ID, _apiVersion.BloksVersionId);

        request.Headers.Add(InstaApiConstants.HEADER_X_IG_BLOKS_PANORAMA_ENABLED, "true");

        var wwwClaim = _instaApi.GetLoggedUser()?.WwwClaim;

        request.Headers.Add(InstaApiConstants.HEADER_X_WWW_CLAIM,
            !string.IsNullOrEmpty(wwwClaim) ? wwwClaim : InstaApiConstants.HEADER_X_WWW_CLAIM_DEFAULT);

        var authorization = _instaApi.GetLoggedUser()?.Authorization;

        if (IsLoggedIn())
            request.Headers.Add(InstaApiConstants.HEADER_AUTHORIZATION, authorization);

        request.Headers.Add(InstaApiConstants.HEADER_X_IG_BLOKS_IS_LAYOUT_RTL, "false");

        request.Headers.Add(InstaApiConstants.HEADER_X_IG_BLOKS_ENABLE_RENDERCODE, "false");

        request.Headers.Add(InstaApiConstants.HEADER_X_IG_DEVICE_ID, deviceInfo.DeviceGuid.ToString());

        request.Headers.Add(InstaApiConstants.HEADER_X_IG_ANDROID_ID, deviceInfo.DeviceId);

        request.Headers.Add(InstaApiConstants.HEADER_IG_CONNECTION_TYPE, InstaApiConstants.IG_CONNECTION_TYPE);

        request.Headers.Add(InstaApiConstants.HEADER_IG_CAPABILITIES, _apiVersion.Capabilities);

        request.Headers.Add(InstaApiConstants.HEADER_IG_APP_ID, InstaApiConstants.IG_APP_ID);

        request.Headers.Add(InstaApiConstants.HEADER_X_IG_TIGON_RETRY, "False");

        request.Headers.Add(InstaApiConstants.HEADER_USER_AGENT, userAgent);

        request.Headers.Add(InstaApiConstants.HEADER_ACCEPT_LANGUAGE, InstaApiConstants.ACCEPT_LANGUAGE);

        if (!string.IsNullOrEmpty(mid))
            request.Headers.Add(InstaApiConstants.HEADER_X_MID, mid);

        if (!string.IsNullOrEmpty(dsUserId) && !string.IsNullOrEmpty(authorization) &&
            !string.IsNullOrEmpty(igDirectRegionHint))
            request.Headers.Add(InstaApiConstants.HEADER_IG_U_DIRECT_REGION_HINT, igDirectRegionHint);

        if (!string.IsNullOrEmpty(dsUserId) && !string.IsNullOrEmpty(authorization) && !string.IsNullOrEmpty(shbid))
            request.Headers.Add(InstaApiConstants.HEADER_IG_U_SHBID, shbid);

        if (!string.IsNullOrEmpty(dsUserId) && !string.IsNullOrEmpty(authorization) && !string.IsNullOrEmpty(shbts))
            request.Headers.Add(InstaApiConstants.HEADER_IG_U_SHBTS, shbts);

        if (!string.IsNullOrEmpty(dsUserId) && !string.IsNullOrEmpty(authorization))
            request.Headers.Add(InstaApiConstants.HEADER_IG_U_DS_USER_ID, dsUserId);

        if (!string.IsNullOrEmpty(dsUserId) && !string.IsNullOrEmpty(authorization) && !string.IsNullOrEmpty(rur))
            request.Headers.Add(InstaApiConstants.HEADER_IG_U_RUR, rur);

        request.Headers.TryAddWithoutValidation(InstaApiConstants.HEADER_ACCEPT_ENCODING,
            InstaApiConstants.ACCEPT_ENCODING2);

        request.Headers.Add(InstaApiConstants.HOST, InstaApiConstants.HOST_URI);

        request.Headers.Add(InstaApiConstants.HEADER_X_FB_HTTP_ENGINE, "Liger");

        request.Headers.Add(InstaApiConstants.HEADER_X_FB_HTTP_IP, "True");

        request.Headers.Add(InstaApiConstants.HEADER_X_FB_SERVER_CLUSTER, "True");

#if !NET452
        CultureInfo.CurrentCulture = currentCulture;
#endif

        bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(authorization) && _instaApi.IsUserAuthenticated;
        }

        return request;
    }

    public HttpRequestMessage GetDefaultRequest(Uri uri, AndroidDevice deviceInfo,
        Dictionary<string, string> data)
    {
        var request = GetDefaultRequest(HttpMethod.Post, uri, deviceInfo);

        foreach (var (key, value) in data.ToDictionary(entry => entry.Key, entry => entry.Value))
            if (value.IsEmpty())
                data.Remove(key);

        request.Content = new FormUrlEncodedContent(data);
        return request;
    }

    /// <summary>
    ///     This is only for https://instagram.com site
    /// </summary>
    public HttpRequestMessage GetWebRequest(Uri uri, AndroidDevice deviceInfo)
    {
        var request = GetDefaultRequest(HttpMethod.Get, uri, deviceInfo);
        request.Headers.Remove(InstaApiConstants.HEADER_USER_AGENT);
        request.Headers.Add(InstaApiConstants.HEADER_USER_AGENT, InstaApiConstants.WEB_USER_AGENT);
        return request;
    }

    public HttpRequestMessage GetSignedRequest(Uri uri,
        AndroidDevice deviceInfo,
        Dictionary<string, string> data)
    {
        var payload = JsonConvert.SerializeObject(data, Formatting.None,
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

        var hash = CryptoHelper.CalculateHash(_apiVersion.SignatureKey,
            payload);

        var signature = $"{hash}.{payload}";

        var fields = new Dictionary<string, string>
        {
            { InstaApiConstants.HEADER_IG_SIGNATURE, signature }
        };
        if (!IsNewerApis)
            fields.Add(InstaApiConstants.HEADER_IG_SIGNATURE_KEY_VERSION,
                InstaApiConstants.IG_SIGNATURE_KEY_VERSION);
        var request = GetDefaultRequest(HttpMethod.Post, uri, deviceInfo);
        request.Content = new FormUrlEncodedContent(fields);
        request.Options.TryAdd(InstaApiConstants.HEADER_IG_SIGNATURE, signature);
        if (!IsNewerApis)
            request.Options.TryAdd(InstaApiConstants.HEADER_IG_SIGNATURE_KEY_VERSION,
                InstaApiConstants.IG_SIGNATURE_KEY_VERSION);

        return request;
    }

    public HttpRequestMessage GetSignedRequest(Uri uri,
        AndroidDevice deviceInfo,
        JObject data)
    {
        var payload = JsonConvert.SerializeObject(data, Formatting.None,
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        var hash = CryptoHelper.CalculateHash(_apiVersion.SignatureKey,
            payload);
        var signature = $"{(IsNewerApis ? _apiVersion.SignatureKey : hash)}.{payload}";
        var fields = new Dictionary<string, string>
        {
            { InstaApiConstants.HEADER_IG_SIGNATURE, signature }
        };
        if (!IsNewerApis)
            fields.Add(InstaApiConstants.HEADER_IG_SIGNATURE_KEY_VERSION,
                InstaApiConstants.IG_SIGNATURE_KEY_VERSION);
        var request = GetDefaultRequest(HttpMethod.Post, uri, deviceInfo);
        request.Content = new FormUrlEncodedContent(fields);
        request.Options.TryAdd(InstaApiConstants.HEADER_IG_SIGNATURE, signature);

        if (!IsNewerApis)
            request.Options.TryAdd(InstaApiConstants.HEADER_IG_SIGNATURE_KEY_VERSION,
                InstaApiConstants.IG_SIGNATURE_KEY_VERSION);
        return request;
    }

    public string GetSignature(JObject data)
    {
        var payload = JsonConvert.SerializeObject(data, Formatting.None,
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        var hash = CryptoHelper.CalculateHash(_apiVersion.SignatureKey, payload);
        var signature = $"{(IsNewerApis ? _apiVersion.SignatureKey : hash)}.{payload}";
        return signature;
    }

    private static CultureInfo GetCurrentCulture()
    {
        return CultureInfo.CurrentCulture;
    }
}
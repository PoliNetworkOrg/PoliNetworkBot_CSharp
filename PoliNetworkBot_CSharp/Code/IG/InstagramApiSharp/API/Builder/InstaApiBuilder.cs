#region

using System;
using System.Globalization;
using System.Net.Http;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using InstagramApiSharp.Classes.SessionHandlers;
using InstagramApiSharp.Enums;
using InstagramApiSharp.Logger;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.API;

#endregion

namespace InstagramApiSharp.API.Builder;

public class InstaApiBuilder
{
    private InstaApiVersionType? _apiVersionType;
    private IRequestDelay _delay = RequestDelay.Empty();
    private AndroidDevice _device;
    private HttpClient _httpClient;
    private HttpClientHandler _httpHandler = new();
    private IHttpRequestProcessor _httpRequestProcessor;
    private IInstaLogger _logger;
    private ApiRequestMessage _requestMessage;
    private ISessionHandler _sessionHandler;
    private UserSessionData _user;

    private InstaApiBuilder()
    {
    }

    /// <summary>
    ///     Create new API instance
    /// </summary>
    /// <returns>
    ///     API instance
    /// </returns>
    /// <exception cref="ArgumentNullException">User auth data must be specified</exception>
    public InstaApi Build()
    {
        _user ??= UserSessionData.Empty;

        _httpHandler ??= new HttpClientHandler();

        _httpClient ??= new HttpClient(_httpHandler) { BaseAddress = new Uri(InstaApiConstants.INSTAGRAM_URL) };

        if (_requestMessage == null)
        {
            _device ??= AndroidDeviceGenerator.GetRandomAndroidDevice();
            _requestMessage = new ApiRequestMessage
            {
                PhoneId = _device.PhoneGuid.ToString(),
                Guid = _device.DeviceGuid,
                Password = _user?.Password,
                Username = _user?.UserName,
                DeviceId = ApiRequestMessage.GenerateDeviceId(),
                AdId = _device.AdId.ToString()
            };
        }

        if (string.IsNullOrEmpty(_requestMessage.Password)) _requestMessage.Password = _user?.Password;
        if (string.IsNullOrEmpty(_requestMessage.Username)) _requestMessage.Username = _user?.UserName;

        try
        {
            InstaApiConstants.TIMEZONE_OFFSET =
                int.Parse(DateTimeOffset.Now.Offset.TotalSeconds.ToString(CultureInfo.InvariantCulture));
        }
        catch
        {
        }

        _httpRequestProcessor ??=
            new HttpRequestProcessor(_delay, _httpClient, _httpHandler, _requestMessage, _logger);

        _apiVersionType ??= InstaApiVersionType.Version180;

        var instaApi = new InstaApi(_user, _logger, _device, _httpRequestProcessor, _apiVersionType.Value);
        if (_sessionHandler == null) return instaApi;
        _sessionHandler.InstaApi = instaApi;
        instaApi.SessionHandler = _sessionHandler;

        return instaApi;
    }

    /// <summary>
    ///     Use custom logger
    /// </summary>
    /// <param name="logger">IInstaLogger implementation</param>
    /// <returns>
    ///     API Builder
    /// </returns>
    public InstaApiBuilder UseLogger(IInstaLogger logger)
    {
        _logger = logger;
        return this;
    }

    /// <summary>
    ///     Set specific HttpClient
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <returns>
    ///     API Builder
    /// </returns>
    public InstaApiBuilder UseHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        return this;
    }

    /// <summary>
    ///     Set custom HttpClientHandler to be able to use certain features, e.g Proxy and so on
    /// </summary>
    /// <param name="handler">HttpClientHandler</param>
    /// <returns>
    ///     API Builder
    /// </returns>
    public InstaApiBuilder UseHttpClientHandler(HttpClientHandler handler)
    {
        _httpHandler = handler;
        return this;
    }

    /// <summary>
    ///     Specify user login, password from here
    /// </summary>
    /// <param name="user">User auth data</param>
    /// <returns>
    ///     API Builder
    /// </returns>
    public InstaApiBuilder SetUser(UserSessionData user)
    {
        _user = user;
        return this;
    }

    /// <summary>
    ///     Set custom request message. Used to be able to customize device info.
    /// </summary>
    /// <param name="requestMessage">Custom request message object</param>
    /// <returns>
    ///     API Builder
    /// </returns>
    /// <remarks>
    ///     Please, do not use if you don't know what you are doing
    /// </remarks>
    public InstaApiBuilder SetApiRequestMessage(ApiRequestMessage requestMessage)
    {
        _requestMessage = requestMessage;
        return this;
    }

    /// <summary>
    ///     Set delay between requests. Useful when API supposed to be used for mass-bombing.
    /// </summary>
    /// <param name="delay">Timespan delay</param>
    /// <returns>
    ///     API Builder
    /// </returns>
    public InstaApiBuilder SetRequestDelay(IRequestDelay delay)
    {
        delay ??= RequestDelay.Empty();
        _delay = delay;
        return this;
    }

    /// <summary>
    ///     Set custom android device.
    ///     <para>Note: this is optional, if you didn't set this, InstagramApiSharp will choose random device.</para>
    /// </summary>
    /// <param name="androidDevice">Android device</param>
    /// <returns>
    ///     API Builder
    /// </returns>
    public InstaApiBuilder SetDevice(AndroidDevice androidDevice)
    {
        _device = androidDevice;
        return this;
    }

    /// <summary>
    ///     Set instagram api version (for user agent version)
    /// </summary>
    /// <param name="apiVersion">Api version</param>
    /// <returns>
    ///     API Builder
    /// </returns>
    public InstaApiBuilder SetApiVersion(InstaApiVersionType apiVersion)
    {
        _apiVersionType = apiVersion;
        return this;
    }

    /// <summary>
    ///     Set session handler
    /// </summary>
    /// <param name="sessionHandler">Session handler</param>
    /// <returns>
    ///     API Builder
    /// </returns>
    public InstaApiBuilder SetSessionHandler(ISessionHandler sessionHandler)
    {
        _sessionHandler = sessionHandler;
        return this;
    }

    /// <summary>
    ///     Set Http request processor
    /// </summary>
    /// <param name="httpRequestProcessor">HttpRequestProcessor</param>
    /// <returns>
    ///     API Builder
    /// </returns>
    public InstaApiBuilder SetHttpRequestProcessor(IHttpRequestProcessor httpRequestProcessor)
    {
        _httpRequestProcessor = httpRequestProcessor;
        return this;
    }

    /// <summary>
    ///     Creates the builder.
    /// </summary>
    /// <returns>
    ///     API Builder
    /// </returns>
    public static InstaApiBuilder CreateBuilder()
    {
        return new InstaApiBuilder();
    }
}
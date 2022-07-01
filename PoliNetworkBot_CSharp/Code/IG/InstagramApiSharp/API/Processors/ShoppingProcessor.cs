﻿#region

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Converters;
using InstagramApiSharp.Converters.Json;
using InstagramApiSharp.Helpers;
using InstagramApiSharp.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.API;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.ResponseWrappers.Media;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Helpers;

#endregion

namespace InstagramApiSharp.API.Processors;

/// <summary>
///     Shopping and commerce api functions.
/// </summary>
internal class ShoppingProcessor : IShoppingProcessor
{
    /// <summary>
    ///     Get all user shoppable media by username
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="paginationParameters">Pagination parameters: next id and max amount of pages to load</param>
    /// <returns>
    ///     <see cref="InstaMediaList" />
    /// </returns>
    public async Task<IResult<InstaMediaList>> GetUserShoppableMediaAsync(string? username,
        PaginationParameters paginationParameters)
    {
        UserAuthValidator.Validate(_userAuthValidate);
        var user = await _instaApi.UserProcessor.GetUserAsync(username);
        if (!user.Succeeded)
            return Result.Fail<InstaMediaList>("Unable to get user to load shoppable media");
        return await GetUserShoppableMedia(user.Value.Pk, paginationParameters);
    }

    /// <summary>
    ///     Get all user shoppable media by user id (pk)
    /// </summary>
    /// <param name="userId">User id (pk)</param>
    /// <param name="paginationParameters">Pagination parameters: next id and max amount of pages to load</param>
    /// <returns>
    ///     <see cref="InstaMediaList" />
    /// </returns>
    public async Task<IResult<InstaMediaList>> GetUserShoppableMediaByIdAsync(long userId,
        PaginationParameters paginationParameters)
    {
        return await GetUserShoppableMedia(userId, paginationParameters);
    }

    /// <summary>
    ///     Get product info
    /// </summary>
    /// <param name="productId">Product id (get it from <see cref="InstaProduct.ProductId" /> )</param>
    /// <param name="mediaPk">Media Pk (get it from <see cref="InstaMedia.Pk" />)</param>
    /// <param name="deviceWidth">Device width (pixel)</param>
    public async Task<IResult<InstaProductInfo>> GetProductInfoAsync(long productId, string mediaPk,
        int deviceWidth = 720)
    {
        try
        {
            var instaUri = UriCreator.GetProductInfoUri(productId, mediaPk, deviceWidth);
            var request = _httpHelper.GetDefaultRequest(HttpMethod.Get, instaUri, _deviceInfo);
            var response = await _httpRequestProcessor.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
                return Result.UnExpectedResponse<InstaProductInfo>(response, json);

            var productInfoResponse = JsonConvert.DeserializeObject<InstaProductInfoResponse>(json);
            var converted = ConvertersFabric.GetProductInfoConverter(productInfoResponse).Convert();

            return Result.Success(converted);
        }
        catch (HttpRequestException httpException)
        {
            _logger?.LogException(httpException);
            return Result.Fail(httpException, default(InstaProductInfo), ResponseType.NetworkProblem);
        }
        catch (Exception exception)
        {
            _logger?.LogException(exception);
            return Result.Fail<InstaProductInfo>(exception);
        }
    }

    public async Task<IResult<InstaProductInfo>> GetCatalogsAsync()
    {
        try
        {
            var instaUri =
                new Uri(
                    $"https://i.instagram.com/api/v1/wwwgraphql/ig/query/?locale={InstaApiConstants.ACCEPT_LANGUAGE.Replace("-", "_")}");

            var sources = new JObject
            {
                { "sources", null }
            };

            var data = new Dictionary<string, string?>
            {
                { "access_token", "undefined" },
                { "fb_api_caller_class", "RelayModern" },
                { "variables", sources.ToString(Formatting.Indented) },
                { "doc_id", "1742970149122229" }
            };

            var request = _httpHelper.GetDefaultRequest(instaUri, _deviceInfo, data);
            var response = await _httpRequestProcessor.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            //{"data":{"me":{"taggable_catalogs":{"edges":[],"page_info":{"has_next_page":false,"end_cursor":null}},"id":"17841407343005740"}}}
            if (response.StatusCode != HttpStatusCode.OK)
                return Result.UnExpectedResponse<InstaProductInfo>(response, json);

            var productInfoResponse = JsonConvert.DeserializeObject<InstaProductInfoResponse>(json);
            var converted = ConvertersFabric.GetProductInfoConverter(productInfoResponse).Convert();

            return Result.Success(converted);
        }
        catch (HttpRequestException httpException)
        {
            _logger?.LogException(httpException);
            return Result.Fail(httpException, default(InstaProductInfo), ResponseType.NetworkProblem);
        }
        catch (Exception exception)
        {
            _logger?.LogException(exception);
            return Result.Fail<InstaProductInfo>(exception);
        }
    }

    private async Task<IResult<InstaMediaList>> GetUserShoppableMedia(long userId,
        PaginationParameters paginationParameters)
    {
        var mediaList = new InstaMediaList();
        try
        {
            paginationParameters ??= PaginationParameters.MaxPagesToLoad(1);

            static InstaMediaList Convert(InstaMediaListResponse mediaListResponse)
            {
                return ConvertersFabric.GetMediaListConverter(mediaListResponse).Convert();
            }

            var mediaResult = await GetShoppableMedia(userId, paginationParameters);
            if (!mediaResult.Succeeded)
                return Result.Fail(mediaResult.Info,
                    mediaResult.Value != null ? Convert(mediaResult.Value) : mediaList);

            var mediaResponse = mediaResult.Value;
            mediaList = ConvertersFabric.GetMediaListConverter(mediaResponse).Convert();
            mediaList.NextMaxId = paginationParameters.NextMaxId = mediaResponse.NextMaxId;
            paginationParameters.PagesLoaded++;

            while (mediaResponse.MoreAvailable
                   && !string.IsNullOrEmpty(paginationParameters.NextMaxId)
                   && paginationParameters.PagesLoaded < paginationParameters.MaximumPagesToLoad)
            {
                var nextMedia = await GetShoppableMedia(userId, paginationParameters);
                if (!nextMedia.Succeeded)
                    return Result.Fail(nextMedia.Info, mediaList);
                mediaList.NextMaxId = paginationParameters.NextMaxId = nextMedia.Value.NextMaxId;
                mediaList.AddRange(Convert(nextMedia.Value));
                mediaResponse.MoreAvailable = nextMedia.Value.MoreAvailable;
                mediaResponse.ResultsCount += nextMedia.Value.ResultsCount;
                paginationParameters.PagesLoaded++;
            }

            mediaList.Pages = paginationParameters.PagesLoaded;
            mediaList.PageSize = mediaResponse.ResultsCount;
            return Result.Success(mediaList);
        }
        catch (HttpRequestException httpException)
        {
            _logger?.LogException(httpException);
            return Result.Fail(httpException, default(InstaMediaList), ResponseType.NetworkProblem);
        }
        catch (Exception exception)
        {
            _logger?.LogException(exception);
            return Result.Fail(exception, mediaList);
        }
    }

    private async Task<IResult<InstaMediaListResponse>> GetShoppableMedia(long userId,
        PaginationParameters paginationParameters)
    {
        try
        {
            var instaUri = UriCreator.GetUserShoppableMediaListUri(userId, paginationParameters.NextMaxId);
            var request = _httpHelper.GetDefaultRequest(HttpMethod.Get, instaUri, _deviceInfo);
            var response = await _httpRequestProcessor.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
                return Result.UnExpectedResponse<InstaMediaListResponse>(response, json);

            var mediaResponse = JsonConvert.DeserializeObject<InstaMediaListResponse>(json,
                new InstaMediaListDataConverter());

            return Result.Success(mediaResponse);
        }
        catch (HttpRequestException httpException)
        {
            _logger?.LogException(httpException);
            return Result.Fail(httpException, default(InstaMediaListResponse), ResponseType.NetworkProblem);
        }
        catch (Exception exception)
        {
            _logger?.LogException(exception);
            return Result.Fail(exception, default(InstaMediaListResponse));
        }
    }

    #region Properties and constructor

    private readonly AndroidDevice? _deviceInfo;
    private readonly IHttpRequestProcessor? _httpRequestProcessor;
    private readonly IInstaLogger? _logger;
    private readonly UserSessionData? _user;
    private readonly UserAuthValidate _userAuthValidate;
    private readonly InstaApi _instaApi;
    private readonly HttpHelper _httpHelper;

    public ShoppingProcessor(AndroidDevice? deviceInfo, UserSessionData? user,
        IHttpRequestProcessor? httpRequestProcessor, IInstaLogger? logger,
        UserAuthValidate userAuthValidate, InstaApi instaApi, HttpHelper httpHelper)
    {
        _deviceInfo = deviceInfo;
        _user = user;
        _httpRequestProcessor = httpRequestProcessor;
        _logger = logger;
        _userAuthValidate = userAuthValidate;
        _instaApi = instaApi;
        _httpHelper = httpHelper;
    }

    #endregion Properties and constructor
}
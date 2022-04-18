﻿#region

using System;
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

#endregion

namespace InstagramApiSharp.API.Processors;

/// <summary>
///     Collection api functions.
/// </summary>
public class CollectionProcessor
{
    private readonly AndroidDevice _deviceInfo;
    private readonly HttpHelper _httpHelper;
    private readonly IHttpRequestProcessor _httpRequestProcessor;
    private readonly InstaApi _instaApi;
    private readonly IInstaLogger _logger;
    private readonly UserSessionData _user;
    private readonly UserAuthValidate _userAuthValidate;

    public CollectionProcessor(AndroidDevice deviceInfo, UserSessionData user,
        IHttpRequestProcessor httpRequestProcessor, IInstaLogger logger,
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

    /// <summary>
    ///     Adds items to collection asynchronous.
    /// </summary>
    /// <param name="collectionId">Collection identifier.</param>
    /// <param name="mediaIds">Media id list.</param>
    public async Task<IResult<InstaCollectionItem>> AddItemsToCollectionAsync(long collectionId,
        params string[] mediaIds)
    {
        UserAuthValidator.Validate(_userAuthValidate);
        try
        {
            if (mediaIds?.Length < 1)
                return Result.Fail<InstaCollectionItem>("Provide at least one media id to add to collection");
            var editCollectionUri = UriCreator.GetEditCollectionUri(collectionId);

            var data = new JObject
            {
                { "module_name", InstaApiConstants.FEED_SAVED_ADD_TO_COLLECTION_MODULE },
                { "added_media_ids", JsonConvert.SerializeObject(mediaIds) },
                { "radio_type", "wifi-none" },
                { "_uuid", _deviceInfo.DeviceGuid.ToString() },
                { "_uid", _user.LoggedInUser.Pk },
                { "_csrftoken", _user.CsrfToken }
            };

            var request =
                _httpHelper.GetSignedRequest(editCollectionUri, _deviceInfo, data);
            var response = await _httpRequestProcessor.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
                return Result.UnExpectedResponse<InstaCollectionItem>(response, json);
            var newCollectionResponse = JsonConvert.DeserializeObject<InstaCollectionItemResponse>(json);
            var converter = ConvertersFabric.GetCollectionConverter(newCollectionResponse);
            return Result.Success(converter.Convert());
        }
        catch (HttpRequestException httpException)
        {
            _logger?.LogException(httpException);
            return Result.Fail(httpException, default(InstaCollectionItem), ResponseType.NetworkProblem);
        }
        catch (Exception exception)
        {
            _logger?.LogException(exception);
            return Result.Fail<InstaCollectionItem>(exception);
        }
    }

    /// <summary>
    ///     Create a new collection
    /// </summary>
    /// <param name="collectionName">The name of the new collection</param>
    /// <returns>
    ///     <see cref="T:InstagramApiSharp.Classes.Models.InstaCollectionItem" />
    /// </returns>
    public async Task<IResult<InstaCollectionItem>> CreateCollectionAsync(string collectionName)
    {
        UserAuthValidator.Validate(_userAuthValidate);
        try
        {
            var createCollectionUri = UriCreator.GetCreateCollectionUri();

            var data = new JObject
            {
                { "_uuid", _deviceInfo.DeviceGuid.ToString() },
                { "_uid", _user.LoggedInUser.Pk },
                { "_csrftoken", _user.CsrfToken },
                { "name", collectionName },
                { "module_name", InstaApiConstants.COLLECTION_CREATE_MODULE }
            };

            var request =
                _httpHelper.GetSignedRequest(createCollectionUri, _deviceInfo, data);
            var response = await _httpRequestProcessor.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            var newCollectionResponse = JsonConvert.DeserializeObject<InstaCollectionItemResponse>(json);
            var converter = ConvertersFabric.GetCollectionConverter(newCollectionResponse);

            return response.StatusCode != HttpStatusCode.OK
                ? Result.UnExpectedResponse<InstaCollectionItem>(response, json)
                : Result.Success(converter.Convert());
        }
        catch (HttpRequestException httpException)
        {
            _logger?.LogException(httpException);
            return Result.Fail(httpException, default(InstaCollectionItem), ResponseType.NetworkProblem);
        }
        catch (Exception exception)
        {
            _logger?.LogException(exception);
            return Result.Fail<InstaCollectionItem>(exception);
        }
    }

    /// <summary>
    ///     Delete your collection for given collection id
    /// </summary>
    /// <param name="collectionId">Collection ID to delete</param>
    /// <returns>true if succeed</returns>
    public async Task<IResult<bool>> DeleteCollectionAsync(long collectionId)
    {
        UserAuthValidator.Validate(_userAuthValidate);
        try
        {
            var createCollectionUri = UriCreator.GetDeleteCollectionUri(collectionId);

            var data = new JObject
            {
                { "_uuid", _deviceInfo.DeviceGuid.ToString() },
                { "_uid", _user.LoggedInUser.Pk },
                { "_csrftoken", _user.CsrfToken },
                { "module_name", "collection_editor" }
            };

            var request =
                _httpHelper.GetSignedRequest(createCollectionUri, _deviceInfo, data);
            var response = await _httpRequestProcessor.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            return response.StatusCode == HttpStatusCode.OK
                ? Result.Success(true)
                : Result.UnExpectedResponse<bool>(response, json);
        }
        catch (HttpRequestException httpException)
        {
            _logger?.LogException(httpException);
            return Result.Fail(httpException, default(bool), ResponseType.NetworkProblem);
        }
        catch (Exception exception)
        {
            _logger?.LogException(exception);
            return Result.Fail(exception.Message, false);
        }
    }

    /// <summary>
    ///     Edit a collection
    /// </summary>
    /// <param name="collectionId">Collection ID to edit</param>
    /// <param name="name">New name for giving collection (set null if you don't want to change it)</param>
    /// <param name="photoCoverMediaId">
    ///     New photo cover media Id (get it from <see cref="InstaMedia.InstaIdentifier" />) => Optional
    ///     <para>Important note: media id must be exists in giving collection!</para>
    /// </param>
    public async Task<IResult<InstaCollectionItem>> EditCollectionAsync(long collectionId, string name,
        string photoCoverMediaId = null)
    {
        UserAuthValidator.Validate(_userAuthValidate);
        try
        {
            var collection = await GetSingleCollection(collectionId, PaginationParameters.MaxPagesToLoad(1));
            if (collection.Succeeded && string.IsNullOrEmpty(name))
                name = collection.Value.CollectionName;

            var editCollectionUri = UriCreator.GetEditCollectionUri(collectionId);

            var data = new JObject
            {
                { "name", name ?? string.Empty },
                { "_uuid", _deviceInfo.DeviceGuid.ToString() },
                { "_uid", _user.LoggedInUser.Pk },
                { "_csrftoken", _user.CsrfToken }
            };
            if (!string.IsNullOrEmpty(photoCoverMediaId))
                data.Add("cover_media_id", photoCoverMediaId);

            var request =
                _httpHelper.GetSignedRequest(editCollectionUri, _deviceInfo, data);
            var response = await _httpRequestProcessor.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
                return Result.UnExpectedResponse<InstaCollectionItem>(response, json);
            var newCollectionResponse = JsonConvert.DeserializeObject<InstaCollectionItemResponse>(json);
            var converter = ConvertersFabric.GetCollectionConverter(newCollectionResponse);
            return Result.Success(converter.Convert());
        }
        catch (HttpRequestException httpException)
        {
            _logger?.LogException(httpException);
            return Result.Fail(httpException, default(InstaCollectionItem), ResponseType.NetworkProblem);
        }
        catch (Exception exception)
        {
            _logger?.LogException(exception);
            return Result.Fail<InstaCollectionItem>(exception);
        }
    }

    /// <summary>
    ///     Get your collection for given collection id
    /// </summary>
    /// <param name="collectionId">Collection ID</param>
    /// <param name="paginationParameters">Pagination parameters: next max id and max amount of pages to load</param>
    /// <returns>
    ///     <see cref="T:InstagramApiSharp.Classes.Models.InstaCollectionItem" />
    /// </returns>
    public async Task<IResult<InstaCollectionItem>> GetSingleCollectionAsync(long collectionId,
        PaginationParameters paginationParameters)
    {
        UserAuthValidator.Validate(_userAuthValidate);
        try
        {
            paginationParameters ??= PaginationParameters.MaxPagesToLoad(1);

            static InstaCollectionItem Convert(InstaCollectionItemResponse instaCollectionItemResponse)
            {
                return ConvertersFabric.GetCollectionConverter(instaCollectionItemResponse).Convert();
            }

            var collectionList = await GetSingleCollection(collectionId, paginationParameters);
            if (!collectionList.Succeeded)
                return Result.Fail(collectionList.Info, default(InstaCollectionItem));

            var collectionsListResponse = collectionList.Value;
            paginationParameters.NextMaxId = collectionsListResponse.NextMaxId;
            var pagesLoaded = 1;

            while (collectionsListResponse.MoreAvailable
                   && !string.IsNullOrEmpty(collectionsListResponse.NextMaxId)
                   && pagesLoaded < paginationParameters.MaximumPagesToLoad)
            {
                var nextCollectionList = await GetSingleCollection(collectionId, paginationParameters);

                if (!nextCollectionList.Succeeded)
                    return Result.Fail(nextCollectionList.Info, Convert(nextCollectionList.Value));

                collectionsListResponse.NextMaxId =
                    paginationParameters.NextMaxId = nextCollectionList.Value.NextMaxId;
                collectionsListResponse.MoreAvailable = nextCollectionList.Value.MoreAvailable;
                collectionsListResponse.AutoLoadMoreEnabled = nextCollectionList.Value.AutoLoadMoreEnabled;
                collectionsListResponse.Status = nextCollectionList.Value.Status;
                collectionsListResponse.Media.Medias.AddRange(nextCollectionList.Value.Media.Medias);
                pagesLoaded++;
            }

            var converter = ConvertersFabric.GetCollectionConverter(collectionsListResponse);
            return Result.Success(converter.Convert());
        }
        catch (HttpRequestException httpException)
        {
            _logger?.LogException(httpException);
            return Result.Fail(httpException, default(InstaCollectionItem), ResponseType.NetworkProblem);
        }
        catch (Exception exception)
        {
            _logger?.LogException(exception);
            return Result.Fail<InstaCollectionItem>(exception);
        }
    }

    private async Task<IResult<InstaCollectionItemResponse>> GetSingleCollection(long collectionId,
        PaginationParameters paginationParameters)
    {
        UserAuthValidator.Validate(_userAuthValidate);
        try
        {
            var collectionUri = UriCreator.GetCollectionUri(collectionId, paginationParameters?.NextMaxId);
            var request = _httpHelper.GetDefaultRequest(HttpMethod.Get, collectionUri, _deviceInfo);
            var response = await _httpRequestProcessor.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
                return Result.UnExpectedResponse<InstaCollectionItemResponse>(response, json);

            var collectionsListResponse =
                JsonConvert.DeserializeObject<InstaCollectionItemResponse>(json,
                    new InstaCollectionDataConverter());
            return Result.Success(collectionsListResponse);
        }
        catch (HttpRequestException httpException)
        {
            _logger?.LogException(httpException);
            return Result.Fail(httpException, default(InstaCollectionItemResponse), ResponseType.NetworkProblem);
        }
        catch (Exception exception)
        {
            _logger?.LogException(exception);
            return Result.Fail<InstaCollectionItemResponse>(exception);
        }
    }
}
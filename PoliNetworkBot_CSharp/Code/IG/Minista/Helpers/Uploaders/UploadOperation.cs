#region

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Minista.Helpers;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.API;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.Minista.Helpers.Uploaders;

internal class UploadOperation
{
    private readonly BackgroundUploader _backgroundUploader;
    private readonly InstaApi _instaApi;
    private readonly Uri _instaUri;


    public UploadOperation(Uri instaUri, BackgroundUploader backgroundUploader, InstaApi instaApi)
    {
        _instaUri = instaUri;
        _backgroundUploader = backgroundUploader;
        _instaApi = instaApi;
    }

    internal async Task StartAsync()
    {
        try
        {
            var client = _instaApi.HttpClient;

            var c = ToDictionary(_backgroundUploader.list);
            var content = new FormUrlEncodedContent(c);

            var response = await client.PostAsync(_instaUri, content);
            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseString);

            var request = _instaApi.HttpHelper.GetDefaultRequest(_instaUri, _instaApi._deviceInfo, c);
            var response2 = await _instaApi.HttpRequestProcessor.SendAsync(request);
            var json = await response2.Content.ReadAsStringAsync();
            Console.WriteLine(json);
        }
        catch
        {
            // ignored
        }
    }

    private static Dictionary<string, string?> ToDictionary(List<Tuple<string, string>> list)
    {
        Dictionary<string, string?> r = new();
        foreach (var (item1, item2) in list) r[item1] = item2;

        return r;
    }
}
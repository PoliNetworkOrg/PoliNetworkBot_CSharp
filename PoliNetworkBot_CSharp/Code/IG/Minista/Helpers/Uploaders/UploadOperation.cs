#region

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.API;

#endregion

namespace Minista.Helpers;

internal class UploadOperation
{
    private readonly BackgroundUploader backgroundUploader;
    private readonly StorageFile file;
    private readonly InstaApi instaApi;
    private readonly Uri instaUri;
    internal string Guid = null;

    public UploadOperation(Uri instaUri, StorageFile file, BackgroundUploader backgroundUploader, InstaApi instaApi)
    {
        this.instaUri = instaUri;
        this.file = file;
        this.backgroundUploader = backgroundUploader;
        this.instaApi = instaApi;
    }

    internal async Task StartAsync()
    {
        ;

        try
        {
            ;
            var client = instaApi.HttpClient;

            var c = ToDictionary(backgroundUploader.list);
            var content = new FormUrlEncodedContent(c);

            var response = await client.PostAsync(instaUri, content);
            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseString);
            ;

            var request = instaApi.HttpHelper.GetDefaultRequest(instaUri, instaApi._deviceInfo, c);
            var response2 = await instaApi.HttpRequestProcessor.SendAsync(request);
            var json = await response2.Content.ReadAsStringAsync();
            ;
        }
        catch
        {
            ;
        }

        ;
    }

    private static Dictionary<string, string> ToDictionary(List<Tuple<string, string>> list)
    {
        Dictionary<string, string> r = new();
        foreach (var (item1, item2) in list) r[item1] = item2;

        return r;
    }
}
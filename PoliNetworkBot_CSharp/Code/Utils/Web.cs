#region

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects.WebObject;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class Web
{
    internal static async Task<WebReply> DownloadHtmlAsync(string urlAddress)

    {
        HttpClient httpClient = new();
        var response = await httpClient.GetAsync(urlAddress);

        if (response.StatusCode != HttpStatusCode.OK)
            return new WebReply(null, response.StatusCode);

        var receiveStream = response.Content;
        try
        {
            var s = await receiveStream.ReadAsStringAsync();

            return new WebReply(s, HttpStatusCode.OK);
        }
        catch
        {
            return new WebReply(null, HttpStatusCode.ExpectationFailed);
        }
    }
}
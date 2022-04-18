#region

using PoliNetworkBot_CSharp.Code.Objects.WebObject;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal class Web
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
            string s = await receiveStream.ReadAsStringAsync();

            return new WebReply(s, HttpStatusCode.OK);
        }
        catch
        {
            return new WebReply(null, HttpStatusCode.ExpectationFailed);
        }
    }
}
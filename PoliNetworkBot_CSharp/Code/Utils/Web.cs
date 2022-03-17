#region

using PoliNetworkBot_CSharp.Code.Objects.WebObject;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class Web
    {
#pragma warning disable CS1998 // Il metodo asincrono non contiene operatori 'await', pertanto verrà eseguito in modo sincrono

        internal static async Task<WebReply> DownloadHtmlAsync(string urlAddress, RequestCacheLevel requestCacheLevel)
#pragma warning restore CS1998 // Il metodo asincrono non contiene operatori 'await', pertanto verrà eseguito in modo sincrono
        {
            var request = (HttpWebRequest)WebRequest.Create(urlAddress);
            request.CachePolicy = new RequestCachePolicy(requestCacheLevel);
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK)
                return new WebReply(null, response.StatusCode);

            var receiveStream = response.GetResponseStream();
            try
            {
                var readStream = string.IsNullOrWhiteSpace(response.CharacterSet)
                    ? new StreamReader(receiveStream)
                    : new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                var data = await readStream.ReadToEndAsync();

                response.Close();
                readStream.Close();

                return new WebReply(data, HttpStatusCode.OK);
            }
            catch
            {
                return new WebReply(null, HttpStatusCode.ExpectationFailed);
            }
        }
    }
}
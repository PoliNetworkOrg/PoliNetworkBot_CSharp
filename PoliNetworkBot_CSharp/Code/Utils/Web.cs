using PoliNetworkBot_CSharp.Code.Objects.WebObject;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class Web
    {
        internal static async Task<WebReply> DownloadHtmlAsync(string urlAddress, RequestCacheLevel requestCacheLevel)
        {
            var request = (HttpWebRequest)WebRequest.Create(urlAddress);
            request.CachePolicy = new RequestCachePolicy(requestCacheLevel);
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var receiveStream = response.GetResponseStream();
                try
                {
                    StreamReader readStream;
                    if (string.IsNullOrWhiteSpace(response.CharacterSet))
                        readStream = new StreamReader(receiveStream);
                    else
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                    var data = readStream.ReadToEnd();

                    response.Close();
                    readStream.Close();

                    return new WebReply(data, HttpStatusCode.OK);
                }
                catch
                {
                    return new WebReply(null, HttpStatusCode.ExpectationFailed);
                }
            }

            return new WebReply(null, response.StatusCode);
        }
    }
}
using PoliNetworkBot_CSharp.Code.Objects.WebObject;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class Web
    {
        internal static async System.Threading.Tasks.Task<WebReply> DownloadHtmlAsync(string urlAddress)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                try
                {
                    StreamReader readStream;
                    if (String.IsNullOrWhiteSpace(response.CharacterSet))
                        readStream = new StreamReader(receiveStream);
                    else
                    {
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                    }

                    string data = readStream.ReadToEnd();

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
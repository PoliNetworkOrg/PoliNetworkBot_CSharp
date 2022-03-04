#region

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;

#endregion

//using Windows.Web.Http;

namespace Minista.Helpers
{
    public static class HttpHelper
    {
        private static readonly HttpClient Client = new();

        public static async Task<Uri> DownloadFileAsync(Uri uri, string name, StorageFolder savedFolder)
        {
            try
            {
                var file = await savedFolder.CreateFileAsync(name);
                using (var result = await Client.GetAsync(uri))
                {
                    await FileIO.WriteBytesAsync(file, await result.Content.ReadAsByteArrayAsync());

                    //using (var filestream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    //{
                    //    await result.Content.WriteToStreamAsync(filestream);
                    //    await filestream.FlushAsync();
                    //}
                }

                return new Uri(file.Path, UriKind.RelativeOrAbsolute);
            }
            catch
            {
            }

            return uri;
        }
    }
}
#region

using System;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class Permissions
    {
        internal static bool CheckPermissions(Permission permission)
        {
            switch (permission)
            {
                case Permission.HEAD_ADMIN:
                    return HeadAdminCheck().Result;
                case Permission.USER:
                case Permission.OWNER:
                case Permission.CREATOR:
                default:
                    throw new NotImplementedException();
            }
        }

        private static async Task<bool> HeadAdminCheck()
        {
            using WebClient client = new WebClient ();
            const string url = "https://polinetwork.org/learnmore/about_us/";
            var webReply = await Web.DownloadHtmlAsync(url, RequestCacheLevel.NoCacheNoStore);
            if (webReply == null || !webReply.IsValid()) return false;
            var doc = new HtmlDocument();
            doc.LoadHtml(webReply.GetData());

            return false;
        }
    }
}
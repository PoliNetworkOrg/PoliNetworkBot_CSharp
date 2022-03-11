#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class Permissions
    {
        /// <summary>
        /// Check if user has enough permissions
        /// </summary>
        /// <returns> true if is allowed, false otherwise</returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static bool CheckPermissions(Permission permission, User messageFrom)
        {
            switch (permission)
            {
                case Permission.HEAD_ADMIN:
                    return HeadAdminCheck(messageFrom).Result;
                case Permission.USER:
                case Permission.OWNER:
                case Permission.CREATOR:
                default:
                    throw new NotImplementedException();
            }
        }

        private static async Task<bool> HeadAdminCheck(User messageFrom)
        {
            using WebClient client = new WebClient ();
            const string url = "https://polinetwork.org/en/learnmore/about_us/";
            var webReply = await Web.DownloadHtmlAsync(url, RequestCacheLevel.NoCacheNoStore);
            if (webReply == null || !webReply.IsValid()) return false;
            var doc = new HtmlDocument();
            doc.LoadHtml(webReply.GetData());
            var delegates = HtmlUtil.GetElementsByTagAndClassName(doc?.DocumentNode, "ul", "delegates", 1);
            if (delegates.Count == 0) return false;
            var delegatesInner = HtmlUtil.GetElementsByTagAndClassName(delegates[0], "li");
            var authorizedUsernames = delegatesInner.Select(x => x.InnerHtml.Replace(" ", "")
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Split(@"https://t.me/")[1]
                    .Split(@""">")[0])
                .ToList();
            var headAdmins = HtmlUtil.GetElementsByTagAndClassName(doc?.DocumentNode, "ul", "headadmins", 1);
            if (headAdmins.Count == 0) return false;
            var headAdminsInner = HtmlUtil.GetElementsByTagAndClassName(headAdmins[0], "li");
            authorizedUsernames.AddRange(headAdminsInner.Select(x => x.InnerHtml.Replace(" ", "")
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Split(@"https://t.me/")[1]
                    .Split(@""">")[0])
                .ToList());
            return messageFrom.Username != null && authorizedUsernames.Any(username => string.Equals(messageFrom.Username, username, StringComparison.CurrentCultureIgnoreCase));
        }
        /*
               <li>
                  <a href="https://t.me/eliamaggioni">
                  &nbsp;
                  <img src="/img/tg.svg" style="width:15px;">
                  &nbsp;
                  </a>Elia Maggioni -
                  Delegate to management of innovative projects and technical support (IT),
                  <span class="nomination">
                  Appointed on Oct 2021</span>
               </li>
            */
    }
}
#region

using System;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PoliNetworkBot_CSharp.Code.Enums;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class Permissions
{
    /// <summary>
    ///     Check if user has enough permissions
    /// </summary>
    /// <returns> true if is allowed, false otherwise</returns>
    /// <exception cref="NotImplementedException"></exception>
    internal static bool CheckPermissions(Permission permission, User? messageFrom)
    {
        return permission switch
        {
            Permission.HEAD_ADMIN => HeadAdminCheck(messageFrom).Result,
            Permission.OWNER => throw new NotImplementedException(),
            Permission.CREATOR => throw new NotImplementedException(),
            Permission.USER => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };
    }

    private static async Task<bool> HeadAdminCheck(User? messageFrom)
    {
        const string url = "https://polinetwork.org/en/learnmore/about_us/";
        var webReply = await Web.DownloadHtmlAsync(url);
        if (!webReply.IsValid()) return false;
        var doc = new HtmlDocument();
        doc.LoadHtml(webReply.GetData());
        var delegates = HtmlUtil.GetElementsByTagAndClassName(doc.DocumentNode, "ul", "delegates", 1);
        if (delegates is { Count: 0 }) return false;
        var delegatesInner = HtmlUtil.GetElementsByTagAndClassName(delegates?[0], "li");
        if (delegatesInner == null) return false;
        var authorizedUsernames = delegatesInner.Select(x => x?.InnerHtml.Replace(" ", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Split(@"https://t.me/")[1]
                .Split(@""">")[0])
            .ToList();
        var headAdmins = HtmlUtil.GetElementsByTagAndClassName(doc.DocumentNode, "ul", "headadmins", 1);
        if (headAdmins is { Count: 0 }) return false;
        var headAdminsInner = HtmlUtil.GetElementsByTagAndClassName(headAdmins?[0], "li");
        if (headAdminsInner != null)
            authorizedUsernames.AddRange(headAdminsInner.Select(x => x?.InnerHtml.Replace(" ", "")
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Split(@"https://t.me/")[1]
                    .Split(@""">")[0])
                .ToList());
        var headAdminsNotRen = HtmlUtil.GetElementsByTagAndClassName(doc.DocumentNode, "ul", "headadmins_notrenewed", 1);
        if (headAdminsNotRen is { Count: 0 }) return false;
        var headAdminsNotRenInner = HtmlUtil.GetElementsByTagAndClassName(headAdminsNotRen?[0], "li");
        if (headAdminsNotRenInner != null)
            authorizedUsernames.AddRange(headAdminsNotRenInner.Select(x => x?.InnerHtml.Replace(" ", "")
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Split(@"https://t.me/")[1]
                    .Split(@""">")[0])
                .ToList());
        return messageFrom?.Username != null && authorizedUsernames.Any(username =>
            string.Equals(messageFrom.Username, username, StringComparison.CurrentCultureIgnoreCase));
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
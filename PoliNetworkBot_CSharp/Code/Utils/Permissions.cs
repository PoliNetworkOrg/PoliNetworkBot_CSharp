#region

using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class Permissions
{
    private static readonly Dictionary<Permission, int> ClearanceLevel = new()
    {
        { Permission.USER, 0 },
        { Permission.HEAD_ADMIN, 100 },
        { Permission.ALLOWED_MUTE_ALL, 200 },
        { Permission.ALLOWED_BAN_ALL, 201 },
        { Permission.CREATOR, 1000 },
        { Permission.OWNER, int.MaxValue }
    };

    private static readonly List<KeyValuePair<Permission, int>> OrderedClearance = new();

    /// <summary>
    ///     Check if user has enough permissions
    /// </summary>
    /// <returns> true if is allowed, false otherwise</returns>
    /// <exception cref="NotImplementedException"></exception>
    internal static bool CheckPermissions(Permission permission, User? messageFrom)
    {
        InitClearance();

        if (messageFrom == null)
            return false;
        var currentClearance = 0;
        foreach (var clearance in OrderedClearance)
        {
            if (currentClearance >= ClearanceLevel.GetValueOrDefault(permission, int.MaxValue)) return true;
            if (GetPermissionFunc(clearance.Key).Invoke(messageFrom)) currentClearance = clearance.Value;
        }

        return false;
    }

    /// <summary>
    ///     Orders OrderedClearance in REVERSE ORDER (from highest to lowest) if OrderedClearance is empty, otherwise does
    ///     nothing.
    /// </summary>
    private static void InitClearance()
    {
        if (OrderedClearance.Count != 0) return;
        // Init ordered clearance
        foreach (var clearanceWithLevel in ClearanceLevel) OrderedClearance.Add(clearanceWithLevel);
        OrderedClearance.Sort((a, b) => b.Value.CompareTo(a.Value));
    }

    private static Func<User, bool> GetPermissionFunc(Permission permission)
    {
        return permission switch
        {
            Permission.HEAD_ADMIN => HeadAdminCheck,
            Permission.OWNER => OwnerCheck,
            Permission.CREATOR => CreatorCheck,
            Permission.USER => UserCheck,
            Permission.ALLOWED_MUTE_ALL => MuteAllCheck,
            Permission.ALLOWED_BAN_ALL => BanAllCheck,
            _ => throw new Exception("No such permission level")
        };
    }

    private static bool BanAllCheck(User messageFrom)
    {
        return GlobalVariables.AllowedBanAll != null &&
               GlobalVariables.AllowedBanAll.ToList().Any(x => x.Matches(messageFrom));
    }

    private static bool MuteAllCheck(User messageFrom)
    {
        return GlobalVariables.AllowedMuteAll != null &&
               GlobalVariables.AllowedMuteAll.ToList().Any(x => x.Matches(messageFrom));
    }

    private static bool UserCheck(User messageFrom)
    {
        return true;
    }

    private static bool CreatorCheck(User messageFrom)
    {
        return GlobalVariables.Creators != null && GlobalVariables.Creators.ToList().Any(x => x.Matches(messageFrom));
    }

    /// <summary>
    ///     Get Permissions from polinetwork.org/learnmore/about_us to check the level HEAD_ADMIN
    /// </summary>
    /// <param name="messageFrom">User to check</param>
    /// <returns></returns>
    private static bool HeadAdminCheck(User? messageFrom)
    {
        const string url = "https://polinetwork.org/en/learnmore/about_us/";
        var webReply = Web.DownloadHtmlAsync(url).Result;
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
        var headAdminsNotRen =
            HtmlUtil.GetElementsByTagAndClassName(doc.DocumentNode, "ul", "headadmins_notrenewed", 1);
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

    private static bool OwnerCheck(User user)
    {
        return Owners.CheckIfOwner(user.Id);
    }

    public static Permission GetPrivileges(User? messageFrom)
    {
        InitClearance();

        return messageFrom == null
            ? Permission.USER
            : OrderedClearance
                .Where(clearance => GetPermissionFunc(clearance.Key).Invoke(messageFrom))
                .Select(x => x.Key)
                .FirstOrDefault(Permission.USER);
    }

    /// <summary>
    ///     returns 1 if a > b, 0 if a = b, -1 otherwise<br />
    ///     Put the one you want to check in the unsafe parameter
    /// </summary>
    /// <param name="a">unsafe parameter</param>
    /// <param name="b">safe parameter</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static int Compare(Permission a, Permission b)
    {
        return ClearanceLevel.GetValueOrDefault(a, int.MinValue)
            .CompareTo(ClearanceLevel.GetValueOrDefault(b, int.MaxValue));
    }
}
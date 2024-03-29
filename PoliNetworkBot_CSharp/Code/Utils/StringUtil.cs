﻿#region

using System;
using HtmlAgilityPack;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class StringUtil
{
    public static string? NotNull(Language? caption, string? lang)
    {
        return caption == null ? "" : caption.Select(lang);
    }

    public static string NotNull(string? caption)
    {
        return caption ?? "";
    }

    private static bool IsRoomChar(char v)
    {
        return v switch
        {
            >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '.' or >= '0' and <= '9' => true,
            _ => false
        };
    }

    internal static bool? CheckIfTheStringIsTheSameAndValidRoomNameInsideAText(string? textToFind,
        HtmlNode? nodeToFindTextInto)
    {
        if (nodeToFindTextInto == null)
            return null;

        if (textToFind == null) return null;
        var j = nodeToFindTextInto.InnerHtml.IndexOf(textToFind, StringComparison.Ordinal);
        return j switch
        {
            < 0 => false,
            0 => IsRoomChar(nodeToFindTextInto.InnerHtml[textToFind.Length]) == false,
            _ => IsRoomChar(nodeToFindTextInto.InnerHtml[j - 1]) == false &&
                 IsRoomChar(nodeToFindTextInto.InnerHtml[j + textToFind.Length]) == false
        };
    }

    internal static char ToSn(bool? b)
    {
        return b == null ? '?' : b.Value ? 'S' : 'N';
    }
}
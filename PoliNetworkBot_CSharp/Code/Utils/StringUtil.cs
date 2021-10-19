﻿#region

using HtmlAgilityPack;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public class StringUtil
    {
        public static string NotNull(Language caption, string lang)
        {
            if (caption == null)
                return "";
            return caption.Select(lang);
        }

        internal static bool IsRoomChar(char v)
        {
            if (v >= 'A' && v <= 'Z')
                return true;

            if (v >= 'a' && v <= 'z')
                return true;

            if (v == '.')
                return true;

            if (v >= '0' && v <= '9')
                return true;

            return false;
        }

        internal static bool? CheckIfTheStringIsTheSameAndValidRoomNameInsideAText(string textToFind,
            HtmlNode nodeToFindTextInto)
        {
            if (nodeToFindTextInto == null)
                return null;

            var j = nodeToFindTextInto.InnerHtml.IndexOf(textToFind);
            if (j < 0)
                return false;

            if (j == 0)
            {
                if (IsRoomChar(nodeToFindTextInto.InnerHtml[j + textToFind.Length]) == false)
                    return true;

                return false;
            }

            if (IsRoomChar(nodeToFindTextInto.InnerHtml[j - 1]) == false &&
                IsRoomChar(nodeToFindTextInto.InnerHtml[j + textToFind.Length]) == false)
                return true;

            return false;
        }

        internal static char ToSN(bool? b)
        {
            return b == null ? '?' : b.Value ? 'S' : 'N';
        }
    }
}
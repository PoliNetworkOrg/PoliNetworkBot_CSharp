#region

using System;
using HtmlAgilityPack;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public class StringUtil
    {
        public static string NotNull(Language caption, string lang)
        {
            return caption == null ? "" : caption.Select(lang);
        }

        internal static bool IsRoomChar(char v)
        {
            switch (v)
            {
                case >= 'A' and <= 'Z':
                case >= 'a' and <= 'z':
                case '.':
                case >= '0' and <= '9':
                    return true;
                default:
                    return false;
            }
        }

        internal static bool? CheckIfTheStringIsTheSameAndValidRoomNameInsideAText(string textToFind,
            HtmlNode nodeToFindTextInto)
        {
            if (nodeToFindTextInto == null)
                return null;

            var j = nodeToFindTextInto.InnerHtml.IndexOf(textToFind, StringComparison.Ordinal);
            return j switch
            {
                < 0 => false,
                0 => IsRoomChar(nodeToFindTextInto.InnerHtml[textToFind.Length]) == false,
                _ => IsRoomChar(nodeToFindTextInto.InnerHtml[j - 1]) == false &&
                     IsRoomChar(nodeToFindTextInto.InnerHtml[j + textToFind.Length]) == false
            };
        }

        internal static char ToSN(bool? b)
        {
            return b == null ? '?' : b.Value ? 'S' : 'N';
        }
    }
}
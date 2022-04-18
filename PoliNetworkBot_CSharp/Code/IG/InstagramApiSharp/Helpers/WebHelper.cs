#region

using System;
using InstagramApiSharp;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Helpers;

internal static class Helper
{
    private const string StartTag = "type=\"text/javascript\">window._sharedData";
    private const string EndTag = ";</script>";

    private static bool CanReadJson(this string html)
    {
        return html.Contains(StartTag);
    }

    public static string GetJson(this string html)
    {
        try
        {
            if (html.CanReadJson())
            {
                var json = html[(html.IndexOf(StartTag, StringComparison.Ordinal) + StartTag.Length)..];
                json = json[..json.IndexOf(EndTag, StringComparison.Ordinal)];
                json = json[(json.IndexOf("=", StringComparison.Ordinal) + 2)..];
                return json;
            }
        }
        catch (Exception ex)
        {
            $"WebHelper.GetJson ex: {ex.Message}\r\nSource: {ex.Source}\r\nTrace: {ex.StackTrace}".PrintInDebug();
        }

        return null;
    }
}
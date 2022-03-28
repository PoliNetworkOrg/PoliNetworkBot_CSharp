#region

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

#endregion

namespace InstagramApiSharp.Helpers;

internal static class DictionaryExtensions
{
    public static string AsQueryString(this Dictionary<string, string> parameters)
    {
        if (!parameters.Any())
            return "";

        var builder = new StringBuilder("?");

        var separator = "";
        foreach (var (key, value) in parameters.Where(kvp => kvp.Value != null))
        {
            builder.Append($"{separator}{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(value)}");
            separator = "&";
        }

        return builder.ToString();
    }
}
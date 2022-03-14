#region

using System;

#endregion

namespace InstagramApiSharp.Helpers
{
    public static class HttpExtensions
    {
        public static Uri AddQueryParameter(this Uri uri, string name, string value, bool dontCheck = false)
        {
            if (!dontCheck)
                if (value is null or "" or "[]")
                    return uri;

            value ??= "";

            var httpValueCollection = HttpUtility.ParseQueryString(uri);

            httpValueCollection.Remove(name);
            httpValueCollection.Add(name, value);

            var ub = new UriBuilder(uri);
            var q = "";
            foreach (var (key, s) in httpValueCollection)
                if (q == "") q += $"{key}={s}";
                else q += $"&{key}={s}";
            ub.Query = q;
            return ub.Uri;
        }

        public static Uri AddQueryParameterIfNotEmpty(this Uri uri, string name, string value)
        {
            if (string.IsNullOrEmpty(value) || value == "[]") return uri;

            var httpValueCollection = HttpUtility.ParseQueryString(uri);
            httpValueCollection.Remove(name);
            httpValueCollection.Add(name, value);
            var ub = new UriBuilder(uri);
            var q = "";
            foreach (var (key, s) in httpValueCollection)
                if (q == "") q += $"{key}={s}";
                else q += $"&{key}={s}";
            ub.Query = q;
            return ub.Uri;
        }
    }
}
#region

using System;
using InstagramApiSharp.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace InstagramApiSharp.Helpers
{
    public class InstaFbHelper
    {
        /// <summary>
        ///     Clear datas and cached from this address
        /// </summary>
        public static readonly Uri FacebookMobileAddress = new("https://m.facebook.com/");

        /// <summary>
        ///     Clear datas and cached from this address
        /// </summary>
        public static readonly Uri FacebookAddress = new("https://facebook.com/");

        /// <summary>
        ///     Clear datas and cached from this address
        /// </summary>
        public static readonly Uri FacebookAddressWithWWWAddress = new("https://www.facebook.com/");

        /// <summary>
        ///     Get facebook login uri
        /// </summary>
        public static Uri GetFacebookLoginUri()
        {
            try
            {
                var init = new JObject
                {
                    { "init", DateTime.UtcNow.ToUnixTimeMiliSeconds() }
                };
                if (Uri.TryCreate(string.Format(InstaApiConstants.FACEBOOK_LOGIN_URI,
                        init.ToString(Formatting.None)), UriKind.RelativeOrAbsolute, out var fbUri))
                    return fbUri;
            }
            catch
            {
            }

            return null;
        }

        /// <summary>
        ///     Get facebook user agent
        /// </summary>
        public static string GetFacebookUserAgent()
        {
            return /*ExtensionHelper.GenerateFacebookUserAgent()*/ InstaApiConstants.FACEBOOK_USER_AGENT_DEFAULT;
        }

        public static bool IsLoggedIn(string html)
        {
            //https://m.facebook.com/v2.3/dialog/oauth/confirm
            return html.Contains("window.location.href=\"fbconnect://success#") ||
                   html.Contains("window.location.href=\"fbconnect:\\/\\/success");
        }

        public static string GetAccessToken(string html)
        {
            try
            {
                var start = "type=\"text/javascript\">window.location.href=\"fbconnect:\\/\\/success";
                var end = "</script>";
                if (html.Contains(start))
                {
                    var str = html[(html.IndexOf(start, StringComparison.Ordinal) + start.Length)..];
                    str = str[..str.IndexOf(end, StringComparison.Ordinal)];
                    //&access_token=EAABwzLixnjYBALFoqFT7uqZCVCCcPM3HZAEXwSUZB3qBi1OxHP6OYP5hEYpzkNEej465HwMiMbvvvz9GyzWhby14KtdwdoiW83xZAzIaThIBTwZDZD&
                    start = "&access_token=";
                    end = "&";
                    var token = str[(str.IndexOf(start, StringComparison.Ordinal) + start.Length)..];
                    token = token[..token.IndexOf(end, StringComparison.Ordinal)];
                    return token;
                }
            }
            catch
            {
            }

            return null;
        }
    }
}
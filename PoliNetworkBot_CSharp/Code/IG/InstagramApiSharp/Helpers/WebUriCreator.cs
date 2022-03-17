#region

using InstagramApiSharp.API;
using System;

#endregion

namespace InstagramApiSharp.Helpers
{
    internal class WebUriCreator
    {
        public static Uri GetAccountsDataUri()
        {
            if (!Uri.TryCreate(InstaApiConstants.InstagramWebUri, InstaApiConstants.WebAccountData, out var instaUri))
                throw new Exception("Cant create URI for accounts data page");
            return instaUri;
        }

        public static Uri GetCurrentFollowRequestsUri(string cursor = null)
        {
            if (!Uri.TryCreate(InstaApiConstants.InstagramWebUri, InstaApiConstants.WebCurrentFollowRequests,
                    out var instaUri))
                throw new Exception("Cant create URI for current follow requests");
            var query = string.Empty;
            if (cursor.IsNotEmpty())
                query = string.Format(InstaApiConstants.WebCursor, Uri.EscapeUriString(cursor));

            return new UriBuilder(instaUri) { Query = query }.Uri;
        }

        public static Uri GetFormerBiographyTextsUri(string cursor = null)
        {
            if (!Uri.TryCreate(InstaApiConstants.InstagramWebUri, InstaApiConstants.WebFormerBioTexts,
                    out var instaUri))
                throw new Exception("Cant create URI for former biography texts");
            var query = string.Empty;
            if (cursor.IsNotEmpty())
                query = string.Format(InstaApiConstants.WebCursor, Uri.EscapeUriString(cursor));

            return new UriBuilder(instaUri) { Query = query }.Uri;
        }

        public static Uri GetFormerBiographyLinksUri(string cursor = null)
        {
            if (!Uri.TryCreate(InstaApiConstants.InstagramWebUri, InstaApiConstants.WebFormerBioLinks,
                    out var instaUri))
                throw new Exception("Cant create URI for former biography links");
            var query = string.Empty;
            if (cursor.IsNotEmpty())
                query = string.Format(InstaApiConstants.WebCursor, Uri.EscapeUriString(cursor));

            return new UriBuilder(instaUri) { Query = query }.Uri;
        }

        public static Uri GetFormerUsernamesUri(string cursor = null)
        {
            if (!Uri.TryCreate(InstaApiConstants.InstagramWebUri, InstaApiConstants.WebFormerUsernames,
                    out var instaUri))
                throw new Exception("Cant create URI for former usernames");
            var query = string.Empty;
            if (cursor.IsNotEmpty())
                query = string.Format(InstaApiConstants.WebCursor, Uri.EscapeUriString(cursor));

            return new UriBuilder(instaUri) { Query = query }.Uri;
        }

        public static Uri GetFormerFullNamesUri(string cursor = null)
        {
            if (!Uri.TryCreate(InstaApiConstants.InstagramWebUri, InstaApiConstants.WebFormerFullNames,
                    out var instaUri))
                throw new Exception("Cant create URI for former full names");
            var query = string.Empty;
            if (cursor.IsNotEmpty())
                query = string.Format(InstaApiConstants.WebCursor, Uri.EscapeUriString(cursor));

            return new UriBuilder(instaUri) { Query = query }.Uri;
        }

        public static Uri GetFormerPhoneNumbersUri(string cursor = null)
        {
            if (!Uri.TryCreate(InstaApiConstants.InstagramWebUri, InstaApiConstants.WebFormerPhones,
                    out var instaUri))
                throw new Exception("Cant create URI for former phone numbers");
            var query = string.Empty;
            if (cursor.IsNotEmpty())
                query = string.Format(InstaApiConstants.WebCursor, Uri.EscapeUriString(cursor));

            return new UriBuilder(instaUri) { Query = query }.Uri;
        }

        public static Uri GetFormerEmailsUri(string cursor = null)
        {
            if (!Uri.TryCreate(InstaApiConstants.InstagramWebUri, InstaApiConstants.WebFormerEmails,
                    out var instaUri))
                throw new Exception("Cant create URI for former emails");
            var query = string.Empty;
            if (cursor.IsNotEmpty())
                query = string.Format(InstaApiConstants.WebCursor, Uri.EscapeUriString(cursor));

            return new UriBuilder(instaUri) { Query = query }.Uri;
        }
    }
}
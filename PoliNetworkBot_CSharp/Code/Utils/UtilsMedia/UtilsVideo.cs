using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Utils.UtilsMedia
{
    public static class UtilsVideo
    {
        public static Video GetLargest(Video replyToVideo)
        {
            return replyToVideo;
        }

        public static int? AddVideoToDb(Video video)
        {
            var photoId = GetVideoId_From_FileId_OR_UniqueFileId(video.FileId, video.FileUniqueId);
            if (photoId != null) return photoId.Value;

            var q =
                "INSERT INTO Videos " +
                "(file_id, file_size, height, width, unique_id, duration, mime) "+
                " VALUES " +
                "(@fi, @fs, @h, @w, @u, @d, @mime)";
            
            
            var keyValuePairs = new Dictionary<string, object>
            {
                {"@fi", video.FileId},
                {"@fs", video.FileSize},
                {"@h", video.Height},
                {"@w", video.Width},
                {"@u", video.FileUniqueId},
                {"@d", video.Duration},
                {"@mime", video.MimeType}
            };

            SqLite.Execute(q, keyValuePairs);
            Tables.FixIdTable("Videos", "id_video", "file_id");

            return GetVideoId_From_FileId_OR_UniqueFileId(video.FileId, video.FileUniqueId);
        }

        private static int? GetVideoId_From_FileId_OR_UniqueFileId(string fileId, string fileUniqueId)
        {
            var a = GetVideoId_From_FileId(fileId);
            return a ?? GetVideoId_From_UniqueFileId(fileUniqueId);
        }

        private static int? GetVideoId_From_UniqueFileId(string fileUniqueId)
        {
            const string q2 = "SELECT id_video FROM Videos WHERE unique_id = @fi";
            var keyValuePairs2 = new Dictionary<string, object>
            {
                {"@fi", fileUniqueId}
            };
            var r1 = SqLite.ExecuteSelect(q2, keyValuePairs2);
            var r2 = SqLite.GetFirstValueFromDataTable(r1);

            if (r2 == null)
                return null;

            try
            {
                return Convert.ToInt32(r2);
            }
            catch
            {
                return null;
            }
        }

        private static int? GetVideoId_From_FileId(string fileId)
        {
            const string q2 = "SELECT id_video FROM Videos WHERE file_id = @fi";
            var keyValuePairs2 = new Dictionary<string, object>
            {
                {"@fi", fileId}
            };
            var r1 = SqLite.ExecuteSelect(q2, keyValuePairs2);
            var r2 = SqLite.GetFirstValueFromDataTable(r1);

            if (r2 == null)
                return null;

            try
            {
                return Convert.ToInt32(r2);
            }
            catch
            {
                return null;
            }
        }
    }
}
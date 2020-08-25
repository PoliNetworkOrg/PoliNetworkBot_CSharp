#region

using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class Photo
    {
        internal static PhotoSize GetLargest(PhotoSize[] photo)
        {
            var max = -1;
            PhotoSize r = null;
            foreach (var p in photo)
                if (p.Height > max)
                {
                    max = p.Height;
                    r = p;
                }

            return r;
        }

        internal static int? AddPhotoToDB(PhotoSize photo_large)
        {
            var photo_id = GetPhotoId_From_FileId_OR_UniqueFileId(photo_large.FileId, photo_large.FileUniqueId);
            if (photo_id != null) return photo_id.Value;

            var q = "INSERT INTO Photos (file_id, file_size, height, width, unique_id) VALUES (@fi, @fs, @h, @w, @u)";
            var keyValuePairs = new Dictionary<string, object>
            {
                {"@fi", photo_large.FileId},
                {"@fs", photo_large.FileSize},
                {"@h", photo_large.Height},
                {"@w", photo_large.Width},
                {"@u", photo_large.FileUniqueId}
            };

            SQLite.Execute(q, keyValuePairs);
            Tables.FixIDTable("Photos", "id_photo", "file_id");

            return GetPhotoId_From_FileId_OR_UniqueFileId(photo_large.FileId, photo_large.FileUniqueId);
        }

        private static int? GetPhotoId_From_FileId_OR_UniqueFileId(string fileId, string fileUniqueId)
        {
            var a = GetPhotoId_From_FileId(fileId);
            return a == null ? GetPhotoId_From_UniqueFileId(fileUniqueId) : a.Value;
        }

        private static int? GetPhotoId_From_UniqueFileId(string fileUniqueId)
        {
            var q2 = "SELECT id_photo FROM Photos WHERE unique_id = @fi";
            var keyValuePairs2 = new Dictionary<string, object>
            {
                {"@fi", fileUniqueId}
            };
            var r1 = SQLite.ExecuteSelect(q2, keyValuePairs2);
            var r2 = SQLite.GetFirstValueFromDataTable(r1);

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

        private static int? GetPhotoId_From_FileId(string fileId)
        {
            var q2 = "SELECT id_photo FROM Photos WHERE file_id = @fi";
            var keyValuePairs2 = new Dictionary<string, object>
            {
                {"@fi", fileId}
            };
            var r1 = SQLite.ExecuteSelect(q2, keyValuePairs2);
            var r2 = SQLite.GetFirstValueFromDataTable(r1);

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
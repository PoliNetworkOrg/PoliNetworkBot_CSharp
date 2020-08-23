using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class Photo
    {
        internal static PhotoSize GetLargest(PhotoSize[] photo)
        {
            int max = -1;
            PhotoSize r = null;
            foreach (var p in photo)
            {
                if (p.Height > max)
                {
                    max = p.Height;
                    r = p;
                }
            }

            return r;
        }

        internal static int? AddPhotoToDB(PhotoSize photo_large)
        {
            int? photo_id = GetPhotoId_From_FileId(photo_large.FileId);
            if (photo_id != null)
            {
                return photo_id.Value;
            }

            string q = "INSERT INTO Photos (file_id, file_size, height, width) VALUES (@fi, @fs, @h, @w)";
            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>() {
                {"@fi", photo_large.FileId },
                {"@fs", photo_large.FileSize },
                {"@h", photo_large.Height },
                {"@w", photo_large.Width }
            };

            Utils.SQLite.Execute(q, keyValuePairs);
            Utils.Tables.FixIDTable("Photos", "id_photo", "file_id");

            return GetPhotoId_From_FileId(photo_large.FileId);
        }

        private static int? GetPhotoId_From_FileId(string fileId)
        {
            string q2 = "SELECT id_photo FROM Photos WHERE file_id = @fi";
            Dictionary<string, object> keyValuePairs2 = new Dictionary<string, object>() {
                {"@fi", fileId }
            };
            var r1 = Utils.SQLite.ExecuteSelect(q2, keyValuePairs2);
            var r2 = Utils.SQLite.GetFirstValueFromDataTable(r1);

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
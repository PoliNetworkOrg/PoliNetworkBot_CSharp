﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.UtilsMedia
{
    internal static class UtilsPhoto
    {
        internal static PhotoSize GetLargest(IEnumerable<PhotoSize> photo)
        {
            if (photo == null || photo.Count() == 0)
                return null;

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

        internal static int? AddPhotoToDb(PhotoSize photoLarge)
        {
            var photoId = GetPhotoId_From_FileId_OR_UniqueFileId(photoLarge.FileId, photoLarge.FileUniqueId);
            if (photoId != null) return photoId.Value;

            const string q =
                "INSERT INTO Photos (file_id, file_size, height, width, unique_id) VALUES (@fi, @fs, @h, @w, @u)";
            var keyValuePairs = new Dictionary<string, object>
            {
                {"@fi", photoLarge.FileId},
                {"@fs", photoLarge.FileSize},
                {"@h", photoLarge.Height},
                {"@w", photoLarge.Width},
                {"@u", photoLarge.FileUniqueId}
            };

            SqLite.Execute(q, keyValuePairs);
            Tables.FixIdTable("Photos", "id_photo", "file_id");

            return GetPhotoId_From_FileId_OR_UniqueFileId(photoLarge.FileId, photoLarge.FileUniqueId);
        }

        private static int? GetPhotoId_From_FileId_OR_UniqueFileId(string fileId, string fileUniqueId)
        {
            var a = GetPhotoId_From_FileId(fileId);
            return a ?? GetPhotoId_From_UniqueFileId(fileUniqueId);
        }

        private static int? GetPhotoId_From_UniqueFileId(string fileUniqueId)
        {
            const string q2 = "SELECT id_photo FROM Photos WHERE unique_id = @fi";
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

        private static int? GetPhotoId_From_FileId(string fileId)
        {
            const string q2 = "SELECT id_photo FROM Photos WHERE file_id = @fi";
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

        public static ObjectPhoto GetPhotoByIdFromDb(int photoIdFromFb, int? messageIdFrom, long chatId,
            ChatType chatType)
        {
            var q = "SELECT * FROM Photos WHERE id_photo = " + photoIdFromFb;
            var dt = SqLite.ExecuteSelect(q);
            if (dt == null || dt.Rows.Count == 0)
                return null;

            var dr = dt.Rows[0];

            return new ObjectPhoto(Convert.ToInt32(dr["id_photo"]), dr["file_id"].ToString(),
                Convert.ToInt32(dr["file_size"]), Convert.ToInt32(dr["height"]),
                Convert.ToInt32(dr["width"]), dr["unique_id"].ToString(),
                messageIdFrom, chatId, chatType);
        }
    }
}
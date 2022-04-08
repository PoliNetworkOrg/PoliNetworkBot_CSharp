#region

using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;

internal static class UtilsPhoto
{
    internal static PhotoSize GetLargest(IEnumerable<PhotoSize> photo)
    {
        if (photo == null || !photo.Any())
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

    internal static long? AddPhotoToDb(PhotoSize photoLarge, TelegramBotAbstract sender)
    {
        var photoId = GetPhotoId_From_FileId_OR_UniqueFileId(photoLarge.FileId, photoLarge.FileUniqueId, sender);
        if (photoId != null) return photoId.Value;

        const string q =
            "INSERT INTO Photos (file_id, file_size, height, width, unique_id) VALUES (@fi, @fs, @h, @w, @u)";
        var keyValuePairs = new Dictionary<string, object>
        {
            { "@fi", photoLarge.FileId },
            { "@fs", photoLarge.FileSize },
            { "@h", photoLarge.Height },
            { "@w", photoLarge.Width },
            { "@u", photoLarge.FileUniqueId }
        };

        Database.Execute(q, sender.DbConfig, keyValuePairs);
        Tables.FixIdTable("Photos", "id_photo", "file_id", sender.DbConfig);

        return GetPhotoId_From_FileId_OR_UniqueFileId(photoLarge.FileId, photoLarge.FileUniqueId, sender);
    }

    private static long? GetPhotoId_From_FileId_OR_UniqueFileId(string fileId, string fileUniqueId,
        TelegramBotAbstract sender)
    {
        var a = GetPhotoId_From_FileId(fileId, sender);
        return a ?? GetPhotoId_From_UniqueFileId(fileUniqueId, sender);
    }

    private static long? GetPhotoId_From_UniqueFileId(string fileUniqueId, TelegramBotAbstract sender)
    {
        const string q2 = "SELECT id_photo FROM Photos WHERE unique_id = @fi";
        var keyValuePairs2 = new Dictionary<string, object>
        {
            { "@fi", fileUniqueId }
        };
        var r1 = Database.ExecuteSelect(q2, sender.DbConfig, keyValuePairs2);
        var r2 = Database.GetFirstValueFromDataTable(r1);

        if (r2 == null)
            return null;

        try
        {
            return Convert.ToInt64(r2);
        }
        catch
        {
            return null;
        }
    }

    private static long? GetPhotoId_From_FileId(string fileId, TelegramBotAbstract sender)
    {
        const string q2 = "SELECT id_photo FROM Photos WHERE file_id = @fi";
        var keyValuePairs2 = new Dictionary<string, object>
        {
            { "@fi", fileId }
        };
        var r1 = Database.ExecuteSelect(q2, sender.DbConfig, keyValuePairs2);
        var r2 = Database.GetFirstValueFromDataTable(r1);

        if (r2 == null)
            return null;

        try
        {
            return Convert.ToInt64(r2);
        }
        catch
        {
            return null;
        }
    }

    public static ObjectPhoto GetPhotoByIdFromDb(long photoIdFromFb, long? messageIdFrom, long chatId,
        ChatType chatType, TelegramBotAbstract sender)
    {
        var q = "SELECT * FROM Photos WHERE id_photo = " + photoIdFromFb;
        var dt = Database.ExecuteSelect(q, sender.DbConfig);
        if (dt == null || dt.Rows.Count == 0)
            return null;

        var dr = dt.Rows[0];

        return new ObjectPhoto((int)Convert.ToInt64(dr["id_photo"]), dr["file_id"].ToString(),
            (int)Convert.ToInt64(dr["file_size"]), (int)Convert.ToInt64(dr["height"]),
            (int)Convert.ToInt64(dr["width"]), dr["unique_id"].ToString(),
            messageIdFrom, chatId, chatType);
    }
}
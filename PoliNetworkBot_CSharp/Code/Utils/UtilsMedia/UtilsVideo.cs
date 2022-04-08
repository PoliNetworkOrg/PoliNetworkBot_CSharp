#region

using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using System;
using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;

public static class UtilsVideo
{
    public static Video GetLargest(Video replyToVideo)
    {
        return replyToVideo;
    }

    public static long? AddVideoToDb(Video video, TelegramBotAbstract sender)
    {
        var photoId = GetVideoId_From_FileId_OR_UniqueFileId(video.FileId, video.FileUniqueId, sender);
        if (photoId != null) return photoId.Value;

        var q =
            "INSERT INTO Videos " +
            "(file_id, file_size, height, width, unique_id, duration, mime) " +
            " VALUES " +
            "(@fi, @fs, @h, @w, @u, @d, @mime)";

        var keyValuePairs = new Dictionary<string, object>
        {
            { "@fi", video.FileId },
            { "@fs", video.FileSize },
            { "@h", video.Height },
            { "@w", video.Width },
            { "@u", video.FileUniqueId },
            { "@d", video.Duration },
            { "@mime", video.MimeType }
        };

        Database.Execute(q, sender.DbConfig, keyValuePairs);
        Tables.FixIdTable("Videos", "id_video", "file_id", sender.DbConfig);

        return GetVideoId_From_FileId_OR_UniqueFileId(video.FileId, video.FileUniqueId, sender);
    }

    private static long? GetVideoId_From_FileId_OR_UniqueFileId(string fileId, string fileUniqueId,
        TelegramBotAbstract sender)
    {
        var a = GetVideoId_From_FileId(fileId, sender);
        return a ?? GetVideoId_From_UniqueFileId(fileUniqueId, sender);
    }

    private static long? GetVideoId_From_UniqueFileId(string fileUniqueId, TelegramBotAbstract sender)
    {
        const string q2 = "SELECT id_video FROM Videos WHERE unique_id = @fi";
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

    private static long? GetVideoId_From_FileId(string fileId, TelegramBotAbstract sender)
    {
        const string q2 = "SELECT id_video FROM Videos WHERE file_id = @fi";
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

    public static ObjectVideo GetVideoByIdFromDb(long videoId, long? messageIdFrom,
        in long chatIdFromIdPerson, ChatType chatType, TelegramBotAbstract sender)
    {
        var q = "SELECT * FROM Videos WHERE id_video = " + videoId;
        var dt = Database.ExecuteSelect(q, sender.DbConfig);
        if (dt == null || dt.Rows.Count == 0)
            return null;

        var dr = dt.Rows[0];

        return new ObjectVideo((int)Convert.ToInt64(dr["id_video"]), dr["file_id"].ToString(),
            (int)Convert.ToInt64(dr["file_size"]), (int)Convert.ToInt64(dr["height"]),
            (int)Convert.ToInt64(dr["width"]), dr["unique_id"].ToString(),
            messageIdFrom, chatIdFromIdPerson, chatType, (int)Convert.ToInt64(dr["duration"]));
    }
}
#region

using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class BackupUtil
{
    internal static void BackupBeforeReboot()
    {
        MessagesStore.BackupToFile();
        if (CallbackUtils.CallbackUtils.CallBackDataFull != null)
            CallbackUtils.CallbackUtils.CallBackDataFull.BackupToFile();
    }

    internal static string GetDB_AsJson(TelegramBotAbstract? telegramBotAbstract)
    {
        try
        {
            DB_Backup db = new();

            const string? q = "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA='polinetwork';";
            if (telegramBotAbstract == null) return JsonConvert.SerializeObject(db);
            var r = Database.ExecuteSelect(q, telegramBotAbstract.DbConfig);
            if (r == null)
                return JsonConvert.SerializeObject("ERROR 1");

            try
            {
                var tableNames = r.Rows.Cast<DataRow>().ToList();

                var c1 = tableNames.Where(row => row is { ItemArray.Length: > 0 } && row.ItemArray[0] != null)
                    .Select(row =>
                    {
                        var argItem = row.ItemArray[0];
                        return argItem != null ? argItem.ToString() : "";
                    });

                foreach (var c3 in c1)
                    if (!string.IsNullOrEmpty(c3))
                        db.tableNames.Add(c3);

                foreach (var tableName in
                         db.tableNames.Where(tableName => string.IsNullOrEmpty(tableName) == false))
                    try
                    {
                        var q2 = "SELECT * FROM " + tableName;
                        var r2 = Database.ExecuteSelect(q2, telegramBotAbstract.DbConfig);
                        if (r2 != null) db.tables[tableName] = r2;
                    }
                    catch
                    {
                        // ignored
                    }
            }
            catch
            {
                // ignored
            }

            return JsonConvert.SerializeObject(db);
        }
        catch
        {
            // ignored
        }

        return JsonConvert.SerializeObject("ERROR 2");
    }

    public static async Task Backup(MessageEventArgs e, TelegramBotAbstract? sender)
    {
        if (e.Message.From != null)
            await CommandDispatcher.BackupHandler(e.Message.From.Id, sender, e.Message.From.Username,
                e.Message.Chat.Type);
    }
}
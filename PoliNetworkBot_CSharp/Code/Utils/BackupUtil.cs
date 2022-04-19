#region

using PoliNetworkBot_CSharp.Code.Objects;
using System.Collections.Generic;
using System.Data;
using System.Linq;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal class BackupUtil
{
    internal static void BackupBeforeReboot()
    {
        MessagesStore.BackupToFile();
        CallbackUtils.CallbackUtils.CallBackDataFull.BackupToFile();
    }

    internal static string GetDB_AsJson(TelegramBotAbstract telegramBotAbstract)
    {
        try
        {
            DB_Backup db = new();


            string q = "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA='polinetwork';";
            var r = Database.ExecuteSelect(q, telegramBotAbstract.DbConfig);
            if (r != null)
            {
                try
                {
                    List<DataRow> tableNames = new();
                    foreach (DataRow row in r.Rows)
                    {
                        tableNames.Add(row);
                    }

                    db.tableNames.AddRange(tableNames.Where(row => row != null && row.ItemArray != null && row.ItemArray.Length > 0 && row.ItemArray[0] != null).Select(row => row.ItemArray[0].ToString()));

                    foreach (string tableName in db.tableNames)
                    {
                        if (string.IsNullOrEmpty(tableName) == false)
                        {
                            try
                            {
                                string q2 = "SELECT * FROM " + tableName;
                                var r2 = Utils.Database.ExecuteSelect(q2, telegramBotAbstract.DbConfig);
                                if (r2 != null)
                                {
                                    db.tables[tableName] = r2;
                                }
                            }
                            catch
                            {
                                ;
                            }
                        }
                    }
                }
                catch
                {
                    ;
                }
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(db);
        }
        catch
        {
            ;
        }

        return Newtonsoft.Json.JsonConvert.SerializeObject("Error");
    }
}
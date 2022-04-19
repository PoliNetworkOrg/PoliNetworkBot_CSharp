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
            MutableTuple<List<string>, Dictionary<string, DataTable>> db = new(null, null);

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
                    List<string> tableNames2 = new();
                    tableNames2.AddRange(tableNames.Where(row => row != null && row.ItemArray != null && row.ItemArray.Length > 0 && row.ItemArray[0] != null).Select(row => row.ItemArray[0].ToString()));

                    db.Item1 = tableNames2;

                    foreach (string tableName in tableNames2)
                    {
                        if (string.IsNullOrEmpty(tableName) == false)
                        {
                            try
                            {
                                string q2 = "SELECT * FROM " + tableName;
                                var r2 = Utils.Database.ExecuteSelect(q2, telegramBotAbstract.DbConfig);
                                if (r2 != null)
                                {
                                    db.Item2[tableName] = r2;
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
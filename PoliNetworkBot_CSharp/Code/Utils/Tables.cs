#region

using System;
using System.Data;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;
using SampleNuGet.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class Tables
{
    public static void FixIdTable(string tableName, string columnIdName, string uniqueColumn,
        DbConfigConnection? dbConfig)
    {
        var r4 = GetMaxId(tableName, columnIdName, dbConfig);
        var q2 = "SELECT * FROM " + tableName + " WHERE " + columnIdName + " IS NULL";
        var r5 = Database.ExecuteSelect(q2, dbConfig);
        if (r5 == null)
            return;

        foreach (DataRow dr in r5.Rows)
        {
            r4++;

            var valueUnique = dr[uniqueColumn].ToString();
            var q3 = "UPDATE " + tableName + " SET " + columnIdName + "=" + r4 + " WHERE " + uniqueColumn +
                     "='" + valueUnique + "'";
            Database.Execute(q3, dbConfig);
        }
    }

    internal static long GetMaxId(string tableName, string columnIdName, DbConfigConnection? dbConfig)
    {
        var q = "SELECT MAX(" + columnIdName + ") FROM " + tableName;
        var r = Database.ExecuteSelect(q, dbConfig);
        var r2 = Database.GetFirstValueFromDataTable(r);
        if (r2 == null) return 0;

        try
        {
            return Convert.ToInt64(r2);
        }
        catch
        {
            return 0;
        }
    }
}
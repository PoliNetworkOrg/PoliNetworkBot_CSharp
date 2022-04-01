#region

using System;
using System.Data;
using MySql.Data.MySqlClient;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class Tables
{
    public static void FixIdTable(string tableName, string columnIdName, string uniqueColumn, MySqlConnection connection)
    {
        var r4 = GetMaxId(tableName, columnIdName, connection);
        var q2 = "SELECT * FROM " + tableName + " WHERE " + columnIdName + " IS NULL";
        var r5 = SqLite.ExecuteSelect(q2, connection);
        if (r5 == null)
            return;

        foreach (DataRow dr in r5.Rows)
        {
            r4++;

            var valueUnique = dr[uniqueColumn].ToString();
            var q3 = "UPDATE " + tableName + " SET " + columnIdName + "=" + r4 + " WHERE " + uniqueColumn +
                     "='" + valueUnique + "'";
            SqLite.Execute(q3, connection);
        }
    }

    internal static long GetMaxId(string tableName, string columnIdName, MySqlConnection connection)
    {
        var q = "SELECT MAX(" + columnIdName + ") FROM " + tableName;
        var r = SqLite.ExecuteSelect(q, connection);
        var r2 = SqLite.GetFirstValueFromDataTable(r);
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
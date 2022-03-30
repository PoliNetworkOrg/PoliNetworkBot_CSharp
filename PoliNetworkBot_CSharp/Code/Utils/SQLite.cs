#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using PoliNetworkBot_CSharp.Code.Data.Constants;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class SqLite
{
    public static int Execute(string query, Dictionary<string, object> args = null)
    {
        //setup the connection to the database
        using var con = new SQLiteConnection(Paths.Db);
        con.Open();

        //open a new command
        using var cmd = new SQLiteCommand(query, con);
        //set the arguments given in the query
        if (args != null)
            foreach (var (key, value) in args)
                cmd.Parameters.AddWithValue(key, value);

        //execute the query and get the number of row affected
        var numberOfRowsAffected = cmd.ExecuteNonQuery();

        return numberOfRowsAffected;
    }

    public static DataTable ExecuteSelect(string query, Dictionary<string, object> args = null)
    {
        try
        {
            if (string.IsNullOrEmpty(query.Trim()))
                return null;

            using var con = new SQLiteConnection(Paths.Db);
            con.Open();
            using var cmd = new SQLiteCommand(query, con);
            if (args != null)
                foreach (var (key, value) in args)
                    cmd.Parameters.AddWithValue(key, value);

            var da = new SQLiteDataAdapter(cmd);

            var dt = new DataTable();
            da.Fill(dt);

            da.Dispose();
            return dt;
        }
        catch (SQLiteException e)
        {
            Logger.Logger.WriteLine(e);
            throw new SQLiteException(e.Message);
        }
    }

    internal static object GetFirstValueFromDataTable(DataTable dt)
    {
        if (dt == null)
            return null;

        try
        {
            return dt.Rows[0].ItemArray[0];
        }
        catch
        {
            return null;
        }
    }

    public static long? GetIntFromColumn(DataRow dr, string columnName)
    {
        var o = dr[columnName];
        if (o is null or DBNull)
            return null;

        try
        {
            return Convert.ToInt64(o);
        }
        catch
        {
            return null;
        }
    }
}
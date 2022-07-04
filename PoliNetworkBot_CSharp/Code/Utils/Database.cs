#region

using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class Database
{
    public static int Execute(string? query, DbConfig? dbConfig, Dictionary<string, object?>? args = null)
    {
        Logger.Logger.WriteLine(query, LogSeverityLevel.DATABASE_QUERY); //todo metti gli args

        if (dbConfig == null) return 0;
        var connection = new MySqlConnection(dbConfig.GetConnectionString());

        var cmd = new MySqlCommand(query, connection);

        OpenConnection(connection);

        if (args != null)
            foreach (var (key, value) in args)
                cmd.Parameters.AddWithValue(key, value);

        var numberOfRowsAffected = cmd.ExecuteNonQuery();

        return numberOfRowsAffected;
    }

    public static DataTable? ExecuteSelect(string? query, DbConfig? dbConfig, Dictionary<string, object?>? args = null)
    {
        Logger.Logger.WriteLine(query, LogSeverityLevel.DATABASE_QUERY); //todo metti gli args

        if (dbConfig == null) return null;
        var connection = new MySqlConnection(dbConfig.GetConnectionString());

        var cmd = new MySqlCommand(query, connection);

        if (args != null)
            foreach (var (key, value) in args)
                cmd.Parameters.AddWithValue(key, value);

        OpenConnection(connection);

        var adapter = new MySqlDataAdapter
        {
            SelectCommand = cmd
        };

        var ret = new DataSet();

        adapter.Fill(ret);

        adapter.Dispose();

        return ret.Tables[0];
    }

    private static void OpenConnection(IDbConnection connection)
    {
        if (connection.State != ConnectionState.Open) connection.Open();
    }

    internal static object? GetFirstValueFromDataTable(DataTable? dt)
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
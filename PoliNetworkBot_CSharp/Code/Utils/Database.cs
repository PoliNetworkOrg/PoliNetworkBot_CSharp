#region

using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class Database
{



    public static int Execute(string query, DbConfig dbConfig, Dictionary<string, object> args = null)
    {
        var connection = new MySqlConnection(dbConfig.GetConnectionString());
        var cmd = new MySqlCommand(query, connection);
        
        OpenConnection(connection);
        
        if (args != null)
            foreach (var (key, value) in args)
                cmd.Parameters.AddWithValue(key, value);


        var numberOfRowsAffected = cmd.ExecuteNonQuery();
            
            
        return numberOfRowsAffected;
        
    }

    public static DataTable ExecuteSelect(string query, DbConfig dbConfig, Dictionary<string, object> args = null)
    {
        var connection = new MySqlConnection(dbConfig.GetConnectionString());
        var cmd = new MySqlCommand(query, connection);
        
        if (args != null)
            foreach (var (key, value) in args)
                cmd.Parameters.AddWithValue(key, value);

        OpenConnection(connection);
        MySqlDataReader dr;

            dr = cmd.ExecuteReader();
     

        var dt = new DataTable();
        var da = new MySqlDataAdapter(cmd);
        
        if (dr.HasRows)
        {
            dr.Read();
        }

        da.Fill(dt);
        da.Dispose();
        dr.Close();
        dr.Dispose();
        return dt;
        
    }

    private static void OpenConnection(MySqlConnection connection)
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
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
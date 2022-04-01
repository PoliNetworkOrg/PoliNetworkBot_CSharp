#region

using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using PoliNetworkBot_CSharp.Code.Data;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class Database
{

    /*
    public static int Execute(string query, Dictionary<string, object> args = null)
    {
        //setup the connection to the database
        using var con = new SQLiteConn(Paths.Db);
        con.Open();
        return Execute(query, con, args);
    }

    public static int Execute(string query, SQLiteConnection connection, Dictionary<string, object> args = null)
    {
        //open a new command
        using var cmd = new SQLiteCommand(query, connection);
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
            return ExecuteSelect(query, con, args);
        }
        catch (SQLiteException e)
        {
            Logger.Logger.WriteLine(e);
            throw new SQLiteException(e.Message);
        }
    }

    public static DataTable ExecuteSelect(string query, SQLiteConnection connection , Dictionary<string, object> args = null)
    {
        using var cmd = new SQLiteCommand(query, connection);
        if (args != null)
            foreach (var (key, value) in args)
                cmd.Parameters.AddWithValue(key, value);

        var da = new SQLiteDataAdapter(cmd);

        var dt = new DataTable();
        da.Fill(dt);

        da.Dispose();
        return dt;
    }
*/

    public static int Execute(string query, MySqlConnection connection, Dictionary<string, object> args = null)
    {
        var cmd = new MySqlCommand(query, connection);
        
        OpenConnection(connection);
        
        if (args != null)
            foreach (var (key, value) in args)
                cmd.Parameters.AddWithValue(key, value);

        lock (connection)
        {
            var numberOfRowsAffected = cmd.ExecuteNonQuery();
            
            
            return numberOfRowsAffected;
        }
    }

    public static DataTable ExecuteSelect(string query, MySqlConnection connection, Dictionary<string, object> args = null)
    {
        
        var cmd = new MySqlCommand(query, connection);
        
        if (args != null)
            foreach (var (key, value) in args)
                cmd.Parameters.AddWithValue(key, value);

        OpenConnection(connection);
        MySqlDataReader dr;
        lock (connection)
        {
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
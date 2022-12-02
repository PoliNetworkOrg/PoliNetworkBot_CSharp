﻿#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.DbObject;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class Database
{
    public static int Execute(string? query, DbConfigConnection? dbConfigConnection,
        Dictionary<string, object?>? args = null)
    {
        Logger.Logger.WriteLine(query, LogSeverityLevel.DATABASE_QUERY); //todo metti gli args

        if (dbConfigConnection == null)
            return 0;
        var connectionWithLock = dbConfigConnection.GetMySqlConnection();
        var connection = connectionWithLock.Conn;
        int numberOfRowsAffected;
        lock (connectionWithLock.Lock)
        {
            var cmd = new MySqlCommand(query, connection);

            OpenConnection(connection);

            if (args != null)
                foreach (var (key, value) in args)
                    cmd.Parameters.AddWithValue(key, value);

            numberOfRowsAffected = cmd.ExecuteNonQuery();
        }

        dbConfigConnection.ReleaseConn(connectionWithLock);
        return numberOfRowsAffected;
    }

    public static DataTable? ExecuteSelect(string? query, DbConfigConnection? dbConfigConnection,
        Dictionary<string, object?>? args = null, ToLog toLog = ToLog.YES)
    {
        if (toLog == ToLog.YES)
            Logger.Logger.WriteLine(query, LogSeverityLevel.DATABASE_QUERY); //todo metti gli args

        if (dbConfigConnection == null) return null;
        var connectionWithLock = dbConfigConnection.GetMySqlConnection();
        var connection = connectionWithLock.Conn;
        var ret = new DataSet();
        lock (connectionWithLock.Lock)
        {
            var cmd = new MySqlCommand(query, connection);

            if (args != null)
                foreach (var (key, value) in args)
                    cmd.Parameters.AddWithValue(key, value);

            OpenConnection(connection);

            var adapter = new MySqlDataAdapter
            {
                SelectCommand = cmd
            };


            adapter.Fill(ret);

            adapter.Dispose();
        }


        dbConfigConnection.ReleaseConn(connectionWithLock);
        return ret.Tables[0];
    }

    private static void OpenConnection(IDbConnection connection)
    {
        if (connection.State != ConnectionState.Open)
            connection.Open();
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

    public static async Task QueryBotExec(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        _ = await CommandDispatcher.QueryBot(true, e, sender);
    }

    public static async Task QueryBotSelect(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        _ = await CommandDispatcher.QueryBot(false, e, sender);
    }

    
    public static int BulkInsertMySql(DataTable table, string tableName, DbConfigConnection? dbConfigConnection)
    {
      
        if (dbConfigConnection == null)
            return 0;
        
        var connectionWithLock = dbConfigConnection.GetMySqlConnection();
        var connection = connectionWithLock.Conn;
        int numberOfRowsAffected;
        
        var colonne =  CreateTable_DestroyIfExist(table, tableName, dbConfigConnection);
        var table2 = FixDataTable(table, colonne);
        
        lock (connectionWithLock.Lock)
        {
            OpenConnection(connection);
            numberOfRowsAffected = BulkInsertMySql2(connection, tableName, table2);
        }

        dbConfigConnection.ReleaseConn(connectionWithLock);

        return numberOfRowsAffected;


    }

    private static DataTable FixDataTable(DataTable table, List<Colonna> colonne)
    {
        var dt = new DataTable();
        for (var i = 0; i < table.Columns.Count; i++)
        {
            var colonna = colonne[i];
            dt.Columns.Add(colonna.Name, colonna.DataType);
        }

        foreach (DataRow dr in table.Rows)
        {
            var objects = FixDataRow(dr, colonne);
            dt.Rows.Add(objects);
        }
        return dt;
    }

    private static object?[] FixDataRow(DataRow dr, List<Colonna> colonne)
    {
        var r = new object?[dr.ItemArray.Length];
        for (var i = 0; i < dr.ItemArray.Length; i++)
        {
            r[i] = FixDataCell(dr.ItemArray[i], colonne[i]);
        }
        return r;
    }

    private static object? FixDataCell(object? source, Colonna colonna)
    {
        if (source == null)
            return null;
        
        var s = source.ToString() ?? "";
        
        if (colonna.DataType == typeof(int))
        {
            return int.Parse(s);
        }

        if (colonna.DataType == typeof(long))
        {
            return long.Parse(s); 
        }
        
        if (colonna.DataType == typeof(bool))
        {
            return s == "1";
        }
        
        if (colonna.DataType == typeof(char))
        {
            var x = int.Parse(s);
            return (char)x;
        }

        if (colonna.DataType == typeof(DateTime))
        {
            return DateTime.Parse(s);
        }

        ;

        return null;
    }

    /// <summary>
    /// Destroy the table if exists and recreate it
    /// </summary>
    /// <param name="table">DataTable of new table</param>
    /// <param name="tableName">Name of new table</param>
    /// <param name="dbConfigConnection">Connessione </param>
    private static List<Colonna> CreateTable_DestroyIfExist(DataTable table, string tableName,
        DbConfigConnection dbConfigConnection)
    {
        TryDestroyTable(tableName, dbConfigConnection);
        return CreateTable_As_It_Doesnt_Exist(table, tableName, dbConfigConnection);
    }

    private static void TryDestroyTable(string tableName, DbConfigConnection dbConfigConnection)
    {
        try
        {
            Execute("DROP TABLE " + tableName, dbConfigConnection);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine("Can't DROP table " + tableName);
        }
    }

    private static List<Colonna> CreateTable_As_It_Doesnt_Exist(DataTable table, string tableName, DbConfigConnection dbConfigConnection)
    {
        var q = GenerateCreateTableQuery(table, tableName);
        Execute(q.Item1, dbConfigConnection);
        return q.Item2;
    }

    private static Tuple<string, List<Colonna>> GenerateCreateTableQuery(DataTable table, string tableName)
    {
        var r = "CREATE TABLE " + tableName + "(";
        var rC = new List<Colonna>();
        for (int i = 0; i < table.Columns.Count; i++)
        {
            var x = table.Columns[i];
            r += x.ColumnName;
            r += " ";
            Tuple<string?> c = MySqlStringTypeFromDataType(x, TryGetNonNullValueAsExample(table, i));

            r += c.Item1;
            
            if (i != table.Columns.Count -1)
                r += ",\n";
        }
        r += ");";
        return new Tuple<string, List<Colonna>>(r,rC);
    }

    private static List<object> TryGetNonNullValueAsExample(DataTable table, int i)
    {
        var r = new List<object>();
        try
        {
            foreach (DataRow dr in table.Rows)
            {
                try
                {
                    var x = dr.ItemArray[i];
                    if (x == null) 
                        continue;
                    
                    var s = x.ToString();
                    if (!string.IsNullOrEmpty(s))
                        r.Add(x);
                }
                catch
                {
                    ;
                }
            }
        }
        catch
        {
            ;
        }

        return r;
    }

    private static Tuple<string?> MySqlStringTypeFromDataType(DataColumn xDataColumn, List<object> exampleValue)
    {
        var xDataType = xDataColumn.DataType;
        
        var strings = GetStrings(exampleValue);
        
        if (typeof(int) == xDataType)
        {
            return new Tuple<string?>( "INT");
        }
        else if (typeof(long) == xDataType)
        {
            if (AllYN(strings))
                return new Tuple<string?>("CHAR");
            return new Tuple<string?>("BIGINT");
        }

        var enumerable = strings.ToList();
        if (enumerable.All(x => x is "0" or "1"))
            return new Tuple<string?>("BOOLEAN");

        var dateTime = TryGetDateTime(exampleValue.First());
        ;

        if (dateTime != null)
        {
            return new Tuple<string?>("DATETIME");
        }

        ;

        var maxLength = GetMaxLength(enumerable);
  
        if (maxLength != null)
        {
            var length = maxLength.Value * 10;
            return length > 500 ? new Tuple<string?>("TEXT") : new Tuple<string?>("VARCHAR(500)");
        }

        return new Tuple<string?>(null);
    }

    private static bool AllYN(IEnumerable<string> strings)
    {
        char _cy = 'Y';
        char _cs = 'S';
        char _cn = 'N';
        int _iy = (int)_cy;
        int _is = (int)_cs;
        int _in = (int)_cn;
        return strings.All(x =>
        {
            try
            {
             

                    int xc = int.Parse(x);
                    if (xc == _in || xc == _is || xc == _iy)
                        return true;

                

                return false;
            }
            catch
            {
                ;
            }

            return false;

        });
    }

    private static int? GetMaxLength(IEnumerable<string> strings)
    {
        return strings.Max(x => x.Length);
    }

    private static IEnumerable<string> GetStrings(List<object> exampleValue)
    {
        var r = new List<string>();
        foreach (var item in exampleValue)
        {
            var s = item.ToString();
            if (s != null) 
                r.Add(s);
        }
        return r;
    }

    private static DateTime? TryGetDateTime(object? exampleValue)
    {
        try
        {
            var s = exampleValue?.ToString();
            if (s != null)
            {
                var x = DateTime.Parse(s);
                return x;
            }
        }
        catch
        {
            ;
        }

        return null;
    }

    private static int BulkInsertMySql2(MySqlConnection connection, string tableName, DataTable table)
    {
        using var tran = connection.BeginTransaction(IsolationLevel.Serializable);
        using var cmd = new MySqlCommand();
        cmd.Connection = connection;
        cmd.Transaction = tran;
        cmd.CommandText = $"SELECT * FROM " + tableName + " limit 0";

        using var adapter = new MySqlDataAdapter(cmd);
        adapter.UpdateBatchSize = 10000;
        using var cb = new MySqlCommandBuilder(adapter);
        cb.SetAllValues = true;
        var numberOfRowsAffected = adapter.Update(table);
        tran.Commit();
        return numberOfRowsAffected;
    }
}
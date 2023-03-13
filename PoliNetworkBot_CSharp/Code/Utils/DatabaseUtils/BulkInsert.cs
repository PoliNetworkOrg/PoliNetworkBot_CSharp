﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.DbObject;

namespace PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;

public static class BulkInsert
{
    public static int? BulkInsertMySql(DataTable table, string tableName, DbConfigConnection? dbConfigConnection)
    {
        if (dbConfigConnection == null)
            return 0;

        var connectionWithLock = dbConfigConnection.GetMySqlConnection();
        int? numberOfRowsAffected;

        var colonne = CreateTable_DestroyIfExist(table, tableName, dbConfigConnection);
        var table2 = FixDataTable(table, colonne);

        lock (connectionWithLock.Lock)
        {
            numberOfRowsAffected = BulkInsertMySql2(connectionWithLock, tableName, table2);
        }

        dbConfigConnection.ReleaseConn(connectionWithLock);

        return numberOfRowsAffected;
    }


    private static DataTable FixDataTable(DataTable table, IReadOnlyList<Colonna> colonne)
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

    private static object?[] FixDataRow(DataRow dr, IReadOnlyList<Colonna> colonne)
    {
        var r = new object?[dr.ItemArray.Length];
        for (var i = 0; i < dr.ItemArray.Length; i++) r[i] = FixDataCell(dr.ItemArray[i], colonne[i]);
        return r;
    }


    private static object? FixDataCell(object? source, Colonna colonna)
    {
        try
        {
            if (source == null)
                return null;

            var s = source.ToString() ?? "";

            if (string.IsNullOrEmpty(s))
                return null;

            if (colonna.DataType == typeof(int)) return int.Parse(s);

            if (colonna.DataType == typeof(long)) return long.Parse(s);

            if (colonna.DataType == typeof(bool)) return s == "1";

            if (colonna.DataType == typeof(char))
            {
                var x = int.Parse(s);
                return (char)x;
            }

            if (colonna.DataType == typeof(DateTime)) return DateTime.Parse(s);

            return s;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return null;
    }


    /// <summary>
    ///     Destroy the table if exists and recreate it
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
            Database.Execute("DROP TABLE " + tableName, dbConfigConnection);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine("Can't DROP table " + tableName);
        }
    }

    private static List<Colonna> CreateTable_As_It_Doesnt_Exist(DataTable table, string tableName,
        DbConfigConnection dbConfigConnection)
    {
        var q = GenerateCreateTableQuery(table, tableName);
        Database.Execute(q.Item1, dbConfigConnection);
        return q.Item2;
    }


    private static Tuple<string, List<Colonna>> GenerateCreateTableQuery(DataTable table, string tableName)
    {
        var r = "CREATE TABLE " + tableName + "(";
        var rC = new List<Colonna>();
        for (var i = 0; i < table.Columns.Count; i++)
        {
            var x = table.Columns[i];
            r += x.ColumnName;
            r += " ";
            var c = MySqlStringTypeFromDataType(x, TryGetNonNullValueAsExample(table, i));

            r += c.Item1;
            rC.Add(c.Item2);

            if (i != table.Columns.Count - 1)
                r += ",\n";
        }

        r += ");";
        return new Tuple<string, List<Colonna>>(r, rC);
    }


    private static List<object> TryGetNonNullValueAsExample(DataTable table, int i)
    {
        var r = new List<object>();
        try
        {
            foreach (DataRow dr in table.Rows)
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
                    // ignored
                }
        }
        catch
        {
            // ignored
        }

        return r;
    }

    private static Tuple<string?, Colonna> MySqlStringTypeFromDataType(DataColumn xDataColumn,
        IReadOnlyCollection<object> exampleValue)
    {
        var xDataType = xDataColumn.DataType;

        var strings = GetStrings(exampleValue);

        if (typeof(int) == xDataType)
            return new Tuple<string?, Colonna>("INT", new Colonna(xDataColumn.ColumnName, typeof(int)));
        if (typeof(long) == xDataType)
            return AllYn(strings)
                ? new Tuple<string?, Colonna>("CHAR", new Colonna(xDataColumn.ColumnName, typeof(char)))
                : new Tuple<string?, Colonna>("BIGINT", new Colonna(xDataColumn.ColumnName, typeof(long)));

        var enumerable = strings.ToList();
        if (enumerable.All(x => x is "0" or "1"))
            return new Tuple<string?, Colonna>("BOOLEAN", new Colonna(xDataColumn.ColumnName, typeof(bool)));

        var dateTime = TryGetDateTime(exampleValue.First());

        if (dateTime != null)
            return new Tuple<string?, Colonna>("DATETIME", new Colonna(xDataColumn.ColumnName, typeof(DateTime)));

        var maxLength = GetMaxLength(enumerable);

        if (maxLength == null)
            return new Tuple<string?, Colonna>(null, new Colonna(xDataColumn.ColumnName, typeof(object)));

        var length = maxLength.Value * 10;
        return length > 500
            ? new Tuple<string?, Colonna>("TEXT", new Colonna(xDataColumn.ColumnName, typeof(string)))
            : new Tuple<string?, Colonna>("VARCHAR(500)", new Colonna(xDataColumn.ColumnName, typeof(string)));
    }

    private static bool AllYn(IEnumerable<string?> strings)
    {
        const char _cy = 'Y';
        const char _cs = 'S';
        const char _cn = 'N';
        const int _iy = _cy;
        const int _is = _cs;
        const int _in = _cn;
        return strings.All(x =>
        {
            if (string.IsNullOrEmpty(x))
                return false;

            try
            {
                var xc = int.Parse(x);
                return xc is _in or _is or _iy;
            }
            catch
            {
                // ignored
            }

            return false;
        });
    }

    private static int? GetMaxLength(IEnumerable<string?> strings)
    {
        return strings.Max(x => x?.Length ?? -1);
    }

    private static IEnumerable<string?> GetStrings(IEnumerable<object?> exampleValue)
    {
        return exampleValue.Select(item => item?.ToString()).Where(s => !string.IsNullOrEmpty(s)).ToList();
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
            // ignored
        }

        return null;
    }

    private static int? BulkInsertMySql2(MySqlConnectionWithLock connection, string tableName, DataTable table)
    {
        return connection.BulkInsert(tableName, table);
    }
}
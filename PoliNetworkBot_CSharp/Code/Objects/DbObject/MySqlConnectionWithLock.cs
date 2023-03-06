#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using MySql.Data.MySqlClient;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.DbObject;

public class MySqlConnectionWithLock
{
    public readonly object Lock;
    private MySqlConnection? _conn;
    private readonly string? _connectionStringMysql;
    private SQLiteConnection? _connectionTemp;

    public MySqlConnectionWithLock(string getConnectionString)
    {
        _connectionStringMysql = getConnectionString;
        Lock = new object();
    }


    public int? BulkInsert(string tableName, DataTable table)
    {
        OpenConnection();

        if (_connectionTemp != null)
            lock (Lock)
            {
                using var tran = _connectionTemp.BeginTransaction(IsolationLevel.Serializable);
                using var cmd = new SQLiteCommand();

                cmd.Connection = _connectionTemp;
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT * FROM " + tableName + " limit 0";


                //DbDataAdapter, IDbDataAdapter, IDataAdapter
                using var adapter = new SQLiteDataAdapter(cmd);
                adapter.UpdateBatchSize = 10000;
                using var cb = new SQLiteCommandBuilder(adapter);
                cb.SetAllValues = true;
                var numberOfRowsAffected = adapter.Update(table);
                tran.Commit();
                return numberOfRowsAffected;
            }

        if (_conn != null)
            lock (_conn)
            {
                using var tran = _conn.BeginTransaction(IsolationLevel.Serializable);
                using var cmd = new MySqlCommand();
                cmd.Connection = _conn;
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT * FROM " + tableName + " limit 0";


                using var adapter = new MySqlDataAdapter(cmd);
                adapter.UpdateBatchSize = 10000;
                using var cb = new MySqlCommandBuilder(adapter);
                cb.SetAllValues = true;
                var numberOfRowsAffected = adapter.Update(table);
                tran.Commit();
                return numberOfRowsAffected;
            }

        return null;
    }

    public int? ExecuteSlave(string? query, DbConfigConnection dbConfigConnection, Dictionary<string, object?>? args)
    {
        OpenConnection();

        if (_connectionTemp != null)
        {
            int numberOfRowsAffected;
            lock (Lock)
            {
                var cmd = new SQLiteCommand(query, _connectionTemp);


                if (args != null)
                    foreach (var (key, value) in args)
                        cmd.Parameters.AddWithValue(key, value);

                numberOfRowsAffected = cmd.ExecuteNonQuery();
            }

            dbConfigConnection.ReleaseConn(this);
            return numberOfRowsAffected;
        }

        if (_conn != null)
        {
            int numberOfRowsAffected;
            lock (Lock)
            {
                var cmd = new MySqlCommand(query, _conn);


                if (args != null)
                    foreach (var (key, value) in args)
                        cmd.Parameters.AddWithValue(key, value);

                numberOfRowsAffected = cmd.ExecuteNonQuery();
            }

            dbConfigConnection.ReleaseConn(this);
            return numberOfRowsAffected;
        }

        return null;
    }

    private void OpenConnection()
    {
        if (_conn == null && _connectionTemp == null)
            lock (Lock)
            {
                _conn = new MySqlConnection(_connectionStringMysql);
            }

        if (this is { _conn.State: ConnectionState.Open } or { _connectionTemp.State: ConnectionState.Open })
            return;

        if (_connectionTemp is { State: ConnectionState.Open })
            return;

        var done = false;
        try
        {
            if (_conn == null) return;
            lock (_conn)
            {
                _conn?.Open();
                done = true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        if (done) return;

        ConnectToSqliteDb();
    }

    private void ConnectToSqliteDb()
    {
        var connectionTemp = new SQLiteConnection("Data Source=temp.db");
        connectionTemp.Open();
        _connectionTemp = connectionTemp;
    }

    public DataTable? ExecuteSelectSlave(string? query, DbConfigConnection? dbConfigConnection,
        Dictionary<string, object?>? args)
    {
        if (dbConfigConnection == null)
            return null;

        OpenConnection();

        if (_connectionTemp != null)
        {
            var ret = new DataSet();
            lock (Lock)
            {
                var cmd = new SQLiteCommand(query, _connectionTemp);

                if (args != null)
                    foreach (var (key, value) in args)
                        cmd.Parameters.AddWithValue(key, value);


                var adapter = new SQLiteDataAdapter
                {
                    SelectCommand = cmd
                };

                adapter.Fill(ret);
                adapter.Dispose();
            }

            dbConfigConnection.ReleaseConn(this);
            return ret.Tables[0];
        }

        if (_conn != null)
        {
            var ret = new DataSet();
            lock (Lock)
            {
                var cmd = new MySqlCommand(query, _conn);

                if (args != null)
                    foreach (var (key, value) in args)
                        cmd.Parameters.AddWithValue(key, value);


                var adapter = new MySqlDataAdapter
                {
                    SelectCommand = cmd
                };

                adapter.Fill(ret);
                adapter.Dispose();
            }

            dbConfigConnection.ReleaseConn(this);
            return ret.Tables[0];
        }

        return null;
    }
}
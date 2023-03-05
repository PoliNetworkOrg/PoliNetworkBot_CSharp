#region

using System.Collections.Generic;
using System.Data; 
using Devart.Data.SQLite; 
using MySql.Data.MySqlClient;
using PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.DbObject;

public class MySqlConnectionWithLock
{
    private readonly MySqlConnection? _conn;
    public readonly object Lock;
    public SQLiteConnection? ConnectionTemp;
    
    public MySqlConnectionWithLock(string getConnectionString)
    {
        _conn = new MySqlConnection(getConnectionString);
        Lock = new object();
    }


    public int? BulkInsert(string tableName, DataTable table)
    {
        if (ConnectionTemp != null)
        {         
            lock (Lock)
            {
                using var tran = ConnectionTemp.BeginTransaction(IsolationLevel.Serializable);
                using var cmd = new SQLiteCommand();

                cmd.Connection = ConnectionTemp;
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT * FROM " + tableName + " limit 0";
                
                OpenConnection();

            
                //DbDataAdapter, IDbDataAdapter, IDataAdapter
                using var adapter = new SQLiteDataAdapter(cmd);
                adapter.UpdateBatchSize = 10000;
                using var cb = new SQLiteCommandBuilder(adapter);
                cb.SetAllValues = true;
                var numberOfRowsAffected = adapter.Update(table);
                tran.Commit();
                return numberOfRowsAffected;
            }
        }

        if (_conn != null)
        {
            using var tran = _conn.BeginTransaction(IsolationLevel.Serializable);
            using var cmd = new MySqlCommand();
            cmd.Connection = _conn;
            cmd.Transaction = tran;
            cmd.CommandText = "SELECT * FROM " + tableName + " limit 0";
            
            OpenConnection();

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
        if (ConnectionTemp != null)
        {
            int numberOfRowsAffected;
            lock (Lock)
            {
                var cmd = new SQLiteCommand(query, this.ConnectionTemp);


                OpenConnection();

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
                var cmd = new MySqlCommand(query, this._conn);


                OpenConnection();

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
        if (this is { _conn.State: ConnectionState.Open } or { ConnectionTemp.State: ConnectionState.Open })
            return;

        try
        {
            this._conn?.Open();
        }
        catch
        {
            var connectionTemp = new SQLiteConnection("Data Source=temp.db");
            connectionTemp.Open();
            this.ConnectionTemp = connectionTemp;
        }
    }
    
    public DataTable? ExecuteSelectSlave(string? query, DbConfigConnection? dbConfigConnection, Dictionary<string, object?>? args)
    {
        if (dbConfigConnection == null)
            return null;
        
        if (ConnectionTemp != null)
        {
            var ret = new DataSet();
            lock (Lock)
            {
                var cmd = new SQLiteCommand(query, this.ConnectionTemp);

                if (args != null)
                    foreach (var (key, value) in args)
                        cmd.Parameters.AddWithValue(key, value);

                OpenConnection();

                var adapter = new SQLiteDataAdapter()
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
                var cmd = new MySqlCommand(query, this._conn);

                if (args != null)
                    foreach (var (key, value) in args)
                        cmd.Parameters.AddWithValue(key, value);

                OpenConnection();

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
#region

using System.Data; 
using Devart.Data.SQLite; 
using MySql.Data.MySqlClient;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.DbObject;

public class MySqlConnectionWithLock
{
    public readonly MySqlConnection? Conn;
    public readonly object Lock;

    public MySqlConnectionWithLock(string getConnectionString)
    {
        Conn = new MySqlConnection(getConnectionString);
        Lock = new object();
    }

    public SQLiteConnection? ConnectionTemp;

    public IDbConnection? GetConnection()
    {
        return (IDbConnection?)this.ConnectionTemp ?? this.Conn;
    }

    public int? BulkInsert(string tableName, DataTable table)
    {
        if (ConnectionTemp != null)
        {
            using var tran = ConnectionTemp.BeginTransaction(IsolationLevel.Serializable);
            using var cmd = new SQLiteCommand();
            cmd.Connection = ConnectionTemp;
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

        if (Conn != null)
        {
            using var tran = Conn.BeginTransaction(IsolationLevel.Serializable);
            using var cmd = new MySqlCommand();
            cmd.Connection = Conn;
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
}
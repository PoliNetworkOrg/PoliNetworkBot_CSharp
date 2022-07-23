#region

using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Objects.DbObject;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class DbConfigConnection
{
    private readonly DbConfig _dbConfig;
    private QueueThreadSafe? _mySqlConnection;

    public DbConfigConnection(DbConfig? dbConfig)
    {
        dbConfig ??= new DbConfig();
        _dbConfig = dbConfig;
    }

    public MySqlConnectionWithLock GetMySqlConnection()
    {
        if (_mySqlConnection != null)
            return _mySqlConnection.GetFirstAvailable();

        _mySqlConnection = new QueueThreadSafe(_dbConfig.GetConnectionString());
        return _mySqlConnection.GetFirstAvailable();
    }

    public DbConfig GetDbConfig()
    {
        return _dbConfig;
    }

    public void ReleaseConn(MySqlConnectionWithLock connectionWithLock)
    {
        _mySqlConnection?.ReleaseConn(connectionWithLock);
    }
}
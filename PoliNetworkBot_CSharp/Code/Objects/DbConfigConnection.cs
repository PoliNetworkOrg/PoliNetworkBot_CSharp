#region

using MySql.Data.MySqlClient;
using PoliNetworkBot_CSharp.Code.Config;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class DbConfigConnection
{
    private readonly DbConfig _dbConfig;
    private MySqlConnection? _mySqlConnection;

    public DbConfigConnection(DbConfig? dbConfig)
    {
        dbConfig ??= new DbConfig();
        _dbConfig = dbConfig;
    }

    public MySqlConnection GetMySqlConnection()
    {
        if (_mySqlConnection != null)
            return _mySqlConnection;

        _mySqlConnection = new MySqlConnection(_dbConfig.GetConnectionString());
        return _mySqlConnection;
    }

    public DbConfig GetDbConfig()
    {
        return _dbConfig;
    }
}
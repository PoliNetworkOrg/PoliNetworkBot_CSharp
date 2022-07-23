using MySql.Data.MySqlClient;

namespace PoliNetworkBot_CSharp.Code.Objects.DbObject;

public class MySqlConnectionWithLock
{
    public readonly MySqlConnection Conn;
    public readonly object Lock;
    public MySqlConnectionWithLock(string getConnectionString)
    {
        Conn = new MySqlConnection(getConnectionString);
        Lock = new object();
    }
}
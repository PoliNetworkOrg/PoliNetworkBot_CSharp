using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Objects.DbObject;

public class QueueThreadSafe
{
    private readonly string _connString;
    private readonly Queue<MySqlConnectionWithLock> _available;
    public QueueThreadSafe(string getConnectionString)
    {
        _connString = getConnectionString;
        _available = new Queue<MySqlConnectionWithLock>();
        _available.Enqueue(new MySqlConnectionWithLock(_connString));
    }

    public MySqlConnectionWithLock GetFirstAvailable()
    {
        lock (this)
        {
            if (_available.Count == 0)
            {
                _available.Enqueue(new MySqlConnectionWithLock(_connString));
            }
            return _available.Dequeue();
          
        }
    }

    public void ReleaseConn(MySqlConnectionWithLock connectionWithLock)
    {
        lock (this)
        {
            _available.Enqueue(connectionWithLock);
        }
    }
}
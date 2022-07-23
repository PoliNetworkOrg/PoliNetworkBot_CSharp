#region

using System.Collections.Generic;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.DbObject;

public class QueueThreadSafe
{
    private readonly Queue<MySqlConnectionWithLock> _available;
    private readonly string _connString;

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
            if (_available.Count == 0) _available.Enqueue(new MySqlConnectionWithLock(_connString));
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
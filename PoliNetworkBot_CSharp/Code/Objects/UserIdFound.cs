namespace PoliNetworkBot_CSharp.Code.Objects;

internal class UserIdFound
{
    private readonly long? _i;
    private readonly string _v;

    private UserIdFound(long? i)
    {
        this._i = i;
    }

    public UserIdFound(long? i, string v) : this(i)
    {
        this._v = v;
    }

    internal long? GetId()
    {
        return _i;
    }

    internal string GetError()
    {
        return _v;
    }
}
namespace PoliNetworkBot_CSharp.Code.Objects;

internal class UserIdFound
{
    private readonly long? _i;
    private readonly string? _v;

    private UserIdFound(long? i)
    {
        _i = i;
    }

    public UserIdFound(long? i, string? v) : this(i)
    {
        _v = v;
    }

    internal long? GetId()
    {
        return _i;
    }

    internal string? GetError()
    {
        return _v;
    }
}
namespace PoliNetworkBot_CSharp.Code.Objects;

internal class UserIdFound
{
    private readonly long? i;
    private readonly string v;

    public UserIdFound(long? i)
    {
        this.i = i;
    }

    public UserIdFound(long? i, string v) : this(i)
    {
        this.v = v;
    }

    internal long? GetID()
    {
        return i;
    }

    internal string GetError()
    {
        return v;
    }
}
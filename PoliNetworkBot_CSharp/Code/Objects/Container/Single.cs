namespace PoliNetworkBot_CSharp.Code.Objects.Container;

public class Single<T>
{
    public T? Obj;

    public Single(T? obj)
    {
        Obj = obj;
    }
}
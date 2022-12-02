namespace PoliNetworkBot_CSharp.Code.Objects.TmpResults;

public class HasBeenSent
{
    public readonly int I;
    public readonly string? S;
    public bool? B;

    public HasBeenSent(bool? b1, int i, string s1)
    {
        B = b1;
        I = i;
        S = s1;
    }
}
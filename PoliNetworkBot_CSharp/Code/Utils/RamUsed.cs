using System;

namespace PoliNetworkBot_CSharp.Code.Utils;

[Serializable]
public class RamUsed
{
    internal long Ram1;
    internal long Ram2;

    public RamUsed(long ramUsed1, long ramUsed2)
    {
        Ram1 = ramUsed1;
        Ram2 = ramUsed2;
    }

    public override string ToString()
    {
        return Ram1 + " " + Ram2;
    }

    public bool InferioreDi(RamUsed ramUsed)
    {
        return Ram1 * 1.2d < ramUsed.Ram1 || Ram2 * 1.2d < ramUsed.Ram2;
    }
}
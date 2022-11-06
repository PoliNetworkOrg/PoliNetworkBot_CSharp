#region

using System.Collections.Generic;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.BanUnban;

public class BanUnbanAllResultComplete
{
    public readonly BanUnbanAllResult BanUnbanAllResult;
    public readonly List<ExceptionNumbered> Exceptions;
    public readonly int NExceptions;

    public BanUnbanAllResultComplete(BanUnbanAllResult banUnbanAllResult, List<ExceptionNumbered> exceptions,
        int nExceptions)
    {
        Exceptions = exceptions;
        NExceptions = nExceptions;
        BanUnbanAllResult = banUnbanAllResult;
    }

    public void Deconstruct(out BanUnbanAllResult banunbanallresult, out int item3)
    {
        banunbanallresult = BanUnbanAllResult;
        item3 = NExceptions;
    }
}
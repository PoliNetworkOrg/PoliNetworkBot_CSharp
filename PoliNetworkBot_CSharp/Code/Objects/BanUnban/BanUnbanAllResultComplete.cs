using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Objects.BanUnban;

public class BanUnbanAllResultComplete
{
    public readonly BanUnbanAllResult BanUnbanAllResult;
    public readonly List<ExceptionNumbered> Exceptions;
    public readonly int NExceptions;
    public BanUnbanAllResultComplete(BanUnbanAllResult banUnbanAllResult, List<ExceptionNumbered> exceptions, int nExceptions )
    {
        this.Exceptions = exceptions;
        this.NExceptions = nExceptions;
        this.BanUnbanAllResult = banUnbanAllResult;
    }

    public void Deconstruct(out BanUnbanAllResult banunbanallresult, out int item3)
    {
        banunbanallresult = this.BanUnbanAllResult;
        item3 = this.NExceptions;
    }
}
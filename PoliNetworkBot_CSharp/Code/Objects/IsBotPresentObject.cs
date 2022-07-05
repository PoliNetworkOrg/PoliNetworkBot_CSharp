#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class IsBotPresentObject
{
    public readonly bool? B;
    public readonly DateTime? DateTime;

    public IsBotPresentObject(bool? b, DateTime? dateTime)
    {
        B = b;
        DateTime = dateTime;
    }
}
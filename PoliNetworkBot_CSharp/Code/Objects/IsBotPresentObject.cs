using System;

namespace PoliNetworkBot_CSharp.Code.Objects;

public class IsBotPresentObject
{
    public readonly bool? B;
    public readonly DateTime? DateTime;

    public IsBotPresentObject(bool? b, DateTime? dateTime)
    {
        this.B = b;
        this.DateTime = dateTime;
    }
}
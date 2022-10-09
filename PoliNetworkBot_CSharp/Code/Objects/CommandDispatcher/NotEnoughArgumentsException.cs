#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.CommandDispatcher;

public class NotEnoughArgumentsException : ArgumentException
{
    public NotEnoughArgumentsException(Language message)
    {
        _message = message;
    }

    private Language _message { get; }
}
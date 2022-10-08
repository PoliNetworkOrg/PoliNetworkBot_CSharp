using System;
using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Objects.CommandDispatcher;

public class NotEnoughArgumentsException : ArgumentException
{
    public Language _message { get; }

    public NotEnoughArgumentsException(Language message)
    {
        _message = message;
    }
}
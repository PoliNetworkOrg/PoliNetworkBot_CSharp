using PoliNetworkBot_CSharp.Code.Data;

namespace PoliNetworkBot_CSharp.Code.Objects;

public class OnMessageMethodObject
{
    public readonly ActionMessageEvent? ActionMessageEvent;
    public readonly string? S;

    public OnMessageMethodObject(ActionMessageEvent? r1, string? s)
    {
        ActionMessageEvent = r1;
        S = s;
    }
}
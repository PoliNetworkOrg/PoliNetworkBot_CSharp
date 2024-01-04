using System;
using Newtonsoft.Json;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Model;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class ChatIdTgWith100
{
    public GithubInfo? GithubInfo;
    public long Id;
    public bool VaAggiuntoMeno100;


    public string GetString()
    {
        return VaAggiuntoMeno100 ? "-100" + Id : Id.ToString();
    }

    public long FullLong()
    {
        var value = GetString();
        return Convert.ToInt64(value);
    }
}
using System;
using Newtonsoft.Json;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Model;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class GithubInfo
{
    public string? CategoryGithub;
    public string? CustomOwnerGithub;
    public string? CustomRepoGithub;
}
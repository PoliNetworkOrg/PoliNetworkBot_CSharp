#region

using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public class GruppoTgUpdate
{
    public readonly GruppoTg? GruppoTg;
    public readonly SuccessoGenerazioneLink SuccessoGenerazioneLink;

    public GruppoTgUpdate(GruppoTg? p, SuccessoGenerazioneLink errore)
    {
        GruppoTg = p;
        SuccessoGenerazioneLink = errore;
    }

    public string? ExceptionMessage { get; internal set; }
    public bool? Query1Fallita { get; internal set; }
    public bool? Query2Fallita { get; internal set; }
    public bool? CreateInviteLinkFallita { get; internal set; }
}